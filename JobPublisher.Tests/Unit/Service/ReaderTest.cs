using NSubstitute;
using JobPublisher.Repository;
using JobPublisher.Config;
using JobPublisher.Service;
using Npgsql;
using JobPublisher.Dto;
using System.Numerics;
using JobPublisher.Database;
using NSubstitute.ReturnsExtensions;

namespace JobPublisher.Tests.Unit.Service;

public class ReaderTest
{
    private readonly Connection _connectionMock;
    private JobCollection JobsFixture;
    private Job JobOne;
    private Job JobTwo;
    private JobCollection MultipleJobsFixture;

    public ReaderTest()
    {
        _connectionMock = Substitute.For<Connection>(new NpgsqlConnection());
        JobOne = new Job(new BigInteger(1), "topic", @"{""Job"": ""Test""}", "2024-01-20 03:26:14.000", false, null);
        JobTwo = new Job(new BigInteger(2), "topic", @"{""Job"": ""Test""}", "2024-01-20 03:26:14.001", false, null);
        JobsFixture = new JobCollection();
        JobsFixture.LoadJob(JobOne);
        MultipleJobsFixture = new JobCollection();
        MultipleJobsFixture.LoadJob(JobOne);
        MultipleJobsFixture.LoadJob(JobTwo);
    }

    [Fact]
    public void TestJobPublisherOnlyReadsOneJob()
    {
        // Arrange
        var jobRepositoryMock = Substitute.For<IJobRepository>();
        jobRepositoryMock.GetAndResolveJobs(
            Arg.Any<Connection>(), Arg.Is<int>(x => x > 0), Arg.Any<PublisherConfig>()
        )
        .Returns(
            JobsFixture,
            JobsFixture,
            JobsFixture
        );

        jobRepositoryMock.GetAndResolveJobs(
            Arg.Any<Connection>(), Arg.Is<int>(x => x == 0), Arg.Any<PublisherConfig>()
        )
        .ReturnsNull();

        var reader = new Reader(jobRepositoryMock, GetConfig(1, 1));

        // Act
        var result = reader.Read(_connectionMock);
        var nullResult = reader.Read(_connectionMock);

        // Assert
        Assert.Equal(JobsFixture, result);
        Assert.Null(nullResult);
    }

    [Fact]
    public void TestJobPublisherReadsThreeJobs()
    {
        // Arrange
        var jobRepositoryMock = Substitute.For<IJobRepository>();
        jobRepositoryMock.GetAndResolveJobs(
            Arg.Any<Connection>(), Arg.Is<int>(x => x > 0), Arg.Any<PublisherConfig>()
        )
        .Returns(
            JobsFixture,
            JobsFixture,
            JobsFixture,
            JobsFixture,
            JobsFixture
        );

        jobRepositoryMock.GetAndResolveJobs(
            Arg.Any<Connection>(), Arg.Is<int>(x => x == 0), Arg.Any<PublisherConfig>()
        )
        .ReturnsNull();

        var reader = new Reader(jobRepositoryMock, GetConfig(1, 3));

        // Act
        var result = reader.Read(_connectionMock);
        var secondResult = reader.Read(_connectionMock);
        var thirdResult = reader.Read(_connectionMock);
        var nullResult = reader.Read(_connectionMock);

        // Assert
        Assert.Equal(JobsFixture, result);
        Assert.Equal(JobsFixture, secondResult);
        Assert.Equal(JobsFixture, thirdResult);
        Assert.Null(nullResult);
    }

    [Fact]
    public void TestJobPublisherReadsInfiniteJobs()
    {
        // Arrange
        var jobRepositoryMock = Substitute.For<IJobRepository>();
        jobRepositoryMock.GetAndResolveJobs(
            Arg.Any<Connection>(), Arg.Is<int>(x => x > 0), Arg.Any<PublisherConfig>()
        )
        .Returns(
            JobsFixture,
            JobsFixture,
            JobsFixture,
            JobsFixture,
            JobsFixture
        );

        jobRepositoryMock.GetAndResolveJobs(
            Arg.Any<Connection>(), Arg.Is<int>(x => x == 0), Arg.Any<PublisherConfig>()
        )
        .ReturnsNull();

        var reader = new Reader(jobRepositoryMock, GetConfig(1, 0));

        // Act
        var results = new List<JobCollection>();
        for (int i = 0; i < 5; i++)
        {
            results.Add(reader.Read(_connectionMock));
        }

        // Assert
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void TestJobPublisherReadsMoreThanOneJobAtOnce()
    {
        // Arrange
        var jobRepositoryMock = Substitute.For<IJobRepository>();
        jobRepositoryMock.GetAndResolveJobs(
            Arg.Any<Connection>(), Arg.Is<int>(x => x > 0), Arg.Any<PublisherConfig>()
        )
        .Returns(
            MultipleJobsFixture,
            JobsFixture,
            JobsFixture,
            JobsFixture,
            JobsFixture
        );

        jobRepositoryMock.GetAndResolveJobs(
            Arg.Any<Connection>(), Arg.Is<int>(x => x == 0), Arg.Any<PublisherConfig>()
        )
        .ReturnsNull();

        var reader = new Reader(jobRepositoryMock, GetConfig(2, 2));

        // Act
        var result = reader.Read(_connectionMock);
        var nullResult = reader.Read(_connectionMock);

        // Assert
        Assert.Equal(2, result.GetJobCount());
        Assert.Null(nullResult);
    }

    private PublisherConfig GetConfig(int jobsPerRead, int maxJobs)
    {
        return new PublisherConfig(jobsPerRead, 500, 1, 1, maxJobs);
    }
}
