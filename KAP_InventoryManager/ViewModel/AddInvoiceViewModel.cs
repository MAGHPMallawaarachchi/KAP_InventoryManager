using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using KAP_InventoryManager.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    public class AddInvoiceViewModel : ViewModelBase
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        private string _invoiceNo;
        private string _customerSearchText;
        private string _partNoSearchText;

        private string _selectedCustomerId;
        private CustomerModel _selectedCustomer;
        private ObservableCollection<string> _customers;

        private string _selectedPartNo;
        private ItemModel _selectedItem;
        private ObservableCollection<string> _partNumbers;

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

        private string _shopName = "";
        private bool _isJaneesh = false;
        private bool _isComboboxEnabled;

        private readonly ICustomerRepository _customerRepository;
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserRepository _userRepository;

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
            get => _customerSearchText; 
            set
            {
                _customerSearchText = value;
                OnPropertyChanged(nameof(CustomerSearchText));
                DebouncePopulateCustomersAsync();
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

        public ObservableCollection<string> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged(nameof(Customers));
            }
        }

        public string PartNoSearchText
        {
            get => _partNoSearchText;
            set
            {
                _partNoSearchText = value;
                OnPropertyChanged(nameof(PartNoSearchText));
                _ = FetchPartNumbersAsync(value);
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

        public ObservableCollection<string> PartNumbers
        {
            get => _partNumbers;
            set
            {
                _partNumbers = value;
                OnPropertyChanged(nameof(PartNumbers));
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
                Discount = CustomerDiscount;
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
                if(Number > 22)
                {
                    IsComboboxEnabled = false;
                }
            }
        }

        public decimal Total
        {
            get => _total;
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

        public bool IsComboboxEnabled
        {
            get => _isComboboxEnabled; 
            set
            {
                _isComboboxEnabled = value;
                OnPropertyChanged(nameof(IsComboboxEnabled));
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

        public string ShopName
        {
            get => _shopName;
            set
            {
                _shopName = value;
                OnPropertyChanged(nameof(ShopName));
            }
        }

        public bool IsJaneesh
        {
            get => _isJaneesh;
            set
            {
                _isJaneesh = value;
                OnPropertyChanged(nameof(IsJaneesh));
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
            _customerRepository = new CustomerRepository();
            _salesRepRepository = new SalesRepRepository();
            _itemRepository = new ItemRepository();
            _invoiceRepository = new InvoiceRepository();
            _userRepository = new UserRepository();

            AddInvoiceItemCommand = new ViewModelCommand(ExecuteAddInvoiceItemCommand);
            ClearInvoiceCommand = new ViewModelCommand(ExecuteClearInvoiceCommand);
            DeleteInvoiceItemCommand = new ViewModelCommand(ExecuteDeleteInvoiceItemCommand);
            EditInvoiceItemCommand = new ViewModelCommand(ExecuteEditInvoiceItemCommand);
            CancelInvoiceItemCommand = new ViewModelCommand(ExecuteCancelInvoiceItemCommand);
            SaveInvoiceCommand = new ViewModelCommand(ExecuteSaveInvoiceCommand);

            _cancellationTokenSource = new CancellationTokenSource();
            Initialize();
        }

        private async Task<string> GetInvoicePathAsync()
        {
            string path = string.Empty;
            try
            {
                path = await Task.Run(() => _userRepository.GetPath(Thread.CurrentPrincipal.Identity.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the path. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return path;
        }

        private async void Initialize()
        {
            InvoiceNo = await _invoiceRepository.GetNextInvoiceNumberAsync();
            InvoiceItems = new ObservableCollection<InvoiceItemModel>();
            Customers = new ObservableCollection<string>();
            PartNumbers = new ObservableCollection<string>();

            DateTime currentDateTime = DateTime.Now;
            CurrentDate = currentDateTime.ToString("yyyy-MM-dd");
            CurrentTime = currentDateTime.ToString("hh.mmtt");

            await PopulateSalesRepsAsync();
            Number = Counter = 1;
            Total = 0;
            IsSelectedInvoiceItem = false;
            IsComboboxEnabled = true;
        }

        private async void DebouncePopulateCustomersAsync()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _cancellationTokenSource.Token); // 300ms debounce time

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await PopulateCustomersAsync(_cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore the TaskCanceledException
            }
        }

        private async Task PopulateCustomersAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var customers = new ObservableCollection<string>();

                if (!string.IsNullOrEmpty(CustomerSearchText))
                {
                    var results = await _customerRepository.SearchCustomerAsync(CustomerSearchText);

                    if (results != null)
                    {
                        foreach (var suggestion in results)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            customers.Add(suggestion);
                        }
                    }
                }

                Customers = customers;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async void PopulateCustomerDetailsAsync()
        {
            try
            {
                SelectedCustomer = await _customerRepository.GetByCustomerIDAsync(SelectedCustomerId);
                if (SelectedCustomer != null)
                {
                    SelectedPaymentType = SelectedCustomer.PaymentType;
                    CustomerDiscount = SelectedPaymentType == "CASH" ? 30 : 25;
                    SelectedRepId = SelectedCustomer.RepID ?? "None";

                    if(SelectedCustomer.CustomerID == "PMS-K" || SelectedCustomer.CustomerID == "PATHMA M. S. RP" || SelectedCustomer.CustomerID == "ORIENT MC-ALA")
                    {
                        CustomerDiscount = 30;
                    }
                    else if(SelectedCustomer.CustomerID == "Mr.Pradeep")
                    {
                        CustomerDiscount = 35;
                    }

                    if(SelectedCustomer.CustomerID == "JANEESH AUTO PARTS")
                    {
                        CustomerDiscount = 35;
                        IsJaneesh = true;
                    }
                    else
                    {
                        IsJaneesh = false;
                        ShopName = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private async void DebouncePopulatePartNumbersAsync()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _cancellationTokenSource.Token); // 300ms debounce time

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await PopulatePartNumbersAsync(_cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore the TaskCanceledException
            }
        }

        private async Task FetchPartNumbersAsync(string searchText)
        {
            await _semaphore.WaitAsync();
            try
            {
                var partNumbers = new ObservableCollection<string>();
                if (!string.IsNullOrEmpty(searchText))
                {
                    var results = await _itemRepository.SearchPartNoAsync(searchText);
                    if (results != null)
                    {
                        foreach (var suggestion in results)
                        {
                            partNumbers.Add(suggestion);
                        }
                    }
                }
                PartNumbers = partNumbers;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task PopulatePartNumbersAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var partNumbers = new ObservableCollection<string>();

                if (!string.IsNullOrEmpty(PartNoSearchText))
                {
                    var results = await _itemRepository.SearchPartNoAsync(PartNoSearchText);

                    if (results != null)
                    {
                        foreach (var suggestion in results)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            partNumbers.Add(suggestion);
                        }
                    }
                }

                PartNumbers = partNumbers;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }


        private async Task PopulateSalesRepsAsync()
        {
            try
            {
                var salesReps = await _salesRepRepository.GetAllRepIdsAsync();
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


        private async void PopulateItemDetailsAsync()
        {
            try
            {
                SelectedItem = await _itemRepository.GetByPartNoAsync(SelectedPartNo);
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
            if (!_itemRepository.CheckQty(SelectedPartNo, Quantity))
            {
                ShowErrorMessage("This item is out of stock");
                return false;
            }
            if(Number > 22)
            {
                ShowErrorMessage("You have already added 22 items");
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
                InvoiceNo = InvoiceNo,
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
            IsJaneesh = false;
            ShopName = string.Empty;
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

                    var invoiceDoc = new InvoiceDocument();
                    string invoicePath = await GetInvoicePathAsync();
                    int totalQty = 0;

                    if (invoicePath != null)
                    {
                        invoiceDoc.GenerateInvoicePDF(InvoiceNo, SelectedCustomer, invoice, InvoiceItems, invoicePath, ShopName, totalQty);

                        await _invoiceRepository.AddInvoiceAsync(invoice);

                        foreach (var invoiceItem in InvoiceItems)
                        {
                            await _invoiceRepository.AddInvoiceItemAsync(invoiceItem);
                            totalQty += invoiceItem.Quantity;
                        }

                        ClearInvoice();
                        Messenger.Default.Send("NewInvoiceAdded");
                        MessageBox.Show("Invoice saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to retrieve the invoice path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
