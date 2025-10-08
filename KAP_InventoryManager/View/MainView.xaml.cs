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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using GalaSoft.MvvmLight.Messaging;

namespace KAP_InventoryManager.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            // Apply title bar color once the native handle is ready
            this.SourceInitialized += MainView_SourceInitialized;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MainView_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;

                // Optionally enable dark mode for consistent caption rendering (best-effort)
                TryEnableImmersiveDarkMode(hwnd, true);

                // Use Color2 from app resources; fallback to a dark neutral
                Color bg = (FindResource("Color2") as SolidColorBrush)?.Color ?? Color.FromRgb(0x24, 0x25, 0x26);
                Color fg = Colors.White;

                SetTitleBarColor(hwnd, bg, fg);
            }
            catch
            {
                // Ignore if OS doesn't support DWM attributes
            }
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
            // Try newer attribute, then older
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
            // COLORREF = 0x00BBGGRR
            return c.R | (c.G << 8) | (c.B << 16);
        }
    }

}
