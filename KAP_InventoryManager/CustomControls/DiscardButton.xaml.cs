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

namespace KAP_InventoryManager.CustomControls
{
    /// <summary>
    /// Interaction logic for DiscardButton.xaml
    /// </summary>
    public partial class DiscardButton : UserControl
    {
        public DiscardButton()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public RoutedEventHandler ButtonClickHandler { get; set; }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Invoke the dynamically assigned click event handler
            ButtonClickHandler?.Invoke(sender, e);
        }
    }
}
