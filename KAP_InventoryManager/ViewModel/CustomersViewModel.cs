using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using LiveCharts;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Primitives;

namespace KAP_InventoryManager.ViewModel
{
    public class CustomersViewModel : ViewModelBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<CustomerModel> _customers;
        private IEnumerable<InvoiceModel> _invoices;

        private CustomerModel _selectedCustomer;
        private CustomerModel _currentCustomer;
        private double _debtPercentage;
        private double _debtRemainder;
        private string _searchCustomerText;
        private string _searchInvoiceText;
        private int _pageNumber;
        private bool _isFinalPage;
        private decimal _debtLimit;
        private string _address;

        public ObservableCollection<CustomerModel> Customers
        {
            get => _customers; 
            set
            {
                _customers = value;
                OnPropertyChanged(nameof(Customers));

                SelectedCustomer = Customers.FirstOrDefault();
            }
        }

        public IEnumerable<InvoiceModel> Invoices
        {
            get => _invoices; 
            set
            {
                _invoices = value;
                OnPropertyChanged(nameof(Invoices));
            }
        }

        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));

                if (SelectedCustomer != null)
                {
                    PopulateDetails();
                }
            }
        }

        public CustomerModel CurrentCustomer
        {
            get => _currentCustomer;
            set
            {
                _currentCustomer = value;
                OnPropertyChanged(nameof(CurrentCustomer));
            }
        }


        public double DebtPercentage
        {
            get => _debtPercentage;
            set
            {
                _debtPercentage = value;
                OnPropertyChanged(nameof(DebtPercentage));
            }
        }

        public double DebtRemainder
        {
            get => _debtRemainder;
            set
            {
                _debtRemainder = value;
                OnPropertyChanged(nameof(DebtRemainder));
            }
        }

        public string SearchCustomerText
        {
            get => _searchCustomerText;
            set
            {
                _searchCustomerText = value;
                OnPropertyChanged(nameof(SearchCustomerText));

                PopulateCustomersAsync();
            }
        }

        public string SearchInvoiceText
        {
            get => _searchInvoiceText;
            set
            {
                _searchInvoiceText = value;
                OnPropertyChanged(nameof(SearchInvoiceText));

                PopulateInvoicesAsync();
            }
        }

        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                _pageNumber = value;
                OnPropertyChanged(nameof(PageNumber));
            }
        }

        public bool IsFinalPage
        {
            get => _isFinalPage;
            set
            {
                _isFinalPage = value;
                OnPropertyChanged(nameof(IsFinalPage));
            }
        }

        public decimal DebtLimit
        {
            get => _debtLimit;
            set
            {
                _debtLimit = value;
                OnPropertyChanged(nameof(DebtLimit));
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }

        public CustomersViewModel() 
        {
            _customerRepository = new CustomerRepository();
            _invoiceRepository = new InvoiceRepository();

            GoToNextPageCommand = new ViewModelCommand(ExecuteGoToNextPageCommand);
            GoToPreviousPageCommand = new ViewModelCommand(ExecuteGoToPreviousPageCommand);

            PageNumber = 1;
            DebtLimit = 1000000;

            Customers = new ObservableCollection<CustomerModel>();
            PopulateCustomersAsync();

            Messenger.Default.Register<string>(this, OnMessageReceived);
        }

        private void ExecuteGoToPreviousPageCommand(object obj)
        {
            if(PageNumber != 1)
            {
                PageNumber--;
                PopulateInvoicesAsync();
            }
        }

        private void ExecuteGoToNextPageCommand(object obj)
        {
            if(IsFinalPage == false)
            {
                PageNumber++;
                PopulateInvoicesAsync();
            }
        }

        private async void PopulateCustomersAsync()
        {
            await _semaphore.WaitAsync();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                Customers.Clear();
                List<CustomerModel> customers = (List<CustomerModel>)(string.IsNullOrEmpty(SearchCustomerText)
                    ? await _customerRepository.GetAllAsync()
                    : await _customerRepository.SearchCustomerListAsync(SearchCustomerText));

                foreach (var customer in customers)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    Customers.Add(customer);
                    await Task.Delay(0, _cancellationTokenSource.Token);
                }

                if (Customers.Any())
                {
                    SelectedCustomer = Customers.First();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch customers. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, ignore the exception
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async void PopulateInvoicesAsync()
        {
            try
            {
                Invoices = string.IsNullOrEmpty(SearchInvoiceText)
                    ? await _invoiceRepository.GetInvoiceByCustomerAsync(CurrentCustomer.CustomerID, 10, PageNumber)
                    : await _invoiceRepository.SearchCustomerInvoiceListAsync(SearchInvoiceText, CurrentCustomer.CustomerID, 10, PageNumber);

                IsFinalPage = Invoices.Count() < 10;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateDetails()
        {
            try
            {
                CurrentCustomer = await _customerRepository.GetByCustomerIDAsync(SelectedCustomer.CustomerID);
                Address = CurrentCustomer.Address + " " +CurrentCustomer.City;
                CalculateDebtPercentage();
                PopulateInvoicesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateDebtPercentage()
        {
            if(CurrentCustomer.TotalDebt != 0 && DebtLimit != 0)
            {
                DebtPercentage = (double)Math.Round((CurrentCustomer.TotalDebt / DebtLimit * 100), 2);
                DebtRemainder = 100 - DebtPercentage;
            }
            else
            {
                DebtPercentage = 0;
                DebtRemainder = 100;
            }
        }

        private void OnMessageReceived(string message)
        {
            if (message == "NewCustomerAdded")
            {
                PopulateCustomersAsync();
            }
            else if (message == "RequestCustomer")
            {
                Messenger.Default.Send(CurrentCustomer);
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetReportsForAllCustomers(DateTime startDate, DateTime endDate, string statusFilter)
        {
            var allReports = new List<PaymentModel>();

            try
            {
                // Fetch the list of customers
                var customers = await _customerRepository.GetCustomersFromInvoice(startDate, endDate);

                // Process reports for each customer in parallel
                var tasks = customers.Select(customerId => _customerRepository.GetCustomerReport(customerId, startDate, endDate, statusFilter));

                var results = await Task.WhenAll(tasks);

                // Combine all reports
                foreach (var report in results)
                {
                    allReports.AddRange(report);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get all customer reports. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return allReports;
        }
    }
}
