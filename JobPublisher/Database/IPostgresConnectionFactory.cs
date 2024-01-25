using Npgsql;

namespace JobPublisher.Database;

public interface IPostgresConnectionFactory : IDisposable
{
    public NpgsqlConnection GetConnection();
}
