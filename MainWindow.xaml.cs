using LANMonitor.Models;
using LANMonitor.Services;
using LANMonitor.ViewModels;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LANMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            // Initialize DB on startup
            var dbManager = new DatabaseManager();
            dbManager.InitializeDatabase();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.StopMonitoring();
            }
            base.OnClosing(e);
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is MonitoredDevice selectedDevice)
            {
                var dialog = new TracerouteDialog(selectedDevice)
                {
                    Owner = Window.GetWindow(this)
                };
                dialog.ShowDialog();
            }
        }
    }
}