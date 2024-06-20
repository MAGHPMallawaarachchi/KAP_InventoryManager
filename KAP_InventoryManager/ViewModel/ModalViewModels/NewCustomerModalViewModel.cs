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
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class NewCustomerModalViewModel : ViewModelBase
    {
        private string _customerID;
        private string _name;
        private string _address;
        private string _city;
        private string _contactNo;
        private string _email;
        private string _paymentType;
        private decimal _debtLimit;
        private string _repID;
        private List<string> _reps;


        private readonly ICustomerRepository CustomerRepository;
        private readonly ISalesRepRepository SalesRepRepository;

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

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
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

        public List<string> Reps
        {
            get { return _reps; }
            set
            {
                _reps = value;
                OnPropertyChanged(nameof(_reps));
            }
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand DiscardCommand { get; }

        public NewCustomerModalViewModel()
        {
            CustomerRepository = new CustomerRepository();
            SalesRepRepository = new SalesRepRepository();
            AddCustomerCommand = new ViewModelCommand(ExecuteAddCustomerCommand, CanExecuteAddCustomerCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);

            GetReps();
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
            CustomerModel newCustomer = new CustomerModel
            {
                CustomerID = CustomerID,
                Name = Name,
                Address = Address,
                City = City,
                ContactNo = ContactNo,
                Email = Email,
                PaymentType = PaymentType,
                DebtLimit = DebtLimit,
                RepID = RepID,
            };

            CustomerRepository.AddAsync(newCustomer);
            ClearTextBoxes();
            Messenger.Default.Send("NewCustomerAdded");
        }

        private async void GetReps()
        {
            Reps = await SalesRepRepository.GetAllRepIdsAsync();
        }

        private void ClearTextBoxes()
        {
            CustomerID = string.Empty;
            Name = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            ContactNo = string.Empty;
            Email = string.Empty;
            PaymentType = string.Empty;
            DebtLimit = 0;
            RepID = string.Empty;
        }
    }
}
