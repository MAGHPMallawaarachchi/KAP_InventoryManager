using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace KAP_InventoryManager.ViewModel
{
    public class ReturnsViewModel : ViewModelBase
    {
        private readonly ICustomerRepository CustomerRepository;
        private readonly IInvoiceRepository InvoiceRepository;
        private readonly IReturnRepository ReturnRepository;

        private IEnumerable<ReturnModel> _returns;
        private string _returnSearchText;
        private ReturnModel _selectedReturn;
        private ReturnModel _currentReturn;
        private CustomerModel _customer;
        private IEnumerable<ReturnItemModel> _returnItems;

        public IEnumerable<ReturnModel> Returns
        {
            get => _returns;
            set
            {
                _returns = value;
                OnPropertyChanged(nameof(Returns));

                SelectedReturn = Returns.FirstOrDefault();
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
            CancelReturnCommand = new ViewModelCommand(ExecuteCancelReturnCommand);

            CustomerRepository = new CustomerRepository();
            ReturnRepository = new ReturnRepository();

            PopulateReturnsAsync();
        }

        private async void ExecuteCancelReturnCommand(object obj)
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
            try
            {
                if (ReturnSearchText == null)
                    Returns = await ReturnRepository.GetAllReturnsAsync();
                else
                    Returns = await ReturnRepository.SearchReturnListAsync(ReturnSearchText);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateReturnDetails()
        {
            try
            {
                if (SelectedReturn != null)
                {
                    CurrentReturn = await ReturnRepository.GetByReturnNo(SelectedReturn.ReturnNo);

                    if (CurrentReturn != null)
                    {
                        Customer = await CustomerRepository.GetByCustomerIDAsync(CurrentReturn.CustomerID);
                        ReturnItems = await ReturnRepository.GetReturnItems(CurrentReturn.ReturnNo);
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
