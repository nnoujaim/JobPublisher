using System.Globalization;

namespace JobPublisher.Utility;

public static class TimeUtility
{
    public const string Format = "yyyy-MM-dd HH:mm:ss.fff";
    public const string TZFormat = "yyyy-MM-dd HH:mm:ss.fff zzz";

    public static DateTime GetUtcTimestampFromString(string timestamp)
    {
        if (DateTimeOffset.TryParseExact(timestamp, TZFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dtFromOffset)) return dtFromOffset.UtcDateTime;
        // If no timezone provided we assume its already UTC
        if (DateTimeOffset.TryParseExact(timestamp, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dt)) return dt.DateTime;

        throw new InvalidTimestamp($"Timestamp does not adhere to the acceptable formats: {Format} OR {TZFormat}");
    }
}

[Serializable]
public class InvalidTimestamp : Exception
{
    public InvalidTimestamp() : base()
    {
    }

    public InvalidTimestamp(string message) : base(message)
    {
    }

    public InvalidTimestamp(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
