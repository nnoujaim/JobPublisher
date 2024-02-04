namespace JobPublisher.Utility;

public interface ITimeUtility
{
    public DateTimeOffset GetTimeOffset(int seconds);

    public string GetFormat();

    public string GetTzFormat();
}