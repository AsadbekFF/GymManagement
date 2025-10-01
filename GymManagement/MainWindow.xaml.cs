using GymManagement.Entities;
using GymManagement.Entities.Context;
using GymManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GymManagement
{
    // =========================================================================
    // MAIN WINDOW LOGIC
    // =========================================================================
    public partial class MainWindow : Window
    {
        // Guard flag to prevent the LanguageComboBox_SelectionChanged event from running during initialization.
        private bool _isLoaded = false;

        public MainWindow()
        {
            // --- DATABASE INITIALIZATION BLOCK ---
            string dbFilePath = "Unknown";

            try
            {
                using (var context = new GymDbContext())
                {
                    dbFilePath = context.Database.GetDbConnection().DataSource;

                    if (File.Exists(dbFilePath))
                    {
                        try
                        {
                            context.Database.ExecuteSqlRaw("SELECT 1 FROM __EFMigrationsHistory LIMIT 1");
                        }
                        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1)
                        {
                            context.Database.EnsureDeleted();
                        }
                    }

                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"FATAL DATABASE ERROR: Could not initialize the database schema.\n\n" +
                                $"The application looked for the file here:\n{dbFilePath}\n\n" +
                                $"Details: {ex.Message}",
                                "Database Startup Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            // --------------------------------------------

            InitializeComponent();
            this.DataContext = new MainViewModel();

            // Attach the event handler to the window's Loaded event.
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = this.FindName("LanguageComboBox") as ComboBox;
            if (comboBox != null)
            {
                // FIX: Set the selected item BEFORE attaching the event handler.
                var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;
                var currentItem = comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(i => i.Tag?.ToString() == currentCulture);
                if (currentItem != null)
                {
                    comboBox.SelectedItem = currentItem;
                }

                // Now attach the event handler to listen for future manual changes.
                comboBox.SelectionChanged += LanguageComboBox_SelectionChanged;

                // Set the guard flag to true once the window is fully initialized.
                _isLoaded = true;
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The guard flag prevents the event from firing during the initial setup in MainWindow_Loaded.
            if (!_isLoaded)
            {
                return;
            }

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string cultureCode = selectedItem.Tag?.ToString();
                if (!string.IsNullOrEmpty(cultureCode) && Application.Current.MainWindow != null)
                {
                    App.ChangeLanguage(cultureCode);
                }
            }
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.Invoke(() =>
                {
                    if (this.DataContext is MainViewModel vm)
                    {
                        if (e.Row.DataContext is Customer customer)
                        {
                            vm.UpdateCustomerCommand.Execute(customer);
                        }
                        else if (e.Row.DataContext is Product product)
                        {
                            vm.UpdateProductCommand.Execute(product);
                        }
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}