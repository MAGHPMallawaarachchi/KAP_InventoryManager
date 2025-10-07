using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class InvoiceItemModel : INotifyPropertyChanged
    {
        private int _no;
        private string _invoiceNo;
        private string _partNo;
        private string _brandID;
        private string _description;
        private decimal _buyingPrice;
        private decimal _unitPrice;
        private int _quantity;
        private decimal _discount;
        private decimal _amount;
        private string _customerID;

        public int No
        {
            get => _no;
            set
            {
                if (_no != value)
                {
                    _no = value;
                    OnPropertyChanged();
                }
            }
        }

        public string InvoiceNo
        {
            get => _invoiceNo;
            set
            {
                if (_invoiceNo != value)
                {
                    _invoiceNo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PartNo
        {
            get => _partNo;
            set
            {
                if (_partNo != value)
                {
                    _partNo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BrandID
        {
            get => _brandID;
            set
            {
                if (_brandID != value)
                {
                    _brandID = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal BuyingPrice
        {
            get => _buyingPrice;
            set
            {
                if (_buyingPrice != value)
                {
                    _buyingPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Discount
        {
            get => _discount;
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CustomerID
        {
            get => _customerID;
            set
            {
                if (_customerID != value)
                {
                    _customerID = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
