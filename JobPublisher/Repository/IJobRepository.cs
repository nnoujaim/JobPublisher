using Npgsql;
using JobPublisher.Dto;
using JobPublisher.Config;
using JobPublisher.Database;

namespace JobPublisher.Repository;

public interface IJobRepository
{
    public JobCollection? GetAndResolveJobs(IConnection conn, int numRows, PublisherConfig config);
}
