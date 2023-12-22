using KAP_InventoryManager.ViewModel.InventoryPanelViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    public class InventoryViewModel:ViewModelBase
    {
        private ViewModelBase _currentChildView;

        public ViewModelBase CurrentChildView
        {
            get
            {
                return _currentChildView;
            }

            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }

        public ICommand ShowOverviewViewCommand { get; }
        public ICommand ShowDetailsViewCommand { get; }
        public ICommand ShowTransactionsViewCommand { get; }

        public InventoryViewModel()
        {
            //Initialize commands
            ShowOverviewViewCommand = new ViewModelCommand(ExecuteShowOverviewViewCommand);
            ShowDetailsViewCommand = new ViewModelCommand(ExecuteShowDetailsViewCommand);
            ShowTransactionsViewCommand = new ViewModelCommand(ExecuteShowTransactionsViewCommand);

            //default view
            ExecuteShowOverviewViewCommand(null);
        }

        private void ExecuteShowTransactionsViewCommand(object obj)
        {
            CurrentChildView = new TransactionsViewModel();
        }

        private void ExecuteShowDetailsViewCommand(object obj)
        {
            CurrentChildView = new DetailsViewModel();
        }

        private void ExecuteShowOverviewViewCommand(object obj)
        {
            CurrentChildView = new OverviewViewModel();
        }
    }
}
