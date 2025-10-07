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
using GalaSoft.MvvmLight.Messaging;

namespace KAP_InventoryManager.View.Modals
{
    /// <summary>
    /// Interaction logic for EditPaymentModalView.xaml
    /// </summary>
    public partial class EditPaymentModalView : Window
    {
        public EditPaymentModalView()
        {
            InitializeComponent();

            // Register to receive close dialog message
            Messenger.Default.Register<NotificationMessage>(this, (message) =>
            {
                if (message.Notification == "CloseDialog")
                {
                    this.Close();
                }
            });
        }

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
