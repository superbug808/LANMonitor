using LANMonitor.Models;
using LANMonitor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LANMonitor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager;
        private readonly PollingEngine _pollingEngine;
        private readonly CancellationTokenSource _cts;

        private MonitoredDevice _selectedDevice;

        public ObservableCollection<MonitoredDevice> Devices { get; set; }

        // Tracks the row highlighted in the WPF DataGrid
        public MonitoredDevice SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }

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

            // CanExecute check ensures button is only active when a device is selected
            DeleteDeviceCommand = new RelayCommand(DeleteSelectedDevice, () => SelectedDevice != null);

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

        private void DeleteSelectedDevice()
        {
            if (SelectedDevice == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to remove '{SelectedDevice.Name}' ({SelectedDevice.IpAddress}) from monitoring?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // 1. Delete from SQLite Database
                _dbManager.DeleteDevice(SelectedDevice.Id);

                // 2. Remove from ObservableCollection (DataGrid updates immediately)
                Devices.Remove(SelectedDevice);

                // 3. Reset selection
                SelectedDevice = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StopMonitoring()
        {
            _cts?.Cancel();
        }
    }
}
