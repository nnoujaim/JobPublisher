using JobPublisher.Dto;

namespace JobPublisher;

public interface IWriter
{
    Task WriteAsync(JobCollection job);
}
