using GalaSoft.MvvmLight.Messaging;
using KAP_InventoryManager.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KAP_InventoryManager.View
{
    /// <summary>
    /// Interaction logic for AddInvoiceView.xaml
    /// </summary>
    public partial class AddInvoiceView : Window
    {

        public AddInvoiceView()
        {
            InitializeComponent();

            var viewModel = DataContext as AddInvoiceViewModel;
            if (viewModel != null)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            // Handle window closing to dispose resources
            this.Closed += AddInvoiceView_Closed;

            // Apply system title bar color when handle is created
            this.SourceInitialized += AddInvoiceView_SourceInitialized;
        }

        private void AddInvoiceView_Closed(object sender, EventArgs e)
        {
            var viewModel = DataContext as AddInvoiceViewModel;
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                viewModel.Dispose();
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AddInvoiceViewModel.IsLoading))
            {
                var viewModel = (AddInvoiceViewModel)sender;
                if (viewModel.IsLoading)
                {
                    StartSpinnerAnimation();
                }
                else
                {
                    StopSpinnerAnimation();
                }
            }
        }

        private void StartSpinnerAnimation()
        {
            var storyboard = (Storyboard)FindResource("SpinnerRotateStoryboard");
            storyboard.Begin(LoadingOverlay, true); // Begin the animation
        }

        private void StopSpinnerAnimation()
        {
            var storyboard = (Storyboard)FindResource("SpinnerRotateStoryboard");
            storyboard.Stop(LoadingOverlay); // Stop the animation
        }

        private void AddInvoiceView_SourceInitialized(object sender, EventArgs e)
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

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.IsEditable)
            {
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                if (textBox != null)
                {
                    textBox.TextChanged += TextBox_TextChanged;
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var comboBox = textBox?.TemplatedParent as ComboBox;
            if (comboBox != null && !comboBox.IsDropDownOpen)
            {
                comboBox.IsDropDownOpen = true;
            }
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

        private void ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
            var currentText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Update the search text manually
            var viewModel = DataContext as AddInvoiceViewModel;
            viewModel.PartNoSearchText = currentText;
            e.Handled = true;

            // Reopen the dropdown
            comboBox.IsDropDownOpen = true;
            textBox.SelectionStart = currentText.Length;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            PartNumberComboBox.Focus();
        }
    }
}
