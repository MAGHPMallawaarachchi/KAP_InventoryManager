using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    public class InvoicesViewModel : ViewModelBase
    {
        private readonly ICustomerRepository CustomerRepository;
        private readonly IInvoiceRepository InvoiceRepository;

        private IEnumerable<InvoiceModel> _invoices;
        private string _invoiceSearchText;
        private InvoiceModel _selectedInvoice;
        private InvoiceModel _currentInvoice;
        private CustomerModel _customer;
        private IEnumerable<InvoiceItemModel> _invoiceItems;

        public IEnumerable<InvoiceModel> Invoices
        {
            get => _invoices; 
            set
            {
                _invoices = value;
                OnPropertyChanged(nameof(Invoices));

                SelectedInvoice = Invoices.FirstOrDefault();
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

        public IEnumerable<InvoiceItemModel> InvoiceItems
        {
            get => _invoiceItems;
            set
            {
                _invoiceItems = value;
                OnPropertyChanged(nameof(InvoiceItems));
            }
        }

        public ICommand CancelInvoiceCommand { get; }

        public InvoicesViewModel()
        {
            CancelInvoiceCommand = new ViewModelCommand(ExecuteCancelInvoiceCommand);

            CustomerRepository = new CustomerRepository();
            InvoiceRepository = new InvoiceRepository();

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
                        await InvoiceRepository.CancelInvoice(CurrentInvoice.InvoiceNo);
                        MessageBox.Show("Invoice cancelled successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
            try
            {
                if (InvoiceSearchText == null)
                    Invoices = await InvoiceRepository.GetAllInvoicesAsync();
                else
                    Invoices = await InvoiceRepository.SearchInvoiceListAsync(InvoiceSearchText);

            }catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateInvoiceDetails()
        {
            try
            {
                if( SelectedInvoice != null)
                {
                    CurrentInvoice = await InvoiceRepository.GetByInvoiceNoAsync(SelectedInvoice.InvoiceNo);

                    if (CurrentInvoice != null)
                    {
                        Customer = await CustomerRepository.GetByCustomerIDAsync(CurrentInvoice.CustomerID);
                        InvoiceItems = await InvoiceRepository.GetInvoiceItems(CurrentInvoice.InvoiceNo);
                    }
                }
            }catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
