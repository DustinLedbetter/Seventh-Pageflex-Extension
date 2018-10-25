/***********************************************************************************************************************************
*                                                 GOD First                                                                        *
* Author: Dustin Ledbetter                                                                                                         *
* Release Date: 10-4-2018                                                                                                          *
* Version: 7.0                                                                                                                     *
* Purpose: To create a seventh extension for the storefront to test how they work with avalara                                     *
************************************************************************************************************************************/

/*
 References: There are five dlls referenced by this template:
    First three are added references
    1. PageflexServices.dll
    2. StorefrontExtension.dll
    3. SXI.dll
    Last two are part of our USING Avalara.AvaTax.RestClient (These are added From NuGet Package Management)
    4. Avalara.AvaTax.RestClient.net45.dll
    5. Newtonsoft.Json.dll
 */
using Avalara.AvaTax.RestClient;
using Pageflex.Interfaces.Storefront;
using PageflexServices;
using System;
using System.IO;


namespace SeventhExtension
{

    public class ExtensionSeven : SXIExtension
    {

        #region |--Fields--|
        // This section holds variables for code used throughout the program for quick refactoring as needed

        private const string _UNIQUENAME = @"ExtensionSeven.FixTax.ByAvalara.website.com";
        private const string _DISPLAYNAME = @"Services: Extension Seven";
        private const string _SADEBUGGINGMODE = @"SaDebuggingMode";
        private static readonly string LOG_FILENAME1 = "D:\\Pageflex\\Deployments\\";
        private static readonly string LOG_FILENAME2 = "\\Logs\\Avalara_Extension_Log_File_";

        #endregion


        #region |--Properties--|
        // At a minimum your extension must override the DisplayName and UniqueName properties.


        // The UniqueName is used to associate a module with any data that it provides to Storefront.
        public override string UniqueName
        {
            get
            {
                return _UNIQUENAME;
            }
        }

        // The DisplayName will be shown on the Extensions and Site Options pages of the Administrator site as the name of your module.
        public override string DisplayName
        {
            get
            {
                return _DISPLAYNAME;
            }
        }

        // Gets the parameter to determine if in debug mode or not. Can also be used to get more variables at one as well
        protected override string[] PARAMS_WE_WANT
        {
            get
            {
                return new string[1]
                {
                  _SADEBUGGINGMODE
                };
            }
        }

        // Used to access the storefront to retrieve variables
        ISINI SF { get { return Storefront; } }

        // This Method is used to write all of our logs to a txt file
        public void LogMessageToFile(string msg)
        {
            // Get the Date and time stamps as desired
            string currentLogDate = DateTime.Now.ToString("MMddyyyy");
            string currentLogTimeInsertMain = DateTime.Now.ToString("HH:mm:ss tt");

            // Get the storefront's name from storefront to send logs to correct folder
            string sfName = SF.GetValue(FieldType.SystemProperty, SystemProperty.STOREFRONT_NAME, null);

            // Setup Message to display in .txt file 
            msg = string.Format("Time: {0:G}:  Message: {1}{2}", currentLogTimeInsertMain, msg, Environment.NewLine);

            // Add message to the file 
            File.AppendAllText(LOG_FILENAME1 + sfName + LOG_FILENAME2 + currentLogDate + ".txt", msg);
        }

        #endregion


        #region |--Setup an option for turning debug mode on or off on the storefront (determines if we send log messages or not)--|

        // This section sets up on the extension page on the storefront a check box for users to turn on or off debug mode
        public override int GetConfigurationHtml(KeyValuePair[] parameters, out string HTML_configString)
        {
            // Load and check if we already have a parameter set
            LoadModuleDataFromParams(parameters);

            // If not then we setup one 
            if (parameters == null)
            {
                SConfigHTMLBuilder sconfigHtmlBuilder = new SConfigHTMLBuilder();
                sconfigHtmlBuilder.AddHeader();
                sconfigHtmlBuilder.AddServicesHeader();

                // Add checkbox to let user turn on and off debug mode
                sconfigHtmlBuilder.AddCheckboxField("Debugging Information", _SADEBUGGINGMODE, "true", "false", (string)ModuleData[_SADEBUGGINGMODE] == "true");
                sconfigHtmlBuilder.AddTip(@"This box should be checked if you wish for debugging information to be output to the Logs.");

                sconfigHtmlBuilder.AddServicesFooter();
                HTML_configString = sconfigHtmlBuilder.html;
            }
            // If we do then move along
            else
            {
                SaveModuleData();
                HTML_configString = null;
            }
            return 0;
        }

        #endregion


        #region |--This section is used to determine if we are in the "shipping" or "payment" module on the storefront or not--|

        public override bool IsModuleType(string x)
        {
            // If we are in the shipping module return true to begin processes for this module
            if (x == "Shipping")
            {
                return true;
            }
            // If there is no shipping step and we go straight to the payment module return true to begin processes for this module here
            else if (x == "Payment")
            {
                return true;
            }
            // if we are not in either then just keep waiting
            else
                return false;
        }

        #endregion


        #region |--This section is used to figure out the tax rates and get the zipcode entered on the shipping form--|

        // This method is used to get adjust the tax rate for the user's order
        public override int CalculateTax2(string OrderID, double taxableAmount, string currencyCode, string[] priceCategories, string[] priceTaxLocales, double[] priceAmount, string[] taxLocaleId, ref double[] taxAmount)
        {

            #region |--This section of code shows what we have been passed if debug mode is "on"--|

            // Check if debug mode is turned on; If it is then we log these messages
            if ((string)ModuleData[_SADEBUGGINGMODE] == "true")
            {
                // Used to help to see where these mesages are in the Storefront logs page
                LogMessage($"*                        *");                                            // Adds a space for easier viewing
                LogMessage($"*          START         *");                                            // Show when we start this process
                LogMessage($"*                        *");

                // Shows what values are passed at beginning
                LogMessage($"OrderID is:              {OrderID}");                                    // Tells the id for the order being calculated
                LogMessage($"TaxableAmount is:        {taxableAmount.ToString()}");                   // Tells the amount to be taxed (currently set to 0)
                LogMessage($"CurrencyCode is:         {currencyCode}");                               // Tells the type of currency used
                LogMessage($"PriceCategories is:      {priceCategories.Length.ToString()}");          // Not Null, but empty
                LogMessage($"PriceTaxLocales is:      {priceTaxLocales.Length.ToString()}");          // Not Null, but empty
                LogMessage($"PriceAmount is:          {priceAmount.Length.ToString()}");              // Not Null, but empty
                LogMessage($"TaxLocaleId is:          {taxLocaleId.Length.ToString()}");              // Shows the number of tax locales found for this order
                LogMessage($"TaxLocaleId[0] is:       {taxLocaleId[0].ToString()}");                  // Sends a number value which corresponds to the tax rate row in the tax rates table excel file  

                // These Log the messages to a log .txt file
                // The logs an be found in the Logs folder in the storefront's deployment
                LogMessageToFile($"*                        *");                                      // Adds a space for easier viewing
                LogMessageToFile($"*          START         *");                                      // Show when we start this process
                LogMessageToFile($"*                        *");

                // Shows what values are passed at beginning in .txt file
                LogMessageToFile($"OrderID is:              {OrderID}");                              // Tells the id for the order being calculated
                LogMessageToFile($"TaxableAmount is:        {taxableAmount.ToString()}");             // Tells the amount to be taxed (currently set to 0)
                LogMessageToFile($"CurrencyCode is:         {currencyCode}");                         // Tells the type of currency used
                LogMessageToFile($"PriceCategories is:      {priceCategories.Length.ToString()}");    // Not Null, but empty
                LogMessageToFile($"PriceTaxLocales is:      {priceTaxLocales.Length.ToString()}");    // Not Null, but empty
                LogMessageToFile($"PriceAmount is:          {priceAmount.Length.ToString()}");        // Not Null, but empty
                LogMessageToFile($"TaxLocaleId is:          {taxLocaleId.Length.ToString()}");        // Shows the number of tax locales found for this order
                LogMessageToFile($"TaxLocaleId[0] is:       {taxLocaleId[0].ToString()}");            // Sends a number value which corresponds to the tax rate row in the tax rates table excel file  
            }

            #endregion


            #region |--This section is where we get and set the values from the shipping page where the user has entered their address info--|

            // Check if debug mode is turned on; If it is then we log these messages
            if ((string)ModuleData[_SADEBUGGINGMODE] == "true")
            {
                // Shows the section where we get and display what has been added to the shipping page fields
                LogMessage($"*                        *");
                LogMessage($"*Shipping Fields Section *");
                LogMessage($"*                        *");

                // Shows the section where we get and display what has been added to the shipping page fields in the .txt file
                LogMessageToFile($"*                        *");
                LogMessageToFile($"*Shipping Fields Section *");
                LogMessageToFile($"*                        *");
            }

            // This section saves the user's shipping info to variables to use with calculating the tax rate to return 
            // Listed in the same order as in the address book on the site
            var SFirstName = Storefront.GetValue("OrderField", "ShippingFirstName", OrderID);         // This gets the first name that the user has on the shipping page
            var SLastName = Storefront.GetValue("OrderField", "ShippingLastName", OrderID);           // This gets the last name that the user has on the shipping page
            var SAddress1 = Storefront.GetValue("OrderField", "ShippingAddress1", OrderID);           // This gets the address field 1 that the user has on the shipping page
            var SAddress2 = Storefront.GetValue("OrderField", "ShippingAddress2", OrderID);           // This gets the address field 2 that the user has on the shipping page 
            var SCity = Storefront.GetValue("OrderField", "ShippingCity", OrderID);                   // This gets the city that the user has on the shipping page
            var SState = Storefront.GetValue("OrderField", "ShippingState", OrderID);                 // This gets the state that the user has on the shipping page
            var SPostalCode = Storefront.GetValue("OrderField", "ShippingPostalCode", OrderID);       // This gets the zip code that the user has on the shipping page
            var SCountry = Storefront.GetValue("OrderField", "ShippingCountry", OrderID);             // This gets the country that the user has on the shipping page                                                                                       // Get the handling charge to use with taxes
            var hCharge = Storefront.GetValue("OrderField", "HandlingCharge", OrderID);               // This gets the handling charge for the order
            var sCharge = Storefront.GetValue("OrderField", "ShippingCharge", OrderID);               // This gets the shipping charge for the order
            // Check if debug mode is turned on; If it is then we log these messages
            if ((string)ModuleData[_SADEBUGGINGMODE] == "true")
            {
                // Log to show that we have retrieved the zipcode form the shipping page
                LogMessage($"Shipping FirstName:      {SFirstName}");                                 // This gets the first name that the user has on the shipping page
                LogMessage($"Shipping LastName:       {SLastName}");                                  // This gets the last name that the user has on the shipping page
                LogMessage($"Shipping Address1:       {SAddress1}");                                  // This gets the address field 1 that the user has on the shipping page
                LogMessage($"Shipping Address2:       {SAddress2}");                                  // This gets the address field 2 that the user has on the shipping page 
                LogMessage($"Shipping City:           {SCity}");                                      // This gets the city that the user has on the shipping page
                LogMessage($"Shipping State:          {SState}");                                     // This gets the state that the user has on the shipping page
                LogMessage($"Shipping PostalCode:     {SPostalCode}");                                // This gets the zip code that the user has on the shipping page
                LogMessage($"Shipping Country:        {SCountry}");                                   // This gets the country that the user has on the shipping page
                LogMessage($"Handling Charge:         {hCharge}");                                    // This gets the zip code that the user has on the shipping page
                LogMessage($"Shipping Charge:         {sCharge}");                                    // This gets the country that the user has on the shipping page

                // Log to show that we have retrieved the zipcode form the shipping page in the .txt file
                LogMessageToFile($"Shipping FirstName:      {SFirstName}");                           // This gets the first name that the user has on the shipping page
                LogMessageToFile($"Shipping LastName:       {SLastName}");                            // This gets the last name that the user has on the shipping page
                LogMessageToFile($"Shipping Address1:       {SAddress1}");                            // This gets the address field 1 that the user has on the shipping page
                LogMessageToFile($"Shipping Address2:       {SAddress2}");                            // This gets the address field 2 that the user has on the shipping page 
                LogMessageToFile($"Shipping City:           {SCity}");                                // This gets the city that the user has on the shipping page
                LogMessageToFile($"Shipping State:          {SState}");                               // This gets the state that the user has on the shipping page
                LogMessageToFile($"Shipping PostalCode:     {SPostalCode}");                          // This gets the zip code that the user has on the shipping page
                LogMessageToFile($"Shipping Country:        {SCountry}");                             // This gets the country that the user has on the shipping page
                LogMessageToFile($"Handling Charge:         {hCharge}");                              // This gets the zip code that the user has on the shipping page
                LogMessageToFile($"Shipping Charge:         {sCharge}");                              // This gets the country that the user has on the shipping page
            }

            #endregion


            #region |--This is the section that connects and pulls info from avalara--|

            // Check if debug mode is turned on; If it is then we log these messages
            if ((string)ModuleData[_SADEBUGGINGMODE] == "true")
            {
                // Shows the section where we change the tax rate
                LogMessage($"*                        *");
                LogMessage($"*  Tax Rates Section     *");
                LogMessage($"*                        *");

                // Set the tax amount based on a few zipcodes and send it back to pageflex
                LogMessage($"Tax amount is:           " + taxAmount[0].ToString() + " before we retrieve from Avalara");          // Shows the current tax amount (currently set to 0)

                // Shows the section where we change the tax rate in the .txt file
                LogMessageToFile($"*                        *");
                LogMessageToFile($"*  Tax Rates Section     *");
                LogMessageToFile($"*                        *");

                // Set the tax amount based on a few zipcodes and send it back to pageflex in the .txt file
                LogMessageToFile($"Tax amount is:           " + taxAmount[0].ToString() + " before we retrieve from Avalara");    // Shows the current tax amount (currently set to 0)
            }



            #region |--Database call to retrieve the subtotal should go here--|



            // TO DO: Database call to retrieve the subtotal should go here if can't get storefront access to work



            #endregion



            // Create a client and set up authentication
            var client = new AvaTaxClient("MyApp", "1.0", Environment.MachineName, AvaTaxEnvironment.Production)
                 .WithSecurity("AvalaraUsername", "AvalaraPassword");

            // Check if debug mode is turned on; If it is then we log these messages
            if ((string)ModuleData[_SADEBUGGINGMODE] == "true")
            {
                // Show user creation passed 
                LogMessage("Client created");

                // Show user creation passed in the .txt file
                LogMessageToFile("Client created");
            }

            // This creates the transaction that reaches out to alavara and gets the amount of tax for the user based on info we send
            // send client we created above in code, the company on alavara I created, typoe of transaction, and the company code I set up for company
            var transaction = new TransactionBuilder(client, "AvalaraCompanyCode", DocumentType.SalesOrder, "AvalaraCustomerCode")

                        // Pass the variables we pulled from pageflex in the address line 
                        .WithAddress(TransactionAddressType.SingleLocation, SAddress1, SAddress2, null, SCity, SState, SPostalCode, SCountry)
                        // Pass the amount of money to calculate tax on (This should be a variable once figure out what it is)
                        .WithLine(150.0m)



            #region |--Here is where we will send the total retrieved from db call--|


                        // Here is where we will send the total retrieved from db call
                        //.WithLine(REPLACE WITH VARIABLE CONTAING TOTAL FROM DB CALL)

            #endregion



                        // Run transaction
                        .Create();

            // Check if debug mode is turned on; If it is then we log these messages
            if ((string)ModuleData[_SADEBUGGINGMODE] == "true")
            {
                // Log that we have created a transaction
                LogMessage("The transaction has been created");

                // Log that we have created a transaction in the .txt file
                LogMessageToFile("The transaction has been created");
            }

            // Retrieves the tax amount from Avalara and sets it to a variable
            // (It is returned as a decimal?  so we havve to convert it to a decimal)
            // (The ?? 0 sets it to 0 if transaction.totalTax is null)
            decimal tax2 = transaction.totalTax ?? 0;

            // Check if debug mode is turned on; If it is then we log these messages
            if ((string)ModuleData[_SADEBUGGINGMODE] == "true")
            {
                //Log what is returned
                LogMessage($"Your calculated tax was: {tax2}");

                //Log what is returned in the .txt file
                LogMessageToFile($"Your calculated tax was: {tax2}");
            }

            //Set the tax amount on pageflex to the returned value from Avalara
            taxAmount[0] = decimal.ToDouble(tax2);                                              // Set our new tax rate to a value we choose (for testing) 

            // Check if debug mode is turned on; If it is then we log these messages
            if ((string)ModuleData[_SADEBUGGINGMODE] == "true")
            { 
                LogMessage($"The zipcode was:         " + SPostalCode);                         // Log message to inform we used this zipcode to get the amount returned
                LogMessage($"The new TaxAmount is:    " + taxAmount[0].ToString());             // Shows the tax amount after we changed it

                // Send message saying we have completed this process
                LogMessage($"*                        *");
                LogMessage($"*           end          *");
                LogMessage($"*                        *");

                // Log for .txt file
                LogMessageToFile($"The zipcode was:         " + SPostalCode);                   // Log message to inform we used this zipcode to get the amount returned
                LogMessageToFile($"The new TaxAmount is:    " + taxAmount[0].ToString());       // Shows the tax amount after we changed it

                // Send message saying we have completed this process in the .txt file
                LogMessageToFile($"*                        *");
                LogMessageToFile($"*           end          *");
                LogMessageToFile($"*                        *");
            }

            // Kept for reference and future use
                 // avalara logging doesn't work from inside extension
                 // client.LogToFile("MySixthExtension\\avataxapi.log"); 

            #endregion


            return eSuccess;
        }
        #endregion
        

        #region |--Used when testing to see if the depreciated version would work (It does not work)--|

        /*
        public override int CalculateTax(string orderID, double taxableAmount, double prevTaxableAmount, ref TaxValue[] tax)
        {
            // Used to help to see where these mesages are in the Storefront logs page
            LogMessage($"*      space       *");                // Adds a space for easier viewing
            LogMessage($"*      START       *");                // Show when we start this process
            LogMessage($"*      space       *");

            // Shows what values are passed at beginning
            LogMessage($"OrderID is: {orderID}");                                      // Tells the id for the order being calculated
            LogMessage($"TaxableAmount is: {taxableAmount.ToString()}");               // Tells the amount to be taxed (currently set to 0)
            LogMessage($"prevTaxableAmount is: {prevTaxableAmount.ToString()}");       // Tells the previous amount to be taxed (currently set to 0)

            return eSuccess;
        }
        */

        #endregion


        //end of the class: ExtensionSeven
    }
    //end of the file
}