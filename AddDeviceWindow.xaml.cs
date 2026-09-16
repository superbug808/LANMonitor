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

namespace LANMonitor
{
    /// <summary>
    /// Interaction logic for AddDeviceWindow.xaml
    /// </summary>
    public partial class AddDeviceWindow : Window
    {
        public string DeviceName => TxtName.Text.Trim();
        public string IpAddress => TxtIp.Text.Trim();

        public AddDeviceWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DeviceName) || string.IsNullOrWhiteSpace(IpAddress))
            {
                MessageBox.Show("Please enter both a Device Name and an IP Address.", "Validation Error", MessageBoxButton.OK, MessageBoxError);
                return;
            }

            DialogResult = true;
        }
    }
}
