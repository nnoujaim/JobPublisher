using JobPublisher.Database;
using JobPublisher.Mqtt;
using JobPublisher.Repository;
using JobPublisher.Config;
using JobPublisher.Service;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace JobPublisher;

public class JobPublisher
{
    private ILogger Logger;
    private PostgresConfig PgConfig;
    private MqttClientConfig MqttConfig;
    private PublisherConfig PublisherConfig;
    private JobHandler? JobHandler;
    private LoopHandler? LoopHandler;

    public JobPublisher(ILogger logger, PostgresConfig pgConfig, MqttClientConfig mqttConfig, PublisherConfig publisherConfig)
    {
        Logger = logger;
        PgConfig = pgConfig;
        MqttConfig = mqttConfig;
        PublisherConfig = publisherConfig;
    }

    public async Task Initialize()
    {
        try
        {
            PostgresConnectionFactory connectionFactory = new PostgresConnectionFactory(PgConfig);
            Reader reader = new Reader(new JobRepository(), PublisherConfig);

            MqttConnection mqtt = new MqttConnection(new MqttFactory(), MqttConfig);
            await mqtt.Connect();

            JobHandler = new JobHandler(Logger, connectionFactory, reader, new Writer(mqtt));
            LoopHandler = new LoopHandler(Logger, JobHandler, PublisherConfig);
            Logger.LogInformation("Initialized publisher");
        }
        catch (Exception exception)
        {
            Logger.LogError($"Error Initializing Job Publisher: {exception}");
            throw new PublisherInitException("Error Initializing Job Publisher", exception);
        }
    }

    public async Task Run()
    {
        await Initialize();
        #nullable disable
        await LoopHandler.LoopForever();
        #nullable enable
    }
}

[Serializable]
public class PublisherInitException : Exception
{
    public PublisherInitException() : base()
    {
    }

    public PublisherInitException(string message) : base(message)
    {
    }

    public PublisherInitException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
