using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANMonitor.Models
{
    public class MonitoredDevice
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string Name { get; set; }
        public string CurrentState { get; set; } // Online, Degraded, Offline
        public double MovingAverageMs { get; set; }
    }

}
