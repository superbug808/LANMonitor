using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace LANMonitor.Services
{
    public class TracerouteEngine
    {
        public async Task<List<string>> RunTracerouteAsync(string ipAddress, int maxHops = 15, int timeoutMs = 1500)
        {
            var hopResults = new List<string>();
            using var ping = new Ping();
            byte[] buffer = Encoding.ASCII.GetBytes("LANMonitorTrace");

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                var options = new PingOptions(ttl, dontFragment: true);
                try
                {
                    var reply = await ping.SendPingAsync(ipAddress, timeoutMs, buffer, options);

                    if (reply.Status == IPStatus.Success)
                    {
                        hopResults.Add($"Hop {ttl}: {reply.Address} ({reply.RoundtripTime} ms) [Destination Reached]");
                        break;
                    }
                    else if (reply.Status == IPStatus.TtlExpired && reply.Address != null)
                    {
                        hopResults.Add($"Hop {ttl}: {reply.Address} ({reply.RoundtripTime} ms)");
                    }
                    else
                    {
                        hopResults.Add($"Hop {ttl}: * Request Timed Out *");
                    }
                }
                catch (Exception ex)
                {
                    hopResults.Add($"Hop {ttl}: Exception - {ex.Message}");
                }
            }

            return hopResults;
        }
    }
}