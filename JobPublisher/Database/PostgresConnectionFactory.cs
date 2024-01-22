using Npgsql;

namespace JobPublisher.Database;

public class PostgresConnectionFactory : IPostgresConnectionFactory
{
    public NpgsqlDataSource DataSource { get; set; }

    public PostgresConnectionFactory(PostgresConfig config)
    {
        DataSource = NpgsqlDataSource.Create(config.GetConnectionString());
    }

    public NpgsqlConnection GetConnection()
    {
        return DataSource.CreateConnection();
    }
}
