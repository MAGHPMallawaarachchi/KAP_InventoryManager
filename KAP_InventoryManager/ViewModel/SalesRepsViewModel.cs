using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace KAP_InventoryManager.ViewModel
{
    public class SalesRepsViewModel : ViewModelBase
    {
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<SalesRepModel> _salesReps;
        private IEnumerable<InvoiceModel> _invoices;

        private SalesRepModel _selectedSalesRep;
        private SalesRepModel _currentSalesRep;

        private decimal _currentMonthCommission;
        private decimal _todayCommission;
        private decimal _lastMonthCommission;
        private decimal _percentageChange;
        private bool _isCurrentMonthCommissionHigh;

        private string _searchSalesRepText;
        private string _searchInvoiceText;
        private int _pageNumber;
        private bool _isFinalPage;

        public ObservableCollection<SalesRepModel> SalesReps
        {
            get => _salesReps;
            set
            {
                _salesReps = value;
                OnPropertyChanged(nameof(SalesReps));
            }
        }

        public IEnumerable<InvoiceModel> Invoices
        {
            get => _invoices;
            set
            {
                _invoices = value;
                OnPropertyChanged(nameof(Invoices));
            }
        }

        public SalesRepModel SelectedSalesRep
        {
            get => _selectedSalesRep;
            set
            {
                _selectedSalesRep = value;
                OnPropertyChanged(nameof(SelectedSalesRep));

                if (SelectedSalesRep != null)
                {
                    PopulateDetails();
                }
            }
        }

        public SalesRepModel CurrentSalesRep
        {
            get => _currentSalesRep;
            set
            {
                _currentSalesRep = value;
                OnPropertyChanged(nameof(CurrentSalesRep));
            }
        }


        public decimal CurrentMonthCommission
        {
            get => _currentMonthCommission;
            set
            {
                _currentMonthCommission = value;
                OnPropertyChanged(nameof(CurrentMonthCommission));
            }
        }

        public decimal TodayCommission
        {
            get => _todayCommission;
            set
            {
                _todayCommission = value;
                OnPropertyChanged(nameof(TodayCommission));
            }
        }

        public decimal LastMonthCommission
        {
            get => _lastMonthCommission;
            set
            {
                _lastMonthCommission = value;
                OnPropertyChanged(nameof(LastMonthCommission));
            }
        }

        public decimal PercentageChange
        {
            get => _percentageChange;
            set
            {
                _percentageChange = value;
                OnPropertyChanged(nameof(PercentageChange));
            }
        }

        public bool IsCurrentMonthCommissionHigh
        {
            get => _isCurrentMonthCommissionHigh;
            set
            {
                _isCurrentMonthCommissionHigh = value;
                OnPropertyChanged(nameof(IsCurrentMonthCommissionHigh));
            }
        }

        public string SearchSalesRepText
        {
            get => _searchSalesRepText;
            set
            {
                _searchSalesRepText = value;
                OnPropertyChanged(nameof(SearchSalesRepText));

                PopulateSalesRepsAsync();
            }
        }

        public string SearchInvoiceText
        {
            get => _searchInvoiceText;
            set
            {
                _searchInvoiceText = value;
                OnPropertyChanged(nameof(SearchInvoiceText));

                PopulateInvoicesAsync();
            }
        }

        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                _pageNumber = value;
                OnPropertyChanged(nameof(PageNumber));
            }
        }

        public bool IsFinalPage
        {
            get => _isFinalPage;
            set
            {
                _isFinalPage = value;
                OnPropertyChanged(nameof(IsFinalPage));
            }
        }

        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }

        public SalesRepsViewModel()
        {
            _salesRepRepository = new SalesRepRepository();
            _invoiceRepository = new InvoiceRepository();

            GoToNextPageCommand = new ViewModelCommand(ExecuteGoToNextPageCommand);
            GoToPreviousPageCommand = new ViewModelCommand(ExecuteGoToPreviousPageCommand);

            PageNumber = 1;

            SalesReps = new ObservableCollection<SalesRepModel>();
            PopulateSalesRepsAsync();
            PopulateRevenue(); 

            Messenger.Default.Register<string>(this, OnMessageReceived);
        }

        private void ExecuteGoToPreviousPageCommand(object obj)
        {
            if (PageNumber != 1)
            {
                PageNumber--;
                PopulateInvoicesAsync();
            }
        }

        private void ExecuteGoToNextPageCommand(object obj)
        {
            if (IsFinalPage == false)
            {
                PageNumber++;
                PopulateInvoicesAsync();
            }
        }

        private async void PopulateSalesRepsAsync()
        {
            await _semaphore.WaitAsync();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                SalesReps.Clear();
                List<SalesRepModel> reps = (List<SalesRepModel>)(string.IsNullOrEmpty(SearchSalesRepText)
                    ? await _salesRepRepository.GetAllAsync()
                    : await _salesRepRepository.SearchRepsListAsync(SearchSalesRepText));

                foreach (var rep in reps)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    SalesReps.Add(rep);
                    await Task.Delay(0, _cancellationTokenSource.Token);
                }

                if (SalesReps.Any())
                {
                    SelectedSalesRep = SalesReps.First();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch sales reps. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, ignore the exception
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async void PopulateInvoicesAsync()
        {
            try
            {
                Invoices = string.IsNullOrEmpty(SearchInvoiceText)
                    ? await _invoiceRepository.GetInvoiceByRepAsync(CurrentSalesRep.RepID, 10, PageNumber)
                    : await _invoiceRepository.SearchRepInvoiceListAsync(SearchInvoiceText, CurrentSalesRep.RepID, 10, PageNumber);

                IsFinalPage = Invoices.Count() < 10;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateDetails()
        {
            try
            {
                CurrentSalesRep = await _salesRepRepository.GetByRepIDAsync(SelectedSalesRep.RepID);
                PopulateInvoicesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateRevenue()
        {
            if (CurrentSalesRep != null && !string.IsNullOrEmpty(CurrentSalesRep.RepID))
            {
                try
                {
                    CurrentMonthCommission = await _salesRepRepository.CalculateCurrentMonthCommissionAsync(CurrentSalesRep.RepID);
                    LastMonthCommission = await _salesRepRepository.CalculateLastMonthCommissionAsync(CurrentSalesRep.RepID);
                    TodayCommission = await _salesRepRepository.CalculateTodayCommissionAsync(CurrentSalesRep.RepID);
                    PercentageChange = await _salesRepRepository.CalculatePercentageChangeAsync(CurrentMonthCommission, LastMonthCommission);

                    IsCurrentMonthCommissionHigh = CurrentMonthCommission > LastMonthCommission;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Failed to populate revenue details. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnMessageReceived(string message)
        {
            if (message == "RequestRep")
            {
                Messenger.Default.Send(CurrentSalesRep);
            }
        }
    }
}
