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
        private int _qtyInHand;
        private decimal _buyingPrice;
        private decimal _unitPrice;
        private List<string> _brands = new List<string> { "None"};
        private List<string> _categories;
        private List<string> _vehicleBrands;

        private readonly IItemRepository ItemRepository;

        public string PartNo
        {
            get => _partNo;
            set
            {
                _partNo = value;
                OnPropertyChanged(nameof(PartNo));
            }
        }

        public string OEMNo
        {
            get => _oemNo;
            set
            {
                _oemNo = value;
                OnPropertyChanged(nameof(OEMNo));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public string BrandID
        {
            get => _brandId;
            set
            {
                _brandId = value;
                OnPropertyChanged(nameof(BrandID));

                GetCategories();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public string VehicleBrand
        {
            get => _vehicleBrand; 
            set
            {
                _vehicleBrand = value;
                OnPropertyChanged(nameof(VehicleBrand));
            }
        }

        public int QtyInHand
        {
            get => _qtyInHand;
            set
            {
                _qtyInHand = value;
                OnPropertyChanged(nameof(QtyInHand));
            }
        }

        public decimal BuyingPrice
        {
            get => _buyingPrice;
            set
            {
                _buyingPrice = value;
                OnPropertyChanged(nameof(BuyingPrice));
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
            }
        }

        public List<string> Brands
        {
            get => _brands;
            set
            {
                _brands = value;
                OnPropertyChanged(nameof(Brands));
            }
        }

        public List<string> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public List<string> VehicleBrands
        {
            get => _vehicleBrands;
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
            AddItemCommand = new ViewModelCommand(ExecuteAddItemCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);

            GetBrands();
            VehicleBrands = new List<string> { "TOYOTA", "NISSAN", "SUZUKI", "MITSUBISHI", "PERODUA", "ISUZU", "MAZDA", "DIHATSU", "HONDA"};
        }

        private void ExecuteDiscardCommand(object obj)
        {
            ClearTextBoxes();
        }

        private bool CanExecuteAddItemCommand()
        {
            bool validate;

            if (string.IsNullOrEmpty(PartNo) || string.IsNullOrEmpty(Description) || BrandID == "None" || string.IsNullOrEmpty(BrandID) || string.IsNullOrEmpty(Category) || UnitPrice == 0 || QtyInHand == 0)
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
            if(CanExecuteAddItemCommand())
            {
                ItemModel newItem = new ItemModel
                {
                    PartNo = PartNo,
                    OEMNo = OEMNo,
                    Description = Description,
                    BrandID = BrandID,
                    Category = Category,
                    VehicleBrand = VehicleBrand,
                    QtyInHand = QtyInHand,
                    BuyingPrice = BuyingPrice,
                    UnitPrice = UnitPrice,
                };

                ItemRepository.AddAsync(newItem);
                ClearTextBoxes();
                Messenger.Default.Send("NewItemAdded");
            }
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
            QtyInHand = 0;
            BuyingPrice = 0;
            UnitPrice = 0;
        }
    }
}
