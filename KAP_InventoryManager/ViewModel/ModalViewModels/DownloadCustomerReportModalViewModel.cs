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
using System.Threading;
using KAP_InventoryManager.Utils;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class DownloadCustomerReportModalViewModel : ViewModelBase
    {
        private List<string> _reportTypes;
        private string _reportType;
        private DateTime _startDate;
        private DateTime _endDate;
        private CustomerModel _customer;

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

        public CustomerModel Customer
        {
            get => _customer;
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
            }
        }

        public ICommand DownloadReportCommand { get; }
        public ICommand DiscardCommand { get; }

        public DownloadCustomerReportModalViewModel()
        {
            _customerRepository = new CustomerRepository();
            _userRepository = new UserRepository();

            ReportTypes = new List<string> { "all", "only paid", "only pending or overdue" };
            Initialize();

            // Register to receive the CustomerModel
            Messenger.Default.Register<CustomerModel>(this, OnMessageReceived);

            // Send the "RequestCustomer" message to request the current customer
            Messenger.Default.Send("RequestCustomer");

            DownloadReportCommand = new ViewModelCommand(ExecuteDownloadReportCommand);
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

        private void OnMessageReceived(CustomerModel customer)
        {
            if (customer != null)
            {
                Customer = customer;
            }

        }

        private void ExecuteDiscardCommand(object obj)
        {
            Initialize();
        }

        private async void ExecuteDownloadReportCommand(object obj)
        {
            try
            {
                if(Customer!= null && ReportType != null && StartDate != null && EndDate != null)
                {
                    var customerReport = new CustomerReport();
                    string path = await GetPathAsync();
                    string month = EndDate.ToString("MMMM yyyy");

                    var payments = await _customerRepository.GetCustomerReport(Customer.CustomerID, StartDate, EndDate, ReportType);

                    customerReport.GenerateCustomerReportPDF(Customer, payments, path, month, ReportType);

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
