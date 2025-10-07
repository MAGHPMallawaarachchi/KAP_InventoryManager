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
    internal class EditPaymentModalViewModel : ViewModelBase
    {
        private List<string> _paymentTypes = new List<string> { "DEPOSIT", "CHEQUE", "CASH" };
        private PaymentModel _payment = new PaymentModel();
        private InvoiceModel _invoice;
        private PaymentSummaryModel _paymentSummary;
        private decimal _originalAmount;

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

        public ICommand SavePaymentCommand { get; }
        public ICommand CancelCommand { get; }

        public EditPaymentModalViewModel()
        {
            _paymentRepository = new PaymentRepository();

            // Register to receive the PaymentModel
            Messenger.Default.Register<PaymentModel>(this, OnPaymentReceived);

            // Send the "RequestPaymentForEdit" message to request the payment
            Messenger.Default.Send("RequestPaymentForEdit");

            SavePaymentCommand = new ViewModelCommand(ExecuteSavePaymentCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);
        }

        private async void OnPaymentReceived(PaymentModel payment)
        {
            if (payment != null && payment.PaymentID > 0)
            {
                Payment = payment;
                _originalAmount = payment.Amount;

                // Load invoice details
                await LoadInvoiceAndSummary();
            }
            else
            {
                Console.WriteLine("Received null or invalid payment");
            }
        }

        private async Task LoadInvoiceAndSummary()
        {
            if (!string.IsNullOrEmpty(Payment.InvoiceNo))
            {
                // Request invoice from parent view
                Messenger.Default.Send("RequestInvoice");

                // Load payment summary
                PaymentSummary = await _paymentRepository.GetPaymentSummaryAsync(Payment.InvoiceNo);
            }
        }

        private void ExecuteCancelCommand(object obj)
        {
            // Close the dialog view
            Messenger.Default.Send(new NotificationMessage("CloseDialog"));
        }

        private async void ExecuteSavePaymentCommand(object obj)
        {
            try
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

                // Check if new amount exceeds available balance
                // Available balance = current remaining + original amount (since we're replacing it)
                if (PaymentSummary != null)
                {
                    decimal availableBalance = PaymentSummary.RemainingBalance + _originalAmount;
                    if (Payment.Amount > availableBalance)
                    {
                        MessageBox.Show(
                            $"Payment amount ({Payment.Amount:C}) exceeds available balance ({availableBalance:C}).\n\nMaximum allowed: {availableBalance:C}",
                            "Payment Exceeds Balance",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                }

                MessageBoxResult result = MessageBox.Show("Are you sure you want to update this payment?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _paymentRepository.UpdatePaymentAsync(Payment);

                    // Close the dialog view
                    Messenger.Default.Send(new NotificationMessage("CloseDialog"));

                    // Send notification that payment was updated
                    Messenger.Default.Send("PaymentUpdated");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
