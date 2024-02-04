using Npgsql;

namespace JobPublisher.Database;

public interface IConnection : IDisposable
{
    public NpgsqlConnection GetConnection();

    public void Open();

    public NpgsqlTransaction BeginTransaction();

    public void Close();
}
