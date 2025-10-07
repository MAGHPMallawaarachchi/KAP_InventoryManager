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
        private ConfirmPaymentModalView confirmPaymentWindow;
        private ViewPaymentModalView viewPaymentWindow;

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
            if (confirmPaymentWindow == null || !confirmPaymentWindow.IsLoaded)
            {
                confirmPaymentWindow = new ConfirmPaymentModalView();
                confirmPaymentWindow.Closed += (s, args) => confirmPaymentWindow = null;
                confirmPaymentWindow.Show();
            }
            else
            {
                if (confirmPaymentWindow.WindowState == WindowState.Minimized)
                {
                    confirmPaymentWindow.WindowState = WindowState.Normal;
                }
                confirmPaymentWindow.Activate();
            }
        }

        private void ViewPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewPaymentWindow == null || !viewPaymentWindow.IsLoaded)
            {
                viewPaymentWindow = new ViewPaymentModalView();
                viewPaymentWindow.Closed += (s, args) => viewPaymentWindow = null;
                viewPaymentWindow.Show();
            }
            else
            {
                if (viewPaymentWindow.WindowState == WindowState.Minimized)
                {
                    viewPaymentWindow.WindowState = WindowState.Normal;
                }
                viewPaymentWindow.Activate();
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var download = new DownloadSalesReportModalView();
            download.ShowDialog();
        }
    }
}
