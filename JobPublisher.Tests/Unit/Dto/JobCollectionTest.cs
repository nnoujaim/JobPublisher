using JobPublisher.Dto;
using System.Collections;
using System.Numerics;

namespace JobPublisher.Tests.Unit.Dto;

public class JobCollectionTest
{
    [Theory]
    [MemberData(nameof(SingleJobData))]
    public void TestJobCollectionPopulatesSingle(Job input, List<object> expected)
    {
        // Arrange
        JobCollection jobs = new JobCollection();

        // Act
        jobs.LoadJob(input);

        // Assert
        foreach (Job job in jobs.GetJobs())
        {
            Assert.Equal((BigInteger)expected[0], job.Id);
            Assert.Equal((string)expected[1], job.Topic);
            Assert.Equal((string)expected[2], job.Payload);
            Assert.Equal((string)expected[3], job.GetFormattedFireAt());
            Assert.Equal((bool)expected[4], job.Processed);
        }
        Assert.Equal(1, jobs.GetJobCount());
    }

    [Theory]
    [MemberData(nameof(MultipleJobData))]
    public void TestJobCollectionPopulatesMultiple(List<Job> inputs, int numJobs)
    {
        // Arrange
        JobCollection jobs = new JobCollection();

        // Act
        foreach (Job job in inputs)
        {
            jobs.LoadJob(job);
        }

        // Assert
        Assert.Equal(numJobs, jobs.GetJobCount());
    }

    [Theory]
    [MemberData(nameof(OrderedJobsData))]
    public void TestJobCollectionReturnsJobsOrderedByFireAtDate(List<Job> inputs, List<BigInteger> orderedJobIds)
    {
        // Arrange
        JobCollection jobs = new JobCollection();

        // Act
        foreach (Job job in inputs)
        {
            jobs.LoadJob(job);
        }

        // Assert
        int index = 0;
        foreach (Job job in jobs.GetJobs())
        {
            Assert.Equal(orderedJobIds[index], job.Id);
            index++;
        }
    }

    public static IEnumerable<object[]> SingleJobData => 
        new List<object[]>
        {
            new object[] {
                new Job(
                    new BigInteger(1),
                    "Job/Test/Topic",
                    @"{""Job"": ""Test""}",
                    "2024-01-20 03:26:14.000",
                    false,
                    null
                ),
                new List<object> () {
                    new BigInteger(1), "Job/Test/Topic", @"{""Job"": ""Test""}", "2024-01-20 03:26:14.000", false
                }
            },
            new object[] {
                new Job(
                    new BigInteger(123453456),
                    "Job/Test/Topic",
                    @"{""Job123"": ""Test""}",
                    "2024-01-25 03:26:14.000",
                    false,
                    null
                ),
                new List<object> () {
                    new BigInteger(123453456), "Job/Test/Topic", @"{""Job123"": ""Test""}", "2024-01-25 03:26:14.000", false
                }
            }
        };

    public static IEnumerable<object[]> MultipleJobData => 
        new List<object[]>
        {
            new object[] {
                new List<Job> () {
                    new Job(
                        new BigInteger(1),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.000",
                        false,
                        null
                    ),
                    new Job(
                        new BigInteger(2),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.001",
                        false,
                        null
                    )
                },
                2
            },
            new object[] {
                new List<Job> () {
                    new Job(
                        new BigInteger(1),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.000",
                        false,
                        null
                    ),
                    new Job(
                        new BigInteger(2),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.001",
                        false,
                        null
                    ),
                    new Job(
                        new BigInteger(3),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.002",
                        false,
                        null
                    )
                },
                3
            }
        };

    public static IEnumerable<object[]> OrderedJobsData => 
        new List<object[]>
        {
            new object[] {
                new List<Job> () {
                    new Job(
                        new BigInteger(1),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.000",
                        false,
                        null
                    ),
                    new Job(
                        new BigInteger(2),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.001",
                        false,
                        null
                    )
                },
                new List<BigInteger> () {new BigInteger(1), new BigInteger(2)}
            },
            new object[] {
                new List<Job> () {
                    new Job(
                        new BigInteger(1),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.003",
                        false,
                        null
                    ),
                    new Job(
                        new BigInteger(2),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.002",
                        false,
                        null
                    ),
                    new Job(
                        new BigInteger(3),
                        "Job/Test/Topic",
                        @"{""Job"": ""Test""}",
                        "2024-01-20 03:26:14.001",
                        false,
                        null
                    )
                },
                new List<BigInteger> () {new BigInteger(3), new BigInteger(2), new BigInteger(1)}
            }
        };
}
