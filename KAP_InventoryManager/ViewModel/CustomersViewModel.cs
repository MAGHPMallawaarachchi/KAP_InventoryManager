using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
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

        private IEnumerable<CustomerModel> _customers;
        private CustomerModel _selectedCustomer;
        private CustomerModel _currentCustomer;
        private double _debtPercentage;

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

        public CustomersViewModel() 
        {
            DebtPercentage = 25;
            CustomerRepository = new CustomerRepository();
            PopulateListBoxAsync();

            Messenger.Default.Register<string>(this, OnMessageReceived);
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

        private async void PopulateListBoxAsync()
        {
            try
            {
                Customers = await CustomerRepository.GetAllAsync();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch customers. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateDetails()
        {
            try
            {
                CurrentCustomer = CustomerRepository.GetByCustomerID(SelectedCustomer.CustomerID);
                CalculateDebtPercentage();
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
            }
            else
            {
                DebtPercentage = 0;
            }
        }

        private void OnMessageReceived(string message)
        {
            if (message == "NewCustomerAdded")
            {
                PopulateListBoxAsync();
            }
        }
    }
}
