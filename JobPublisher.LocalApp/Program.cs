using JobPublisher;
using JobPublisher.Database;
using JobPublisher.Mqtt;
using JobPublisher.Repository;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;


namespace JobPublisher.LocalApp;

class Program
{
    public static async Task Main(string[] args)
    {
        // Setup job reader
        PostgresConfig pgConfig = new PostgresConfig("postgres", "postgres", 5432, "localhost", "postgres");
        PostgresConnectionFactory connectionFactory = new PostgresConnectionFactory(pgConfig);
        JobRespository repository = new JobRespository();
        Reader reader = new Reader(repository, 4, 600, 5);

        // Setup job writer
        MqttClientConfig mqttConfig = new MqttClientConfig("test", "test", 1883, "localhost");
        MqttConnection mqtt = new MqttConnection(new MqttFactory(), mqttConfig);
        await mqtt.Connect();
        Writer writer = new Writer(mqtt);


        JobPublisher loop = new JobPublisher(connectionFactory, reader, writer);
        await loop.ReadAndPublish();
        await loop.ReadAndPublish();


        // LoopHandler loop = new LoopHandler(connectionFactory, reader, writer);
        // await loop.ReadAndPublish();

    }
}
