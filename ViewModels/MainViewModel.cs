using LANMonitor.Models;
using LANMonitor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LANMonitor.ViewModels
{
    public class MainViewModel
    {
        private readonly DatabaseManager _dbManager;
        private readonly PollingEngine _pollingEngine;
        private readonly CancellationTokenSource _cts;

        public ObservableCollection<MonitoredDevice> Devices { get; set; }
        public ICommand AddDeviceCommand { get; }

        public MainViewModel()
        {

            _dbManager = new DatabaseManager();
            _dbManager.InitializeDatabase();

            _pollingEngine = new PollingEngine(_dbManager);
            _cts = new CancellationTokenSource();

            // Initial device list for live testing (replace/expand with real local IPs)
            Devices = new ObservableCollection<MonitoredDevice>
            {
                // Dummy data for initial UI testing
                new MonitoredDevice { Name = "Main Router", IpAddress = "192.168.1.1", CurrentState = "Online", MovingAverageMs = 12.5 },
                new MonitoredDevice { Name = "Office Printer", IpAddress = "192.168.1.50", CurrentState = "Offline", MovingAverageMs = 0 },

                // New data for testing
                new MonitoredDevice { Id = 1, Name = "Localhost Loopback", IpAddress = "127.0.0.1", CurrentState = "Unknown", MovingAverageMs = 0 },
                new MonitoredDevice { Id = 2, Name = "Primary Gateway", IpAddress = "192.168.1.1", CurrentState = "Unknown", MovingAverageMs = 0 },
                new MonitoredDevice { Id = 3, Name = "Google Public DNS", IpAddress = "8.8.8.8", CurrentState = "Unknown", MovingAverageMs = 0 }
            };

            // Instantiate command for button binding
            AddDeviceCommand = new RelayCommand(OpenAddDeviceDialog);

            // Launch background polling loop without blocking the UI thread
            Task.Run(() => _pollingEngine.StartMonitoringAsync(Devices.ToList(), _cts.Token));
        }

        private void OpenAddDeviceDialog()
        {
            var dialog = new AddDeviceWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                // Save to Database
                int newId = _dbManager.AddDevice(dialog.DeviceName, dialog.IpAddress);

                // Create Model instance
                var newDevice = new MonitoredDevice
                {
                    Id = newId,
                    Name = dialog.DeviceName,
                    IpAddress = dialog.IpAddress,
                    CurrentState = "Unknown",
                    MovingAverageMs = 0
                };

                // Add to ObservableCollection so UI updates instantly
                Devices.Add(newDevice);
            }
        }

        public void StopMonitoring()
        {
            _cts?.Cancel();
        }
    }
}
