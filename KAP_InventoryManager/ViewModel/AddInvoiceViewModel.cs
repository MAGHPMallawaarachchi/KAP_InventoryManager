using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
using System.Windows.Threading;

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
        private int _number;
        private decimal _total;
        private InvoiceItemModel _selectedInvoiceItem;
        private bool _isSelectedInvoiceItem;
        private bool _isAddingNewItem;

        private string _errorMessage;
        private ObservableCollection<InvoiceItemModel> _invoiceItems;

        private readonly ICustomerRepository CustomerRepository;
        private readonly ISalesRepRepository SalesRepRepository;
        private readonly IItemRepository ItemRepository;

        public ObservableCollection<string> Customers { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> PartNumbers { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> SalesReps { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<InvoiceItemModel> InvoiceItems
        {
            get => _invoiceItems;
            set
            {
                _invoiceItems = value;
                OnPropertyChanged(nameof(InvoiceItems));
            }
        }

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

        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public InvoiceItemModel SelectedInvoiceItem
        {
            get => _selectedInvoiceItem;
            set
            {
                _selectedInvoiceItem = value;
                OnPropertyChanged(nameof(SelectedInvoiceItem));
                if(value != null)
                {
                    SelectedPartNo = SelectedInvoiceItem.PartNo;
                    Discount = SelectedInvoiceItem.Discount;
                    Quantity = SelectedInvoiceItem.Quantity;
                    Amount = SelectedInvoiceItem.Amount;
                    Number = SelectedInvoiceItem.No;
                    IsSelectedInvoiceItem = true;
                }
            }
        }

        public bool IsSelectedInvoiceItem
        {
            get => _isSelectedInvoiceItem;
            set
            {
                _isSelectedInvoiceItem = value;
                OnPropertyChanged(nameof(IsSelectedInvoiceItem));
                IsAddingNewItem = !IsSelectedInvoiceItem;
            }
        }

        public bool IsAddingNewItem
        {
            get => _isAddingNewItem;
            set
            {
                _isAddingNewItem = value;
                OnPropertyChanged(nameof(IsAddingNewItem));
            }
        }

        public ICommand AddInvoiceItemCommand { get; }
        public ICommand ClearInvoiceCommand { get; }
        public ICommand DeleteInvoiceItemCommand { get; }
        public ICommand EditInvoiceItemCommand { get; }
        public ICommand CancelInvoiceItemCommand { get; }


        public AddInvoiceViewModel() 
        {
            CustomerRepository = new CustomerRepository();
            SalesRepRepository = new SalesRepRepository();
            ItemRepository = new ItemRepository();

            AddInvoiceItemCommand = new ViewModelCommand(ExecuteAddInvoiceItemCommand);
            ClearInvoiceCommand = new ViewModelCommand(ExecuteClearInvoiceCommand);
            DeleteInvoiceItemCommand = new ViewModelCommand(ExecuteDeleteInvoiceItemCommand);
            EditInvoiceItemCommand = new ViewModelCommand(ExecuteEditInvoiceItemCommand);
            CancelInvoiceItemCommand = new ViewModelCommand(ExecuteCancelInvoiceItemCommand);

            DateTime currentDateTime = DateTime.Now;

            CurrentDate = currentDateTime.ToString("yyyy-MM-dd");
            CurrentTime = currentDateTime.ToString("t");

            PopulateSalesReps();
            InvoiceItems = new ObservableCollection<InvoiceItemModel>();
            Number = Counter = 1;
            Total = 0;
            IsSelectedInvoiceItem = false;
        }

        private void ExecuteCancelInvoiceItemCommand(object obj)
        {
            SelectedInvoiceItem = null;
            Clear();
            IsSelectedInvoiceItem = false;
            Number = Counter;
        }

        private void ExecuteEditInvoiceItemCommand(object obj)
        {
            foreach (var item in InvoiceItems)
            {
                if (item.PartNo == SelectedPartNo)
                {
                    item.Quantity = Quantity;
                    item.Discount = Discount;
                    Total -= item.Amount;
                    item.Amount = Amount;
                    Total += Amount;
                }
            }
            InvoiceItems = new ObservableCollection<InvoiceItemModel>(InvoiceItems);

            SelectedInvoiceItem = null;
            Clear();
            IsSelectedInvoiceItem = false;
            Number = Counter;
        }

        private void ExecuteDeleteInvoiceItemCommand(object obj)
        {
            if(SelectedInvoiceItem != null)
            {
                Total -= SelectedInvoiceItem.Amount;
                InvoiceItems.Remove(SelectedInvoiceItem);
                Counter--;
                Number = Counter;
                Clear();
                SelectedInvoiceItem = null;
                IsSelectedInvoiceItem = false;

                int newNumber = 1;
                foreach (var item in InvoiceItems)
                {
                    item.No = newNumber++;
                    OnPropertyChanged(nameof(item.No));
                }

                OnPropertyChanged(nameof(InvoiceItems));
                InvoiceItems = new ObservableCollection<InvoiceItemModel>(InvoiceItems);
            }
            else
            {
                MessageBox.Show("Please select a item to delete", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteAddInvoiceCommand()
        {
            bool validate;
            if (SelectedCustomer == null || string.IsNullOrEmpty(SelectedCustomerId))
            {
                validate = false;
                MessageBox.Show("Please select a customer", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if(string.IsNullOrEmpty(SelectedPartNo) || SelectedItem == null)
            {
                validate = false;
                MessageBox.Show("Please select an item", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Quantity == 0)
            {
                validate = false;
                MessageBox.Show("Please enter the quantity", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (IsItemExist() == true)
            {
                validate = false;
                MessageBox.Show("Oops! You've Already Added This Item to the Invoice", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                validate = true;
            return validate;
        }

        private bool IsItemExist()
        {
            bool isItemExist = false;
            foreach (var item in InvoiceItems)
            {
                if(item.PartNo == SelectedPartNo)
                {
                    isItemExist = true;
                }
            }
            return isItemExist;
        }

        private void ExecuteClearInvoiceCommand(object obj)
        {
            ClearInvoice();
        }

        private void ExecuteAddInvoiceItemCommand(object obj)
        {
            if(CanExecuteAddInvoiceCommand() == true)
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
                No = Number,
                PartNo = SelectedItem.PartNo,
                BrandID = SelectedItem.BrandID,
                Description = SelectedItem.Description,
                Quantity = Quantity,
                UnitPrice = SelectedItem.UnitPrice,
                Discount = Discount,
                Amount = Amount,
            };

            InvoiceItems.Add(invoiceItem);
            Total += Amount;
            Counter++;
            Number = Counter;
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
            SelectedItem = null;
            SelectedPartNo = null;
            Quantity = 0;
            Discount = CustomerDiscount;
            Amount = 0;
        }

        private void ClearInvoice()
        {
            Clear();
            SelectedCustomer = null;
            SelectedCustomerId = null;
            SelectedPaymentType = null;
            CustomerDiscount = 0;
            SelectedRepId = null;
            InvoiceItems.Clear();
            Discount = 0;
            Total = 0;
            Counter = 1;
            Number = 1;
        }
    }
}
