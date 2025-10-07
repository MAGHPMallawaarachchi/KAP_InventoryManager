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
    /// Interaction logic for InvoicesView.xaml
    /// </summary>
    public partial class InvoicesView : UserControl
    {
        private AddInvoiceView addInvoiceWindow;

        public InvoicesView()
        {
            InitializeComponent();
        }

        private void AddInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (addInvoiceWindow == null || !addInvoiceWindow.IsLoaded)
            {
                addInvoiceWindow = new AddInvoiceView();
                addInvoiceWindow.Closed += (s, args) => addInvoiceWindow = null;
                addInvoiceWindow.Show();
            }
            else
            {
                if (addInvoiceWindow.WindowState == WindowState.Minimized)
                {
                    addInvoiceWindow.WindowState = WindowState.Normal;
                }
                addInvoiceWindow.Activate();
            }
        }

        private void ConfirmPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmPaymentWindow = new ConfirmPaymentModalView();
            confirmPaymentWindow.Show();
        }

        private void ViewPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var viewPaymentWindow = new ViewPaymentModalView();
            viewPaymentWindow.Show();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var download = new DownloadSalesReportModalView();
            download.ShowDialog();
        }
    }
}
