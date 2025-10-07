using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class ConfirmPaymentModalViewModel : ViewModelBase
    {
        private List<string> _paymentTypes = new List<string> { "DEPOSIT", "CHEQUE", "CASH" };
        private PaymentModel _payment = new PaymentModel();
        private InvoiceModel _invoice;
        private PaymentSummaryModel _paymentSummary;

        private readonly IPaymentRepository _paymentRepository;

        public List<string> PaymentTypes
        {
            get => _paymentTypes;
            set
            {
                _paymentTypes = value;
                OnPropertyChanged(nameof(PaymentTypes));
            }
        }

        public PaymentModel Payment
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

        public PaymentSummaryModel PaymentSummary
        {
            get => _paymentSummary;
            set
            {
                _paymentSummary = value;
                OnPropertyChanged(nameof(PaymentSummary));
            }
        }

        public ICommand ConfirmPaymentCommand { get; }
        public ICommand DiscardCommand { get; }

        public ConfirmPaymentModalViewModel()
        {
            _paymentRepository = new PaymentRepository();

            // Register to receive the InvoiceModel
            Messenger.Default.Register<InvoiceModel>(this, OnMessageReceived);

            // Send the "RequestInvoice" message to request the current invoice
            Messenger.Default.Send("RequestInvoice");

            ConfirmPaymentCommand = new ViewModelCommand(ExecuteConfirmPaymentCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private async void OnMessageReceived(InvoiceModel invoice)
        {
            if (invoice != null)
            {
                Invoice = invoice;

                Payment.InvoiceNo = Invoice.InvoiceNo;
                Payment.CustomerId = Invoice.CustomerID;
                Payment.Date = DateTime.Now;

                // Load payment summary to show remaining balance
                await LoadPaymentSummary();
            }
            else
            {
                Console.WriteLine("Received null invoice");
            }
        }

        private async Task LoadPaymentSummary()
        {
            if (Invoice != null)
            {
                PaymentSummary = await _paymentRepository.GetPaymentSummaryAsync(Invoice.InvoiceNo);
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
                if (Invoice != null)
                {
                    // Validation
                    if (Payment.Amount <= 0)
                    {
                        MessageBox.Show("Payment amount must be greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(Payment.PaymentType))
                    {
                        MessageBox.Show("Please select a payment type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Check if payment exceeds remaining balance
                    if (PaymentSummary != null && Payment.Amount > PaymentSummary.RemainingBalance)
                    {
                        MessageBoxResult confirmOverpayment = MessageBox.Show(
                            $"Payment amount ({Payment.Amount:C}) exceeds remaining balance ({PaymentSummary.RemainingBalance:C}).\n\nMaximum allowed: {PaymentSummary.RemainingBalance:C}",
                            "Payment Exceeds Balance",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    MessageBoxResult result = MessageBox.Show("Are you sure you want to add this payment?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        PaymentModel payment = new PaymentModel
                        {
                            CustomerId = Invoice.CustomerID,
                            InvoiceNo = Invoice.InvoiceNo,
                            PaymentType = Payment.PaymentType,
                            ReceiptNo = Payment.ReceiptNo,
                            ChequeNo = Payment.ChequeNo,
                            Bank = Payment.Bank,
                            Amount = Payment.Amount,
                            Date = Payment.Date,
                            Comment = Payment.Comment,
                        };

                        int paymentId = await _paymentRepository.AddPaymentAsync(payment);

                        if (paymentId > 0)
                        {
                            // Close the dialog view
                            Messenger.Default.Send(new NotificationMessage("CloseDialog"));

                            // Send notification that payment was added
                            Messenger.Default.Send("PaymentConfirmed");
                        }
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
            Payment.PaymentType = null;
            Payment.ChequeNo = string.Empty;
            Payment.Bank = string.Empty;
            Payment.Date = DateTime.Now;
            Payment.Comment = string.Empty;
            Payment.ReceiptNo = string.Empty;
            Payment.Amount = 0;
        }
    }
}
