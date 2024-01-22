using System.Numerics;
using JobPublisher;
using System.Globalization;

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
    public SortedDictionary<BigInteger, Job> Jobs { get; set; }

    // public JobCollection(List<Dictionary<string, object>> jobs)
    // {
    //     Jobs = new SortedDictionary<DateTime, Job> ();
    //     foreach (Dictionary<string, object> job in jobs)
    //     {
    //         Jobs[TimeUtility.GetUtcTimestampFromString((string)job["fire_at"])] = new Job(
    //             (BigInteger)job["id"],
    //             (string)job["topic"],
    //             (string)job["payload"],
    //             (string)job["fire_at"],
    //             (bool)job["processed"],
    //             (string)job["processed_at"]
    //         );
    //     }
    // }

    public JobCollection(List<Job> jobs)
    {
        Jobs = new SortedDictionary<DateTime, Job> ();
        foreach (Job job in jobs)
        {
            Jobs[job.FireAt] = job;
        }
    }

    public JobCollection()
    {
        Jobs = new SortedDictionary<BigInteger, Job> ();
    }

    public void LoadJob(Job job)
    {
        Jobs[job.Id] = job;
    }

    public IEnumerable<Job> GetJobs()
    {
        foreach (var jobEntry in Jobs)
        {
            yield return jobEntry.Value;
        }
    }

    public int GetJobCount()
    {
        return Jobs.Count;
    }
}
