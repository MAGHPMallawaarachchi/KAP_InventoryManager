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

namespace KAP_InventoryManager.ViewModel.ModalViewModels
{
    public class NewItemModalViewModel : ViewModelBase
    {
        private string _partNo;
        private string _oemNo;
        private string _description;
        private string _brandId;
        private string _category;
        private string _supplierId;
        private decimal _buyingPrice;
        private decimal _unitPrice;

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

        public ICommand AddItemCommand { get; }

        public NewItemModalViewModel()
        {
            ItemRepository = new ItemRepository();
            AddItemCommand = new ViewModelCommand(ExecuteAddItemCommand, CanExecuteAddItemCommand);
        }

        private bool CanExecuteAddItemCommand(object obj)
        {
            bool validate;

            if (string.IsNullOrEmpty(PartNo) || string.IsNullOrEmpty(OEMNo) || string.IsNullOrEmpty(Description) || string.IsNullOrEmpty(BrandID) || string.IsNullOrEmpty(Category) || string.IsNullOrEmpty(SupplierID) || BuyingPrice == 0 || UnitPrice == 0)
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
            try
            {
                ItemModel newItem = new ItemModel
                {
                    PartNo = PartNo,
                    OEMNo = OEMNo,
                    Description = Description,
                    BrandID = BrandID,
                    Category = Category,
                    SupplierID = SupplierID,
                    BuyingPrice = BuyingPrice,
                    UnitPrice = UnitPrice,
                };

                ItemRepository.Add(newItem);
                MessageBox.Show("Item added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Failed to add item. MySQL Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
