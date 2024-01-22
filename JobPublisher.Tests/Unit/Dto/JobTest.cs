using JobPublisher.Dto;
using System.Collections;
using System.Numerics;

namespace JobPublisher.Tests.Unit.Dto;

public class JobTest
{
    [Theory]
    [ClassData(typeof(SingleJobFixture))]
    public void TestJobPopulates(BigInteger id, string topic, string payload, string fireAt, bool processed, string? processedAt, string fireAtUtc, string? processedAtUtc)
    {
        Job job = new(id, topic, payload, fireAt, processed, processedAt);
        Assert.Equal(id, job.Id);
        Assert.Equal(topic, job.Topic);
        Assert.Equal(payload, job.Payload);
        Assert.Equal(fireAtUtc, job.FireAt.ToString(TimeUtility.Format));
        Assert.Equal(processed, job.Processed);
        if (job.ProcessedAt is not null)
        {
            #nullable disable
            DateTime processedDto = (DateTime)job.ProcessedAt;
            #nullable enable
            Assert.Equal(processedAtUtc, processedDto.ToString(TimeUtility.Format));
        }
    }
}

public class SingleJobFixture : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        #nullable disable
        yield return new object[] {
            new BigInteger(1), "Job/Test/Topic1", @"{""Job1"": ""Test""}", "2024-01-20 03:26:14.000 -0500", false, null, "2024-01-20 08:26:14.000", null
        };
        yield return new object[] {
            new BigInteger(1389749827134), "Job/Test/Topic2", @"{""Job2"": {""TestKey"": ""TestValue""}}", "2024-01-18 12:01:01.000 -0800", false, null, "2024-01-18 20:01:01.000", null
        };
        yield return new object[] {
            new BigInteger(567), "Job/Test/Topic3", @"{""Job3"": {""TestKey"": ""TestValue123""}}", "2024-01-18 12:01:01.000", false, null, "2024-01-18 12:01:01.000", null
        };
        yield return new object[] {
            new BigInteger(567), "Job/Test/Topic4", @"{""Job4"": {""TestKey"": ""TestValue""}}", "2024-01-18 12:01:01.000", true, "2024-01-18 12:01:01.000", "2024-01-18 12:01:01.000", "2024-01-18 12:01:01.000"
        };
        yield return new object[] {
            new BigInteger(567), "Job/Test/Topic5", @"", "2024-01-18 12:01:01.000", true, "2024-01-18 12:01:01.000 +0400", "2024-01-18 12:01:01.000", "2024-01-18 08:01:01.000"
        };
        #nullable enable
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
