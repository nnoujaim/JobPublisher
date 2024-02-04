using Npgsql;
using JobPublisher.Dto;
using JobPublisher.Database;

namespace JobPublisher.Service;

public interface IReader
{
    public JobCollection? Read(IConnection conn);
    public bool ReadLimitReached();
}
