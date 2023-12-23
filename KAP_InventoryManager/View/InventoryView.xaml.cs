using KAP_InventoryManager.View.Modals;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KAP_InventoryManager.View
{
    /// <summary>
    /// Interaction logic for InventoryView.xaml
    /// </summary>
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
        }

        public void ExecuteSearch(string searchText)
        {

        }

        private void OnSearchTextChanged(object sender, string searchText)
        {

        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            var newItemModalWindow = new NewItemModal();
            newItemModalWindow.ShowDialog();
        }

    }
}
