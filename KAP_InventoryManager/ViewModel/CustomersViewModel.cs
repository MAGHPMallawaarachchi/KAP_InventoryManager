using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using LiveCharts;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace KAP_InventoryManager.ViewModel
{
    public class CustomersViewModel : ViewModelBase
    {
        private readonly ICustomerRepository CustomerRepository;
        private readonly IInvoiceRepository InvoiceRepository;

        private IEnumerable<CustomerModel> _customers;
        private IEnumerable<InvoiceModel> _invoices;
        private CustomerModel _selectedCustomer;
        private CustomerModel _currentCustomer;
        private double _debtPercentage;
        private double _debtRemainder;
        private string _searchCustomerText;
        private string _searchInvoiceText;
        private int _pageNumber;
        private bool _isFinalPage;

        public IEnumerable<CustomerModel> Customers
        {
            get { return _customers; }
            set
            {
                _customers = value;
                OnPropertyChanged(nameof(Customers));

                SelectedCustomer = Customers.FirstOrDefault();
            }
        }

        public IEnumerable<InvoiceModel> Invoices
        {
            get { return _invoices; }
            set
            {
                _invoices = value;
                OnPropertyChanged(nameof(Invoices));
            }
        }

        public CustomerModel SelectedCustomer
        {
            get { return _selectedCustomer; }
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
            get { return _currentCustomer; }
            set
            {
                _currentCustomer = value;
                OnPropertyChanged(nameof(CurrentCustomer));
            }
        }


        public double DebtPercentage
        {
            get { return _debtPercentage; }
            set
            {
                _debtPercentage = value;
                OnPropertyChanged(nameof(DebtPercentage));
            }
        }

        public double DebtRemainder
        {
            get { return _debtRemainder; }
            set
            {
                _debtRemainder = value;
                OnPropertyChanged(nameof(DebtRemainder));
            }
        }

        public string SearchCustomerText
        {
            get { return _searchCustomerText; }
            set
            {
                _searchCustomerText = value;
                OnPropertyChanged(nameof(SearchCustomerText));

                PopulateCustomersAsync();
            }
        }

        public string SearchInvoiceText
        {
            get { return _searchInvoiceText; }
            set
            {
                _searchInvoiceText = value;
                OnPropertyChanged(nameof(SearchInvoiceText));

                PopulateInvoicesAsync();
            }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                _pageNumber = value;
                OnPropertyChanged(nameof(PageNumber));
            }
        }

        public bool IsFinalPage
        {
            get { return _isFinalPage; }
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
            CustomerRepository = new CustomerRepository();
            InvoiceRepository = new InvoiceRepository();

            GoToNextPageCommand = new ViewModelCommand(ExecuteGoToNextPageCommand);
            GoToPreviousPageCommand = new ViewModelCommand(ExecuteGoToPreviousPageCommand);

            PageNumber = 1;
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
            try
            {
                if (SearchCustomerText == null)
                    Customers = await CustomerRepository.GetAllAsync();
                else
                    Customers = await CustomerRepository.SearchCustomerListAsync(SearchCustomerText);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch customers. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateInvoicesAsync()
        {
            try
            {
                if(SearchInvoiceText == null)
                {
                    Invoices = await InvoiceRepository.GetInvoiceByCustomerAsync(CurrentCustomer.CustomerID, 10, PageNumber);
                    if (Invoices.Count() < 10)
                        IsFinalPage = true;
                }
                else
                {
                    Invoices = await InvoiceRepository.SearchCustomerInvoiceListAsync(SearchInvoiceText, CurrentCustomer.CustomerID, 10, PageNumber);
                    if (Invoices.Count() < 10)
                        IsFinalPage = true;
                }

            }catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch invoices. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateDetails()
        {
            try
            {
                CurrentCustomer = CustomerRepository.GetByCustomerID(SelectedCustomer.CustomerID);               
                CalculateDebtPercentage();
                PopulateInvoicesAsync();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch customer's details. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
