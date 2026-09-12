using LANMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANMonitor.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<MonitoredDevice> Devices { get; set; }

        public MainViewModel()
        {
            Devices = new ObservableCollection<MonitoredDevice>
        {
            // Dummy data for initial UI testing
            new MonitoredDevice { Name = "Main Router", IpAddress = "192.168.1.1", CurrentState = "Online", MovingAverageMs = 12.5 },
            new MonitoredDevice { Name = "Office Printer", IpAddress = "192.168.1.50", CurrentState = "Offline", MovingAverageMs = 0 }
        };
        }
    }
}
