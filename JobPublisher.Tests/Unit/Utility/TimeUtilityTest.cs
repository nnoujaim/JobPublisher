using JobPublisher.Utility;

namespace JobPublisher.Tests.Unit;

public class TimeUtilityTest
{
    [Theory]
    [InlineData("2024-01-20 03:26:14.000 -0500", "2024-01-20 08:26:14.000")]
    [InlineData("2024-01-19 22:26:14.023 -0400", "2024-01-20 02:26:14.023")]
    public void TestTimeConvertedToUtcFromStringWithTZ(string input, string expected)
    {
        // Act
        DateTime utc = TimeUtility.GetUtcTimestampFromString(input);

        // Assert
        Assert.Equal(expected, utc.ToString(TimeUtility.Format));
    }

    [Theory]
    [InlineData("2024-01-20 03:26:14.000", "2024-01-20 03:26:14.000")]
    [InlineData("2024-01-19 22:26:14.023", "2024-01-19 22:26:14.023")]
    public void TestTimeConvertedToUtcFromStringWithoutTZ(string input, string expected)
    {
        // Act
        DateTime utc = TimeUtility.GetUtcTimestampFromString(input);

        // Assert
        Assert.Equal(expected, utc.ToString(TimeUtility.Format));
    }
}
