using JobPublisher.Database;
using Npgsql;
using JobPublisher.Dto;

namespace JobPublisher;

public class LoopHandler
{
    private readonly IPostgresConnectionFactory ConnectionFactory;
    private readonly IReader Reader;
    private readonly IWriter Writer;

    public LoopHandler(IPostgresConnectionFactory connectionFactory, IReader reader, IWriter writer)
    {
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
                }
            }
            catch (Exception exception)
            {
                tx.Rollback();
                Console.WriteLine("Exception");
                Console.WriteLine(exception);
            }
            conn.Close();
        }
    }
}