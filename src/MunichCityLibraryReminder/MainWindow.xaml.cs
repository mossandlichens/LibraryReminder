namespace MunichCityLibraryReminder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Timers;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;
    using NetSparkle;
    using Microsoft.Win32;
    using MunichCityLibraryReminder.Properties;
    using WPFLocalizeExtension.Engine;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool shouldClose;
        private Sparkle _sparkle; 

        // 1 hour
        Timer timer = new Timer(3600000);

        private void _sparkle_checkLoopFinished(object sender, bool UpdateRequired)
        {
            // TODO
        }

        private void _sparkle_checkLoopStarted(object sender)
        {
            // TODO
        }

        public MainWindow()
        {
            InitializeComponent();

            if (Settings.Default.CallUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.CallUpgrade = false;
                Settings.Default.Save();
            }

            _sparkle = new Sparkle(Settings.Default.Appcast, null);

#if DEBUG
            // _sparkle.ShowDiagnosticWindow = true;
#endif

            _sparkle.CheckLoopFinished += new LoopFinishedOperation(_sparkle_checkLoopFinished);
            _sparkle.CheckLoopStarted += new LoopStartedOperation(_sparkle_checkLoopStarted);

            var sparkleConfig = _sparkle.GetApplicationConfig();

            // TODO Change config incase skipped

            _sparkle.StartLoop(true, true);               

            // Register for row highlighting based on due date
            this.dataGridItems.LoadingRow += new EventHandler<System.Windows.Controls.DataGridRowEventArgs>(dataGridItems_LoadingRow);
            
            // Load credentials if saved earlier
            if (Settings.Default.ID != string.Empty && Settings.Default.Password != string.Empty)
            {
                this.textBlockWarning.Visibility = Visibility.Hidden;
                this.buttonCheck.IsEnabled = true;
                this.textBoxID.Text = Settings.Default.ID;
                this.passwordBox.Password = Helper.Decrypt(Settings.Default.Password);                
            }
            else
            {
                this.textBlockWarning.Visibility = Visibility.Visible;
            }

            AddAppToStartup();
            
            if (Settings.Default.Language != string.Empty)
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(Settings.Default.Language);                
            }
            else
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(Settings.Default.DefaultLanguage);
                Settings.Default.Language = Settings.Default.DefaultLanguage;
                Settings.Default.Save();
            }

            switch (Settings.Default.Language)
            {
                case "de":
                    this.radioButtonGerman.IsChecked = true;
                    break;
                case "en":
                    this.radioButtonEnglish.IsChecked = true;
                    break;
            }

            this.checkBoxAutoextend.IsChecked = Settings.Default.Autoextend;

            this.CheckAndUpdateBorrowals();

            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(dailyTimer_Elapsed);
            timer.Start();     
        }

        #region Daily Task

        private void dailyTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            // Logger.Write("Daily Timer to check borrowed items" + DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString());
            Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(CheckAndUpdateBorrowals));

        }
        #endregion

        #region Startup
        private static void AddAppToStartup()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(Helper.StartupParentKey, true);
            bool registryEntryExists = (registryKey.GetValue(Helper.AssemblyProduct) != null);
            if (registryEntryExists == false)
            {
                string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                registryKey.SetValue(Helper.AssemblyProduct, executablePath);
            }
        }

        private static void RemoveAppFromStartup()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(Helper.StartupParentKey, true);
            registryKey.DeleteValue(Helper.AssemblyProduct, false);
        }
        #endregion

        #region Grid highlighting
        void dataGridItems_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            LibraryItem libraryItem = e.Row.DataContext as LibraryItem;
            if (libraryItem != null)
            {
                DateTime dateTime = DateTime.Parse(libraryItem.DueDate);
                int days = dateTime.Subtract(DateTime.Now).Days;
                if (days < 0)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                }
                else if (days <= Settings.Default.WarningDayCount)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    e.Row.Background = new SolidColorBrush(Colors.LightGreen);
                }
            }
            else
            {
                e.Row.Height = 0;
            }
        }

        private void dataGridItems_AutoGeneratingColumn(object sender, System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            PropertyDescriptor pd = e.PropertyDescriptor as PropertyDescriptor;
            e.Column.Header = pd.DisplayName;
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!shouldClose)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            // Check if user id and password are valid
            bool validID = Helper.Validate(textBoxID.Text, passwordBox.Password);

            // Save user id and password in user settings
            if (validID)
            {
                Settings.Default.ID = textBoxID.Text;
                Settings.Default.Password = Helper.Encrypt(passwordBox.Password);
                Settings.Default.Save();
                this.buttonCheck.IsEnabled = true;
                this.textBlockWarning.Visibility = Visibility.Hidden;
            }
            else
            {
                this.passwordBox.Password = string.Empty;
                this.textBlockWarning.Visibility = Visibility.Visible;
            }            
        }

        private void buttonCheck_Click(object sender, RoutedEventArgs e)
        {
            CheckAndUpdateBorrowals();
        }

        private void CheckAndUpdateBorrowals()
        {
            if (Helper.IsNetworkAvailable() == true
                    || Helper.IsConnectedToInternet() == true)
            {

                List<LibraryItem> borrowals = Helper.GetUserBorrowals(Settings.Default.ID, Helper.Decrypt(Settings.Default.Password));

                borrowals.Sort((a, b) => b.DueDate.CompareTo(a.DueDate));

                this.dataGridItems.ItemsSource = borrowals;

                Settings.Default.LastCheckedTimestamp = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                Settings.Default.Save();

                this.labelLastCheckedDate.Content = Settings.Default.LastCheckedTimestamp;

                SetApplicationIcon(borrowals);

                CheckForAutoextend(borrowals);
            }
        }

        private void SetApplicationIcon(List<LibraryItem> borrowals)
        {
            // Issue 32307 
            this.NotifyIcon.Icon = Properties.Resources.NotifyIconGreen;

            int redCount = (borrowals.FindAll(x => x.DueDate == DateTime.Now.ToShortDateString())).Count;
            if (redCount > 0)
            {
                this.NotifyIcon.Icon = Properties.Resources.NotifyIconRed;
            }
            else
            {
                int yellowCount = (borrowals.FindAll(x => DateTime.Parse(x.DueDate).Subtract(DateTime.Now).Days <= Settings.Default.WarningDayCount)).Count;
                if (yellowCount > 0)
                {
                    this.NotifyIcon.Icon = Properties.Resources.NotifyIconYellow;
                }
            }
        }

        private void CheckForAutoextend(List<LibraryItem> borrowals)
        {
            string today = DateTime.Now.ToShortDateString();
            bool autoextend = false;
            foreach (LibraryItem borrowal in borrowals)
            {
                if (borrowal.DueDate == today)
                {
                    autoextend = true;
                    break;
                }
            }
            if (autoextend == true)
            {
                // TODO
                // <input type="checkbox" name="cellCheck" fld="###CHK_1"/>
                // <input type="checkbox" name="cellCheck" fld="###CHK_1" disabled="disabled"/>
                // <input type="checkbox" name="cellCheck$0" fld="###CHK_2"/>
                // <input type="submit" name="textButton$4" value="Markierte Titel verl&#228;ngern" fld="$$GFBO_14" class="buttong_l"/>

                // <td class="zellgb">
                // Heute verlängert<br/>1 Verlängerung
                // </td>

                // <td class="zellgb">
                // Keine Verlängerung möglich. Verlängerung noch nicht möglich- Stand 21.02.2012
                // </td>
            }
        }
        
        #region Localization
        private void radioButtonGerman_Click(object sender, RoutedEventArgs e)
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("de");
        }

        private void radioButtonEnglish_Click(object sender, RoutedEventArgs e)
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en");
        }
        #endregion           
        
        private void checkBoxAutoextend_Click(object sender, RoutedEventArgs e)
        {
            if (this.checkBoxAutoextend.IsChecked == true)
            {
                Settings.Default.Autoextend = true;
            }
            else
            {
                Settings.Default.Autoextend = false;
            }
            Settings.Default.Save();
        }
                
        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedIndex = 0;
            this.CheckAndUpdateBorrowals();
            this.Show();
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedIndex = 1;
            this.CheckAndUpdateBorrowals();
            this.Show();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            shouldClose = true;
            Close();
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Settings.Default.ApplicationWebsite);
        }
        
    }
}