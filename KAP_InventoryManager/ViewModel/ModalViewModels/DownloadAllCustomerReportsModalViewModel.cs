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
using System.Runtime.InteropServices.ComTypes;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class DownloadAllCustomerReportsModalViewModel : ViewModelBase
    {
        private List<string> _reportTypes;
        private string _reportType;
        private DateTime _startDate;
        private DateTime _endDate;
        private bool _isLoading;

        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;

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

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand DownloadReportsCommand { get; }
        public ICommand DiscardCommand { get; }

        public DownloadAllCustomerReportsModalViewModel()
        {
            _customerRepository = new CustomerRepository();
            _userRepository = new UserRepository();

            ReportTypes = new List<string> { "all", "only paid", "only pending or overdue" };
            Initialize();

            DownloadReportsCommand = new ViewModelCommand(ExecuteDownloadReportsCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private void Initialize()
        {
            ReportType = ReportTypes[0];
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

        private async void ExecuteDownloadReportsCommand(object obj)
        {
            IsLoading = true;
            try
            {
                var customerReport = new CustomerReport();
                string path = await GetPathAsync();
                string month = StartDate.ToString("MMMM yyyy");

                // Fetch the list of customers
                var customers = await _customerRepository.GetCustomersFromInvoice(StartDate, EndDate);

                // Run the report generation in a background task
                await Task.Run(async () =>
                {
                    var tasks = customers.Select(async customer =>
                    {
                        var customerModel = await _customerRepository.GetByCustomerIDAsync(customer);
                        var invoices = await _customerRepository.GetCustomerInvoices(customer, StartDate, EndDate, ReportType);
                        var returns = await _customerRepository.GetCustomerReturns(customer, StartDate, EndDate);
                        customerReport.GenerateCustomerReportPDF(customerModel, invoices, returns, path, month, ReportType, StartDate, EndDate);
                    });

                    await Task.WhenAll(tasks);
                });

                Initialize();
                Messenger.Default.Send(new NotificationMessage("CloseDialog"));
                MessageBox.Show("The reports were saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download all customer reports. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}
