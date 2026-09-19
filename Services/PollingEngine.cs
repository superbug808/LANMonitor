using LANMonitor.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LANMonitor.Services
{
    public class PollingEngine
    {
        private readonly DatabaseManager _dbManager;
        private readonly NotificationService _notificationService;
        private readonly TcpProbeEngine _tcpProbeEngine;
        private readonly TracerouteEngine _tracerouteEngine;
        private readonly ConcurrentDictionary<string, SlidingWindowAnalyzer> _analyzers;
        private readonly ConcurrentDictionary<string, string> _currentStates;

        // Optional Webhook URL (Replace with your actual Teams/Slack incoming webhook endpoint)
        private const string TeamsWebhookUrl = "";

        public PollingEngine(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            _notificationService = new NotificationService();
            _tcpProbeEngine = new TcpProbeEngine();
            _tracerouteEngine = new TracerouteEngine();
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

                    // 1. TCP Port Probe (Runs if device has a TargetPort specified, e.g., 80, 9100, 3389)
                    if (device.TargetPort > 0)
                    {
                        var (isOpen, _) = await _tcpProbeEngine.ProbePortAsync(device.IpAddress, device.TargetPort, 1500);
                        device.IsPortOpen = isOpen;
                    }
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

            // Push point to chart history on UI thread
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                device.LatencyHistory.Add(latency >= 0 ? latency : 0);
                if (device.LatencyHistory.Count > 30)
                {
                    device.LatencyHistory.RemoveAt(0);
                }
            });

            var previousState = _currentStates.GetOrAdd(device.IpAddress, "Unknown");

            // Only log to DB if the state has changed
            if (newState != previousState)
            {
                _currentStates[device.IpAddress] = newState;
                _dbManager.LogEvent(device.Id, newState, movingAverage);

                // 2. Automated Traceroute on State Transition (Triggers when host transitions into Offline)
                if (newState == "Offline" && previousState != "Offline")
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var hops = await _tracerouteEngine.RunTracerouteAsync(device.IpAddress);
                            string formattedLog = $"--- Automated Traceroute ({DateTime.Now:yyyy-MM-dd HH:mm:ss}) ---\n" +
                                                 string.Join(Environment.NewLine, hops);
                            
                            // 1. Persist the traceroute log to SQLite
                            _dbManager.UpdateTracerouteLog(device.Id, formattedLog);

                            // Dispatch update to WPF UI thread safely
                            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                            {
                                device.LastTracerouteLog = formattedLog;
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Traceroute Engine] Error: {ex.Message}");
                        }
                    });
                }

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

                    // 3. Email Alert
                    string smtpHost = _dbManager.GetSetting("SmtpHost");
                    string fromEmail = _dbManager.GetSetting("FromEmail");
                    string toEmail = _dbManager.GetSetting("ToEmail");
                    int.TryParse(_dbManager.GetSetting("SmtpPort", "587"), out int port);

                    if (!string.IsNullOrEmpty(smtpHost) && !string.IsNullOrEmpty(toEmail))
                    {
                        _ = _notificationService.SendEmailAlertAsync(smtpHost, port, fromEmail, "", toEmail, device.Name, device.IpAddress, newState);
                    }
                }
            }
        }
    }
}
