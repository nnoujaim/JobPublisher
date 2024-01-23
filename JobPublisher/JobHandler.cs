using JobPublisher.Database;
using Npgsql;
using JobPublisher.Dto;
using Microsoft.Extensions.Logging;
using JobPublisher.Utility;

namespace JobPublisher;

public class JobHandler
{
    private readonly ILogger Logger;
    private readonly IPostgresConnectionFactory ConnectionFactory;
    private readonly IReader Reader;
    private readonly IWriter Writer;

    public JobHandler(ILogger logger, IPostgresConnectionFactory connectionFactory, IReader reader, IWriter writer)
    {
        Logger = logger;
        ConnectionFactory = connectionFactory;
        Reader = reader;
        Writer = writer;
    }

    public async Task ReadAndPublish()
    {
        using (NpgsqlConnection conn = ConnectionFactory.GetConnection())
        {
            conn.Open();
            NpgsqlTransaction tx = conn.BeginTransaction();
            try
            {
                JobCollection? jobs = Reader.Read(conn);
                if (jobs is not null)
                {
                    await Writer.WriteAsync(jobs);
                    tx.Commit();
                    Logger.LogInformation("Read {count} jobs at {time}", jobs.GetJobCount(), DateTime.Now.ToString(TimeUtility.Format));
                }
                else {
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
