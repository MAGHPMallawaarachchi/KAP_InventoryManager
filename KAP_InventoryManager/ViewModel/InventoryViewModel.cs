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
        private ViewModelBase _currentChildView;

        private IEnumerable<InventoryItemModel> _inventoryItems;
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

        public ICommand ShowOverviewViewCommand { get; }
        public ICommand ShowDetailsViewCommand { get; }
        public ICommand ShowTransactionsViewCommand { get; }
        public ICommand GetAllCommand { get; }

        public InventoryViewModel()
        {
            //Initialize commands
            ShowOverviewViewCommand = new ViewModelCommand(ExecuteShowOverviewViewCommand);
            ShowDetailsViewCommand = new ViewModelCommand(ExecuteShowDetailsViewCommand);
            ShowTransactionsViewCommand = new ViewModelCommand(ExecuteShowTransactionsViewCommand);

            //default view
            ExecuteShowOverviewViewCommand(null);
            GetAllAndPopulateDataGrid();
        }

        public void GetAllAndPopulateDataGrid()
        {
            try
            {
                ItemRepository itemrepo = new ItemRepository();
                InventoryItems = itemrepo.GetAllInventoryItems();
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
            CurrentChildView = new DetailsViewModel();
        }

        private void ExecuteShowOverviewViewCommand(object obj)
        {
            CurrentChildView = new OverviewViewModel();
        }
    }
}
