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

        public ItemModel Item
        {
            get { return _item; }
            set
            {

                _item = value;
                OnPropertyChanged(nameof(Item));
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
            Item = item;
        }

        private void OnRequestSelectedItem(object obj)
        {
            Messenger.Default.Send(Item);
        }

        private void OnItemUpdated(string partNo)
        {
            Item = ItemRepository.GetByPartNo(partNo);
        }
    }
}
