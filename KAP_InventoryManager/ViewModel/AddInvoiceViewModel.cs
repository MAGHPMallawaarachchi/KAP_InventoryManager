using GalaSoft.MvvmLight.Command;
using KAP_InventoryManager.CustomControls;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    public class AddInvoiceViewModel : ViewModelBase
    {
        private string _customerSearchText;
        private string _partNoSearchText;

        private string _selectedCustomerId;
        private CustomerModel _selectedCustomer;

        private string _selectedPartNo;
        private ItemModel _selectedItem;

        private decimal _discount;
        private decimal _customerDiscount;
        private string _selectedRepId;
        private string _selectedPaymentType;
        private decimal _amount;
        private int _quantity;

        private string _currentTime;
        private string _currentDate;

        private int _counter;


        private readonly ICustomerRepository CustomerRepository;
        private readonly ISalesRepRepository SalesRepRepository;
        private readonly IItemRepository ItemRepository;

        public ObservableCollection<string> Customers { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> PartNumbers { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> SalesReps { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<InvoiceItemModel> InvoiceItems { get; set; } = new ObservableCollection<InvoiceItemModel>();

        public ICommand AddInvoiceItemCommand { get; }

        public string CustomerSearchText
        {
            get { return _customerSearchText; }
            set
            {
                _customerSearchText = value;
                OnPropertyChanged(nameof(CustomerSearchText));
                PopulateCustomers();
            }
        }
        public string PartNoSearchText
        {
            get { return _partNoSearchText; }
            set
            {
                _partNoSearchText = value;
                OnPropertyChanged(nameof(PartNoSearchText));
                PopulatePartNumbers();
            }
        }

        public string SelectedCustomerId
        {
            get => _selectedCustomerId;
            set
            {
                _selectedCustomerId = value;
                OnPropertyChanged(nameof(SelectedCustomerId));
                PopulateCustomerDetails();
            }
        }

        public string SelectedPartNo
        {
            get => _selectedPartNo;
            set
            {
                _selectedPartNo = value;
                OnPropertyChanged(nameof(SelectedPartNo));
                PopulateItemDetails();
            }
        }

        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
            }
        }

        public ItemModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public string SelectedPaymentType
        {
            get => _selectedPaymentType;
            set
            {
                _selectedPaymentType = value;
                OnPropertyChanged(nameof(SelectedPaymentType));
            }
        }

        public decimal Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
                CalculateAmount();
            }
        }
        public decimal CustomerDiscount
        {
            get => _customerDiscount;
            set
            {
                _customerDiscount = value;
                OnPropertyChanged(nameof(CustomerDiscount));
            }
        }

        public string SelectedRepId
        {
            get => _selectedRepId;
            set
            {
                _selectedRepId = value;
                OnPropertyChanged(nameof(SelectedRepId));
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                CalculateAmount();
            }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged(nameof(CurrentDate));
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }

        public int Counter
        {
            get => _counter;
            set
            {
                _counter = value;
                OnPropertyChanged(nameof(Counter));
            }
        }

        public AddInvoiceViewModel() 
        {
            CustomerRepository = new CustomerRepository();
            SalesRepRepository = new SalesRepRepository();
            ItemRepository = new ItemRepository();

            AddInvoiceItemCommand = new ViewModelCommand(ExecuteAddInvoiceItemCommand);

            DateTime currentDateTime = DateTime.Now;

            CurrentDate = currentDateTime.ToString("yyyy-MM-dd");
            CurrentTime = currentDateTime.ToString("t");

            PopulateSalesReps();
            Counter = 1;
        }

        private void ExecuteAddInvoiceItemCommand(object obj)
        {
            AddInvoiceItem();
        }

        private void PopulateCustomers()
        {
            try
            {
                Customers.Clear();

                if(CustomerSearchText != null || CustomerSearchText != "")
                {
                    var results = CustomerRepository.SearchCustomer(CustomerSearchText);

                    if (results != null)
                    {
                        foreach (var suggestion in results)
                        {
                            Customers.Add(suggestion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to fetch suggestions. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulatePartNumbers()
        {
            try
            {
                PartNumbers.Clear();

                if (PartNoSearchText != null || PartNoSearchText != "")
                {
                    var results = ItemRepository.SearchPartNo(PartNoSearchText);

                    if (results != null)
                    {
                        foreach (var suggestion in results)
                        {
                            PartNumbers.Add(suggestion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to fetch suggestions. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateSalesReps()
        {
            try
            {
                var salesReps = SalesRepRepository.GetAllRepIds();
                if (salesReps != null)
                {
                    foreach(var salesRep in salesReps)
                    {
                        SalesReps.Add(salesRep);
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"Failed to fetch sales reps. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateCustomerDetails()
        {
            try
            {
                SelectedCustomer = CustomerRepository.GetByCustomerID(SelectedCustomerId);
                if(SelectedCustomer != null)
                {
                    SelectedPaymentType = SelectedCustomer.PaymentType;

                    if(SelectedPaymentType == "CASH")
                    {
                        CustomerDiscount = 30;
                    }
                    else
                    {
                        CustomerDiscount = 25;
                    }

                    if (SelectedCustomer.RepID != null)
                    {
                        SelectedRepId = SelectedCustomer.RepID;
                    }
                    else
                    {
                        SelectedRepId = "None";
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch customer's details. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddInvoiceItem()
        {
            var invoiceItem = new InvoiceItemModel
            {
                No = Counter,
                PartNo = SelectedItem.PartNo,
                BrandID = SelectedItem.BrandID,
                Description = SelectedItem.Description,
                Quantity = Quantity,
                UnitPrice = SelectedItem.UnitPrice,
                Discount = Discount,
                Amount = Amount,
            };

            InvoiceItems.Add(invoiceItem);
            Clear();
        }

        private void PopulateItemDetails()
        {
            try
            {
                SelectedItem = ItemRepository.GetByPartNo(SelectedPartNo);
                Discount = CustomerDiscount;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch item's details. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateAmount()
        {
            if(SelectedItem != null)
            {
                Amount = SelectedItem.UnitPrice * Quantity * (100 - Discount) / 100;
            }
        }

        private void Clear()
        {
            Counter++;
            SelectedItem = null;
            SelectedPartNo = null;
            Quantity = 0;
            Discount = CustomerDiscount;
            Amount = 0;
        }
    }
}
