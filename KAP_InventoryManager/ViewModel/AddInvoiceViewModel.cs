using KAP_InventoryManager.CustomControls;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    public class AddInvoiceViewModel : ViewModelBase
    {
        private string _searchText;
        private string _selectedCustomerId;
        private CustomerModel _selectedCustomer;
        private readonly ICustomerRepository CustomerRepository;

        public ObservableCollection<string> Suggestions { get; set; } = new ObservableCollection<string>();

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    PopulateSuggestionList();
                }
            }
        }

        public string SelectedCustomerId
        {
            get => _selectedCustomerId;
            set
            {
                if (_selectedCustomerId != value)
                {
                    _selectedCustomerId = value;
                    OnPropertyChanged(nameof(SelectedCustomerId));
                    PopulateDetails();
                }
            }
        }

        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (_selectedCustomer != value)
                {
                    _selectedCustomer = value;
                    OnPropertyChanged(nameof(SelectedCustomer));
                }
            }
        }

        public AddInvoiceViewModel() 
        {
            CustomerRepository = new CustomerRepository();
        }

        private void PopulateSuggestionList()
        {
            try
            {
                Suggestions.Clear();

                if(SearchText != null || SearchText != "")
                {
                    var results = CustomerRepository.SearchCustomer(SearchText);

                    if (results != null)
                    {
                        foreach (var suggestion in results)
                        {
                            Suggestions.Add(suggestion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to fetch suggestions. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateDetails()
        {
            try
            {
                SelectedCustomer = CustomerRepository.GetByCustomerID(SelectedCustomerId);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch customer's details. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
