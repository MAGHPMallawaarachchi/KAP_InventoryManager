using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class EditCustomerModalViewModel : ViewModelBase
    {
        private CustomerModel _customer;
        private string _customerID;
        private string _name;
        private string _address;
        private string _city;
        private string _contactNo;
        private string _email;
        private string _paymentType;
        private string _repID;

        private readonly ICustomerRepository CustomerRepository;

        public CustomerModel Customer
        {
            get => _customer;
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
            }
        }

        public string CustomerID
        {
            get => _customerID; 
            set
            {
                _customerID = value;
                OnPropertyChanged(nameof(CustomerID));
            }
        }

        public string Name
        {
            get => _name; 
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
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

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged(nameof(City));
            }
        }

        public string ContactNo
        {
            get => _contactNo;
            set
            {
                _contactNo = value;
                OnPropertyChanged(nameof(ContactNo));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string PaymentType
        {
            get => _paymentType;
            set
            {
                _paymentType = value;
                OnPropertyChanged(nameof(PaymentType));
            }
        }

        public string RepID
        {
            get => _repID;
            set
            {
                _repID = value;
                OnPropertyChanged(nameof(RepID));
            }
        }

        public ICommand UpdateCustomerCommand { get; }
        public ICommand DiscardCommand { get; }

        public EditCustomerModalViewModel()
        {
            CustomerRepository = new CustomerRepository();

            // Register to receive the CustomerModel
            Messenger.Default.Register<CustomerModel>(this, OnMessageReceived);

            // Send the "RequestCustomer" message to request the current customer
            Messenger.Default.Send("RequestCustomer");

            UpdateCustomerCommand = new ViewModelCommand(ExecuteUpdateCustomerCommand, CanExecuteUpdateCustomerCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private void OnMessageReceived(CustomerModel customer)
        {
            if (customer != null)
            {
                Customer = customer;
                ResetTextBoxes();
            }
        }

        private void ExecuteDiscardCommand(object obj)
        {
            ResetTextBoxes();
        }

        private bool CanExecuteUpdateCustomerCommand(object obj)
        {
            bool validate;

            if (string.IsNullOrEmpty(CustomerID) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(City) || string.IsNullOrEmpty(PaymentType))
            {
                validate = false;
            }
            else
            {
                validate = true;
            }

            return validate;
        }

        private void ExecuteUpdateCustomerCommand(object obj)
        {
            CustomerModel updatedCustomer = new CustomerModel
            {
                CustomerID = CustomerID,
                Name = Name,
                Address = Address,
                City = City,
                ContactNo = ContactNo,
                Email = Email,
                PaymentType = PaymentType,
                RepID = RepID,
            };

            CustomerRepository.EditAsync(updatedCustomer);

            Messenger.Default.Send("CustomerUpdated");

            //close the dialog view
            Messenger.Default.Send(new NotificationMessage("CloseDialog"));

        }

        private void ResetTextBoxes()
        {
            CustomerID = Customer.CustomerID;
            Name = Customer.Name;
            Address = Customer.Address;
            City = Customer.City;
            ContactNo = Customer.ContactNo;
            Email = Customer.Email;
            PaymentType = Customer.PaymentType;
            if (Customer.RepID == null)
                RepID = "NONE";
            else
                RepID = Customer.RepID;
        }
    }
}
