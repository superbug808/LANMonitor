using LANMonitor.Models;
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
    /// Interaction logic for TracerouteDialog.xaml
    /// </summary>
    public partial class TracerouteDialog : Window
    {
        public TracerouteDialog(MonitoredDevice device)
        {
            InitializeComponent();

            DeviceHeaderTitle.Text = $"Traceroute Diagnostics: {device.Name} ({device.IpAddress})";

            TxtTracerouteLog.Text = string.IsNullOrWhiteSpace(device.LastTracerouteLog)
                ? "No traceroute diagnostic data recorded yet for this device."
                : device.LastTracerouteLog;
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtTracerouteLog.Text))
            {
                Clipboard.SetText(TxtTracerouteLog.Text);

                TxtStatusMessage.Text = "Copied!";
                TxtStatusMessage.Visibility = Visibility.Visible;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
