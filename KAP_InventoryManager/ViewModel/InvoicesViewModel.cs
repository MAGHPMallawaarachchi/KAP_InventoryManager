using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.Primitives;

namespace KAP_InventoryManager.ViewModel
{
    public class InvoicesViewModel : ViewModelBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;
        private DispatcherTimer _searchDebounceTimer;

        private ObservableCollection<InvoiceModel> _invoices;
        private IEnumerable<InvoiceItemModel> _invoiceItems;
        private string _invoiceSearchText;
        private InvoiceModel _selectedInvoice;
        private InvoiceModel _currentInvoice;
        private InvoiceModel _previousInvoice;
        private CustomerModel _customer;
        private Timer _timer;
        private bool _isLoading;

        public ObservableCollection<InvoiceModel> Invoices
        {
            get => _invoices; 
            set
            {
                _invoices = value;
                OnPropertyChanged(nameof(Invoices));
            }
        }

        public IEnumerable<InvoiceItemModel> InvoiceItems
        {
            get => _invoiceItems;
            set
            {
                _invoiceItems = value;
                OnPropertyChanged(nameof(InvoiceItems));
            }
        }

        public string InvoiceSearchText
        {
            get => _invoiceSearchText;
            set
            {
                _invoiceSearchText = value;
                OnPropertyChanged(nameof(InvoiceSearchText));

                // Debounce search - wait 300ms after user stops typing
                _searchDebounceTimer?.Stop();
                _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
                _searchDebounceTimer.Tick += (s, e) =>
                {
                    _searchDebounceTimer.Stop();
                    PopulateInvoicesAsync();
                };
                _searchDebounceTimer.Start();
            }
        }

        public InvoiceModel SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                _selectedInvoice = value;
                OnPropertyChanged(nameof(SelectedInvoice));
                PopulateInvoiceDetails();
            }
        }

        public InvoiceModel CurrentInvoice
        {
            get => _currentInvoice;
            set
            {
                _currentInvoice = value;
                OnPropertyChanged(nameof(CurrentInvoice));
            }
        }

        public InvoiceModel PreviousInvoice
        {
            get => _previousInvoice;
            set
            {
                _previousInvoice = value;
                OnPropertyChanged(nameof(PreviousInvoice));
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

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand CancelInvoiceCommand { get; }

        public InvoicesViewModel()
        {
            _customerRepository = new CustomerRepository();
            _invoiceRepository = new InvoiceRepository();

            CancelInvoiceCommand = new ViewModelCommand(ExecuteCancelInvoiceCommand);
            Invoices = new ObservableCollection<InvoiceModel>();

            SetUpDailyTimer();
            PopulateInvoicesAsync();

            Messenger.Default.Register<string>(this, OnMessageReceived);
        }

        private void OnMessageReceived(string message)
        {
            if (message == "NewInvoiceAdded" || message == "PaymentConfirmed")
            {
                PopulateInvoicesAsync();
            }
            else if(message == "RequestInvoice")
            {
                Messenger.Default.Send(CurrentInvoice);
            }
        }

        private async void ExecuteCancelInvoiceCommand(object obj)
        {
            try
            {
                if (CurrentInvoice != null)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to cancel this invoice?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _invoiceRepository.CancelInvoiceAsync(CurrentInvoice.InvoiceNo);
                        MessageBox.Show("Invoice cancelled successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    PopulateInvoicesAsync();
                }
                else
                {
                    MessageBox.Show($"No invoice selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateInvoicesAsync()
        {
            await _semaphore.WaitAsync();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if(CurrentInvoice != null)
            {
                PreviousInvoice = CurrentInvoice;
            }

            IsLoading = true;

            try
            {
                var invoices = (string.IsNullOrEmpty(InvoiceSearchText)
                    ? await _invoiceRepository.GetPastTwoDaysInvoicesAsync()
                    : await _invoiceRepository.SearchInvoiceListAsync(InvoiceSearchText));

                var invoicesList = invoices.ToList();

                // Smart update - only add/remove changed items
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateCollection(Invoices, invoicesList);
                });

                if(PreviousInvoice != null)
                {
                    SelectedInvoice = Invoices.FirstOrDefault(inv => inv.InvoiceNo == PreviousInvoice.InvoiceNo);
                }
                else if (Invoices.Any())
                {
                    SelectedInvoice = Invoices.First();
                }

            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, ignore the exception
            }
            finally
            {
                IsLoading = false;
                _semaphore.Release();
            }
        }

        private async void PopulateInvoiceDetails()
        {
            try
            {
                if( SelectedInvoice != null)
                {
                    CurrentInvoice = await _invoiceRepository.GetByInvoiceNoAsync(SelectedInvoice.InvoiceNo);

                    if (CurrentInvoice != null)
                    {
                        Customer = await _customerRepository.GetByCustomerIDAsync(CurrentInvoice.CustomerID);

                        var items = await _invoiceRepository.GetInvoiceItemsAsync(CurrentInvoice.InvoiceNo);
                        if (items != null)
                        {
                            InvoiceItems = new ObservableCollection<InvoiceItemModel>(items);
                        }
                        else
                        {
                            MessageBox.Show("No items returned from the database.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetUpDailyTimer()
        {
            DateTime now = DateTime.Now;
            DateTime nextRun = new DateTime(now.Year, now.Month, now.Day, 0, 1, 0);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            TimeSpan initialDelay = nextRun - now;
            _timer = new Timer(async _ => await _invoiceRepository.UpdateOverdueInvoices(), null, initialDelay, TimeSpan.FromHours(24));
        }

        private void UpdateCollection(ObservableCollection<InvoiceModel> collection, List<InvoiceModel> newItems)
        {
            // Remove items not in new list
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                var existingItem = collection[i];
                if (!newItems.Any(x => x.InvoiceNo == existingItem.InvoiceNo))
                {
                    collection.RemoveAt(i);
                }
            }

            // Add or update items
            for (int i = 0; i < newItems.Count; i++)
            {
                var newItem = newItems[i];
                var existingItem = collection.FirstOrDefault(x => x.InvoiceNo == newItem.InvoiceNo);

                if (existingItem == null)
                {
                    // Add new item at correct position
                    if (i < collection.Count)
                        collection.Insert(i, newItem);
                    else
                        collection.Add(newItem);
                }
                else if (collection.IndexOf(existingItem) != i)
                {
                    // Move item to correct position
                    collection.Move(collection.IndexOf(existingItem), i);
                }
            }
        }
    }
}
