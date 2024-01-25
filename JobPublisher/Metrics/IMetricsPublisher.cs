using System.Diagnostics;

namespace JobPubisher.Metrics;

public interface IMetricsPublisher
{
    public void PublishMetric(string service, string measurement, Stopwatch time, int count = 1, Tuple<string, string>? extra = null);
}
