using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace KAP_InventoryManager.ViewModel
{
    public class CustomersViewModel : ViewModelBase
    {
        private double _progressPercentage;
      
        public CustomersViewModel() 
        {
            ProgressPercentage = 25;
        }

        public double ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                _progressPercentage = value;
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }
    }
}
