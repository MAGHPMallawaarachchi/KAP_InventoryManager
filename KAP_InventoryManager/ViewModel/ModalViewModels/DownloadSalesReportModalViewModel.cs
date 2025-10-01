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
        private List<string> _reportTypes;
        private string _reportType;
        private List<string> _fileTypes;
        private string _fileType;
        private bool _isLoading;

        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReturnRepository _returnRepository;

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

        public List<string> ReportTypes
        {
            get => _reportTypes;
            set
            {
                _reportTypes = value;
                OnPropertyChanged(nameof(ReportTypes));
            }
        }

        public string ReportType
        {
            get => _reportType;
            set
            {
                _reportType = value;
                OnPropertyChanged(nameof(ReportType));
            }
        }

        public List<string> FileTypes
        {
            get => _fileTypes;
            set
            {
                _fileTypes = value;
                OnPropertyChanged(nameof(FileTypes));
            }
        }

        public string FileType
        {
            get => _fileType;
            set
            {
                _fileType = value;
                OnPropertyChanged(nameof(FileType));
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

        public ICommand DownloadReportCommand { get; }
        public ICommand DiscardCommand { get; }

        public DownloadSalesReportModalViewModel()
        {
            _invoiceRepository = new InvoiceRepository();
            _userRepository = new UserRepository();
            _returnRepository = new ReturnRepository();

            ReportTypes = new List<string> { "all", "only paid", "only pending or overdue" };
            FileTypes = new List<string> { "PDF", "Excel" };
            Initialize();

            DownloadReportCommand = new ViewModelCommand(ExecuteDownloadReportCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private void Initialize()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            ReportType = ReportTypes[0];
            FileType = FileTypes[0];
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
            IsLoading = true;
            try
            {
                if (StartDate != null && EndDate != null && FileType != null)
                {
                    var SalesReport = new SalesReport();
                    string path = await GetPathAsync();
                    string month = StartDate.ToString("MMMM yyyy");
                    var invoices = await _invoiceRepository.GetSalesReportAsync(StartDate, EndDate, ReportType);
                    var returns = await _returnRepository.GetReturns(StartDate, EndDate);

                    if (FileType == "PDF")
                        SalesReport.GenerateSalesReportPDF(invoices, returns, path, month, ReportType, StartDate, EndDate);
                    else
                        SalesReport.GenerateSalesReportExcel(invoices, returns,path, month, ReportType, StartDate, EndDate);

                    Initialize();
                    Messenger.Default.Send(new NotificationMessage("CloseDialog"));
                    MessageBox.Show("The report was saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
