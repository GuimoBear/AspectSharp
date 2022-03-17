using AspectSharp.Benchmarks.Clients.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace AspectSharp.Benchmarks.Clients
{
    public sealed class MetricsClient : IMetricsClient
    {
        private readonly ILogger<MetricsClient> _logger;

        public MetricsClient(ILogger<MetricsClient> logger)
        {
            _logger = logger;
        }

        public IDisposable NewScope(string name)
            => new MetricScope(_logger, name);

        private sealed class MetricScope : IDisposable
        {
            private readonly ILogger<MetricsClient> _logger;
            private readonly string _metricName;
            private readonly Stopwatch _stopwatch;

            public MetricScope(ILogger<MetricsClient> logger, string metricName)
            {
                _logger = logger;
                _metricName = metricName;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                _logger.LogInformation("{0}: {1}", _metricName, _stopwatch.Elapsed);
            }
        }
    }
}
