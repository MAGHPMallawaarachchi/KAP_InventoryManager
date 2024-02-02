using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface ISalesRepRepository
    {
        List<string> GetAllRepIds();
    }
}
