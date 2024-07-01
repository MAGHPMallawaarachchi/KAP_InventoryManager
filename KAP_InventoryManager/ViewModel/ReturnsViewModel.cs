using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using System.Threading;
using MySql.Data.MySqlClient;

namespace KAP_InventoryManager.ViewModel
{
    public class ReturnsViewModel : ViewModelBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IReturnRepository _returnRepository;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<ReturnModel> _returns;
        private string _returnSearchText;
        private ReturnModel _selectedReturn;
        private ReturnModel _currentReturn;
        private CustomerModel _customer;
        private IEnumerable<ReturnItemModel> _returnItems;

        public ObservableCollection<ReturnModel> Returns
        {
            get => _returns;
            set
            {
                _returns = value;
                OnPropertyChanged(nameof(Returns));
            }
        }

        public string ReturnSearchText
        {
            get => _returnSearchText;
            set
            {
                _returnSearchText = value;
                OnPropertyChanged(nameof(ReturnSearchText));

                PopulateReturnsAsync();
            }
        }

        public ReturnModel SelectedReturn
        {
            get => _selectedReturn;
            set
            {
                _selectedReturn = value;
                OnPropertyChanged(nameof(SelectedReturn));
                PopulateReturnDetails();
            }
        }

        public ReturnModel CurrentReturn
        {
            get => _currentReturn;
            set
            {
                _currentReturn = value;
                OnPropertyChanged(nameof(CurrentReturn));
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

        public IEnumerable<ReturnItemModel> ReturnItems
        {
            get => _returnItems;
            set
            {
                _returnItems = value;
                OnPropertyChanged(nameof(ReturnItems));
            }
        }

        public ICommand CancelReturnCommand { get; }

        public ReturnsViewModel()
        {
            _customerRepository = new CustomerRepository();
            _returnRepository = new ReturnRepository();

            CancelReturnCommand = new ViewModelCommand(ExecuteCancelReturnCommand);
            Returns = new ObservableCollection<ReturnModel>();

            PopulateReturnsAsync();
            Messenger.Default.Register<string>(this, OnMessageReceived);
        }

        private void OnMessageReceived(string obj)
        {
            PopulateReturnsAsync();
        }

        private void ExecuteCancelReturnCommand(object obj)
        {
            try
            {
                if (CurrentReturn != null)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to cancel this invoice?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        //await ReturnRepository.CancelReturn(CurrentReturn.ReturnNo);
                        MessageBox.Show("Return cancelled successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private async void PopulateReturnsAsync()
        {
            await _semaphore.WaitAsync();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var returns = (string.IsNullOrEmpty(ReturnSearchText)
                    ? await _returnRepository.GetAllReturnsAsync()
                    : await _returnRepository.SearchReturnListAsync(ReturnSearchText));

                var returnsList = new List<ReturnModel>(returns);

                Returns.Clear();

                foreach (var returnReceipt in returnsList)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    Returns.Add(returnReceipt);
                    await Task.Delay(0, _cancellationTokenSource.Token);
                }

                if (Returns.Any())
                {
                    SelectedReturn = Returns.First();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async void PopulateReturnDetails()
        {
            try
            {
                if (SelectedReturn != null)
                {
                    CurrentReturn = await _returnRepository.GetByReturnNoAsync(SelectedReturn.ReturnNo);

                    if (CurrentReturn != null)
                    {
                        Customer = await _customerRepository.GetByCustomerIDAsync(CurrentReturn.CustomerID);
                        ReturnItems = await _returnRepository.GetReturnItemsAsync(CurrentReturn.ReturnNo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
