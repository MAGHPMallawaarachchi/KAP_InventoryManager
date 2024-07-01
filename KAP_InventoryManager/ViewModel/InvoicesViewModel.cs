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
using Xceed.Wpf.Toolkit.Primitives;

namespace KAP_InventoryManager.ViewModel
{
    public class InvoicesViewModel : ViewModelBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<InvoiceModel> _invoices;
        private IEnumerable<InvoiceItemModel> _invoiceItems;
        private string _invoiceSearchText;
        private InvoiceModel _selectedInvoice;
        private InvoiceModel _currentInvoice;
        private CustomerModel _customer;

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

                PopulateInvoicesAsync();
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

        public CustomerModel Customer
        {
            get => _customer;
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
            }
        }


        public ICommand CancelInvoiceCommand { get; }

        public InvoicesViewModel()
        {
            _customerRepository = new CustomerRepository();
            _invoiceRepository = new InvoiceRepository();

            CancelInvoiceCommand = new ViewModelCommand(ExecuteCancelInvoiceCommand);

            Invoices = new ObservableCollection<InvoiceModel>();

            PopulateInvoicesAsync();

            Messenger.Default.Register<string>(this, OnMessageReceived);
        }

        private void OnMessageReceived(string obj)
        {
            PopulateInvoicesAsync();
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

            try
            {

                var invoices = (string.IsNullOrEmpty(InvoiceSearchText)
                    ? await _invoiceRepository.GetAllInvoicesAsync()
                    : await _invoiceRepository.SearchInvoiceListAsync(InvoiceSearchText));

                var invoicesList = new List<InvoiceModel>(invoices);

                Invoices.Clear();

                foreach (var invoice in invoicesList)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    Invoices.Add(invoice);
                    await Task.Delay(0, _cancellationTokenSource.Token);
                }

                if (Invoices.Any())
                {
                    SelectedInvoice = Invoices.First();
                }

            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, ignore the exception
            }
            finally
            {
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
                        InvoiceItems = await _invoiceRepository.GetInvoiceItemsAsync(CurrentInvoice.InvoiceNo);
                    }
                }
            }catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
