using LANMonitor.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace LANMonitor.Services
{
    public class PollingEngine
    {
        private readonly DatabaseManager _dbManager;
        private readonly ConcurrentDictionary<string, SlidingWindowAnalyzer> _analyzers;
        private readonly ConcurrentDictionary<string, string> _currentStates;

        public PollingEngine(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            _analyzers = new ConcurrentDictionary<string, SlidingWindowAnalyzer>();
            _currentStates = new ConcurrentDictionary<string, string>();
        }

        public async Task StartMonitoringAsync(List<MonitoredDevice> devices, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var pingTasks = devices.Select(d => PingAndAnalyzeAsync(d));
                await Task.WhenAll(pingTasks);

                await Task.Delay(5000, cancellationToken); // 5-second polling interval
            }
        }

        private async Task PingAndAnalyzeAsync(MonitoredDevice device)
        {
            var analyzer = _analyzers.GetOrAdd(device.IpAddress, new SlidingWindowAnalyzer(10));
            long latency = -1;

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(device.IpAddress, 2000); // 2 second timeout

                if (reply.Status == IPStatus.Success)
                {
                    latency = reply.RoundtripTime;
                }
            }
            catch
            {
                // Network failure or unresolvable
            }

            analyzer.AddPingResult(latency);
            double movingAverage = analyzer.GetMovingAverage();
            string newState = analyzer.DetermineState(degradationThresholdMs: 100); // 100ms threshold for LAN

            var previousState = _currentStates.GetOrAdd(device.IpAddress, "Unknown");

            // Only log to DB if the state has changed
            if (newState != previousState)
            {
                _currentStates[device.IpAddress] = newState;
                _dbManager.LogEvent(device.Id, newState, movingAverage);

                // Here you would also fire an event to the WPF UI EventAggregator
            }
        }
    }
}
