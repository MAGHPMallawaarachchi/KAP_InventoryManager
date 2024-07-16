using GalaSoft.MvvmLight.Messaging;
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
using System.Windows.Shapes;

namespace KAP_InventoryManager.View
{
    /// <summary>
    /// Interaction logic for AddInvoiceView.xaml
    /// </summary>
    public partial class AddInvoiceView : Window
    {
        private readonly AddInvoiceViewModel viewModel;

        public AddInvoiceView()
        {
            InitializeComponent();
            viewModel = new AddInvoiceViewModel();
            DataContext = viewModel;
            Messenger.Default.Register<NotificationMessage>(this, Notify);
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

        private void Notify(NotificationMessage message)
        {
            if (message.Notification == "CloseDialog")
            {
                this.Close();
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
