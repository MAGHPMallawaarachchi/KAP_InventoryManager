using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public InvoicesViewModel()
        {
            CustomerRepository = new CustomerRepository();
            InvoiceRepository = new InvoiceRepository();

            PopulateInvoicesAsync();
        }

        private async void PopulateInvoicesAsync()
        {
            try
            {
                if (InvoiceSearchText == null)
                    Invoices = await InvoiceRepository.GetAllInvoicesAsync();
                else
                    Invoices = await InvoiceRepository.SearchInvoiceListAsync(InvoiceSearchText);

            }catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch invoices. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateInvoiceDetails()
        {
            try
            {
                if( SelectedInvoice != null)
                {
                    CurrentInvoice = await InvoiceRepository.GetByInvoiceNo(SelectedInvoice.InvoiceNo);

                    if (CurrentInvoice != null)
                    {
                        Customer = CustomerRepository.GetByCustomerID(CurrentInvoice.CustomerID);
                        InvoiceItems = await InvoiceRepository.GetInvoiceItems(CurrentInvoice.InvoiceNo);
                    }
                }
            }catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch invoice details. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
