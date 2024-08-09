using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
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
        private decimal _buyingPrice;
        private decimal _unitPrice;
        private int _newQty;
        private List<string> _brands = new List<string> { "None" };
        private List<string> _categories;
        private List<string> _vehicleBrands = new List<string> { "TOYOTA", "NISSAN", "SUZUKI", "MITSUBISHI", "PERODUA", "ISUZU", "MAZDA", "DIHATSU", "HONDA" };
        private ItemModel _item;

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
                LoadCategoriesAsync();
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

        public ItemModel Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }

        public int NewQty
        {
            get => _newQty;
            set
            {
                _newQty = value;
                OnPropertyChanged(nameof(NewQty));
            }
        }

        public ICommand AddItemCommand { get; }
        public ICommand DiscardCommand { get; }

        public EditItemModalViewModel()
        {
            ItemRepository = new ItemRepository();

            Messenger.Default.Register<ItemModel>(this, OnMessageReceived);
            Messenger.Default.Send("RequestSelectedItem");

            AddItemCommand = new ViewModelCommand(ExecuteEditItemCommand, CanExecuteEditItemCommand);
            DiscardCommand = new ViewModelCommand(ExecuteDiscardCommand);
        }

        private async void OnMessageReceived(ItemModel item)
        {

            if(item != null)
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
                    BuyingPrice = Item.BuyingPrice;
                    UnitPrice = Item.UnitPrice;
                    VehicleBrand = Item.VehicleBrand;
                }

                // Unregister the message handler after receiving the item
                Messenger.Default.Unregister<ItemModel>(this, OnMessageReceived);
            }
        }

        private void ExecuteDiscardCommand(object obj)
        {
            ResetTextBoxes();
        }

        private bool CanExecuteEditItemCommand(object obj)
        {
            bool validate = !(string.IsNullOrEmpty(PartNo) || string.IsNullOrEmpty(Description) || BrandID == "None" || string.IsNullOrEmpty(BrandID) || string.IsNullOrEmpty(Category) || UnitPrice == 0);

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
                BuyingPrice = BuyingPrice,
                UnitPrice = UnitPrice,
                TotalQty = Item.TotalQty + NewQty,
                QtyInHand = Item.QtyInHand + NewQty,
            };

            ItemRepository.EditAsync(editedItem);

            //close the dialog view
            Messenger.Default.Send(new NotificationMessage("CloseDialog"));

            //send the edited item back to the details view
            Messenger.Default.Send(Item.PartNo, "ItemUpdated");
        }

        private async Task LoadBrandsAsync()
        {
            Brands = await Task.Run(() => ItemRepository.GetBrandsAsync());
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

        private async void LoadCategoriesAsync()
        {
            if (BrandID != null && BrandID != "None")
            {
                Categories = await Task.Run(() => ItemRepository.GetCategoriesAsync(BrandID));
            }
            else
            {
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
            BuyingPrice = Item.BuyingPrice;
            UnitPrice = Item.UnitPrice;
            NewQty = 0;
        }
    }
}
