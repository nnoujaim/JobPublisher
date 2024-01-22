using JobPublisher.Database;
using JobPublisher.Mqtt;
using JobPublisher.Repository;

using MQTTnet;

namespace JobPublisher;

public class JobPublisher
{
    private PostgresConfig PgConfig;
    private MqttClientConfig MqttConfig;
    private PublisherConfig PublisherConfig;
    private JobHandler? JobHandler;
    private LoopHandler? LoopHandler;

    public JobPublisher(PostgresConfig pgConfig, MqttClientConfig mqttConfig, PublisherConfig publisherConfig)
    {
        PgConfig = pgConfig;
        MqttConfig = mqttConfig;
        PublisherConfig = publisherConfig;
    }

    public async Task Initialize()
    {
        PostgresConnectionFactory connectionFactory = new PostgresConnectionFactory(PgConfig);
        Reader reader = new Reader(new JobRespository(), PublisherConfig);

        MqttConnection mqtt = new MqttConnection(new MqttFactory(), MqttConfig);
        await mqtt.Connect();

        JobHandler = new JobHandler(connectionFactory, reader, new Writer(mqtt));
        LoopHandler = new LoopHandler(JobHandler, PublisherConfig);
    }

    public async Task Run()
    {
        await Initialize();
        #nullable disable
        await LoopHandler.LoopForever();
        #nullable enable
    }
}
