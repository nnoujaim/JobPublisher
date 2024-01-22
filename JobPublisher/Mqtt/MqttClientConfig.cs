using System.Net;
using MQTTnet.Client;

namespace JobPublisher.Mqtt;

public class MqttClientConfig : IMqttClientCredentialsProvider
{
    private NetworkCredential? Credentials;
    public int Port;
    public string ClientId;
    public string Host;


    public MqttClientConfig(string username, string password, int port, string host)
    {
        Credentials = new NetworkCredential(username, password);
        Port = port;
        Host = host;
        ClientId = GetClientId();
    }

    public string GetUserName(MqttClientOptions clientOptions)
    {
        #nullable disable
        return Credentials.UserName;
        #nullable enable
    }

    public byte[] GetPassword(MqttClientOptions clientOptions)
    {
        #nullable disable
        return System.Text.Encoding.UTF8.GetBytes(Credentials.Password);
        #nullable enable
    }

    private string GetClientId()
    {
        Random rand = new Random();
        int randValue;
        string str = "job-publisher-";
        char letter;
        for (int i = 0; i < rand.Next(15, 15); i++)
        {
            randValue = rand.Next(0, 26);
            letter = Convert.ToChar(randValue + 65);
            str = str + letter;
        }
        return str;
    }
}
