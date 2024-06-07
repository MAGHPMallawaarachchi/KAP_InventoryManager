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
        private string _percentageChange;
        private bool _isCurrentMonthRevenueHigh;
        private bool _isLastMonthRevenueHigh;

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

        public bool IsLastMonthRevenueHigh
        {
            get => _isLastMonthRevenueHigh;
            set
            {
                _isLastMonthRevenueHigh = value;
                OnPropertyChanged(nameof(IsLastMonthRevenueHigh));
            }
        }

        public DetailsViewModel()
        {
            ItemRepository = new ItemRepository();

            IsCurrentMonthRevenueHigh = false;
            IsLastMonthRevenueHigh = false;

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
                var percentageChange = await ItemRepository.CalculatePercentageChange(currentMonthRevenue, lastMonthRevenue);

                CurrentMonthRevenue = $"Rs. {currentMonthRevenue:N2}";
                LastMonthRevenue = $"Compared to Rs. {lastMonthRevenue:N2} last month";
                TodayRevenue = $"Rs. {todayRevenue:N2}";
                PercentageChange = $"{percentageChange}%";

                if (currentMonthRevenue > lastMonthRevenue)
                {
                    IsCurrentMonthRevenueHigh = true;
                    IsLastMonthRevenueHigh = false;
                }
                else
                {
                    IsCurrentMonthRevenueHigh = false;
                    IsLastMonthRevenueHigh = true;
                }

            }
        }
    }
}
