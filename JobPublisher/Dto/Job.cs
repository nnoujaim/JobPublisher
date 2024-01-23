using System.Numerics;
using JobPublisher.Utility;

namespace JobPublisher.Dto;

public class Job
{
    public BigInteger Id { get; private set; }
    public string Topic { get; private set; }
    public string Payload { get; private set; }
    public DateTime FireAt { get; private set; }
    public bool Processed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public Job(BigInteger id, string topic, string payload, string fireAt, bool alreadyProcessed, string? processingCompletedAt)
    {
        Id = id;
        Topic = topic;
        Payload = payload;
        Processed = alreadyProcessed;
        FireAt = TimeUtility.GetUtcTimestampFromString(fireAt);
        if (processingCompletedAt is not null) ProcessedAt = TimeUtility.GetUtcTimestampFromString(processingCompletedAt);
    }

    public string GetFormattedFireAt()
    {
        return FireAt.ToString(TimeUtility.Format);
    }
}

public class JobCollection
{
    public Dictionary<BigInteger, Job> Jobs { get; set; }

    public JobCollection()
    {
        Jobs = new Dictionary<BigInteger, Job> ();
    }

    public void LoadJob(Job job)
    {
        Jobs[job.Id] = job;
    }

    public IEnumerable<Job> GetJobs()
    {
        foreach (var job in Jobs.OrderBy(pair => pair.Value.FireAt))
        {
            yield return job.Value;
        }
    }

    public int GetJobCount()
    {
        return Jobs.Count;
    }
}
