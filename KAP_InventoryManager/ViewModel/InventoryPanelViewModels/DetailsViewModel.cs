using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel.InventoryPanelViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        public string DisplayName => "Details";

        private readonly IItemRepository _itemRepository;

        private ItemModel _item;
        private string _currentMonthRevenue;
        private string _lastMonthRevenue;
        private string _todayRevenue;
        private string _percentageChange;
        private bool _isCurrentMonthRevenueHigh;
        private string _supplierId;

        public ItemModel Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }

        public string CurrentMonthRevenue 
        { 
            get => _currentMonthRevenue;
            set 
            {
                _currentMonthRevenue = value;
                OnPropertyChanged(nameof(CurrentMonthRevenue));
            } 
        }

        public string LastMonthRevenue
        {
            get => _lastMonthRevenue;
            set
            {
                _lastMonthRevenue = value;
                OnPropertyChanged(nameof(LastMonthRevenue));
            }
        }

        public string TodayRevenue
        {
            get => _todayRevenue;
            set
            {
                _todayRevenue = value;
                OnPropertyChanged(nameof(TodayRevenue));
            }
        }

        public string PercentageChange
        {
            get => _percentageChange;
            set
            {
                _percentageChange = value;
                OnPropertyChanged(nameof(PercentageChange));
            }
        }

        public bool IsCurrentMonthRevenueHigh
        {
            get => _isCurrentMonthRevenueHigh;
            set
            {
                _isCurrentMonthRevenueHigh = value;
                OnPropertyChanged(nameof(IsCurrentMonthRevenueHigh));
            }
        }

        public string SupplierId
        {
            get => _supplierId;
            set
            {
                _supplierId = value;
                OnPropertyChanged(nameof(SupplierId));
            }
        }

        public ICommand DeleteItemCommand { get; }

        public DetailsViewModel()
        {
            _itemRepository = new ItemRepository();
            DeleteItemCommand = new ViewModelCommand(ExecuteDeleteItemCommand);

            IsCurrentMonthRevenueHigh = false;

            Messenger.Default.Register<ItemModel>(this, OnMessageReceived);
            Messenger.Default.Send(new object(), "RequestSelectedItem");
            Messenger.Default.Register<object>(this, "RequestSelectedItem", OnRequestSelectedItem);
            Messenger.Default.Register<string>(this, "ItemUpdated", OnItemUpdated);
        }

        private void ExecuteDeleteItemCommand(object obj)
        {
            if(Item != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _itemRepository.DeleteAsync(Item.PartNo);
                    Messenger.Default.Send("ItemDeleted");
                }
            }
        }

        private void OnMessageReceived(ItemModel item)
        {
            if (item != null && (Item == null || Item.PartNo != item.PartNo))
            {
                Item = item;
                PopulateRevenue();
            }
        } 

        private void OnRequestSelectedItem(object obj)
        {
            Messenger.Default.Send(Item);
        }

        private async void OnItemUpdated(string partNo)
        {
            try
            {
                Item = await _itemRepository.GetByPartNoAsync(partNo);
                if (Item != null)
                {
                    PopulateRevenue();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to update item details. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PopulateRevenue()
        {
            if (Item != null && !string.IsNullOrEmpty(Item.PartNo))
            {
                try
                {
                    var currentMonthRevenue = await _itemRepository.CalculateCurrentMonthRevenueByItemAsync(Item.PartNo);
                    var lastMonthRevenue = await _itemRepository.CalculateLastMonthRevenueByItemAsync(Item.PartNo);
                    var todayRevenue = await _itemRepository.CalculateTodayRevenueByItemAsync(Item.PartNo);
                    var percentageChange = await _itemRepository.CalculatePercentageChangeAsync(currentMonthRevenue, lastMonthRevenue);

                    CurrentMonthRevenue = $"Rs. {currentMonthRevenue:N2}";
                    LastMonthRevenue = $"Compared to Rs. {lastMonthRevenue:N2} last month";
                    TodayRevenue = $"Rs. {todayRevenue:N2}";
                    PercentageChange = $"{percentageChange}%";

                    IsCurrentMonthRevenueHigh = currentMonthRevenue > lastMonthRevenue;
                    SupplierId = await _itemRepository.GetSupplierByBrandAsync(Item.BrandID);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Failed to populate revenue details. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
