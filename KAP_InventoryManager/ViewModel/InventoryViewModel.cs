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
     
        private IEnumerable<ItemModel> _items;
        private ItemModel _selectedItem;

        public ObservableCollection<ViewModelBase> ViewModels { get; } = new ObservableCollection<ViewModelBase>();

        private ViewModelBase _selectedViewModel;

        public ViewModelBase SelectedViewModel
        {
            get => _selectedViewModel;
            set
            {
                _selectedViewModel = value;
                OnPropertyChanged(nameof(SelectedViewModel));
            }
        }

        public IEnumerable<ItemModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }
        public ItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));

                Messenger.Default.Send(SelectedItem);
            }
        }

        public ICommand ShowOverviewViewCommand { get; }
        public ICommand ShowDetailsViewCommand { get; }
        public ICommand ShowTransactionsViewCommand { get; }
        public ICommand GetAllCommand { get; }

        public InventoryViewModel()
        {
            ItemRepository = new ItemRepository();

            // Initialize and add your view models to the collection
            ViewModels.Add(new OverviewViewModel());
            ViewModels.Add(new DetailsViewModel());
            ViewModels.Add(new TransactionsViewModel());

            ShowOverviewViewCommand = new ViewModelCommand(ExecuteShowOverviewViewCommand);
            ShowDetailsViewCommand = new ViewModelCommand(ExecuteShowDetailsViewCommand);
            ShowTransactionsViewCommand = new ViewModelCommand(ExecuteShowTransactionsViewCommand);

            // Set the default selected view model
            SelectedViewModel = ViewModels.First();

            PopulateListBox();
        }

        private void PopulateListBox()
        {
            try
            {            
                Items = ItemRepository.GetAll();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch items. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteShowTransactionsViewCommand(object obj)
        {
            SelectedViewModel = ViewModels.OfType<TransactionsViewModel>().FirstOrDefault();
        }

        private void ExecuteShowDetailsViewCommand(object obj)
        {
            Messenger.Default.Send(SelectedItem);
            SelectedViewModel = ViewModels.OfType<DetailsViewModel>().FirstOrDefault();
        }

        private void ExecuteShowOverviewViewCommand(object obj)
        {
            SelectedViewModel = ViewModels.OfType<OverviewViewModel>().FirstOrDefault();
        }
    }
}
