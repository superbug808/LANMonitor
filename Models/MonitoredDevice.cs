using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LANMonitor.Models
{
    public class MonitoredDevice : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _ipAddress;
        private double _movingAverageMs;
        private string _currentState = "Unknown";
        private double _uptimePercentage;
        private int _targetPort;
        private bool _isPortOpen;
        private string _lastTracerouteLog;

        public int Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(); } }
        }

        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        public string IpAddress
        {
            get => _ipAddress;
            set { if (_ipAddress != value) { _ipAddress = value; OnPropertyChanged(); } }
        }

        public double MovingAverageMs
        {
            get => _movingAverageMs;
            set { if (_movingAverageMs != value) { _movingAverageMs = value; OnPropertyChanged(); } }
        }

        public string CurrentState
        {
            get => _currentState;
            set { if (_currentState != value) { _currentState = value; OnPropertyChanged(); } }
        }

        public double UptimePercentage
        {
            get => _uptimePercentage;
            set { if (_uptimePercentage != value) { _uptimePercentage = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Optional TCP port to probe (e.g., 80 for Web, 3389 for RDP, 9100 for Printers). 
        /// Set to 0 to disable TCP probing.
        /// </summary>
        public int TargetPort
        {
            get => _targetPort;
            set { if (_targetPort != value) { _targetPort = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Indicates whether the specified TargetPort responds to TCP SYN connection requests.
        /// </summary>
        public bool IsPortOpen
        {
            get => _isPortOpen;
            set { if (_isPortOpen != value) { _isPortOpen = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Holds the formatted output from the most recent automated ICMP traceroute run when the device went offline.
        /// </summary>
        public string LastTracerouteLog
        {
            get => _lastTracerouteLog;
            set { if (_lastTracerouteLog != value) { _lastTracerouteLog = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Sliding window latency buffer for UI chart rendering.
        /// </summary>
        public ObservableCollection<double> LatencyHistory { get; set; } = new ObservableCollection<double>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /*public class MonitoredDevice : INotifyPropertyChanged
    {

        private string _currentState;
        private double _movingAverageMs;

        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string Name { get; set; }
        //public string CurrentState { get; set; } // Online, Degraded, Offline
        //public double MovingAverageMs { get; set; }

        private double _uptimePercentage = 100.0;

        // Stores rolling ping values for LiveCharts
        public ObservableCollection<double> LatencyHistory { get; } = new ObservableCollection<double>();

        // Dynamic property: Triggers UI update when state changes
        public string CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    OnPropertyChanged();
                }
            }
        }

        // Dynamic property: Triggers UI update when latency changes
        public double MovingAverageMs
        {
            get => _movingAverageMs;
            set
            {
                if (_movingAverageMs != value)
                {
                    _movingAverageMs = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Dynamic property: Displays 24-hour uptime percentage (e.g., 99.8%)
        public double UptimePercentage
        {
            get => _uptimePercentage;
            set
            {
                if (_uptimePercentage != value)
                {
                    _uptimePercentage = value;
                    OnPropertyChanged();
                }
            }
        }

    }*/

}
