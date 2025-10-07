using KAP_InventoryManager.View.Modals;
using KAP_InventoryManager.ViewModel;
using LiveCharts;
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
        private readonly CustomersViewModel viewModel;

        public CustomersView()
        {
            InitializeComponent();
            viewModel = new CustomersViewModel();
            DataContext = viewModel;

            // Subscribe to property changes to update chart
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateChart();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(viewModel.DebtPercentage) || e.PropertyName == nameof(viewModel.DebtRemainder))
            {
                Dispatcher.Invoke(() => UpdateChart());
            }
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var newCustomerModalWindow = new NewCustomerModal();
            newCustomerModalWindow.ShowDialog();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var download = new DownloadCustomerReportModalView();
            download.ShowDialog();
        }

        private void DownloadAllButton_Click(object sender, RoutedEventArgs e)
        {
            var download = new DownloadAllCustomerReportsModalView();
            download.ShowDialog();
        }

        private void DownloadCustomerInvoiceSummaryButton_Click(Object sender, RoutedEventArgs e)
        {
            var download = new DownloadCustomerInvoiceSummaryModalView();
            download.ShowDialog();
        }

        private void UpdateChart()
        {
            // If debt exceeds 100%, fill entire chart with green but display actual percentage
            if (viewModel.DebtPercentage > 100)
            {
                DebtChart.Series[0].Values = new ChartValues<double> { 100 };
                DebtChart.Series[1].Values = new ChartValues<double> { 0 };
            }
            else
            {
                DebtChart.Series[0].Values = new ChartValues<double> { viewModel.DebtPercentage };
                DebtChart.Series[1].Values = new ChartValues<double> { viewModel.DebtRemainder };
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editCustomerModalWindow = new EditCustomerModalView();
            editCustomerModalWindow.ShowDialog();
        }
    }
}
