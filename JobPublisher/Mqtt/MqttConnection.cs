using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace JobPublisher.Mqtt;

public class MqttConnection : IDisposable
{
    protected IMqttClient Client { get; private set; }
    protected MqttClientConfig Config { get; private set; }
    protected TimeSpan KeepAlive { get; private set; }

    public MqttConnection(MqttFactory factory, MqttClientConfig config, int keepalive = 10)
    {
        Config = config;
        Client = factory.CreateMqttClient();
        KeepAlive = new TimeSpan(0, 0, keepalive);
    }

    public async Task Connect()
    {
        MqttClientOptions mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(Config.Host, Config.Port)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithClientId(Config.ClientId)
                .WithCredentials(Config)
                .WithKeepAlivePeriod(KeepAlive)
                .Build();

        var response = await Client.ConnectAsync(mqttClientOptions, CancellationToken.None);
    }

    public IMqttClient GetConnection()
    {
        return Client;
    }

    public void Dispose()
    {
        Client.DisconnectAsync();
    }
}
