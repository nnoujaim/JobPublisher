namespace JobPublisher;

public class LoopHandler
{
    private readonly JobHandler Handler;
    private readonly PublisherConfig Config;

    public LoopHandler(JobHandler handler, PublisherConfig config)
    {
        Handler = handler;
        Config = config;
    }

    public async Task LoopForever()
    {
        await Task.Run(async () =>
        {
            while (!Handler.ReadLimitReached())
            {
                Console.WriteLine("Read and publish at: " + DateTime.Now.ToString(TimeUtility.Format));
                await Handler.ReadAndPublish();
                await Task.Delay(Config.LoopFrequencyMs);
            }
        });
    }
}