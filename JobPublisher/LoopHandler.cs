using JobPublisher.Utility;
using Microsoft.Extensions.Logging;

namespace JobPublisher;

public class LoopHandler
{
    private readonly ILogger Logger;
    private readonly JobHandler Handler;
    private readonly PublisherConfig Config;

    public LoopHandler(ILogger logger, JobHandler handler, PublisherConfig config)
    {
        Handler = handler;
        Config = config;
        Logger = logger;
    }

    public async Task LoopForever()
    {
        await Task.Run(async () =>
        {
            while (!Handler.ReadLimitReached())
            {
                Logger.LogInformation("Reading and publishing jobs at {time}", DateTime.Now.ToString(TimeUtility.Format));
                await Handler.ReadAndPublish();
                await Task.Delay(Config.LoopFrequencyMs);
            }
        });
    }
}