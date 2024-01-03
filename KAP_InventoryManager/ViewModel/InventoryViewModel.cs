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
        private IEnumerable<InventoryItemModel> _inventoryItems;
        private InventoryItemModel _selectedInventoryItem;

        public IEnumerable<InventoryItemModel> InventoryItems
        {
            get { return _inventoryItems; }
            set
            {
                _inventoryItems = value;
                OnPropertyChanged(nameof(InventoryItems));
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
        public InventoryItemModel SelectedInventoryItem
        {
            get { return _selectedInventoryItem; }
            set
            {
                _selectedInventoryItem = value;
                OnPropertyChanged(nameof(SelectedInventoryItem));

                // Show the details view with the selected part number
                ExecuteShowDetailsViewCommand(_selectedInventoryItem?.PartNo);
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

            //default view
            ExecuteShowOverviewViewCommand(null);
            PopulateDataGrid();
        }

        private void PopulateDataGrid()
        {
            try
            {            
                InventoryItems = ItemRepository.GetAllInventoryItems();
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
            if (obj is string partNumber && !string.IsNullOrEmpty(partNumber))
            {
                CurrentChildView = new DetailsViewModel(partNumber);
            }
            else
            {
                MessageBox.Show("Something went wrong!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteShowOverviewViewCommand(object obj)
        {
            CurrentChildView = new OverviewViewModel();
        }
    }
}
