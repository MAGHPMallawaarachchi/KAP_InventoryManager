using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    public class NewItemModalViewModel : ViewModelBase
    {
        private string _partNo;
        private string _oemNo;
        private string _description;
        private string _brandId;
        private string _category;
        private string _vehicleBrand;
        private decimal _buyingPrice;
        private decimal _unitPrice;
        private List<string> _brands = new List<string> { "None"};
        private List<string> _categories;
        private List<string> _vehicleBrands;

        private readonly IItemRepository ItemRepository;

        public string PartNo
        {
            get { return _partNo; }
            set
            {
                _partNo = value;
                OnPropertyChanged(nameof(PartNo));
            }
        }

        public string OEMNo
        {
            get { return _oemNo; }
            set
            {
                _oemNo = value;
                OnPropertyChanged(nameof(OEMNo));
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public string BrandID
        {
            get { return _brandId; }
            set
            {
                _brandId = value;
                OnPropertyChanged(nameof(BrandID));

                GetCategories();
            }
        }

        public string Category
        {
            get { return _category; }
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public string VehicleBrand
        {
            get { return _vehicleBrand; }
            set
            {
                _vehicleBrand = value;
                OnPropertyChanged(nameof(VehicleBrand));
            }
        }

        public decimal BuyingPrice
        {
            get { return _buyingPrice; }
            set
            {
                _buyingPrice = value;
                OnPropertyChanged(nameof(BuyingPrice));
            }
        }

        public decimal UnitPrice
        {
            get { return _unitPrice; }
            set
            {
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
            }
        }

        public List<string> Brands
        {
            get { return _brands; }
            set
            {
                _brands = value;
                OnPropertyChanged(nameof(Brands));
            }
        }

        public List<string> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public List<string> VehicleBrands
        {
            get { return _vehicleBrands; }
            set
            {
                _vehicleBrands = value;
                OnPropertyChanged(nameof(VehicleBrands));
            }
        }

        public ICommand AddItemCommand { get; }
        public ICommand DiscardCommand { get; }

        public NewItemModalViewModel()
        {
            ItemRepository = new ItemRepository();
            AddItemCommand = new ViewModelCommand(ExecuteAddItemCommand, CanExecuteAddItemCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);

            GetBrands();
            VehicleBrands = new List<string> { "TOYOTA", "NISSAN", "SUZUKI", "MITSUBISHI", "PERODUA", "ISUZU", "MAZDA", "DIHATSU", "HONDA"};
        }

        private void ExecuteDiscardCommand(object obj)
        {
            ClearTextBoxes();
        }

        private bool CanExecuteAddItemCommand(object obj)
        {
            bool validate;

            if (string.IsNullOrEmpty(PartNo) || string.IsNullOrEmpty(Description) || BrandID == "None" || string.IsNullOrEmpty(BrandID) || string.IsNullOrEmpty(Category) || UnitPrice == 0)
            {
                validate = false;
            }
            else
            {
                validate = true;
            }

            return validate;
        }

        private void ExecuteAddItemCommand(object obj)
        {
            ItemModel newItem = new ItemModel
            {
                PartNo = PartNo,
                OEMNo = OEMNo,
                Description = Description,
                BrandID = BrandID,
                Category = Category,
                VehicleBrand = VehicleBrand,
                BuyingPrice = BuyingPrice,
                UnitPrice = UnitPrice,
            };

            ItemRepository.AddAsync(newItem);
            ClearTextBoxes();
            Messenger.Default.Send("NewItemAdded");
        }

        private async void GetBrands()
        {
            Brands = await ItemRepository.GetBrandsAsync();
            Brands.Insert(0, "None");
        }

        private async void GetCategories()
        {
            if(BrandID == null || BrandID != "None")
                Categories = await ItemRepository.GetCategoriesAsync(BrandID);
            else 
                Categories = null;
        }

        private void ClearTextBoxes()
        {
            PartNo = string.Empty;
            OEMNo = string.Empty;
            Description = string.Empty;
            BrandID = Brands.FirstOrDefault();
            Category = string.Empty;
            VehicleBrand = string.Empty;
            BuyingPrice = 0;
            UnitPrice = 0;
        }
    }
}
