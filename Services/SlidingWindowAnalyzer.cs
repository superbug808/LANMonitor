using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANMonitor.Services
{
    public class SlidingWindowAnalyzer
    {
        private readonly Queue<long> _latencyWindow;
        private readonly int _windowSize;

        public SlidingWindowAnalyzer(int windowSize = 5)
        {
            _latencyWindow = new Queue<long>(windowSize);
            _windowSize = windowSize;
        }

        public void AddPingResult(long latencyMs)
        {
            if (_latencyWindow.Count >= _windowSize)
            {
                _latencyWindow.Dequeue();
            }
            _latencyWindow.Enqueue(latencyMs);
        }

        public double GetMovingAverage()
        {
            if (_latencyWindow.Count == 0) return 0;
            return _latencyWindow.Average();
        }

        public string DetermineState(double degradationThresholdMs = 50.0, int timeoutThreshold = 2)
        {
            // If the window is full of zeroes/failures (depending on how you log timeouts)
            if (_latencyWindow.Count(x => x == -1) >= timeoutThreshold)
                return "Offline";

            var validPings = _latencyWindow.Where(x => x != -1).ToList();
            if (!validPings.Any()) return "Offline";

            return validPings.Average() > degradationThresholdMs ? "Degraded" : "Online";
        }
    }
}
