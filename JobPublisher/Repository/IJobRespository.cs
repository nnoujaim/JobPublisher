using Npgsql;
using JobPublisher.Dto;

namespace JobPublisher.Repository;

public interface IJobRespository
{
    public JobCollection? GetAndResolveJobs(NpgsqlConnection conn, int numRows, PublisherConfig config);
}
