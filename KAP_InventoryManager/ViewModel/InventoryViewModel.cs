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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    public class InventoryViewModel : ViewModelBase
    {
        private readonly IItemRepository _itemRepository;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;
     
        private ObservableCollection<ItemModel> _items;
        private ItemModel _selectedItem;
        private ItemModel _currentItem;
        private string _searchItemText;

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

        public ObservableCollection<ItemModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ItemModel SelectedItem
        {
            get => _selectedItem; 
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));

                if(SelectedItem != null)
                    PopulateDetails();
            }
        }

        public ItemModel CurrentItem
        {
            get => _currentItem;
            set
            {
                _currentItem = value;
                OnPropertyChanged(nameof(CurrentItem));

                Messenger.Default.Send(CurrentItem);
            }
        }

        public string SearchItemText
        {
            get => _searchItemText;
            set
            {
                _searchItemText = value;
                OnPropertyChanged(nameof(SearchItemText));

                PopulateItemsAsync();
            }
        }

        public ICommand ShowOverviewViewCommand { get; }
        public ICommand ShowDetailsViewCommand { get; }
        public ICommand ShowTransactionsViewCommand { get; }
        public ICommand GetAllCommand { get; }

        public InventoryViewModel()
        {
            _itemRepository = new ItemRepository();

            ViewModels.Add(new OverviewViewModel());
            ViewModels.Add(new DetailsViewModel());
            ViewModels.Add(new TransactionsViewModel());

            ShowOverviewViewCommand = new ViewModelCommand(ExecuteShowOverviewViewCommand);
            ShowDetailsViewCommand = new ViewModelCommand(ExecuteShowDetailsViewCommand);
            ShowTransactionsViewCommand = new ViewModelCommand(ExecuteShowTransactionsViewCommand);

            SelectedViewModel = ViewModels.First();

            Messenger.Default.Register<string>(this, OnMessageReceived);

            Items = new ObservableCollection<ItemModel>();
            PopulateItemsAsync();
        }

        private async void PopulateItemsAsync()
        {
            await _semaphore.WaitAsync();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                Items.Clear();

                List<ItemModel> items = (List<ItemModel>)(string.IsNullOrEmpty(SearchItemText)
                    ? await _itemRepository.GetAllAsync()
                    : await _itemRepository.SearchItemListAsync(SearchItemText));

                foreach (var item in items)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    // Determine stock status based on QtyInHand
                    if (item.QtyInHand <= 0)
                    {
                        item.StockStatus = "Out of Stock";
                    }
                    else if (item.QtyInHand < 20)
                    {
                        item.StockStatus = "Low in Stock";
                    }
                    else
                    {
                        item.StockStatus = "Adequate in Stock";
                    }

                    Items.Add(item);
                    await Task.Delay(0, _cancellationTokenSource.Token);
                }

                if (Items.Any())
                {
                    SelectedItem = Items.First();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch items. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, ignore the exception
            }
            finally
            {
                _semaphore.Release();
            }
        }

 
        private void ExecuteShowDetailsViewCommand(object obj)
        {
            Messenger.Default.Send(CurrentItem);
            SelectedViewModel = ViewModels.OfType<DetailsViewModel>().FirstOrDefault();
        }

        private void ExecuteShowOverviewViewCommand(object obj)
        {
            SelectedViewModel = ViewModels.OfType<OverviewViewModel>().FirstOrDefault();
        }

        private void ExecuteShowTransactionsViewCommand(object obj)
        {
            Messenger.Default.Send(CurrentItem);
            SelectedViewModel = ViewModels.OfType<TransactionsViewModel>().FirstOrDefault();
        }

        private async void PopulateDetails()
        {
            try
            {
                CurrentItem = await _itemRepository.GetByPartNoAsync(SelectedItem.PartNo);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch item details. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMessageReceived(string message)
        {
            if (message == "NewItemAdded")
            {
                PopulateItemsAsync();
            }
            else if (message == "ItemDeleted")
            {
                SearchItemText = string.Empty;
                PopulateItemsAsync();
            }
            else if (message == "RequestSelectedItem")
            {
                if(CurrentItem!= null && CurrentItem.PartNo != null)
                {
                    Messenger.Default.Send(CurrentItem);
                }
            }
        }
    }
}
