using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.Shell;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace MunichCityLibraryReminder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        [STAThread]
        public static void Main()
        {
            string Unique = ((GuidAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value; 

            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // ...            
            return true;
        }

        public bool SignalExternalApplicationCurrent(Application app)
        {
            app.MainWindow.Show();
            return true;
        }
        #endregion

        /// <summary>
        ///     Set Foreground Window
        /// </summary>
        /// <param name="handle">handle to window</param>
        /// <returns>true or false</returns>
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        /// <summary>
        ///     Show Window Async
        /// </summary>
        /// <param name="handle">handle to window</param>
        /// <param name="showType">type of show</param>
        /// <returns>true or false</returns>
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr handle, int showType);

        /// <summary>
        ///     Is Iconic Window
        /// </summary>
        /// <param name="handle">handle to window</param>
        /// <returns>true or false</returns>
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr handle);

    }
}
