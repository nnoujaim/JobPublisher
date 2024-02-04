using Npgsql;

namespace JobPublisher.Database;

public class PostgresConnectionFactory : IPostgresConnectionFactory, IDisposable
{
    public NpgsqlDataSource DataSource { get; set; }

    public PostgresConnectionFactory(PostgresConfig config)
    {
        DataSource = NpgsqlDataSource.Create(config.GetConnectionString());
    }

    public Connection GetConnection()
    {
        return new Connection(DataSource.CreateConnection());
    }

    public void Dispose()
    {
        DataSource.Dispose();
    }
}
