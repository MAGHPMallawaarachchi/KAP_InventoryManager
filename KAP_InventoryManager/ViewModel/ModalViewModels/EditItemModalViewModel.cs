using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    internal class EditItemModalViewModel : ViewModelBase
    {
        private string _partNo;
        private string _oemNo;
        private string _description;
        private string _brandId;
        private string _category;
        private string _vehicleBrand;
        private string _supplierId;
        private decimal _buyingPrice;
        private decimal _unitPrice;
        private List<string> _brands = new List<string> { "None" };
        private List<string> _categories;
        private List<string> _vehicleBrands = new List<string> { "TOYOTA", "NISSAN", "SUZUKI", "MITSUBISHI" };
        private ItemModel _item;

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
                LoadSupplierAndCategoriesAsync();
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

        public string SupplierID
        {
            get { return _supplierId; }
            set
            {
                _supplierId = value;
                OnPropertyChanged(nameof(SupplierID));
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

        public ItemModel Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }

        public ICommand AddItemCommand { get; }
        public ICommand DiscardCommand { get; }

        public EditItemModalViewModel()
        {
            ItemRepository = new ItemRepository();

            Messenger.Default.Register<ItemModel>(this, OnMessageReceived);
            Messenger.Default.Send(new object(), "RequestSelectedItem");

            AddItemCommand = new ViewModelCommand(ExecuteEditItemCommand, CanExecuteEditItemCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private async void OnMessageReceived(ItemModel item)
        {
            Item = item;

            if (item != null)
            {
                await LoadBrandsAsync();
                LoadVehicleBrands();

                PartNo = Item.PartNo;
                OEMNo = Item.OEMNo;
                Description = Item.Description;
                BrandID = Item.BrandID;
                Category = Item.Category;
                SupplierID = Item.SupplierID;
                BuyingPrice = Item.BuyingPrice;
                UnitPrice = Item.UnitPrice;
                VehicleBrand = Item.VehicleBrand;
            }
        }

        private void ExecuteDiscardCommand(object obj)
        {
            ResetTextBoxes();
        }

        private bool CanExecuteEditItemCommand(object obj)
        {
            bool validate = !(string.IsNullOrEmpty(PartNo) || string.IsNullOrEmpty(OEMNo) || string.IsNullOrEmpty(Description) || BrandID == "None" || string.IsNullOrEmpty(BrandID) || string.IsNullOrEmpty(Category) || string.IsNullOrEmpty(SupplierID) || BuyingPrice == 0 || UnitPrice == 0);

            return validate;
        }

        private void ExecuteEditItemCommand(object obj)
        {
            ItemModel editedItem = new ItemModel
            {
                PartNo = PartNo,
                OEMNo = OEMNo,
                Description = Description,
                BrandID = BrandID,
                Category = Category,
                VehicleBrand = VehicleBrand,
                SupplierID = SupplierID,
                BuyingPrice = BuyingPrice,
                UnitPrice = UnitPrice,
            };

            ItemRepository.Edit(editedItem);

            //close the dialog view
            Messenger.Default.Send(new NotificationMessage("CloseDialog"));

            //send the edited item back to the details view
            Messenger.Default.Send(Item.PartNo, "ItemUpdated");
        }

        private async Task LoadBrandsAsync()
        {
            Brands = await Task.Run(() => ItemRepository.GetBrands());
            Brands.Insert(0, "None");

            if (Item != null && !string.IsNullOrEmpty(Item.BrandID))
            {
                if (!Brands.Contains(Item.BrandID))
                {
                    Brands.Add(Item.BrandID);
                }

                BrandID = Item.BrandID;
            }
        }

        private void LoadVehicleBrands()
        {
            if (Item != null && !string.IsNullOrEmpty(Item.VehicleBrand))
            {
                if (!VehicleBrands.Contains(Item.VehicleBrand))
                {
                    VehicleBrands.Add(Item.VehicleBrand);
                }
            }
        }

        private async void LoadSupplierAndCategoriesAsync()
        {
            if (BrandID != null && BrandID != "None")
            {
                SupplierID = await Task.Run(() => ItemRepository.GetSupplierByBrand(BrandID));
                Categories = await Task.Run(() => ItemRepository.GetCategories(BrandID));
            }
            else
            {
                SupplierID = null;
                Categories = null;
            }
        }

        private void ResetTextBoxes()
        {
            OEMNo = Item.OEMNo;
            Description = Item.Description;
            BrandID = Item.BrandID;
            Category = Item.Category;
            VehicleBrand = Item.VehicleBrand;
            SupplierID = Item.SupplierID;
            BuyingPrice = Item.BuyingPrice;
            UnitPrice = Item.UnitPrice;
        }
    }
}
