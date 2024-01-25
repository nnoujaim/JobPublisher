using Npgsql;
using JobPublisher.Dto;
using JobPublisher.Config;

namespace JobPublisher.Repository;

public interface IJobRespository
{
    public JobCollection? GetAndResolveJobs(NpgsqlConnection conn, int numRows, PublisherConfig config);
}
