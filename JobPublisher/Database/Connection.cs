using Npgsql;

namespace JobPublisher.Database;

public class Connection : IConnection
{
    private NpgsqlConnection Conn;

    public Connection(NpgsqlConnection connection)
    {
        Conn = connection;
    }

    public NpgsqlConnection GetConnection()
    {
        return Conn;
    }

    public void Open()
    {
        Conn.Open();
    }

    public NpgsqlTransaction BeginTransaction()
    {
        return Conn.BeginTransaction();
    }

    public void Close()
    {
        Conn.Close();
    }

    public void Dispose()
    {
        Conn.Dispose();
    }
}
