using Testcontainers;
using Testcontainers.PostgreSql;
using Npgsql;
using JobPublisher.Repository;
using JobPublisher.Config;
using JobPublisher.Database;
using JobPublisher.Dto;
using System.Numerics;
using NSubstitute;
using JobPublisher.Utility;
using System.Globalization;

namespace JobPublisher.Tests.Integration.Repository;

public sealed class JobRepositoryTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:14.7")
        .WithDatabase("postgres")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    private readonly Job JobOne;
    private readonly Job JobTwo;
    private readonly Job JobThree;
    private readonly Job JobFour;
    private readonly Job JobFive;
    private readonly Job OutOfLookbackJob;
    private readonly DateTimeOffset DtStart;
    private readonly DateTimeOffset DtEnd;
    private readonly TimeUtility TimeUtil;
    private readonly ITimeUtility TimeUtilMock;

    public JobRepositoryTest()
    {
        JobOne = new Job(new BigInteger(1), "topic1", @"{""Job"": ""Test""}", "2024-01-20 03:00:00.000 +00:00", false, null);
        JobTwo = new Job(new BigInteger(2), "topic2", @"{""Job"": ""Test""}", "2024-01-20 03:00:00.001 +00:00", false, null);
        JobThree = new Job(new BigInteger(3), "topic3", @"{""Job"": ""Test""}", "2024-01-20 03:00:00.002 +00:00", false, null);
        JobFour = new Job(new BigInteger(4), "topic4", @"{""Job"": ""Test""}", "2024-01-20 03:00:00.003 +00:00", false, null);
        JobFive = new Job(new BigInteger(5), "topic5", @"{""Job"": ""Test""}", "2024-01-20 03:00:00.004 +00:00", false, null);

        OutOfLookbackJob = new Job(new BigInteger(6), "topic6", @"{""Job"": ""Test""}", "2024-01-21 03:00:00.000 +00:00", false, null);

        DateTimeOffset.TryParseExact(
            "2024-01-20 02:55:00.000 +00:00",
            "yyyy-MM-dd HH:mm:ss.fff zzz",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTimeOffset start
        );
        DtStart = start.UtcDateTime;

        DateTimeOffset.TryParseExact(
            "2024-01-20 03:01:00.000 +00:00",
            "yyyy-MM-dd HH:mm:ss.fff zzz",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTimeOffset end
        );
        DtEnd = end.UtcDateTime;

        TimeUtil = new TimeUtility();
        TimeUtilMock = Substitute.For<ITimeUtility>();
        TimeUtilMock.GetTimeOffset(
            Arg.Is<int>(x => x < 0)
        )
        .Returns(
            DtStart
        );

        TimeUtilMock.GetTimeOffset(
            Arg.Is<int>(x => x > 0)
        )
        .Returns(
            DtEnd
        );
        TimeUtilMock.GetFormat().Returns("yyyy-MM-dd HH:mm:ss.fff");
    }

    [Fact]
    public void TestRepository_ReadsAllJobsAndMarksProcessed_WhenWithinTimeframe()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        var conn = new Connection(connection);
        conn.Open();
        NpgsqlTransaction tx = conn.BeginTransaction();
        InitializeDb(conn);
        CleanDB(conn);
        PopulateFixtures(conn, new List<Job>() {
            JobOne,
            JobTwo,
            JobThree,
            JobFour,
            JobFive
        });
        tx.Commit();

        var config = new PublisherConfig(5, 500, 1, 1, 0);
        var repo = new JobRepository(TimeUtilMock);

        // Act
        tx = conn.BeginTransaction();
        var results = repo.GetAndResolveJobs(conn, 5, config);
        tx.Commit();

        var dbRows = GetRows(conn);

        // Assert
        Assert.Equal(5, results.GetJobCount());

        foreach (Job job in dbRows.GetJobs())
        {
            Assert.True(job.Processed);
        }
    }

    [Fact]
    public void TestRepository_DoesntReadJobOutsideTimeframe_WhenExists()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        var conn = new Connection(connection);
        conn.Open();
        NpgsqlTransaction tx = conn.BeginTransaction();
        InitializeDb(conn);
        CleanDB(conn);
        PopulateFixtures(conn, new List<Job>() {
            JobOne,
            OutOfLookbackJob
        });
        tx.Commit();

        var config = new PublisherConfig(5, 500, 1, 1, 0);
        var repo = new JobRepository(TimeUtilMock);

        // Act
        tx = conn.BeginTransaction();
        var results = repo.GetAndResolveJobs(conn, 5, config);
        tx.Commit();

        var dbRows = GetRows(conn);

        // Assert
        Assert.Equal(1, results.GetJobCount());

        Assert.True(dbRows.Jobs[1].Processed);
        Assert.False(dbRows.Jobs[2].Processed);
    }

    [Fact]
    public void TestRepository_ReadsOnlyEvenJobs_WhenConsumerPartitioned()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        var conn = new Connection(connection);
        conn.Open();
        NpgsqlTransaction tx = conn.BeginTransaction();
        InitializeDb(conn);
        CleanDB(conn);
        PopulateFixtures(conn, new List<Job>() {
            JobOne,
            JobTwo,
            JobThree,
            JobFour
        });
        tx.Commit();

        var config = new PublisherConfig(1, 500, 2, 1, 0);
        var repo = new JobRepository(TimeUtilMock);

        // Act
        tx = conn.BeginTransaction();
        var results = repo.GetAndResolveJobs(conn, 5, config);
        tx.Commit();

        var dbRows = GetRows(conn);

        // Assert
        Assert.Equal(2, results.GetJobCount());
        Assert.True(results.Jobs.ContainsKey(2));
        Assert.True(results.Jobs.ContainsKey(4));
        Assert.False(results.Jobs.ContainsKey(1));
        Assert.False(results.Jobs.ContainsKey(3));
        Assert.True(dbRows.Jobs[2].Processed);
        Assert.True(dbRows.Jobs[4].Processed);
        Assert.False(dbRows.Jobs[1].Processed);
        Assert.False(dbRows.Jobs[3].Processed);
    }

    [Fact]
    public void TestRepository_ReadsOnlyOddJobs_WhenConsumerPartitioned()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        var conn = new Connection(connection);
        conn.Open();
        NpgsqlTransaction tx = conn.BeginTransaction();
        InitializeDb(conn);
        CleanDB(conn);
        PopulateFixtures(conn, new List<Job>() {
            JobOne,
            JobTwo,
            JobThree,
            JobFour
        });
        tx.Commit();

        var config = new PublisherConfig(1, 500, 2, 2, 0);
        var repo = new JobRepository(TimeUtilMock);

        // Act
        tx = conn.BeginTransaction();
        var results = repo.GetAndResolveJobs(conn, 5, config);
        tx.Commit();

        var dbRows = GetRows(conn);

        // Assert
        Assert.Equal(2, results.GetJobCount());
        Assert.True(results.Jobs.ContainsKey(1));
        Assert.True(results.Jobs.ContainsKey(3));
        Assert.False(results.Jobs.ContainsKey(2));
        Assert.False(results.Jobs.ContainsKey(4));
        Assert.True(dbRows.Jobs[1].Processed);
        Assert.True(dbRows.Jobs[3].Processed);
        Assert.False(dbRows.Jobs[2].Processed);
        Assert.False(dbRows.Jobs[4].Processed);
    }

    [Fact]
    public void TestRepository_ReadsOldestJob_WhenNewerJobsExistButReadsAreRestricted()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        var conn = new Connection(connection);
        conn.Open();
        NpgsqlTransaction tx = conn.BeginTransaction();
        InitializeDb(conn);
        CleanDB(conn);
        PopulateFixtures(conn, new List<Job>() {
            JobOne,
            JobTwo,
            JobThree,
            JobFour,
            JobFive
        });
        tx.Commit();

        var config = new PublisherConfig(1, 500, 1, 1, 0);
        var repo = new JobRepository(TimeUtilMock);

        // Act
        tx = conn.BeginTransaction();
        var results = repo.GetAndResolveJobs(conn, 1, config);
        tx.Commit();

        var dbRows = GetRows(conn);

        // Assert
        Assert.Equal(1, results.GetJobCount());
        Assert.True(results.Jobs.ContainsKey(1));
        Assert.True(results.Jobs[1].Topic == "topic1");
    }

    private void InitializeDb(IConnection conn)
    {
        var sql = @"
            CREATE TABLE jobs (
                id SERIAL PRIMARY KEY,
                topic VARCHAR(1000) NOT NULL,
                payload VARCHAR(5000) NOT NULL,
                fire_at TIMESTAMPTZ NOT NULL,
                processed BOOLEAN NOT NULL,
                processed_at TIMESTAMPTZ NULL
            );
        ";
        var command = new NpgsqlCommand(sql, conn.GetConnection());
        command.ExecuteNonQuery();

        sql = @"
            CREATE INDEX ix_jobs_fire_at ON jobs (fire_at);
            CREATE INDEX ix_jobs_processed_fire_at ON jobs (processed, fire_at);
            CREATE INDEX ix_jobs_processed_fire_at_id ON jobs (processed, fire_at, id) INCLUDE (topic, payload);
        ";
        command = new NpgsqlCommand(sql, conn.GetConnection());
        command.ExecuteNonQuery();
    }

    private void PopulateFixtures(IConnection conn, List<Job> jobs)
    {
        foreach (Job job in jobs)
        {
            string sql = @"
                INSERT INTO jobs (
                    topic,
                    payload,
                    fire_at,
                    processed
                ) VALUES (
                    :topic,
                    :payload,
                    :fire_at,
                    :processed
                )
            ";
            NpgsqlCommand command = new NpgsqlCommand(sql, conn.GetConnection());
            command.Parameters.AddWithValue(":topic", job.Topic);
            command.Parameters.AddWithValue(":payload", job.Payload);
            command.Parameters.AddWithValue(":fire_at", job.FireAt);
            command.Parameters.AddWithValue(":processed", job.Processed);
            command.ExecuteNonQuery();
        }
    }

    private void CleanDB(IConnection conn)
    {
        string sql = @"
            DELETE FROM jobs
        ";
        NpgsqlCommand command = new NpgsqlCommand(sql, conn.GetConnection());
        command.ExecuteNonQuery();
    }

    private JobCollection GetRows(IConnection conn)
    {
        string sql = @"
            SELECT
                id,
                topic,
                payload,
                fire_at,
                processed,
                processed_at
            FROM jobs
            ORDER BY fire_at ASC
        ";
        NpgsqlCommand command = new NpgsqlCommand(sql, conn.GetConnection());
        using (var reader = command.ExecuteReader())
        {
            JobCollection jobs = new JobCollection();
            while (reader.Read())
            {
                jobs.LoadJob(
                    new Job(
                        new BigInteger(reader.GetInt64(0)),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetDateTime(3).ToString(TimeUtil.GetFormat()),
                        reader.GetBoolean(4),
                        null
                    )
                );
            }
            return jobs;
        }
    }

    public Task InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
}
