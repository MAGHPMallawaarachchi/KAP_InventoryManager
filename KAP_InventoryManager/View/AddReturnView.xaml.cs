using GalaSoft.MvvmLight.Messaging;
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
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KAP_InventoryManager.View
{
    /// <summary>
    /// Interaction logic for AddReturnView.xaml
    /// </summary>
    public partial class AddReturnView : Window
    {
        public AddReturnView()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, Notify);

            // Apply system title bar color when handle is created
            this.SourceInitialized += AddReturnView_SourceInitialized;
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "0")
            {
                textBox.Text = string.Empty;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = "0";
            }
        }
        private void Notify(NotificationMessage message)
        {
            if (message.Notification == "CloseDialog")
            {
                this.Close();
            }
        }

        private void AddReturnView_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;

                TryEnableImmersiveDarkMode(hwnd, true);

                Color bg = (FindResource("Color2") as SolidColorBrush)?.Color ?? Color.FromRgb(0x24, 0x25, 0x26);
                Color fg = Colors.White;

                SetTitleBarColor(hwnd, bg, fg);
            }
            catch { }
        }

        // --- Native title bar coloring via DWM ---
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_19 = 19; // Windows 10 1809
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_20 = 20; // Windows 10 1903+
        private const int DWMWA_CAPTION_COLOR = 35;              // Windows 11 22000+
        private const int DWMWA_TEXT_COLOR = 36;                 // Windows 11 22000+

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private static void TryEnableImmersiveDarkMode(IntPtr hwnd, bool enabled)
        {
            int on = enabled ? 1 : 0;
            _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_20, ref on, sizeof(int));
            _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_19, ref on, sizeof(int));
        }

        private static void SetTitleBarColor(IntPtr hwnd, Color background, Color text)
        {
            int bg = ToColorRef(background);
            int fg = ToColorRef(text);

            _ = DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref bg, sizeof(int));
            _ = DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref fg, sizeof(int));
        }

        private static int ToColorRef(Color c)
        {
            return c.R | (c.G << 8) | (c.B << 16);
        }

    }
}
