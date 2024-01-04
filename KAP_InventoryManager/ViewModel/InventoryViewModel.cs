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
        private IEnumerable<InventoryItemModel> _inventoryItems;
        private InventoryItemModel _selectedInventoryItem;
        private ItemModel _item;
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

        public ItemModel SelectedItem
        {
            get { return _item; }
            set
            {
                if (_item != value)
                {
                    _item = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

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

                ExecuteShowDetailsViewCommand(null);
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

            PopulateDataGrid();
            SelectedInventoryItem = InventoryItems?.FirstOrDefault();

            ExecuteShowOverviewViewCommand(null); 
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
            ItemModel item = LoadItemData();

            DetailsViewModel detailsViewModel = new DetailsViewModel
            {
                Item = item
            };

            Messenger.Default.Send(item);

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

        public ItemModel LoadItemData()
        {
            try
            {
                var item = ItemRepository.GetByPartNo(SelectedInventoryItem.PartNo);

                if (item != null)
                {
                    SelectedItem = new ItemModel
                    {
                        PartNo = item.PartNo,
                        OEMNo = item.OEMNo,
                        Description = item.Description,
                        BrandID = item.BrandID,
                        Category = item.Category,
                        SupplierID = item.SupplierID,
                        TotalQty = item.TotalQty,
                        QtyInHand = item.QtyInHand,
                        QtySold = item.QtySold,
                        BuyingPrice = item.BuyingPrice,
                        UnitPrice = item.UnitPrice
                    };

                    return item;

                }
                else
                {
                    Console.WriteLine("No item found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading item data: {ex.Message}");
                return null;
            }
        }
    }
}
