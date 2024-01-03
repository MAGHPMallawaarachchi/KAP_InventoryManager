using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.ViewModel.InventoryPanelViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        private string _partNumber;

        public string PartNumber
        {
            get { return _partNumber; }
            set
            {
                _partNumber = value;
                OnPropertyChanged(nameof(PartNumber));
            }
        }

        public DetailsViewModel(string partNumber)
        {
            PartNumber = partNumber;
        }

        public DetailsViewModel()
        {
            
        }
    }
}
