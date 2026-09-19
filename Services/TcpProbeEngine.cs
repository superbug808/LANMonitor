using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LANMonitor.Services
{
    public class TcpProbeEngine
    {
        public async Task<(bool IsOpen, double LatencyMs)> ProbePortAsync(string ipAddress, int port, int timeoutMs = 2000)
        {
            using var client = new TcpClient();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var cts = new CancellationTokenSource(timeoutMs);
                await client.ConnectAsync(ipAddress, port, cts.Token);
                stopwatch.Stop();
                return (true, stopwatch.Elapsed.TotalMilliseconds);
            }
            catch
            {
                stopwatch.Stop();
                return (false, stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
