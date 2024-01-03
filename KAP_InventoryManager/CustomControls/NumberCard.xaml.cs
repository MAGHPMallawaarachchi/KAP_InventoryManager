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
    /// Interaction logic for NumberCard.xaml
    /// </summary>
    public partial class NumberCard : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NumberCard));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumberCard));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(NumberCard));

        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register("BackColor", typeof(System.Windows.Media.Brush), typeof(NumberCard));

        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register("IconColor", typeof(System.Windows.Media.Brush), typeof(NumberCard));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public System.Windows.Media.Brush BackColor
        {
            get { return (System.Windows.Media.Brush)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }
        public System.Windows.Media.Brush IconColor
        {
            get { return (System.Windows.Media.Brush)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }
        public NumberCard()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
