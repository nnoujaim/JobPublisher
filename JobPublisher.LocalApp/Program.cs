using JobPublisher.Database;
using JobPublisher.Mqtt;
using JobPublisher.Config;
using Microsoft.Extensions.Logging;

namespace JobPublisher.LocalApp;

class Program
{
    public static async Task Main(string[] args)
    {
        int jobsPerRead = int.Parse(args[0]);
        int loopFrequencyMs = int.Parse(args[1]);
        int consumerCount = int.Parse(args[2]);
        int consumerIndex = int.Parse(args[3]);
        int maxReads = int.Parse(args[4]);

        #nullable disable
        string dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        string dbPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        int dbPort = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT"));
        string dbServer = Environment.GetEnvironmentVariable("POSTGRES_SERVER");
        string dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");

        string mqttUser = Environment.GetEnvironmentVariable("MOSQUITTO_USER");
        string mqttPass = Environment.GetEnvironmentVariable("MOSQUITTO_PASSWORD");
        int mqttPort = int.Parse(Environment.GetEnvironmentVariable("MOSQUITTO_PORT"));
        string mqttServer = Environment.GetEnvironmentVariable("MOSQUITTO_HOST");

        PostgresConfig pgConfig = new PostgresConfig(dbUser, dbPass, dbPort, dbServer, dbName);
        MqttClientConfig mqttConfig = new MqttClientConfig(mqttUser, mqttPass, mqttPort, mqttServer);
        PublisherConfig publisherConfig = new PublisherConfig(jobsPerRead, loopFrequencyMs, consumerCount, consumerIndex, maxReads);
        #nullable enable

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("Job-Publisher");

        JobPublisher publisher = new JobPublisher(logger, pgConfig, mqttConfig, publisherConfig);
        await publisher.Run();
    }
}
