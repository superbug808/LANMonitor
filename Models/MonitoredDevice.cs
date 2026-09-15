using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
    }

}
