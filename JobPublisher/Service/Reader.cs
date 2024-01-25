using JobPublisher.Repository;
using Npgsql;
using JobPublisher.Dto;
using System.Numerics;
using JobPublisher.Config;

namespace JobPublisher.Service;

public class Reader : IReader
{
    private readonly IJobRespository Repository;
    public BigInteger JobsRead { get; private set; } = 0;
    public int ReadCount { get; private set; } = 0;
    public PublisherConfig Config;

    public Reader(IJobRespository respository, PublisherConfig config)
    {
        Repository = respository;
        Config = config;
    }

    public JobCollection? Read(NpgsqlConnection conn)
    {
        JobCollection? jobs = Repository.GetAndResolveJobs(conn, GetRowsToRead(), Config);
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
        if (Config.MaxReads == 0) return Config.NumJobsPerRead;
        return (int)(Config.MaxReads - JobsRead);
    }

    public bool ReadLimitReached()
    {
        if (Config.MaxReads == 0) return false;
        return (int)JobsRead >= (int)Config.MaxReads;
    }
}
