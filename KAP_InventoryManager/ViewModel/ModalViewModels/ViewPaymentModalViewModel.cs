using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class ViewPaymentModalViewModel : ViewModelBase
    {
        private ObservableCollection<PaymentModel> _payments;
        private PaymentModel _selectedPayment;
        private InvoiceModel _invoice;
        private PaymentSummaryModel _paymentSummary;
        private bool _isTableReadOnly = false;

        private readonly IPaymentRepository _paymentRepository;

        public ObservableCollection<PaymentModel> Payments
        {
            get => _payments;
            set
            {
                _payments = value;
                OnPropertyChanged(nameof(Payments));
            }
        }

        public PaymentModel SelectedPayment
        {
            get => _selectedPayment;
            set
            {
                _selectedPayment = value;
                OnPropertyChanged(nameof(SelectedPayment));
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

        public bool IsTableReadOnly
        {
            get => _isTableReadOnly;
            set
            {
                _isTableReadOnly = value;
                OnPropertyChanged(nameof(IsTableReadOnly));
            }
        }

        public ICommand AddPaymentCommand { get; }
        public ICommand EditPaymentCommand { get; }
        public ICommand DeletePaymentCommand { get; }
        public ICommand RefreshCommand { get; }

        public ViewPaymentModalViewModel()
        {
            _paymentRepository = new PaymentRepository();
            Payments = new ObservableCollection<PaymentModel>();

            // Register to receive the InvoiceModel
            Messenger.Default.Register<InvoiceModel>(this, OnMessageReceived);

            // Listen for payment updates
            Messenger.Default.Register<string>(this, OnNotificationReceived);

            // Send the "RequestInvoice" message to request the current invoice
            Messenger.Default.Send("RequestInvoice");

            AddPaymentCommand = new ViewModelCommand(ExecuteAddPaymentCommand);
            EditPaymentCommand = new ViewModelCommand(ExecuteEditPaymentCommand);
            DeletePaymentCommand = new ViewModelCommand(ExecuteDeletePaymentCommand, CanExecuteEditOrDelete);
            RefreshCommand = new ViewModelCommand(ExecuteRefreshCommand);
        }

        private async void OnMessageReceived(InvoiceModel invoice)
        {
            if (invoice != null)
            {
                Invoice = invoice;
                await LoadPayments();
                await LoadPaymentSummary();
            }
            else
            {
                Console.WriteLine("Received null invoice");
            }
        }

        private async void OnNotificationReceived(string message)
        {
            if (message == "PaymentConfirmed" || message == "PaymentUpdated" || message == "PaymentDeleted")
            {
                await LoadPayments();
                await LoadPaymentSummary();
            }
            else if (message == "RequestPaymentForEdit")
            {
                // Send selected payment for editing
                if (SelectedPayment != null)
                {
                    Messenger.Default.Send(SelectedPayment);
                }
            }
            else if (message == "RequestInvoice")
            {
                // Send the current invoice to child modals
                if (Invoice != null)
                {
                    Messenger.Default.Send(Invoice);
                }
            }
        }

        private async Task LoadPayments()
        {
            if (Invoice != null)
            {
                var payments = await _paymentRepository.GetPaymentsByInvoiceAsync(Invoice.InvoiceNo, Invoice.CustomerID);

                Payments.Clear();
                if (payments != null)
                {
                    foreach (var payment in payments)
                    {
                        Payments.Add(payment);
                    }
                }
            }
        }

        private async Task LoadPaymentSummary()
        {
            if (Invoice != null)
            {
                PaymentSummary = await _paymentRepository.GetPaymentSummaryAsync(Invoice.InvoiceNo);
            }
        }

        private void ExecuteAddPaymentCommand(object obj)
        {
            // Send message to open Add Payment modal
            Messenger.Default.Send("OpenAddPaymentModal");
        }

        private bool CanExecuteEditOrDelete(object obj)
        {
            return SelectedPayment != null;
        }

        private async void ExecuteEditPaymentCommand(object obj)
        {
            // Save all payments in the collection
            if (Payments == null || Payments.Count == 0)
            {
                MessageBox.Show("No payments to save.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                foreach (var payment in Payments)
                {
                    await _paymentRepository.UpdatePaymentAsync(payment);
                }

                // Reload data to ensure consistency
                await LoadPayments();
                await LoadPaymentSummary();

                // Notify parent view
                Messenger.Default.Send("PaymentUpdated");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteDeletePaymentCommand(object obj)
        {
            if (SelectedPayment != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete this payment of {SelectedPayment.Amount:C}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await _paymentRepository.DeletePaymentAsync(SelectedPayment.PaymentID);

                    // Reload payments and summary
                    await LoadPayments();
                    await LoadPaymentSummary();

                    // Notify parent view
                    Messenger.Default.Send("PaymentDeleted");
                }
            }
        }

        private async void ExecuteRefreshCommand(object obj)
        {
            await LoadPayments();
            await LoadPaymentSummary();
        }
    }
}
