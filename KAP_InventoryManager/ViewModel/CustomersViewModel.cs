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
        private double _progressPercentage;

        public IEnumerable<CustomerModel> Customers
        {
            get { return _customers; }
            set
            {
                _customers = value;
                OnPropertyChanged(nameof(Customers));
            }
        }
        public CustomerModel SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));

                Messenger.Default.Send(SelectedCustomer);
            }
        }

        public CustomersViewModel() 
        {
            ProgressPercentage = 25;
            CustomerRepository = new CustomerRepository();
            PopulateListBox();
        }

        public double ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                _progressPercentage = value;
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        private void PopulateListBox()
        {
            try
            {
                Customers = CustomerRepository.GetAll();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch customers. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
