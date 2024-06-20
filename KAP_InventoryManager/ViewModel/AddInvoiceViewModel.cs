using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using QuestPDF.ExampleInvoice;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    public class AddInvoiceViewModel : ViewModelBase
    {
        private string _invoiceNo;
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
        private readonly IInvoiceRepository InvoiceRepository;

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

        public string InvoiceNo
        {
            get => _invoiceNo;
            set
            {
                _invoiceNo = value;
                OnPropertyChanged(nameof(InvoiceNo));
            }
        }

        public string CustomerSearchText
        {
            get { return _customerSearchText; }
            set
            {
                _customerSearchText = value;
                OnPropertyChanged(nameof(CustomerSearchText));
                PopulateCustomersAsync();
            }
        }
        public string PartNoSearchText
        {
            get { return _partNoSearchText; }
            set
            {
                _partNoSearchText = value;
                OnPropertyChanged(nameof(PartNoSearchText));
                PopulatePartNumbersAsync();
            }
        }

        public string SelectedCustomerId
        {
            get => _selectedCustomerId;
            set
            {
                _selectedCustomerId = value;
                OnPropertyChanged(nameof(SelectedCustomerId));
                PopulateCustomerDetailsAsync();
            }
        }

        public string SelectedPartNo
        {
            get => _selectedPartNo;
            set
            {
                _selectedPartNo = value;
                OnPropertyChanged(nameof(SelectedPartNo));
                PopulateItemDetailsAsync();
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
                Console.WriteLine(SelectedRepId);
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
        public ICommand SaveInvoiceCommand { get; }

        public AddInvoiceViewModel() 
        {
            CustomerRepository = new CustomerRepository();
            SalesRepRepository = new SalesRepRepository();
            ItemRepository = new ItemRepository();
            InvoiceRepository = new InvoiceRepository();

            AddInvoiceItemCommand = new ViewModelCommand(ExecuteAddInvoiceItemCommand);
            ClearInvoiceCommand = new ViewModelCommand(ExecuteClearInvoiceCommand);
            DeleteInvoiceItemCommand = new ViewModelCommand(ExecuteDeleteInvoiceItemCommand);
            EditInvoiceItemCommand = new ViewModelCommand(ExecuteEditInvoiceItemCommand);
            CancelInvoiceItemCommand = new ViewModelCommand(ExecuteCancelInvoiceItemCommand);
            SaveInvoiceCommand = new ViewModelCommand(ExecuteSaveInvoiceCommand);

            Initialize();
        }

        private async void Initialize()
        {
            InvoiceNo = await InvoiceRepository.GetNextInvoiceNumberAsync();
            InvoiceItems = new ObservableCollection<InvoiceItemModel>();

            DateTime currentDateTime = DateTime.Now;
            CurrentDate = currentDateTime.ToString("yyyy-MM-dd");
            CurrentTime = currentDateTime.ToString("t");

            await PopulateSalesRepsAsync();
            Number = Counter = 1;
            Total = 0;
            IsSelectedInvoiceItem = false;
        }

        private async void PopulateCustomersAsync()
        {
            try
            {
                Customers.Clear();

                if (!string.IsNullOrEmpty(CustomerSearchText))
                {
                    var results = await CustomerRepository.SearchCustomerAsync(CustomerSearchText);

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
                ShowErrorMessage(ex.Message);
            }
        }

        private async void PopulatePartNumbersAsync()
        {
            try
            {
                PartNumbers.Clear();

                if (!string.IsNullOrEmpty(PartNoSearchText))
                {
                    var results = await ItemRepository.SearchPartNoAsync(PartNoSearchText);

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
                ShowErrorMessage(ex.Message);
            }
        }

        private async Task PopulateSalesRepsAsync()
        {
            try
            {
                var salesReps = await SalesRepRepository.GetAllRepIdsAsync();
                if (salesReps != null)
                {
                    foreach (var salesRep in salesReps)
                    {
                        SalesReps.Add(salesRep);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private async void PopulateCustomerDetailsAsync()
        {
            try
            {
                SelectedCustomer = await CustomerRepository.GetByCustomerIDAsync(SelectedCustomerId);
                if (SelectedCustomer != null)
                {
                    SelectedPaymentType = SelectedCustomer.PaymentType;
                    CustomerDiscount = SelectedPaymentType == "CASH" ? 30 : 25;
                    SelectedRepId = SelectedCustomer.RepID ?? "None";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private async void PopulateItemDetailsAsync()
        {
            try
            {
                SelectedItem = await ItemRepository.GetByPartNoAsync(SelectedPartNo);
                Discount = CustomerDiscount;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void CalculateAmount()
        {
            if (SelectedItem != null)
            {
                Amount = SelectedItem.UnitPrice * Quantity * (100 - Discount) / 100;
            }
        }

        private bool CanExecuteAddInvoiceItemCommand()
        {
            if (SelectedCustomer == null || string.IsNullOrEmpty(SelectedCustomerId))
            {
                ShowErrorMessage("Please select a customer");
                return false;
            }
            if (string.IsNullOrEmpty(SelectedPartNo) || SelectedItem == null)
            {
                ShowErrorMessage("Please select an item");
                return false;
            }
            if (IsItemExist())
            {
                ShowErrorMessage("Oops! You've already added this item to the invoice");
                return false;
            }
            if (Quantity == 0)
            {
                ShowErrorMessage("Please enter the quantity");
                return false;
            }
            if (!ItemRepository.CheckQty(SelectedPartNo, Quantity))
            {
                ShowErrorMessage("This item is out of stock");
                return false;
            }
            return true;
        }

        private void ExecuteAddInvoiceItemCommand(object obj)
        {
            if (CanExecuteAddInvoiceItemCommand() == true)
                AddInvoiceItem();
        }

        private bool CanExecuteSaveInvoiceCommand()
        {
            if (SelectedCustomer == null || string.IsNullOrEmpty(SelectedCustomerId))
            {
                ShowErrorMessage("Please select a customer");
                return false;
            }
            if (InvoiceItems == null || InvoiceItems.Count == 0)
            {
                ShowErrorMessage("Please add at least one item to the invoice");
                return false;
            }
            return true;
        }

        private void ExecuteSaveInvoiceCommand(object obj)
        {
            AddInvoice();
        }

        private void ExecuteCancelInvoiceItemCommand(object obj)
        {
            SelectedInvoiceItem = null;
            ClearItemDetails();
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
            ClearItemDetails();
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
                ClearItemDetails();
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
                ShowErrorMessage("Please select an item to delete");
            }
        }

        private void ExecuteClearInvoiceCommand(object obj)
        {
            ClearInvoice();
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
            ClearItemDetails();
        }

        private bool IsItemExist()
        {
            return InvoiceItems.Any(item => item.PartNo == SelectedPartNo);
        }

        private void ClearItemDetails()
        {
            SelectedItem = null;
            SelectedPartNo = null;
            Quantity = 0;
            Discount = CustomerDiscount;
            Amount = 0;
        }

        private void ClearInvoice()
        {
            ClearItemDetails();
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
            Initialize();
        }

        private async void AddInvoice()
        {
            try
            {
                if (CanExecuteSaveInvoiceCommand())
                {
                    var invoice = new InvoiceModel
                    {
                        InvoiceNo = InvoiceNo,
                        Terms = SelectedPaymentType.ToUpper(),
                        TotalAmount = Total,
                        CustomerID = SelectedCustomerId,
                        RepID = SelectedRepId == "None" ? null : SelectedRepId,
                        Date = DateTime.Now,
                        DueDate = SelectedPaymentType == "CASH" ? DateTime.Now.AddDays(7) : DateTime.Now.AddDays(60)
                    };

                    await InvoiceRepository.AddInvoiceAsync(invoice);

                    foreach (var invoiceItem in InvoiceItems)
                    {
                        invoiceItem.InvoiceNo = InvoiceNo;
                        await InvoiceRepository.AddInvoiceItemAsync(invoiceItem);
                    }

                    var invoiceDoc = new InvoiceDocument();
                    invoiceDoc.GenerateInvoicePDF(InvoiceNo, SelectedCustomer, invoice, InvoiceItems);

                    ClearInvoice();
                    MessageBox.Show("Invoice saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
