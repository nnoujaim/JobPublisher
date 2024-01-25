using JobPublisher.Dto;

namespace JobPublisher.Service;

public interface IWriter
{
    Task WriteAsync(JobCollection job);
}
