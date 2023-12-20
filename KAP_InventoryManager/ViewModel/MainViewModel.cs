using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;

namespace KAP_InventoryManager.ViewModel
{
    public class MainViewModel:ViewModelBase
    {
        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;

        private readonly IUserRepository userRepository;

        public UserAccountModel CurrentUserAccount
        {
            get
            {
                return _currentUserAccount;
            }

            set
            {
                _currentUserAccount = value;
                OnPropertyChanged(nameof(CurrentUserAccount));
            }
        }

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

        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowInventoryViewCommand { get; }
        public ICommand ShowCustomersViewCommand {  get; }
        public ICommand ShowInvoicesViewCommand { get; }

        public MainViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Design-time initialization
                _currentUserAccount = new UserAccountModel
                {
                    DisplayName = "Design Time User",
                };
            }
            else
            {
                // Runtime initialization
                userRepository = new UserRepository();
                _currentUserAccount = new UserAccountModel();
                LoadCurrentUserData();
            }

            //Initialize commands
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowInventoryViewCommand = new ViewModelCommand(ExecuteShowInventoryViewCommand);
            ShowCustomersViewCommand = new ViewModelCommand(ExecuteShowCustomersViewCommand);
            ShowInvoicesViewCommand = new ViewModelCommand(ExecuteShowInvoicesViewCommand);

            //default view
            ExecuteShowHomeViewCommand(null);
        }
        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new HomeViewModel();
        }

        private void ExecuteShowInventoryViewCommand(object obj)
        {
            CurrentChildView = new InventoryViewModel();
        }
        private void ExecuteShowCustomersViewCommand(object obj)
        {
            CurrentChildView = new CustomersViewModel();
        }

        private void ExecuteShowInvoicesViewCommand(object obj)
        {
            CurrentChildView = new InvoicesViewModel();
        }

        private void LoadCurrentUserData()
        {
            var user = userRepository.GetByUsername(Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                CurrentUserAccount.Username = user.UserName;
                CurrentUserAccount.DisplayName = $"Hello {user.Name}!";
            }
            else
            {
                CurrentUserAccount.DisplayName = "Invalid User";
                //Hide child view
            }
        }

    }

}
