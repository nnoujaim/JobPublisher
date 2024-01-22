using Npgsql;
using JobPublisher.Dto;

namespace JobPublisher.Repository;

public interface IJobRespository
{
    public JobCollection? GetAndResolveJobs(NpgsqlConnection tx, int numRows, int lookbackSeconds, int lookaheadSeconds);
}
