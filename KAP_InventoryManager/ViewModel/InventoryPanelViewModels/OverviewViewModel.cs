using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAP_InventoryManager.ViewModel.InventoryPanelViewModels
{
    public class OverviewViewModel : ViewModelBase
    {
        public string DisplayName => "Overview";
        private readonly IItemRepository _itemRepository;

        private int _itemCount;
        private int _categoryCount;
        private int _outOfStockCount;

        public int ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;
                OnPropertyChanged(nameof(ItemCount));
            }
        }

        public int CategoryCount
        {
            get => _categoryCount;
            set
            {
                _categoryCount = value;
                OnPropertyChanged(nameof(CategoryCount));
            }
        }

        public int OutOfStockCount
        {
            get => _outOfStockCount;
            set
            {
                _outOfStockCount = value;
                OnPropertyChanged(nameof(OutOfStockCount));
            }
        }

        public OverviewViewModel()
        {
            _itemRepository = new ItemRepository();
            PopulateInventorySummary();
        }

        private async void PopulateInventorySummary()
        {
            try
            {
                ItemCount = await _itemRepository.GetItemCountAsync();
                CategoryCount = await _itemRepository.GetCategoryCountAsync();
                OutOfStockCount = await _itemRepository.GetOutOfStockCountAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to populate inventory summary. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
