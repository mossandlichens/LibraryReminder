namespace MunichCityLibraryReminder
{
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using MunichCityLibraryReminder.Properties;
    using SimpleBrowser;
    using System.Text;
    using System.Security.Cryptography;
            
    public static class Helper
    {
        internal static string StartupParentKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        internal static string applicationEntropy = "473B7E65-658C-453C-A61C-58E6B9FDDD30";

        internal static bool Validate(string id, string password)
        {
            var browser = new Browser();

            browser.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10";

            browser.Navigate(Settings.Default.LandingPageUrl);
            if (LastRequestFailed(browser)) return false; // always check the last request in case the page failed to load

            browser.Find("input", FindBy.Id, "L#AUSW_1").Value = id;
            browser.Find("input", FindBy.Id, "LPASSW_1").Value = password;
            browser.Find("input", FindBy.Name, "textButton").Click();
            if (LastRequestFailed(browser)) return false;

            var logoutText = browser.Find(ElementType.Anchor, FindBy.Text, Settings.Default.LogoutText);

            if (logoutText.Exists)
                return true;
            else
                return false;                        
        }

        static bool LastRequestFailed(Browser browser)
        {
            if (browser.LastWebException != null)
            {
                return true;
            }
            return false;
        }

        internal static List<LibraryItem> GetUserBorrowals(string id, string password)
        {
            List<LibraryItem> borrowedItems = new List<LibraryItem>();

            var browser = new Browser();
            browser.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10";

            browser.Navigate(Settings.Default.LandingPageUrl);
            if (LastRequestFailed(browser))
            {
                return borrowedItems;
            }

            // fill in the form and click the login button - the fields are easy to locate because they have ID attributes
            browser.Find("input", FindBy.Id, "L#AUSW_1").Value = id;
            browser.Find("input", FindBy.Id, "LPASSW_1").Value = password;
            browser.Find("input", FindBy.Name, "textButton").Click();
            if (LastRequestFailed(browser))
            {
                return borrowedItems;
            }

            var borrowal = browser.Find(ElementType.Anchor, FindBy.Text, Settings.Default.BorrowalText);
            var borrowals = browser.Find(ElementType.Anchor, FindBy.Text, Settings.Default.BorrowalsText);
            
            if (borrowal.Exists || borrowals.Exists)
            {
                if (borrowals.Exists)
                {
                    borrowals.Click();
                }
                else
                {
                    borrowal.Click();
                }

                if (LastRequestFailed(browser))
                {
                    return borrowedItems;
                }

                var evenItems = browser.Find("tr", FindBy.Class, "rTable_tr_even");
                var oddItems = browser.Find("tr", FindBy.Class, "rTable_tr_odd");

                borrowedItems = new List<LibraryItem>();

                AddItemsToBorrowedItems(borrowedItems, evenItems);
                AddItemsToBorrowedItems(borrowedItems, oddItems);
            }

            return borrowedItems;

        }

        private static void AddItemsToBorrowedItems(List<LibraryItem> borrowedItems, HtmlResult items)
        {
            foreach (var item in items)
            {
                borrowedItems.Add(new LibraryItem(item));
            }
        }

        public static string AssemblyProduct 
        { 
            get
            {
                // Get all Product attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                // If there aren't any Product attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;

            }
        }

        /// <summary>
        ///     Check if Network is available
        /// </summary>
        /// <returns>true if Network is available</returns>
        public static bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>
        ///     Check if connected to Internet
        /// </summary>
        /// <returns>true if connected</returns>
        public static bool IsConnectedToInternet()
        {
            int desc;
            return NativeMethods.InternetGetConnectedState(out desc, 0);
        }

        internal static string Encrypt(string plaintext)
        {
            byte[] encodedPlaintext = Encoding.UTF8.GetBytes(plaintext);
            byte[] encodedEntropy = Encoding.UTF8.GetBytes(applicationEntropy);
            byte[] ciphertext = ProtectedData.Protect(encodedPlaintext, encodedEntropy, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(ciphertext);
        }

        internal static string Decrypt(string base64Ciphertext)
        {
            string result = string.Empty;
            if (base64Ciphertext != string.Empty)
            {
                byte[] ciphertext = Convert.FromBase64String(base64Ciphertext);
                byte[] encodedEntropy = Encoding.UTF8.GetBytes(applicationEntropy);
                byte[] encodedPlaintext = ProtectedData.Unprotect(ciphertext, encodedEntropy, DataProtectionScope.LocalMachine);
                result = Encoding.UTF8.GetString(encodedPlaintext);
            }
            return result;
        }
    }
}
