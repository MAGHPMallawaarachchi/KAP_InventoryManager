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
    /// Interaction logic for Button.xaml
    /// </summary>
    public partial class Button : UserControl
    {
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(Button));

        public static readonly DependencyProperty ButtonIconProperty =
            DependencyProperty.Register("ButtonIcon", typeof(string), typeof(Button));

        public static readonly DependencyProperty ButtonBackgroundColorProperty =
            DependencyProperty.Register("ButtonBackgroundColor", typeof(System.Windows.Media.Brush), typeof(Button));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public string ButtonIcon
        {
            get { return (string)GetValue(ButtonIconProperty); }
            set { SetValue(ButtonIconProperty, value); }
        }

        public System.Windows.Media.Brush ButtonBackgroundColor
        {
            get { return (System.Windows.Media.Brush)GetValue(ButtonBackgroundColorProperty); }
            set { SetValue(ButtonBackgroundColorProperty, value); }
        }

        // Regular property for the click event handler
        public RoutedEventHandler ButtonClickHandler { get; set; }

        public Button()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Invoke the dynamically assigned click event handler
            ButtonClickHandler?.Invoke(sender, e);
        }
    }
}
