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
        private readonly NotificationService _notificationService;
        private readonly ConcurrentDictionary<string, SlidingWindowAnalyzer> _analyzers;
        private readonly ConcurrentDictionary<string, string> _currentStates;

        // Optional Webhook URL (Replace with your actual Teams/Slack incoming webhook endpoint)
        private const string TeamsWebhookUrl = "";

        public PollingEngine(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            _notificationService = new NotificationService();
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

            // Assign results back to the model (triggers INotifyPropertyChanged for WPF)
            device.MovingAverageMs = movingAverage;
            device.CurrentState = newState;
            device.UptimePercentage = _dbManager.Get24HourUptimePercentage(device.Id);

            var previousState = _currentStates.GetOrAdd(device.IpAddress, "Unknown");

            // Only log to DB if the state has changed
            if (newState != previousState)
            {
                _currentStates[device.IpAddress] = newState;
                _dbManager.LogEvent(device.Id, newState, movingAverage);

                // TODO: fire an event to the WPF UI EventAggregator
                // Trigger alerts only when state deteriorates to Offline or Degraded
                if (newState == "Offline" || newState == "Degraded")
                {
                    // 1. Toast Notification
                    _notificationService.SendToastNotification(device.Name, device.IpAddress, newState);

                    // 2. Webhook Notification (Fire and forget task)
                    if (!string.IsNullOrEmpty(TeamsWebhookUrl))
                    {
                        _ = _notificationService.SendWebhookAlertAsync(TeamsWebhookUrl, device.Name, device.IpAddress, newState);
                    }
                }
            }
        }
    }
}
