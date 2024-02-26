using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel.InventoryPanelViewModels
{
    public class TransactionsViewModel : ViewModelBase
    {
        public string DisplayName => "Transactions";
        private readonly IInvoiceRepository InvoiceRepository;

        private ItemModel _item;
        private IEnumerable<InvoiceItemModel> _transactions;
        private int _pageNumber;
        private bool _isFinalPage;

        public ItemModel Item
        {
            get { return _item; }
            set
            {

                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }

        public IEnumerable<InvoiceItemModel> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                _pageNumber = value;
                OnPropertyChanged(nameof(PageNumber));
            }
        }

        public bool IsFinalPage
        {
            get { return _isFinalPage; }
            set
            {
                _isFinalPage = value;
                OnPropertyChanged(nameof(IsFinalPage));
            }
        }

        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }

        public TransactionsViewModel()
        {
            InvoiceRepository = new InvoiceRepository();
            PageNumber = 1;

            GoToNextPageCommand = new ViewModelCommand(ExecuteGoToNextPageCommand);
            GoToPreviousPageCommand = new ViewModelCommand(ExecuteGoToPreviousPageCommand);

            Messenger.Default.Register<ItemModel>(this, OnMessageReceived);
            Messenger.Default.Send(new object(), "RequestSelectedItem");
        }

        private void ExecuteGoToPreviousPageCommand(object obj)
        {
            if (PageNumber != 1)
            {
                PageNumber--;
                PopulateTransactionsAsync();
            }
        }

        private void ExecuteGoToNextPageCommand(object obj)
        {
            if (IsFinalPage == false)
            {
                PageNumber++;
                PopulateTransactionsAsync();
            }
        }

        private void OnMessageReceived(ItemModel item)
        {
            Item = item;
            PopulateTransactionsAsync();
        }

        private async void PopulateTransactionsAsync()
        {
            try
            {
                if (Item != null)
                {
                    Transactions = await InvoiceRepository.GetInvoicesByPartNo(Item.PartNo, 15, PageNumber);
                    if(Transactions.Count()<15)
                        IsFinalPage = true;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to fetch transactions. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
