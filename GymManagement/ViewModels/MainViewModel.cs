using GymManagement.Entities.Base;
using GymManagement.Entities;
using GymManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GymManagement.Models;
using LiveCharts;
using LiveCharts.Wpf;
using GymManagement.Models.Enum;

namespace GymManagement.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly GymDataService _dbService;

        // --- Data Collections ---
        public ObservableCollection<Customer> Customers { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<CheckIn> CheckIns { get; set; }
        public ObservableCollection<Purchase> Purchases { get; set; }

        public Array SubscriptionTypes => Enum.GetValues(typeof(SubscriptionType));

        // Filtered collections (what the UI actually displays)
        private ObservableCollection<Customer> _filteredCustomers;
        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set { _filteredCustomers = value; OnPropertyChanged(nameof(FilteredCustomers)); }
        }

        private ObservableCollection<Customer> _filteredCheckinCustomers;
        public ObservableCollection<Customer> FilteredCheckinCustomers
        {
            get => _filteredCheckinCustomers;
            set { _filteredCheckinCustomers = value; OnPropertyChanged(nameof(FilteredCheckinCustomers)); }
        }

        private ObservableCollection<Customer> _filteredPurchaseCustomers;
        public ObservableCollection<Customer> FilteredPurchaseCustomers
        {
            get => _filteredPurchaseCustomers;
            set { _filteredPurchaseCustomers = value; OnPropertyChanged(nameof(FilteredPurchaseCustomers)); }
        }

        private ObservableCollection<Product> _filteredProducts;
        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set { _filteredProducts = value; OnPropertyChanged(nameof(FilteredProducts)); }
        }

        private ObservableCollection<Product> _filteredPurchaseProducts;
        public ObservableCollection<Product> FilteredPurchaseProducts
        {
            get => _filteredPurchaseProducts;
            set { _filteredPurchaseProducts = value; OnPropertyChanged(nameof(FilteredPurchaseProducts)); }
        }

        private ObservableCollection<Purchase> _filteredPurchases;
        public ObservableCollection<Purchase> FilteredPurchases
        {
            get => _filteredPurchases;
            set { _filteredPurchases = value; OnPropertyChanged(nameof(FilteredPurchases)); }
        }

        private ObservableCollection<Subscription> _subscriptions;
        public ObservableCollection<Subscription> Subscriptions
        {
            get => _subscriptions;
            set { _subscriptions = value; OnPropertyChanged(nameof(Subscriptions)); }
        }

        private ObservableCollection<Subscription> _filteredSubscriptions;
        public ObservableCollection<Subscription> FilteredSubscriptions
        {
            get => _filteredSubscriptions;
            set { _filteredSubscriptions = value; OnPropertyChanged(nameof(FilteredSubscriptions)); }
        }

        // --- New/Selected Entities (Input Forms) ---
        public Customer NewCustomer { get; set; } = new Customer();
        public Product NewProduct { get; set; } = new Product();

        private Customer _selectedCustomerForCheckIn;
        public Customer SelectedCustomerForCheckIn
        {
            get => _selectedCustomerForCheckIn;
            set { _selectedCustomerForCheckIn = value; OnPropertyChanged(nameof(SelectedCustomerForCheckIn)); }
        }

        private Customer _selectedCustomerForPurchase;
        public Customer SelectedCustomerForPurchase
        {
            get => _selectedCustomerForPurchase;
            set { _selectedCustomerForPurchase = value; OnPropertyChanged(nameof(SelectedCustomerForPurchase)); }
        }

        private Product _selectedProductForPurchase;
        public Product SelectedProductForPurchase
        {
            get => _selectedProductForPurchase;
            set { _selectedProductForPurchase = value; OnPropertyChanged(nameof(SelectedProductForPurchase)); }
        }

        private int _purchaseQuantity = 1;
        public int PurchaseQuantity
        {
            get => _purchaseQuantity;
            set { _purchaseQuantity = value; OnPropertyChanged(nameof(PurchaseQuantity)); }
        }

        private string _purchaseMessage;
        public string PurchaseMessage
        {
            get => _purchaseMessage;
            set { _purchaseMessage = value; OnPropertyChanged(nameof(PurchaseMessage)); }
        }

        public Subscription NewSubscription { get; set; } = new Subscription { StartDate = DateTime.Today, EndDate = DateTime.Today };

        private string _subscriptionCustomerFilterText { get; set; }
        public string SubscriptionCustomerFilterText 
        { 
            get => _subscriptionCustomerFilterText; 
            set
            {
                _subscriptionCustomerFilterText = value;
                OnPropertyChanged(nameof(SubscriptionCustomerFilterText));
                FilterSubscriptions();
            } 
        }

        private DateTime? _subscriptionStartDateFilter;
        public DateTime? SubscriptionStartDateFilter 
        { 
            get => _subscriptionStartDateFilter; 
            set
            {
                _subscriptionStartDateFilter = value;
                OnPropertyChanged(nameof(SubscriptionStartDateFilter));
                FilterSubscriptions();
            }
        }

        private DateTime? _subscriptionEndDateFilter;
        public DateTime? SubscriptionEndDateFilter 
        {
            get => _subscriptionEndDateFilter;
            set
            {
                _subscriptionEndDateFilter = value;
                OnPropertyChanged(nameof(SubscriptionEndDateFilter));
                FilterSubscriptions();
            }
        }

        // --- Filtering Properties ---
        private string _customerFilterText;
        public string CustomerFilterText
        {
            get => _customerFilterText;
            set
            {
                _customerFilterText = value;
                OnPropertyChanged(nameof(CustomerFilterText));
                FilterCustomers();
            }
        }

        private string _customerCheckinFilterText;
        public string CustomerCheckinFilterText
        {
            get => _customerCheckinFilterText;
            set
            {
                _customerCheckinFilterText = value;
                OnPropertyChanged(nameof(CustomerCheckinFilterText));
                FilterCheckinCustomers();
            }
        }

        private string _customerPurchaseFilterText;
        public string CustomerPurchaseFilterText
        {
            get => _customerPurchaseFilterText;
            set
            {
                _customerPurchaseFilterText = value;
                OnPropertyChanged(nameof(CustomerPurchaseFilterText));
                FilterPurchaseCustomers();
            }
        }

        private string _productPurchaseFilterText;
        public string ProductPurchaseFilterText
        {
            get => _productPurchaseFilterText;
            set
            {
                _productPurchaseFilterText = value;
                OnPropertyChanged(nameof(ProductPurchaseFilterText));
                FilterPurchaseProducts();
            }
        }

        // --- Chart Data Properties (FIXED: LiveCharts specific) ---
        private SeriesCollection _attendanceSeries;
        public SeriesCollection AttendanceSeries
        {
            get => _attendanceSeries;
            set { _attendanceSeries = value; OnPropertyChanged(nameof(AttendanceSeries)); }
        }

        private string[] _attendanceLabels;
        public string[] AttendanceLabels
        {
            get => _attendanceLabels;
            set { _attendanceLabels = value; OnPropertyChanged(nameof(AttendanceLabels)); }
        }

        private SeriesCollection _salesSeries;
        public SeriesCollection SalesSeries
        {
            get => _salesSeries;
            set { _salesSeries = value; OnPropertyChanged(nameof(SalesSeries)); }
        }

        // --- Commands ---
        public ICommand AddCustomerCommand { get; }
        public ICommand UpdateCustomerCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand UpdateProductCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand ProcessPurchaseCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand ClearCustomersFilterCommand { get;  }
        public ICommand AddSubscriptionCommand { get; }
        public ICommand ClearSubscriptionsFilterCommand { get; }


        // =========================================================================
        // CONSTRUCTOR & INITIALIZATION
        // =========================================================================

        public MainViewModel()
        {
            _dbService = new GymDataService();
            _dbService.InitializeDatabase();

            // Initialize Collections
            Customers = new ObservableCollection<Customer>();
            Products = new ObservableCollection<Product>();
            CheckIns = new ObservableCollection<CheckIn>();
            Purchases = new ObservableCollection<Purchase>();
            Subscriptions = new ObservableCollection<Subscription>();

            // Initialize Filtered Collections (to avoid null reference in UI)
            FilteredCustomers = new ObservableCollection<Customer>();
            FilteredProducts = new ObservableCollection<Product>();
            FilteredPurchases = new ObservableCollection<Purchase>();

            // Initialize Commands
            AddCustomerCommand = new RelayCommand(ExecuteAddCustomer);
            UpdateCustomerCommand = new RelayCommand(ExecuteUpdateCustomer);
            AddProductCommand = new RelayCommand(ExecuteAddProduct);
            UpdateProductCommand = new RelayCommand(ExecuteUpdateProduct);
            CheckInCommand = new RelayCommand(ExecuteCheckIn, CanExecuteCheckIn);
            ProcessPurchaseCommand = new RelayCommand(ExecuteProcessPurchase, CanExecuteProcessPurchase);
            LoadDataCommand = new RelayCommand(ExecuteLoadData);
            AddSubscriptionCommand = new RelayCommand(ExecuteProcessSubscription, CanExecuteProcessSubscription);
            ClearCustomersFilterCommand = new RelayCommand(ClearCustomersFilterText);
            ClearSubscriptionsFilterCommand = new RelayCommand(ClearSubscriptionsFilter);

            // Initial Data Load (must be deferred to the View's Loaded event in XAML 
            // if we were running a migration/init command here, but since the migration 
            // is run externally, we can load data now.)
            ExecuteLoadData(null);
        }

        // =========================================================================
        // DATA ACCESS METHODS
        // =========================================================================

        private void ExecuteLoadData(object obj)
        {
            try
            {
                // Ensure the database is initialized with dummy data if needed
                _dbService.InitializeDatabase();

                // Load all collections
                Customers.Clear();
                _dbService.GetCustomers().ForEach(c => Customers.Add(c));

                Products.Clear();
                _dbService.GetProducts().ForEach(p => Products.Add(p));

                CheckIns.Clear();
                _dbService.GetCheckIns().ForEach(ci => CheckIns.Add(ci));

                Purchases.Clear();
                _dbService.GetPurchases().ForEach(p => Purchases.Add(p));

                // Apply initial filters
                FilterCustomers();
                FilterProducts();
                FilterPurchases();
                FilterPurchaseCustomers();
                FilterPurchaseProducts();
                FilterCheckinCustomers();

                LoadSubscriptionData();

                // Load Chart Data
                LoadChartData();

                PurchaseMessage = "Ready for purchases.";
            }
            catch (Exception ex)
            {
                PurchaseMessage = $"Error loading data: {ex.Message}";
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void ClearCustomersFilterText(object obj)
        {
            CustomerFilterText = "";
        }

        // FIXED: This method now correctly populates the LiveCharts-specific properties.
        private void LoadChartData()
        {
            // Get data points from service
            var attendancePoints = _dbService.GetCustomerAttendanceData();
            var salesPoints = _dbService.GetProductSalesData();

            // Create a SeriesCollection for the Attendance Chart
            AttendanceSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Check-ins",
                    Values = new ChartValues<int>(attendancePoints.Select(dp => dp.CheckInCount)),
                    DataLabels = true
                }
            };
            // Set the X-axis labels
            AttendanceLabels = attendancePoints.Select(dp => dp.CustomerName).ToArray();

            // Create a SeriesCollection for the Sales Pie Chart
            SalesSeries = new SeriesCollection();
            foreach (var sale in salesPoints)
            {
                SalesSeries.Add(new PieSeries
                {
                    Title = sale.ProductName,
                    Values = new ChartValues<int> { sale.TotalQuantitySold },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.SeriesView.Title}: {chartPoint.Y} ({chartPoint.Participation:P0})"
                });
            }
        }

        // =========================================================================
        // CUSTOMER MANAGEMENT
        // =========================================================================

        private void ExecuteAddCustomer(object obj)
        {
            if (string.IsNullOrWhiteSpace(NewCustomer.Name))
            {
                PurchaseMessage = "Customer name is required.";
                return;
            }

            _dbService.AddCustomer(NewCustomer);
            Customers.Add(NewCustomer);
            NewCustomer = new Customer(); // Reset input form
            OnPropertyChanged(nameof(NewCustomer));
            PurchaseMessage = $"Customer '{Customers.Last().Name}' added successfully.";
            FilterCustomers();
        }

        private void ExecuteUpdateCustomer(object customerObj)
        {
            if (customerObj is Customer customer && customer.Id > 0)
            {
                _dbService.UpdateCustomer(customer);
                PurchaseMessage = $"Customer '{customer.Name}' updated.";
            }
        }

        // =========================================================================
        // PRODUCT MANAGEMENT
        // =========================================================================

        private void ExecuteAddProduct(object obj)
        {
            if (string.IsNullOrWhiteSpace(NewProduct.Name) || NewProduct.Price <= 0)
            {
                PurchaseMessage = "Product name and positive price are required.";
                return;
            }

            _dbService.AddProduct(NewProduct);
            Products.Add(NewProduct);
            NewProduct = new Product { StockQuantity = 1 }; // Reset input form
            OnPropertyChanged(nameof(NewProduct));
            PurchaseMessage = $"Product '{Products.Last().Name}' added successfully.";
            FilterProducts();
        }

        private void ExecuteUpdateProduct(object productObj)
        {
            if (productObj is Product product && product.Id > 0)
            {
                _dbService.UpdateProduct(product);
                PurchaseMessage = $"Inventory for '{product.Name}' updated.";
                FilterProducts(); // Re-filter to update StockStatus color
            }
        }

        private string _productFilterText;
        public string ProductFilterText
        {
            get => _productFilterText;
            set
            {
                _productFilterText = value;
                OnPropertyChanged(nameof(ProductFilterText));
                FilterProducts();
            }
        }

        // Subscription management
        private bool CanExecuteProcessSubscription(object obj)
        {
            return NewSubscription?.Customer != null &&
                   NewSubscription?.Type != null &&
                   NewSubscription?.Amount > 0;
        }

        private void ExecuteProcessSubscription(object obj)
        {
            if (!CanExecuteProcessSubscription(null))
            {
                PurchaseMessage = "Select customer, type, and amount > 0.";
                return;
            }

            // Attempt purchase which updates inventory in the DB
            bool success = _dbService.ProcessSubscription(
                NewSubscription.Customer.Id,
                NewSubscription.Type,
                NewSubscription.Amount);

            if (success)
            {
                // Refresh data from DB to reflect stock change and new purchase record
                PurchaseMessage = $"Successfull.";
                ExecuteLoadData(null);
                LoadChartData(); // Update sales chart
            }
            else
            {
                PurchaseMessage = $"Something went wrong.";
            }
        }

        private void LoadSubscriptionData()
        {
            Subscriptions.Clear();
            _dbService.GetSubscriptions().ForEach(s => Subscriptions.Add(s));
            FilterSubscriptions(); // Apply initial filter (or no filter)
        }

        private void ClearSubscriptionsFilter(object obj)
        {
            // Reset the filter properties, which will trigger a new filter automatically
            SubscriptionCustomerFilterText = "";
            SubscriptionStartDateFilter = null;
            SubscriptionEndDateFilter = null;
        }

        private void FilterSubscriptions()
        {
            var filteredList = _dbService.FilterSubscriptions(
                SubscriptionCustomerFilterText,
                SubscriptionStartDateFilter,
                SubscriptionEndDateFilter
            );
            FilteredSubscriptions = new ObservableCollection<Subscription>(filteredList);
        }

        // ... inside the MainViewModel class ...

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(ProductFilterText))
            {
                FilteredProducts = new ObservableCollection<Product>(Products.OrderBy(p => p.StockQuantity));
            }
            else
            {
                var query = Products.Where(p => p.Name.Contains(ProductFilterText, StringComparison.OrdinalIgnoreCase));
                FilteredProducts = new ObservableCollection<Product>(query);
            }
        }

        private void FilterCustomers()
        {
            if (string.IsNullOrWhiteSpace(CustomerFilterText))
            {
                FilteredCustomers = new ObservableCollection<Customer>(Customers);
            }
            else
            {
                var query = Customers
                    .Where(c => c.Name.Contains(CustomerFilterText, StringComparison.OrdinalIgnoreCase) ||
                                (!string.IsNullOrWhiteSpace(c.Phone) && c.Phone.Contains(CustomerFilterText, StringComparison.OrdinalIgnoreCase)));
                FilteredCustomers = new ObservableCollection<Customer>(query);
            }
        }

        private void FilterCheckinCustomers()
        {
            if (string.IsNullOrWhiteSpace(CustomerCheckinFilterText))
            {
                FilteredCheckinCustomers = new ObservableCollection<Customer>(Customers);
            }
            else
            {
                var query = Customers
                    .Where(c => c.Name.Contains(CustomerCheckinFilterText, StringComparison.OrdinalIgnoreCase));
                FilteredCheckinCustomers = new ObservableCollection<Customer>(query);
            }
        }

        private void FilterPurchaseCustomers()
        {
            if (string.IsNullOrWhiteSpace(CustomerPurchaseFilterText))
            {
                FilteredPurchaseCustomers = new ObservableCollection<Customer>(Customers);
            }
            else
            {
                var query = Customers
                    .Where(c => c.Name.Contains(CustomerPurchaseFilterText, StringComparison.OrdinalIgnoreCase));
                FilteredPurchaseCustomers = new ObservableCollection<Customer>(query);
            }
        }

        private void FilterPurchaseProducts()
        {
            if (string.IsNullOrWhiteSpace(ProductPurchaseFilterText))
            {
                FilteredPurchaseProducts = new ObservableCollection<Product>(Products);
            }
            else
            {
                var query = Products
                    .Where(c => c.Name.Contains(ProductPurchaseFilterText, StringComparison.OrdinalIgnoreCase));
                FilteredPurchaseProducts = new ObservableCollection<Product>(query);
            }
        }

        // =========================================================================
        // ATTENDANCE & PURCHASES
        // =========================================================================

        private bool CanExecuteCheckIn(object obj) => SelectedCustomerForCheckIn != null;

        private void ExecuteCheckIn(object obj)
        {
            if (SelectedCustomerForCheckIn == null) return;

            var checkIn = new CheckIn
            {
                CustomerId = SelectedCustomerForCheckIn.Id,
                CheckInDate = DateTime.Now
            };

            _dbService.AddCheckIn(checkIn);
            CheckIns.Insert(0, checkIn); // Add to top of list
            PurchaseMessage = $"{SelectedCustomerForCheckIn.Name} checked in successfully.";
            LoadChartData(); // Update attendance chart
        }

        private bool CanExecuteProcessPurchase(object obj)
        {
            return SelectedCustomerForPurchase != null &&
                   SelectedProductForPurchase != null &&
                   PurchaseQuantity > 0;
        }

        private void ExecuteProcessPurchase(object obj)
        {
            if (!CanExecuteProcessPurchase(null))
            {
                PurchaseMessage = "Select customer, product, and quantity > 0.";
                return;
            }

            // Attempt purchase which updates inventory in the DB
            bool success = _dbService.ProcessPurchase(
                SelectedCustomerForPurchase.Id,
                SelectedProductForPurchase.Id,
                PurchaseQuantity);

            if (success)
            {
                // Refresh data from DB to reflect stock change and new purchase record
                PurchaseMessage = $"Sale complete: {PurchaseQuantity}x {SelectedProductForPurchase.Name} to {SelectedCustomerForPurchase.Name}.";
                ExecuteLoadData(null);
                LoadChartData(); // Update sales chart
            }
            else
            {
                PurchaseMessage = $"ERROR: Insufficient stock for {SelectedProductForPurchase.Name}.";
            }
        }

        private void FilterPurchases()
        {
            // Currently, no complex filtering, just show the latest purchases
            FilteredPurchases = new ObservableCollection<Purchase>(Purchases);
        }
    }

    // =========================================================================
    // COMMAND IMPLEMENTATION
    // =========================================================================

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
