using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using KAP_InventoryManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class DownloadSalesReportModalViewModel : ViewModelBase
    {
        private DateTime _startDate;
        private DateTime _endDate;

        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserRepository _userRepository;

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public ICommand DownloadReportCommand { get; }
        public ICommand DiscardCommand { get; }

        public DownloadSalesReportModalViewModel()
        {
            _invoiceRepository = new InvoiceRepository();
            _userRepository = new UserRepository();

            Initialize();

            DownloadReportCommand = new ViewModelCommand(ExecuteDownloadReportCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private void Initialize()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
        }

        private async Task<string> GetPathAsync()
        {
            string path = string.Empty;
            try
            {
                path = await Task.Run(() => _userRepository.GetPath(Thread.CurrentPrincipal.Identity.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the path. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return path;
        }

        private void ExecuteDiscardCommand(object obj)
        {
            Initialize();
        }

        private async void ExecuteDownloadReportCommand(object obj)
        {
            try
            {
                if (StartDate != null && EndDate != null)
                {
                    var SalesReport = new SalesReport();
                    string path = await GetPathAsync();
                    string month = EndDate.ToString("MMMM yyyy");
                    var invoices = await _invoiceRepository.GetSalesReportAsync(StartDate, EndDate);
                   
                    SalesReport.GenerateSalesReportPDF(invoices, path, month);

                    Initialize();
                    Messenger.Default.Send(new NotificationMessage("CloseDialog"));
                    MessageBox.Show("The report was saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
