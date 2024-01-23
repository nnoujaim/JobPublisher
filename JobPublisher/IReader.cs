using Npgsql;
using JobPublisher.Dto;

namespace JobPublisher;

public interface IReader
{
    public JobCollection? Read(NpgsqlConnection conn);
    public bool ReadLimitReached();
}
