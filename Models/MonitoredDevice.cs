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

    }

}
