using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class ViewPaymentModalViewModel : ViewModelBase
    {
        private InvoiceCustomerModel _payment = new InvoiceCustomerModel();
        private InvoiceModel _invoice;

        private readonly IInvoiceCustomerRepository _invoiceCustomerRepository;

        public InvoiceCustomerModel Payment
        {
            get => _payment;
            set
            {
                _payment = value;
                OnPropertyChanged(nameof(Payment));
            }
        }

        public InvoiceModel Invoice
        {
            get => _invoice;
            set
            {
                _invoice = value;
                OnPropertyChanged(nameof(Invoice));
            }
        }

        public ViewPaymentModalViewModel()
        {
            _invoiceCustomerRepository = new InvoiceCustomerRepository();

            // Register to receive the InvoiceModel
            Messenger.Default.Register<InvoiceModel>(this, OnMessageReceived);

            // Send the "RequestInvoice" message to request the current invoice
            Messenger.Default.Send("RequestInvoice");
        }

        private async void OnMessageReceived(InvoiceModel invoice)
        {
            if (invoice != null)
            {
                Invoice = invoice;
                Payment = await _invoiceCustomerRepository.GetAsync(Invoice.InvoiceNo, Invoice.CustomerID);                
            }
            else
            {
                Console.WriteLine("Received null invoice");
            }
        }

    }
}
