using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KAP_InventoryManager.View.Modals
{
    /// <summary>
    /// Interaction logic for ViewPaymentModalView.xaml
    /// </summary>
    public partial class ViewPaymentModalView : Window
    {
        public ViewPaymentModalView()
        {
            InitializeComponent();
            DataContext = new ViewModel.ModalViewModels.ViewPaymentModalViewModel();
            Messenger.Default.Register<NotificationMessage>(this, Notify);
            Messenger.Default.Register<string>(this, OnMessageReceived);
        }

        private void OnMessageReceived(string message)
        {
            if (message == "OpenAddPaymentModal")
            {
                var confirmPaymentWindow = new ConfirmPaymentModalView();
                confirmPaymentWindow.Show();
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Notify(NotificationMessage message)
        {
            if (message.Notification == "CloseDialog")
            {
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Messenger.Default.Unregister(this);
            base.OnClosed(e);
        }
    }
}
