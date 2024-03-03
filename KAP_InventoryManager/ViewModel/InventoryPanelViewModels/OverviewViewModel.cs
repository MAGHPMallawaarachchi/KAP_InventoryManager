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
        private readonly IItemRepository ItemRepository;

        private int _itemCount;
        private int _categoryCount;
        private int _outOfStockCount;

        public int ItemCount
        {
            get { return _itemCount; }
            set
            {
                _itemCount = value;
                OnPropertyChanged(nameof(ItemCount));
            }
        }

        public int CategoryCount
        {
            get { return _categoryCount; }
            set
            {
                _categoryCount = value;
                OnPropertyChanged(nameof(CategoryCount));
            }
        }

        public int OutOfStockCount
        {
            get { return _outOfStockCount; }
            set
            {
                _outOfStockCount = value;
                OnPropertyChanged(nameof(OutOfStockCount));
            }
        }

        public OverviewViewModel()
        {
            ItemRepository = new ItemRepository();
            PopulateInventorySummary();
        }

        private async void PopulateInventorySummary()
        {
            ItemCount = await ItemRepository.GetItemCount();
            CategoryCount = await ItemRepository.GetCategoryCount();
            OutOfStockCount = await ItemRepository.GetOutOfStockCount();
        }
    }
}
