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
        private int _discount;
        private string _selectedRepId;
        private string _selectedPaymentType;
        private string _currentTime;
        private string _currentDate;
        private readonly ICustomerRepository CustomerRepository;
        private readonly ISalesRepRepository SalesRepRepository;

        public ObservableCollection<string> Suggestions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> SalesReps { get; set; } = new ObservableCollection<string>();

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                PopulateSuggestionList();
            }
        }

        public string SelectedCustomerId
        {
            get => _selectedCustomerId;
            set
            {
                _selectedCustomerId = value;
                OnPropertyChanged(nameof(SelectedCustomerId));
                PopulateDetails();
            }
        }

        public string SelectedPaymentType
        {
            get => _selectedPaymentType;
            set
            {
                _selectedPaymentType = value;
                OnPropertyChanged(nameof(SelectedPaymentType));
            }
        }

        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
            }
        }

        public int Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
            }
        }

        public string SelectedRepId
        {
            get => _selectedRepId;
            set
            {
                _selectedRepId = value;
                OnPropertyChanged(nameof(SelectedRepId));
            }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged(nameof(CurrentDate));
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }

        public AddInvoiceViewModel() 
        {
            CustomerRepository = new CustomerRepository();
            SalesRepRepository = new SalesRepRepository();
            
            DateTime currentDateTime = DateTime.Now;

            CurrentDate = currentDateTime.ToString("yyyy-MM-dd");
            CurrentTime = currentDateTime.ToString("t");

            PopulateSalesReps();
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


        private void PopulateSalesReps()
        {
            try
            {
                var salesReps = SalesRepRepository.GetAllRepIds();
                if (salesReps != null)
                {
                    foreach(var salesRep in salesReps)
                    {
                        SalesReps.Add(salesRep);
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"Failed to fetch sales reps. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateDetails()
        {
            try
            {
                SelectedCustomer = CustomerRepository.GetByCustomerID(SelectedCustomerId);
                if(SelectedCustomer != null)
                {
                    SelectedPaymentType = SelectedCustomer.PaymentType;

                    if(SelectedPaymentType == "CASH")
                    {
                        Discount = 30;
                    }
                    else
                    {
                        Discount = 25;
                    }

                    if (SelectedCustomer.RepID != null)
                    {
                        SelectedRepId = SelectedCustomer.RepID;
                    }
                    else
                    {
                        SelectedRepId = "None";
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch customer's details. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
