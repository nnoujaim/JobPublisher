using JobPublisher.Database;
using Npgsql;
using JobPublisher.Dto;
using Microsoft.Extensions.Logging;
using JobPublisher.Utility;
using System.Diagnostics;
using JobPublisher.Metrics;

namespace JobPublisher.Service;

public class JobHandler
{
    private readonly ILogger Logger;
    private readonly IPostgresConnectionFactory ConnectionFactory;
    private readonly IReader Reader;
    private readonly IWriter Writer;
    private readonly IMetricsPublisher MetricsPublisher;

    public JobHandler(ILogger logger, IPostgresConnectionFactory connectionFactory, IReader reader, IWriter writer, IMetricsPublisher? metricPublisher = null)
    {
        Logger = logger;
        ConnectionFactory = connectionFactory;
        Reader = reader;
        Writer = writer;
        MetricsPublisher = metricPublisher ?? new ConsoleMetricsPublisher();
    }

    public async Task ReadAndPublish()
    {
        Stopwatch timer = Stopwatch.StartNew();
        using (Connection conn = ConnectionFactory.GetConnection())
        {
            conn.Open();
            MetricsPublisher.PublishMetric("job-publisher", "open-connection", timer);
            NpgsqlTransaction tx = conn.BeginTransaction();
            try
            {
                Stopwatch readTimer = Stopwatch.StartNew();
                JobCollection? jobs = Reader.Read(conn);
                if (jobs is not null)
                {
                    MetricsPublisher.PublishMetric("job-publisher", "read-jobs", timer, jobs.GetJobCount(), Tuple.Create("num-jobs", jobs.GetJobCount().ToString()));
                    Stopwatch publishTimer = Stopwatch.StartNew();
                    await Writer.WriteAsync(jobs);
                    MetricsPublisher.PublishMetric("job-publisher", "publish-jobs", timer, jobs.GetJobCount(), Tuple.Create("num-jobs", jobs.GetJobCount().ToString()));
                    tx.Commit();
                    Logger.LogInformation("Read and published {count} jobs at {time}", jobs.GetJobCount(), DateTime.Now.ToString(TimeUtility.Format));
                }
                else
                {
                    MetricsPublisher.PublishMetric("job-publisher", "read-jobs", timer, 1, Tuple.Create("num-jobs", "0"));
                    Logger.LogInformation("Read no jobs at {time}", DateTime.Now.ToString(TimeUtility.Format));
                }
            }
            catch (Exception exception)
            {
                tx.Rollback();
                Logger.LogError("Exception occured while reading and publishing jobs: {Exception}", exception.ToString());
                throw new ReadAndPublishException("Exception occured while reading and publishing jobs", exception);
            }
            conn.Close();
            MetricsPublisher.PublishMetric("job-publisher", "read-and-publish", timer);
        }
    }

    public bool ReadLimitReached()
    {
        return Reader.ReadLimitReached();
    }
}


[Serializable]
public class ReadAndPublishException : Exception
{
    public ReadAndPublishException() : base()
    {
    }

    public ReadAndPublishException(string message) : base(message)
    {
    }

    public ReadAndPublishException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
