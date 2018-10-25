# Seventh-Pageflex-Extension
This extension checks if we are on the shipping page, prints to the logs what we have passed to calculate tax rates, retrieves all of the user's shipping information, connects to avalara and sends the data of user to retrieve tax amount (currently the price or subtotal is harcoded as can't access the actual storefronts yet). and then displays the amount back to the storefront. This one is a little bit more polished than six and has added comments to help.  

*(This version has been updated to add logging features and has added commenting for use in debugging )*

Methods:
1. DisplayName()
2. UniqueName()
3. PARAMS_WE_WANT
4. private ISINI GetSf () (reduces code throughout project)
5. LogMessageToFile
6. GetConfigurationHtml
7 . IsModuleType (string x) (determines if at shipping step)
8. CalculateTax2 () 
