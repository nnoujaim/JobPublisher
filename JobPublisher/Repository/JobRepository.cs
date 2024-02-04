using Npgsql;
using JobPublisher.Dto;
using JobPublisher.Utility;
using System.Numerics;
using JobPublisher.Config;
using JobPublisher.Database;

namespace JobPublisher.Repository;

public class JobRepository : IJobRepository
{
    public ITimeUtility TimeUtility;

    public JobRepository(ITimeUtility? timeUtility = null)
    {
        TimeUtility = timeUtility ?? new TimeUtility();
    }

    public JobCollection? GetAndResolveJobs(IConnection conn, int numRows, PublisherConfig config)
    {
        string sql = @"
            UPDATE jobs
            SET processed = :processed,
            processed_at = :processed_at
            WHERE id IN (
                SELECT id
                FROM jobs
                WHERE processed = false
                AND fire_at > :lookback
                AND fire_at < :lookahead
                AND id % :consumer_count = :consumer_index
                ORDER BY fire_at ASC
                LIMIT :limit
            )
            RETURNING id,
                topic,
                payload,
                fire_at,
                processed,
                processed_at;
        ";
        NpgsqlCommand update = new NpgsqlCommand(sql, conn.GetConnection());
        update.Parameters.AddWithValue(":processed", true);
        update.Parameters.AddWithValue(":processed_at", DateTimeOffset.UtcNow);
        update.Parameters.AddWithValue(":lookback", GetLookbackDateUtc(config.LookbackSeconds));
        update.Parameters.AddWithValue(":lookahead", GetLookaheadDateUtc(config.LookaheadSeconds));
        update.Parameters.AddWithValue(":consumer_count", config.ConsumerCount);
        update.Parameters.AddWithValue(":consumer_index", config.ConsumerIndex);
        update.Parameters.AddWithValue(":limit", numRows);

        using (var reader = update.ExecuteReader())
        {
            if (!reader.HasRows) return null;

            JobCollection jobs = new JobCollection();
            while (reader.Read())
            {

                jobs.LoadJob(
                    new Job(
                        new BigInteger(reader.GetInt64(0)),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetDateTime(3).ToString(TimeUtility.GetFormat()),
                        reader.GetBoolean(4),
                        reader.GetDateTime(5).ToString(TimeUtility.GetFormat())
                    )
                );
            }
            return jobs;
        }
    }

    private DateTimeOffset GetLookbackDateUtc(int seconds)
    {
        return TimeUtility.GetTimeOffset(-seconds);
    }

    private DateTimeOffset GetLookaheadDateUtc(int seconds)
    {
        return TimeUtility.GetTimeOffset(seconds);
    }
}
