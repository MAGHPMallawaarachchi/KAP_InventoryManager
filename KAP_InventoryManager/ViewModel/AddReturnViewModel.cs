using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using QuestPDF.ExampleInvoice;
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

namespace KAP_InventoryManager.ViewModel
{
    public class AddReturnViewModel: ViewModelBase
    {
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

        private readonly ICustomerRepository CustomerRepository;
        private readonly ISalesRepRepository SalesRepRepository;
        private readonly IItemRepository ItemRepository;
        private readonly IInvoiceRepository InvoiceRepository;
        private readonly IReturnRepository ReturnRepository;

        public ObservableCollection<string> PartNumbers { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Invoices { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<ReturnItemModel> ReturnItems
        {
            get => _returnItems;
            set
            {
                _returnItems = value;
                OnPropertyChanged(nameof(_returnItems));
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
            get { return _invoiceSearchText; }
            set
            {
                _invoiceSearchText = value;
                OnPropertyChanged(nameof(InvoiceSearchText));
                PopulateInvoices();
            }
        }
        public string PartNoSearchText
        {
            get { return _partNoSearchText; }
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

        public ICommand AddReturnItemCommand { get; }
        public ICommand ClearReturnCommand { get; }
        public ICommand DeleteReturnItemCommand { get; }
        public ICommand EditReturnItemCommand { get; }
        public ICommand CancelReturnItemCommand { get; }
        public ICommand SaveReturnCommand { get; }



        public AddReturnViewModel()
        {
            CustomerRepository = new CustomerRepository();
            SalesRepRepository = new SalesRepRepository();
            ItemRepository = new ItemRepository();
            InvoiceRepository = new InvoiceRepository();
            ReturnRepository = new ReturnRepository();

            ReturnNo = ReturnRepository.GetNextReturnNumber();
            ReturnItems = new ObservableCollection<ReturnItemModel>();

            AddReturnItemCommand = new ViewModelCommand(ExecuteAddReturnItemCommand);
            ClearReturnCommand = new ViewModelCommand(ExecuteClearReturnCommand);
            DeleteReturnItemCommand = new ViewModelCommand(ExecuteDeleteReturnItemCommand);
            EditReturnItemCommand = new ViewModelCommand(ExecuteEditReturnItemCommand);
            CancelReturnItemCommand = new ViewModelCommand(ExecuteCancelReturnItemCommand);
            SaveReturnCommand = new ViewModelCommand(ExecuteSaveReturnCommand);

            DateTime currentDateTime = DateTime.Now;
            CurrentDate = currentDateTime.ToString("yyyy-MM-dd");
            CurrentTime = currentDateTime.ToString("t");

            PopulateInvoices();

            Number = Counter = 1;
            Total = 0;
            IsSelectedReturnItem = false;
        }

        private void ExecuteSaveReturnCommand(object obj)
        {
            AddReturn();
        }

        private void ExecuteCancelReturnItemCommand(object obj)
        {
            SelectedReturnItem = null;
            Clear();
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
            Clear();
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
                Clear();
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
                MessageBox.Show("Please select a item to delete", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddReturnItem()
        {
            bool validate;
            if (Invoice == null || string.IsNullOrEmpty(SelectedInvoiceNo))
            {
                validate = false;
                MessageBox.Show("Please select an invoice", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrEmpty(SelectedPartNo) || SelectedInvoiceItem == null)
            {
                validate = false;
                MessageBox.Show("Please select an item", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (IsItemExist() == true)
            {
                validate = false;
                MessageBox.Show("Oops! You've Already Added This Item to the Return", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Qty == 0)
            {
                validate = false;
                MessageBox.Show("Please enter the quantity", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if(Qty > SelectedInvoiceItem.Quantity || DamagedQty > SelectedInvoiceItem.Quantity)
            {
                validate = false;
                MessageBox.Show("Invalid quantity", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                validate = true;
            return validate;
        }

        private bool CanAddReturn()
        {
            bool validate;
            if (Invoice == null || string.IsNullOrEmpty(SelectedInvoiceNo))
            {
                validate = false;
                MessageBox.Show("Please select an Invoice", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (ReturnItems == null || ReturnItems.Count == 0)
            {
                validate = false;
                MessageBox.Show("Please add at least one item to the Return", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                validate = true;
            return validate;
        }

        private bool IsItemExist()
        {
            bool isItemExist = false;
            foreach (var item in ReturnItems)
            {
                if (item.PartNo == SelectedPartNo)
                {
                    isItemExist = true;
                }
            }
            return isItemExist;
        }

        private void ExecuteClearReturnCommand(object obj)
        {
            ClearReturn();
        }

        private void ExecuteAddReturnItemCommand(object obj)
        {
            if (CanAddReturnItem() == true)
                AddReturnItem();
        }

        private void PopulateInvoices()
        {
            try
            {
                Invoices.Clear();

                if (InvoiceSearchText != null || InvoiceSearchText != "")
                {
                    var results = InvoiceRepository.SearchInvoiceNumber(InvoiceSearchText);

                    if (results != null)
                    {
                        foreach (var suggestion in results)
                        {
                            Invoices.Add(suggestion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulatePartNumbers()
        {
            try
            {
                PartNumbers.Clear();
                var partNumbers = InvoiceRepository.GetPartNumbersByInvoice(SelectedInvoiceNo);
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
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            Clear();
        }

        private async void PopulateItemDetails()
        {
            try
            {
                if(SelectedPartNo != null && SelectedInvoiceNo != null)
                {
                    SelectedInvoiceItem = await InvoiceRepository.GetInvoiceItem(SelectedInvoiceNo, SelectedPartNo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateInvoiceDetails()
        {
            try
            {
                Invoice = InvoiceRepository.GetByInvoiceNo(SelectedInvoiceNo);
                if (Invoice != null)
                {
                    Customer = await CustomerRepository.GetByCustomerIDAsync(Invoice.CustomerID);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateAmount()
        {
            if (SelectedInvoiceItem != null)
            {
                Amount = SelectedInvoiceItem.UnitPrice * Qty * (100 - SelectedInvoiceItem.Discount) / 100;
            }
        }

        private void Clear()
        {
            SelectedInvoiceItem = null;
            SelectedPartNo = null;
            Qty = 0;
            DamagedQty = 0;
            Amount = 0;
        }

        private void ClearReturn()
        {
            Clear();
            Invoice = null;
            Customer = null;
            SelectedInvoiceNo = null;
            ReturnItems.Clear();
            Total = 0;
            Counter = 1;
            Number = 1;
            ReturnNo = ReturnRepository.GetNextReturnNumber();
        }

        private void AddReturn()
        {
            try
            {
                if (CanAddReturn() == true)
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

                    bool success = ReturnRepository.AddReturn(returnReceipt);

                    foreach (var returnItem in ReturnItems)
                    {
                        returnItem.ReturnNo = ReturnNo;
                        ReturnRepository.AddReturnItem(returnItem, SelectedInvoiceNo);
                    }

/*                    InvoiceDocument invoiceDoc = new InvoiceDocument();
                    invoiceDoc.GenerateInvoicePDF(InvoiceNo, SelectedCustomer, invoice, InvoiceItems);*/

                    if(success == true)
                    {
                        ClearReturn();
                        MessageBox.Show("Return saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to save the Return!", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the return. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
