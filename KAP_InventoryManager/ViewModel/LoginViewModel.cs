using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel
{
    internal class LoginViewModel : ViewModelBase
    {
        //Fields
        private string _username;
        private SecureString _password;
        private string _errorMessage;
        private bool _isViewVisible = true;

        public string Username {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public SecureString Password {
            get 
            {
                return _password;
            }
            set 
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            } 
        }

        public string ErrorMessage { 
            get 
            {
                return _errorMessage;
            } 
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }
            set 
            { 
                _isViewVisible = value; 
                OnPropertyChanged(nameof(IsViewVisible)); 
            }
        }

        //-> Commands
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RememberPasswordCommand { get; }

        //Constructor
        public LoginViewModel()
        {
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));
        }


        private bool CanExecuteLoginCommand(object obj)
        {
            bool validate;
            if(string.IsNullOrEmpty(Username) || Username.Length < 3 || Password == null || Password.Length < 3)
                validate = false;
            else 
                validate = true;
            return validate;
        }

        private void ExecuteLoginCommand(object obj)
        {
            throw new NotImplementedException();
        }
        private void ExecuteRecoverPassCommand(string username, string email)
        {
            throw new NotImplementedException();
        }
    }
}
