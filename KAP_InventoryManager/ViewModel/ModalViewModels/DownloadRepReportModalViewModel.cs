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
    internal class DownloadRepReportModalViewModel : ViewModelBase
    {
        private List<string> _reportTypes;
        private string _reportType;
        private DateTime _startDate;
        private DateTime _endDate;
        private SalesRepModel _rep;
        private CustomerModel _customer;

        private readonly ISalesRepRepository _salesRepRepository;
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

        public SalesRepModel Rep
        {
            get => _rep;
            set
            {
                _rep = value;
                OnPropertyChanged(nameof(Rep));
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

        public DownloadRepReportModalViewModel()
        {
            _salesRepRepository = new SalesRepRepository();
            _userRepository = new UserRepository();
            _customerRepository = new CustomerRepository();

            ReportTypes = new List<string> { "all", "only paid", "only pending or overdue" };
            Initialize();

            // Register to receive the RepModel
            Messenger.Default.Register<SalesRepModel>(this, OnMessageReceived);

            // Send the "RequestRep" message to request the current customer
            Messenger.Default.Send("RequestRep");

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

        private void OnMessageReceived(SalesRepModel rep)
        {
            if (rep != null)
            {
                Rep = rep;
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
                if (Rep != null && ReportType != null && StartDate != null && EndDate != null)
                {
                    var repReport = new RepReport();
                    string path = await GetPathAsync();
                    string month = EndDate.ToString("MMMM yyyy");
                    var repReports = new List<RepReportModel>();
                    var customers = await _salesRepRepository.GetCustomersFromInovoiceByRep(Rep.RepID, StartDate, EndDate, ReportType);

                    foreach (var customer in customers)
                    {
                        var payments = await _salesRepRepository.GetRepReport(customer, Rep.RepID, StartDate, EndDate, ReportType);
                        decimal totalAmount = payments.Sum(payment => payment.TotalAmount);
                        decimal totalReturnAmount = payments.Sum(payment => payment.ReturnAmount);
                        decimal commissionAmount = (totalAmount - totalReturnAmount) * Rep.CommissionPercentage / 100;
                        Customer = await _customerRepository.GetByCustomerIDAsync(customer);

                        repReports.Add(new RepReportModel
                        {
                            CustomrName = Customer.Name,
                            CustomerCity = Customer.City,
                            Payments = payments,
                            TotalAmount = totalAmount,
                            TotalReturnAmount = totalReturnAmount,
                            CommissionAmount = commissionAmount
                        });
                    }

                    repReport.GenerateRepReportPDF(Rep, repReports, path, month, ReportType);

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
