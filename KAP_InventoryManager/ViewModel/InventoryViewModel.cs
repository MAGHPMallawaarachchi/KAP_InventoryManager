using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using KAP_InventoryManager.ViewModel.InventoryPanelViewModels;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    public class InventoryViewModel : ViewModelBase
    {
        private readonly IItemRepository ItemRepository;

        private ViewModelBase _currentChildView;       
        private IEnumerable<ItemModel> _items;
        private ItemModel _selectedItem;

        private bool _isCheckedOverview = true;
        private bool _isCheckedDetails = false;

        public bool IsCheckedOverview
        {
            get { return _isCheckedOverview; }
            set
            {
                _isCheckedOverview = value;
                OnPropertyChanged(nameof(IsCheckedOverview));
            }
        }

        public bool IsCheckedDetails { 
            get { return _isCheckedDetails; }
            set
            {
                _isCheckedDetails = value;
                OnPropertyChanged(nameof(IsCheckedDetails));
            }
        }


        public IEnumerable<ItemModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }
        public ItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));

                ExecuteShowDetailsViewCommand(null);
            }
        }

        public ViewModelBase CurrentChildView
        {
            get
            {
                return _currentChildView;
            }

            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }

        public ICommand ShowOverviewViewCommand { get; }
        public ICommand ShowDetailsViewCommand { get; }
        public ICommand ShowTransactionsViewCommand { get; }
        public ICommand GetAllCommand { get; }

        public InventoryViewModel()
        {
            ItemRepository = new ItemRepository();

            //Initialize commands
            ShowOverviewViewCommand = new ViewModelCommand(ExecuteShowOverviewViewCommand);
            ShowDetailsViewCommand = new ViewModelCommand(ExecuteShowDetailsViewCommand);
            ShowTransactionsViewCommand = new ViewModelCommand(ExecuteShowTransactionsViewCommand);

            PopulateListBox();
            SelectedItem = Items?.FirstOrDefault();

            ExecuteShowOverviewViewCommand(null); 
        }

        private void PopulateListBox()
        {
            try
            {            
                Items = ItemRepository.GetAll();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch items. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteShowTransactionsViewCommand(object obj)
        {
            CurrentChildView = new TransactionsViewModel();
        }

        private void ExecuteShowDetailsViewCommand(object obj)
        {

            DetailsViewModel detailsViewModel = new DetailsViewModel
            {
                Item = SelectedItem
            };

            Messenger.Default.Send(SelectedItem);

            CurrentChildView = detailsViewModel;
            IsCheckedDetails = true;
            IsCheckedOverview = false;
        }

        private void ExecuteShowOverviewViewCommand(object obj)
        {
            IsCheckedOverview = true;
            IsCheckedDetails = false;
            CurrentChildView = new OverviewViewModel();
        }
    }
}
