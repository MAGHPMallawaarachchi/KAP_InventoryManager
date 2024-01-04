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
        private ItemModel _item;

        public ItemModel Item
        {
            get { return _item; }
            set
            {
                if (_item != value)
                {
                    _item = value;
                    OnPropertyChanged(nameof(Item));
                }
            }
        }

        public DetailsViewModel()
        {
            Messenger.Default.Register<ItemModel>(this, HandleItemMessage);
        }

        private void HandleItemMessage(ItemModel item)
        {
            Item = item;
        }
    }

}
