using System.Diagnostics;

namespace JobPublisher.Metrics;

public class ConsoleMetricsPublisher : IMetricsPublisher
{
    public void PublishMetric(string service, string measurement, Stopwatch time, int count = 1, Tuple<string, string>? extra = null)
    {
        float averageActionMs = count > 0 ? time.ElapsedMilliseconds / count : time.ElapsedMilliseconds;
        string message = $"Service: {service}, Metric: {measurement}, TimeMs {time.ElapsedMilliseconds}, AverageMsPerAction {averageActionMs}";
        if (extra != null) message = message + ", Extra: { " + extra.Item1 + " : " + extra.Item2 + " }";

        Console.WriteLine(message);
    }
}
