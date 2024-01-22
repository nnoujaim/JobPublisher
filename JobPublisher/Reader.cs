using JobPublisher.Repository;
using Npgsql;
using JobPublisher.Dto;
using System.Numerics;

namespace JobPublisher;

public class Reader : IReader
{
    private readonly IJobRespository Repository;
    private int JobsAtATime;
    private int LookbackSeconds;
    private int LookaheadSeconds;
    public BigInteger JobsRead { get; private set; } = 0;
    public int ReadCount { get; private set; } = 0;
    public BigInteger StopAfter;

    public Reader(IJobRespository respository, int jobsAtATime, int lookbackSeconds, int lookaheadSeconds, int stopAfter = 0)
    {
        Repository = respository;
        JobsAtATime = jobsAtATime;
        LookbackSeconds = lookbackSeconds;
        LookaheadSeconds = lookaheadSeconds;
        StopAfter = (BigInteger)stopAfter;
    }

    public JobCollection? Read(NpgsqlConnection conn)
    {
        JobCollection? jobs = Repository.GetAndResolveJobs(conn, GetRowsToRead(), LookbackSeconds, LookaheadSeconds);
        if (jobs is not null) IncrementCounts(jobs);
        return jobs;
    }

    private void IncrementCounts(JobCollection jobs)
    {
        JobsRead += jobs.GetJobCount();

        if (ReadCount == int.MaxValue) ReadCount = 0;
        ReadCount++;
    }

    private int GetRowsToRead()
    {
        if (StopAfter == 0) return JobsAtATime;
        return (int)(StopAfter - JobsRead);
    }
}
