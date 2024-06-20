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

        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }

        public CustomersViewModel() 
        {
            _customerRepository = new CustomerRepository();
            _invoiceRepository = new InvoiceRepository();

            GoToNextPageCommand = new ViewModelCommand(ExecuteGoToNextPageCommand);
            GoToPreviousPageCommand = new ViewModelCommand(ExecuteGoToPreviousPageCommand);

            PageNumber = 1;

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
                    Customers.Add(customer);
                    await Task.Delay(50, _cancellationTokenSource.Token);
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
            if(CurrentCustomer.TotalDebt != 0 && CurrentCustomer.DebtLimit != 0)
            {
                DebtPercentage = (double)(CurrentCustomer.TotalDebt / CurrentCustomer.DebtLimit * 100);
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
        }
    }
}
