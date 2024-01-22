namespace JobPublisher;

public class PublisherConfig
{
    public int NumJobsPerRead { get; }
    public int LookbackSeconds { get; } = 600; // 10 minutes
    public int LookaheadSeconds { get; } = 1;
    public int LoopFrequencyMs { get; }
    public int MaxReads { get; }
    public int ConsumerCount { get; }
    public int ConsumerIndex { get; }

    public PublisherConfig(
        int jobsPerRead,
        int loopFrequencyMs = 500,
        int totalConsumerCount = 1,
        int consumerIndex = 1,
        int maxReads = 0
    )
    {
        ValidateInput(totalConsumerCount, consumerIndex, maxReads, loopFrequencyMs, jobsPerRead);
        NumJobsPerRead = jobsPerRead;
        LoopFrequencyMs = loopFrequencyMs;
        ConsumerCount = totalConsumerCount;
        ConsumerIndex = consumerIndex - 1; // Index is always 1 less than its position
        MaxReads = maxReads;
    }

    public void ValidateInput(int totalConsumerCount, int consumerIndex, int maxReads, int loopFrequencyMs, int jobsPerRead)
    {
        if (totalConsumerCount < 1) throw new InvalidPublisherConfig("Consumer count must be >= 1");
        if (consumerIndex > totalConsumerCount) throw new InvalidPublisherConfig("Consumer index must be <= the total number of consumers");
        if (maxReads < 0) throw new InvalidPublisherConfig("Max reads must be zero or higher");
        if (loopFrequencyMs < 50) throw new InvalidPublisherConfig("Cannot loop faster than every 50ms");
        if (jobsPerRead < 1) throw new InvalidPublisherConfig("Must read at least one job at a time");
    }
}

[Serializable]
public class InvalidPublisherConfig : Exception
{
    public InvalidPublisherConfig() : base()
    {
    }

    public InvalidPublisherConfig(string message) : base(message)
    {
    }

    public InvalidPublisherConfig(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
