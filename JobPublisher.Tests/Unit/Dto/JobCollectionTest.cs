using JobPublisher.Dto;
using System.Collections;
using System.Numerics;

namespace JobPublisher.Tests.Unit.Dto;

public class JobCollectionTest
{
    [Theory]
    [ClassData(typeof(SingleJobCollectionFixture))]
    public void TestJobCollectionPopulatesSingle(List<Dictionary<string, object>> inputs, List<object> expected)
    {
        JobCollection jobs = new JobCollection(inputs);
        foreach (Job job in jobs.GetJobs())
        {
            Assert.Equal((BigInteger)expected[0], job.Id);
            Assert.Equal((string)expected[1], job.Topic);
            Assert.Equal((string)expected[2], job.Payload);
            Assert.Equal((string)expected[3], job.GetFormattedFireAt());
            Assert.Equal((bool)expected[4], job.Processed);
        }
    }

    [Theory]
    [ClassData(typeof(MultipleJobCollectionFixture))]
    public void TestJobCollectionPopulatesMultiple(List<Dictionary<string, object>> inputs, int numberOfRows)
    {
        JobCollection jobs = new JobCollection(inputs);
        Assert.Equal(numberOfRows, jobs.Jobs.Count);

        foreach (var input in inputs)
        {
            foreach (Job job in jobs.GetJobs())
            {
                if ((BigInteger)input["id"] == job.Id)
                {
                    Assert.Equal((string)input["topic"], job.Topic);
                    Assert.Equal((string)input["payload"], job.Payload);
                    Assert.Equal((string)input["fire_at"], job.GetFormattedFireAt());
                    Assert.Equal((bool)input["processed"], job.Processed);
                }
            }
        }
    }

    [Theory]
    [ClassData(typeof(MultipleJobCollectionFixture))]
    public void TestJobCollectionSortsByTimestamp(List<Dictionary<string, object>> inputs, int numberOfRows)
    {
        JobCollection jobs = new JobCollection(inputs);
        Assert.Equal(numberOfRows, jobs.Jobs.Count);

        List<BigInteger> orderOfJobsBasedOnMillisecondsInTimestamp = new List<BigInteger> () {
            new BigInteger(5),
            new BigInteger(2),
            new BigInteger(1),
            new BigInteger(4),
            new BigInteger(3)
        };
        int index = 0;
        foreach (Job job in jobs.GetJobs())
        {
            Assert.Equal(orderOfJobsBasedOnMillisecondsInTimestamp[index], job.Id);
            index++;
        }
    }
}

public class SingleJobCollectionFixture : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        #nullable disable
        yield return new object[] {
            new List<Dictionary<string, object>> () {
                new Dictionary<string, object> {
                    { "id", new BigInteger(1) },
                    { "topic", "Job/Test/Topic" },
                    { "payload", @"{""Job"": ""Test""}" },
                    { "fire_at", "2024-01-20 03:26:14.000" },
                    { "processed", false },
                    { "processed_at", null }
                }
            },
            new List<object> () {
                new BigInteger(1), "Job/Test/Topic", @"{""Job"": ""Test""}", "2024-01-20 03:26:14.000", false
            }
        };
        #nullable enable
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class MultipleJobCollectionFixture : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        #nullable disable
        yield return new object[] {
            new List<Dictionary<string, object>> () {
                new Dictionary<string, object> {
                    { "id", new BigInteger(1) },
                    { "topic", "Job/Test/Topic1" },
                    { "payload", @"{""Job"": ""Test""}" },
                    { "fire_at", "2024-01-20 03:26:14.002" },
                    { "processed", false },
                    { "processed_at", null }
                },
                new Dictionary<string, object> {
                    { "id", new BigInteger(2) },
                    { "topic", "Job/Test/Topic2" },
                    { "payload", @"{""Job"": ""Test""}" },
                    { "fire_at", "2024-01-20 03:26:14.001" },
                    { "processed", false },
                    { "processed_at", null }
                },
                new Dictionary<string, object> {
                    { "id", new BigInteger(3) },
                    { "topic", "Job/Test/Topic3" },
                    { "payload", @"{""Job"": ""Test""}" },
                    { "fire_at", "2024-01-20 03:26:14.004" },
                    { "processed", false },
                    { "processed_at", null }
                },
                new Dictionary<string, object> {
                    { "id", new BigInteger(4) },
                    { "topic", "Job/Test/Topic4" },
                    { "payload", @"{""Job"": ""Test""}" },
                    { "fire_at", "2024-01-20 03:26:14.003" },
                    { "processed", false },
                    { "processed_at", null }
                },
                new Dictionary<string, object> {
                    { "id", new BigInteger(5) },
                    { "topic", "Job/Test/Topic4" },
                    { "payload", @"{""Job"": ""Test""}" },
                    { "fire_at", "2024-01-20 03:26:13.999" },
                    { "processed", false },
                    { "processed_at", null }
                }
            },
            5
        };
        #nullable enable
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
