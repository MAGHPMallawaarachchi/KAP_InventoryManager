using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class NewCustomerModalViewModel : ViewModelBase
    {
        private string _customerID;
        private string _name;
        private string _address;
        private string _city;
        private string _contactNo;
        private string _paymentType;
        private decimal _debtLimit;
        private string _repID;

        private readonly ICustomerRepository CustomerRepository;

        public string CustomerID
        {
            get { return _customerID; }
            set
            {
                _customerID = value;
                OnPropertyChanged(nameof(CustomerID));
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                OnPropertyChanged(nameof(City));
            }
        }

        public string ContactNo
        {
            get { return _contactNo; }
            set
            {
                _contactNo = value;
                OnPropertyChanged(nameof(ContactNo));
            }
        }

        public string PaymentType
        {
            get { return _paymentType; }
            set
            {
                _paymentType = value;
                OnPropertyChanged(nameof(PaymentType));
            }
        }

        public decimal DebtLimit
        {
            get { return _debtLimit; }
            set
            {
                _debtLimit = value;
                OnPropertyChanged(nameof(DebtLimit));
            }
        }

        public string RepID
        {
            get { return _repID; }
            set
            {
                _repID = value;
                OnPropertyChanged(nameof(RepID));
            }
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand DiscardCommand { get; }

        public NewCustomerModalViewModel()
        {
            CustomerRepository = new CustomerRepository();
            AddCustomerCommand = new ViewModelCommand(ExecuteAddCustomerCommand, CanExecuteAddCustomerCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private void ExecuteDiscardCommand(object obj)
        {
            ClearTextBoxes();
        }

        private bool CanExecuteAddCustomerCommand(object obj)
        {
            bool validate;

            if (string.IsNullOrEmpty(CustomerID) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(City) || string.IsNullOrEmpty(ContactNo) || string.IsNullOrEmpty(PaymentType))
            {
                validate = false;
            }
            else if (PaymentType == "CREDIT" && DebtLimit == 0)
            {
                validate = false;
            }
            else
            {
                validate = true;
            }

            return validate;
        }

        private void ExecuteAddCustomerCommand(object obj)
        {
            try
            {
                CustomerModel newCustomer = new CustomerModel
                {
                    CustomerID = CustomerID,
                    Name = Name,
                    Address = Address,
                    City = City,
                    ContactNo = ContactNo,
                    PaymentType = PaymentType,
                    DebtLimit = DebtLimit,
                    RepID = RepID,
                };

                CustomerRepository.Add(newCustomer);
                ClearTextBoxes();
                MessageBox.Show("Customer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to add customer. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearTextBoxes()
        {
            CustomerID = string.Empty;
            Name = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            ContactNo = string.Empty;
            PaymentType = string.Empty;
            DebtLimit = 0;
            RepID = string.Empty;
        }
    }
}
