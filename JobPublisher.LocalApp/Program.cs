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
        int jobsPerRead = int.Parse(args[0]);
        int loopFrequencyMs = int.Parse(args[1]);
        int consumerCount = int.Parse(args[2]);
        int consumerIndex = int.Parse(args[3]);
        int maxReads = int.Parse(args[4]);

        PostgresConfig pgConfig = new PostgresConfig("postgres", "postgres", 5432, "localhost", "postgres");
        MqttClientConfig mqttConfig = new MqttClientConfig("test", "test", 1883, "localhost");
        PublisherConfig publisherConfig = new PublisherConfig(jobsPerRead, loopFrequencyMs, consumerCount, consumerIndex, maxReads);

        JobPublisher publisher = new JobPublisher(pgConfig, mqttConfig, publisherConfig);
        await publisher.Run();
    }
}
