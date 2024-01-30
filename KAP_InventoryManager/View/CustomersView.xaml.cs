using KAP_InventoryManager.View.Modals;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KAP_InventoryManager.View
{
    /// <summary>
    /// Interaction logic for CustomersView.xaml
    /// </summary>
    public partial class CustomersView : UserControl
    {
        private double debtPercentage;
        private CustomersViewModel viewModel;

        public CustomersView()
        {
            InitializeComponent();
            viewModel = new CustomersViewModel();
            DataContext = viewModel;
            SetProgress(viewModel.DebtPercentage);
        }

        private void SetProgress(double percentage)
        {
            debtPercentage = Math.Max(0, Math.Min(100, percentage));

            double angle = (360 * (100 - debtPercentage)) / 100;
            ProgressBarClip.Rect = new Rect(0, 0, 100, 100);
            ProgressBarClip.Transform = new RotateTransform(angle, 50, 0);
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var newCustomerModalWindow = new NewCustomerModal();
            newCustomerModalWindow.ShowDialog();
        }
    }
}
