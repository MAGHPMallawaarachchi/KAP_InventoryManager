using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class CustomerModel
    {
        public int? Id { get; set; }
        public string CustomerID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string ContactNo { get; set; }
        public string PaymentType { get; set; }
        public decimal DebtLimit { get; set; }
        public decimal? TotalDebt { get; set;}
        public string RepID { get; set; }
    }
}
