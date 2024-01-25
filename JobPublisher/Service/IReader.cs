using Npgsql;
using JobPublisher.Dto;

namespace JobPublisher.Service;

public interface IReader
{
    public JobCollection? Read(NpgsqlConnection conn);
    public bool ReadLimitReached();
}
