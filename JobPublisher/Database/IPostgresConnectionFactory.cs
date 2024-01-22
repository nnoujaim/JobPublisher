using Npgsql;

namespace JobPublisher.Database;

public interface IPostgresConnectionFactory
{
    public NpgsqlConnection GetConnection();
}
