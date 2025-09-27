using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace GymManagement
{
    public partial class App : Application
    {
        public static void ChangeLanguage(string cultureCode)
        {
            // 1. Change the culture of the current thread
            CultureInfo culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // 2. Safely create a new instance of the main window
            var newWindow = new MainWindow();

            // 3. Set the new window as the main window
            Application.Current.MainWindow = newWindow;

            // 4. Close the old main window to avoid recursion and memory leaks.
            // We use a safe check to ensure we don't close the new window.
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow oldWindow && oldWindow != newWindow)
                {
                    oldWindow.Close();
                    break; // Only one old window should exist
                }
            }

            // 5. Show the new main window
            newWindow.Show();
        }
    }
}
