using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class ConfirmPaymentModalViewModel : ViewModelBase
    {
        private List<string> _paymentTypes = new List<string> { "DEPOSIT", "CHEQUE" };
        private InvoiceCustomerModel _payment = new InvoiceCustomerModel();
        private InvoiceModel _invoice;

        private readonly IInvoiceCustomerRepository _invoiceCustomerRepository;

        public List<string> PaymentTypes
        {
            get => _paymentTypes;
            set
            {
                _paymentTypes = value;
                OnPropertyChanged(nameof(PaymentTypes));
            }
        }

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

        public ICommand ConfirmPaymentCommand { get; }
        public ICommand DiscardCommand { get; }

        public ConfirmPaymentModalViewModel()
        {
            _invoiceCustomerRepository = new InvoiceCustomerRepository();

            // Register to receive the InvoiceModel
            Messenger.Default.Register<InvoiceModel>(this, OnMessageReceived);

            // Send the "RequestInvoice" message to request the current invoice
            Messenger.Default.Send("RequestInvoice");

            ConfirmPaymentCommand = new ViewModelCommand(ExecuteConfirmPaymentCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private void OnMessageReceived(InvoiceModel invoice)
        {
            if (invoice != null)
            {
                Invoice = invoice;

                Payment.InvoiceNo = Invoice.InvoiceNo;
                Payment.CustomerId = Invoice.CustomerID;
                Payment.Date = DateTime.Now;
            }
            else
            {
                Console.WriteLine("Received null invoice");
            }
        }

        private void ExecuteDiscardCommand(object obj)
        {
            ResetTextBoxes();
        }

        private async void ExecuteConfirmPaymentCommand(object obj)
        {
            try
            {
                if (Invoice != null && Invoice.Status == "Pending")
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to confirm payment?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        InvoiceCustomerModel payment = new InvoiceCustomerModel
                        {
                            CustomerId = Invoice.CustomerID,
                            InvoiceNo = Invoice.InvoiceNo,
                            PaymentType = Payment.PaymentType,
                            ChequeNo = Payment.ChequeNo,
                            Bank = Payment.Bank,
                            Date = Payment.Date,
                        };

                        await _invoiceCustomerRepository.ConfirmPaymentAsync(payment);

                        // Close the dialog view
                        Messenger.Default.Send(new NotificationMessage("CloseDialog"));

                        // Send the edited item back to the details view
                        Messenger.Default.Send("PaymentConfirmed");
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

        private void ResetTextBoxes()
        {
            Payment.ChequeNo = "";
            Payment.Bank = "";
            Payment.Date = DateTime.Now;
        }
    }
}
