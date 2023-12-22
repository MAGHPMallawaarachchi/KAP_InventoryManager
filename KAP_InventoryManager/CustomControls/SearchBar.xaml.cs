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
    /// Interaction logic for SearchBar.xaml
    /// </summary>
    public partial class SearchBar : UserControl
    {
        public static readonly DependencyProperty SearchFunctionProperty = DependencyProperty.Register("SearchFunction", typeof(Action<string>), typeof(SearchBar));

        public Action<string> SearchFunction
        {
            get { return (Action<string>)GetValue(SearchFunctionProperty); }
            set { SetValue(SearchFunctionProperty, value); }
        }

        public SearchBar()
        {
            InitializeComponent();
        }

        private void OnSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string searchText = SearchTextBox.Text;
                SearchFunction?.Invoke(searchText);
            }
        }
    }
}
