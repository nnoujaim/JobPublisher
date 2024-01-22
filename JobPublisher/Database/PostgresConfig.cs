using System.Net;

namespace JobPublisher.Database;

public class PostgresConfig
{
    private NetworkCredential Credentials;
    public int Port;
    public string Host;
    public string Database;

    public PostgresConfig(string username, string password, int port, string host, string database)
    {
        Credentials = new NetworkCredential(username, password);
        Port = port;
        Host = host;
        Database = database;
    }

    public string GetConnectionString()
    {
        return "Host=" + Host
            + ";Username=" + Credentials.UserName
            + ";Password=" + Credentials.Password
            + ";Database=" + Database
            + ";Port=" + Port;
    }
}
