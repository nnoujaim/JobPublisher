using JobPublisher.Dto;
using JobPublisher.Mqtt;
using MQTTnet;

namespace JobPublisher.Service;

public class Writer : IWriter
{
    public MqttConnection Mqtt;

    public Writer(MqttConnection mqtt)
    {
        Mqtt = mqtt;
    }

    public async Task WriteAsync(JobCollection jobs)
    {
        foreach (Job job in jobs.GetJobs())
        {
            await Mqtt.GetConnection().PublishAsync(GetMessage(job), CancellationToken.None);
        }
    }

    private MqttApplicationMessage GetMessage(Job job)
    {
        return new MqttApplicationMessageBuilder()
            .WithTopic(job.Topic)
            .WithPayload(job.Payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
            .WithRetainFlag()
            .Build();
    }
}
