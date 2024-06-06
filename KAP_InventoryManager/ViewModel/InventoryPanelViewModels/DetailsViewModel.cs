using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KAP_InventoryManager.ViewModel.InventoryPanelViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        public string DisplayName => "Details";
        private ItemModel _item;
        private readonly IItemRepository ItemRepository;

        private string _currentMonthRevenue;
        private string _lastMonthRevenue;
        private string _todayRevenue;
        private decimal _percentageChange;
        private bool _isCurrentMonthRevenueHigh;

        public ItemModel Item
        {
            get { return _item; }
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

        public decimal PercentageChange
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

        public DetailsViewModel()
        {
            ItemRepository = new ItemRepository();

            Messenger.Default.Register<ItemModel>(this, OnMessageReceived);
            Messenger.Default.Send(new object(), "RequestSelectedItem");
            Messenger.Default.Register<object>(this, "RequestSelectedItem", OnRequestSelectedItem);

            Messenger.Default.Register<string>(this, "ItemUpdated", OnItemUpdated);
        }

        private void OnMessageReceived(ItemModel item)
        {
            if (Item == null || Item.PartNo != item.PartNo)
            {
                Item = item;
                PopulateRevenue();
            }
        }

        private void OnRequestSelectedItem(object obj)
        {
            Messenger.Default.Send(Item);
        }

        private void OnItemUpdated(string partNo)
        {
            Item = ItemRepository.GetByPartNo(partNo);
        }

        private async void PopulateRevenue()
        {
            if(Item != null &&  Item.PartNo != null)
            {
                var currentMonthRevenue = await ItemRepository.CalculateCurrentMonthRevenueByItem(Item.PartNo);
                var lastMonthRevenue = await ItemRepository.CalculateLastMonthRevenueByItem(Item.PartNo);
                var todayRevenue = await ItemRepository.CalculateTodayRevenueByItem(Item.PartNo);

                CurrentMonthRevenue = $"Rs. {currentMonthRevenue:N2}";
                LastMonthRevenue = $"Compared to Rs. {lastMonthRevenue:N2} last month";
                TodayRevenue = $"Rs. {todayRevenue:N2}";
                PercentageChange = await ItemRepository.CalculatePercentageChange(currentMonthRevenue, lastMonthRevenue);

                if (currentMonthRevenue > lastMonthRevenue)
                    IsCurrentMonthRevenueHigh = true;
                else
                    IsCurrentMonthRevenueHigh = false;
            }
        }
    }
}
