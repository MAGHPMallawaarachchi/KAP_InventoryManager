using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Xceed.Wpf.Toolkit.Primitives;
using System.Threading;
using GalaSoft.MvvmLight.Messaging;

namespace KAP_InventoryManager.ViewModel
{
    public class AddReturnViewModel: ViewModelBase
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        private string _returnNo;
        private string _currentTime;
        private string _currentDate;

        private string _invoiceSearchText;
        private string _selectedInvoiceNo;
        private InvoiceModel _invoice;
        private CustomerModel _customer;

        private string _partNoSearchText;
        private string _selectedPartNo;
        private InvoiceItemModel _selectedInvoiceItem;


        private bool _isSelectedReturnItem;
        private ReturnItemModel _selectedReturnItem;

        private int _qty;
        private int _damagedQty;
        private decimal _amount;

        private int _counter;
        private int _number;
        private decimal _total;

        private bool _isAddingNewItem;

        private string _errorMessage;
        private ObservableCollection<ReturnItemModel> _returnItems;

        private ObservableCollection<string> _partNumbers;
        private ObservableCollection<string> _invoices;

        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IReturnRepository _returnRepository;

        public ObservableCollection<ReturnItemModel> ReturnItems
        {
            get => _returnItems;
            set
            {
                _returnItems = value;
                OnPropertyChanged(nameof(ReturnItems));
            }
        }

        public string ReturnNo
        {
            get => _returnNo;
            set
            {
                _returnNo = value;
                OnPropertyChanged(nameof(ReturnNo));
            }
        }

        public string InvoiceSearchText
        {
            get => _invoiceSearchText; 
            set
            {
                _invoiceSearchText = value;
                OnPropertyChanged(nameof(InvoiceSearchText));
                DebouncePopulateInvoicesAsync();
            }
        }
        public string PartNoSearchText
        {
            get => _partNoSearchText; 
            set
            {
                _partNoSearchText = value;
                OnPropertyChanged(nameof(PartNoSearchText));
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

        public string SelectedInvoiceNo
        {
            get => _selectedInvoiceNo;
            set
            {
                _selectedInvoiceNo = value;
                OnPropertyChanged(nameof(SelectedInvoiceNo));
                PopulateInvoiceDetails();
            }
        }

        public InvoiceModel Invoice
        {
            get => _invoice;
            set
            {
                _invoice = value;
                OnPropertyChanged(nameof(Invoice));
                PopulatePartNumbers();
            }
        }

        public CustomerModel Customer
        {
            get => _customer;
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
            }
        }

        public InvoiceItemModel SelectedInvoiceItem
        {
            get => _selectedInvoiceItem;
            set
            {
                _selectedInvoiceItem = value;
                OnPropertyChanged(nameof(SelectedInvoiceItem));
            }
        }

        public int Qty
        {
            get => _qty;
            set
            {
                _qty = value;
                OnPropertyChanged(nameof(Qty));
                CalculateAmount();
            }
        }

        public int DamagedQty
        {
            get => _damagedQty;
            set
            {
                _damagedQty = value;
                OnPropertyChanged(nameof(DamagedQty));
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

        public ReturnItemModel SelectedReturnItem
        {
            get => _selectedReturnItem;
            set
            {
                _selectedReturnItem = value;
                OnPropertyChanged(nameof(SelectedReturnItem));
                if (value != null)
                {
                    SelectedPartNo = SelectedReturnItem.PartNo;
                    Qty = SelectedReturnItem.Quantity;
                    DamagedQty = SelectedReturnItem.DamagedQty;
                    Number = SelectedReturnItem.No;
                    IsSelectedReturnItem = true;
                }
            }
        }

        public bool IsSelectedReturnItem
        {
            get => _isSelectedReturnItem;
            set
            {
                _isSelectedReturnItem = value;
                OnPropertyChanged(nameof(IsSelectedReturnItem));
                IsAddingNewItem = !IsSelectedReturnItem;
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

        public ObservableCollection<string> PartNumbers
        {
            get => _partNumbers;
            set
            {
                _partNumbers = value;
                OnPropertyChanged(nameof(PartNumbers));
            }
        }

        public ObservableCollection<string> Invoices
        {
            get => _invoices;
            set
            {
                _invoices = value;
                OnPropertyChanged(nameof(Invoices));
            }
        }

        public ICommand AddReturnItemCommand { get; }
        public ICommand ClearReturnCommand { get; }
        public ICommand DeleteReturnItemCommand { get; }
        public ICommand EditReturnItemCommand { get; }
        public ICommand CancelReturnItemCommand { get; }
        public ICommand SaveReturnCommand { get; }


        public AddReturnViewModel()
        {
            _customerRepository = new CustomerRepository();
            _invoiceRepository = new InvoiceRepository();
            _returnRepository = new ReturnRepository();

            AddReturnItemCommand = new ViewModelCommand(ExecuteAddReturnItemCommand);
            ClearReturnCommand = new ViewModelCommand(ExecuteClearReturnCommand);
            DeleteReturnItemCommand = new ViewModelCommand(ExecuteDeleteReturnItemCommand);
            EditReturnItemCommand = new ViewModelCommand(ExecuteEditReturnItemCommand);
            CancelReturnItemCommand = new ViewModelCommand(ExecuteCancelReturnItemCommand);
            SaveReturnCommand = new ViewModelCommand(ExecuteSaveReturnCommand);

            _cancellationTokenSource = new CancellationTokenSource();
            Initialize();
        }

        private async void Initialize()
        {
            ReturnNo = await _returnRepository.GetNextReturnNumberAsync();
            ReturnItems = new ObservableCollection<ReturnItemModel>();
            PartNumbers = new ObservableCollection<string>();
            Invoices = new ObservableCollection<string>();

            DateTime currentDateTime = DateTime.Now;
            CurrentDate = currentDateTime.ToString("yyyy-MM-dd");
            CurrentTime = currentDateTime.ToString("t");

            Number = Counter = 1;
            Total = 0;
            IsSelectedReturnItem = false;
        }

        private async void DebouncePopulateInvoicesAsync()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _cancellationTokenSource.Token); // 300ms debounce time

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await PopulateInvoicesAsync(_cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore the TaskCanceledException
            }
        }

        private async Task PopulateInvoicesAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var invoices = new ObservableCollection<string>();

                if (!string.IsNullOrEmpty(InvoiceSearchText))
                {
                    var results = await _invoiceRepository.SearchInvoiceNumberAsync(InvoiceSearchText);

                    if (results != null)
                    {
                        foreach (var suggestion in results)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            invoices.Add(suggestion);
                        }
                    }
                }

                Invoices = invoices;
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

        private async void PopulatePartNumbers()
        {
            try
            {
                PartNumbers.Clear();
                var partNumbers = await _invoiceRepository.GetPartNumbersByInvoiceAsync(SelectedInvoiceNo);
                if (partNumbers != null)
                {
                    foreach (var partNumber in partNumbers)
                    {
                        PartNumbers.Add(partNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
        }
        private async void PopulateItemDetails()
        {
            try
            {
                if (SelectedPartNo != null && SelectedInvoiceNo != null)
                {
                    SelectedInvoiceItem = await _invoiceRepository.GetInvoiceItemAsync(SelectedInvoiceNo, SelectedPartNo);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
        }

        private async void PopulateInvoiceDetails()
        {
            try
            {
                Invoice = await _invoiceRepository.GetByInvoiceNoAsync(SelectedInvoiceNo);
                if (Invoice != null)
                {
                    Customer = await _customerRepository.GetByCustomerIDAsync(Invoice.CustomerID);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
        }

        private void CalculateAmount()
        {
            if (SelectedInvoiceItem != null)
            {
                Amount = Math.Round(SelectedInvoiceItem.UnitPrice * Qty * (100 - SelectedInvoiceItem.Discount) / 100, 2);
            }
        }

        private bool CanExecuteAddReturnItemCommand()
        {
            if (Invoice == null || string.IsNullOrEmpty(SelectedInvoiceNo))
            {
                ShowErrorMessage("Please select an invoice");
                return false;
            }
            if (string.IsNullOrEmpty(SelectedPartNo) || SelectedInvoiceItem == null)
            {
                ShowErrorMessage("Please select an item");
                return false;
            }
            if (IsItemExist() == true)
            {
                ShowErrorMessage("Oops! You've Already Added This Item to the Return");
                return false;
            }
            if (Qty == 0)
            {
                ShowErrorMessage("Please enter the quantity");
                return false;
            }
            if (Qty > SelectedInvoiceItem.Quantity || DamagedQty > SelectedInvoiceItem.Quantity)
            {
                ShowErrorMessage("Invalid quantity");
                return false;
            }
            return true;
        }

        private void ExecuteAddReturnItemCommand(object obj)
        {
            if (CanExecuteAddReturnItemCommand())
                AddReturnItem();
        }

        private bool CanExecuteSaveReturnCommand()
        {
            if (Invoice == null || string.IsNullOrEmpty(SelectedInvoiceNo))
            {
                ShowErrorMessage("Please select an Invoice");
                return false;
            }
            if (ReturnItems == null || ReturnItems.Count == 0)
            {
                ShowErrorMessage("Please add at least one item to the Return");
                return false;
            }
            return true;
        }

        private void ExecuteSaveReturnCommand(object obj)
        {
            if (CanExecuteSaveReturnCommand())
                AddReturn();
        }

        private void ExecuteCancelReturnItemCommand(object obj)
        {
            SelectedReturnItem = null;
            ClearItemDetails();
            IsSelectedReturnItem = false;
            Number = Counter;
        }

        private void ExecuteEditReturnItemCommand(object obj)
        {
            foreach (var item in ReturnItems)
            {
                if (item.PartNo == SelectedPartNo)
                {
                    item.Quantity = Qty;
                    item.DamagedQty = DamagedQty;
                    Total -= item.Amount;
                    item.Amount = Amount;
                    Total += Amount;
                }
            }

            ReturnItems = new ObservableCollection<ReturnItemModel>(ReturnItems);

            SelectedReturnItem = null;
            ClearItemDetails();
            IsSelectedReturnItem = false;
            Number = Counter;
        }

        private void ExecuteDeleteReturnItemCommand(object obj)
        {
            if (SelectedReturnItem != null)
            {
                Total -= SelectedReturnItem.Amount;
                ReturnItems.Remove(SelectedReturnItem);
                Counter--;
                Number = Counter;
                ClearItemDetails();
                SelectedReturnItem = null;
                IsSelectedReturnItem = false;

                int newNumber = 1;
                foreach (var item in ReturnItems)
                {
                    item.No = newNumber++;
                    OnPropertyChanged(nameof(item.No));
                }

                OnPropertyChanged(nameof(ReturnItems));
                ReturnItems = new ObservableCollection<ReturnItemModel>(ReturnItems);
            }
            else
            {
                ShowErrorMessage("Please select a item to delete");
            }
        }

        private bool IsItemExist()
        {
            return ReturnItems.Any(item => item.PartNo == SelectedPartNo);
        }

        private void ExecuteClearReturnCommand(object obj)
        {
            ClearReturn();
        }

        private void ClearItemDetails()
        {
            SelectedInvoiceItem = null;
            SelectedPartNo = null;
            Qty = 0;
            DamagedQty = 0;
            Amount = 0;
        }

        private void ClearReturn()
        {
            ClearItemDetails();
            Invoice = null;
            Customer = null;
            SelectedInvoiceNo = null;
            ReturnItems.Clear();
            Total = 0;
            Counter = 1;
            Number = 1;
            Initialize();
        }

        private void AddReturnItem()
        {
            var returnItem = new ReturnItemModel
            {
                No = Number,
                PartNo = SelectedInvoiceItem.PartNo,
                BuyingPrice = SelectedInvoiceItem.BuyingPrice,
                UnitPrice = SelectedInvoiceItem.UnitPrice,
                Quantity = Qty,
                DamagedQty = DamagedQty,
                Discount = SelectedInvoiceItem.Discount,
                Amount = Amount,
            };

            ReturnItems.Add(returnItem);
            Total += Amount;
            Counter++;
            Number = Counter;
            ClearItemDetails();
        }

        private async void AddReturn()
        {
            try
            {
                var returnReceipt = new ReturnModel
                {
                    ReturnNo = ReturnNo,
                    InvoiceNo = SelectedInvoiceNo,
                    CustomerID = Invoice.CustomerID,
                    RepID = Invoice.RepID == "None" ? null : Invoice.RepID,
                    Date = DateTime.Now,
                    TotalAmount = Total,
                };

                bool success = await _returnRepository.AddReturnAsync(returnReceipt);

                foreach (var returnItem in ReturnItems)
                {
                    returnItem.ReturnNo = ReturnNo;
                    await _returnRepository.AddReturnItemAsync(returnItem, SelectedInvoiceNo);
                }

/*                    InvoiceDocument invoiceDoc = new InvoiceDocument();
                invoiceDoc.GenerateInvoicePDF(InvoiceNo, SelectedCustomer, invoice, InvoiceItems);*/

                if(success == true)
                {
                    ClearReturn();
                    Messenger.Default.Send("NewInvoiceAdded");
                    MessageBox.Show("Return saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to save the Return!", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the return. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
