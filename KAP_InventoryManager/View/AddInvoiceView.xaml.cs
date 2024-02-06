using KAP_InventoryManager.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KAP_InventoryManager.View
{
    /// <summary>
    /// Interaction logic for AddInvoiceView.xaml
    /// </summary>
    public partial class AddInvoiceView : Window
    {
        private readonly AddInvoiceViewModel viewModel;

        public AddInvoiceView()
        {
            InitializeComponent();
            viewModel = new AddInvoiceViewModel();
            DataContext = viewModel;
        }
    }
}
