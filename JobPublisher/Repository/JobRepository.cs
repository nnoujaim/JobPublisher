using Npgsql;
using JobPublisher.Dto;
using System.Numerics;

namespace JobPublisher.Repository;

public class JobRespository : IJobRespository
{
    public JobCollection? GetAndResolveJobs(NpgsqlConnection conn, int numRows, int lookbackSeconds, int lookaheadSeconds)
    {
        string sql = @"
            UPDATE jobs
            SET processed = :processed,
            processed_at = :processed_at
            WHERE id IN (SELECT id
                FROM jobs
                WHERE processed = false
                AND fire_at AT TIME ZONE 'UTC' > :lookback
                AND fire_at AT TIME ZONE 'UTC' < :lookahead
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
        NpgsqlCommand update = new NpgsqlCommand(sql, conn);
        update.Parameters.AddWithValue(":processed", true);
        update.Parameters.AddWithValue(":processed_at", DateTimeOffset.UtcNow);
        update.Parameters.AddWithValue(":lookback", GetLookbackDateUtc(lookbackSeconds));
        update.Parameters.AddWithValue(":lookahead", GetLookaheadDateUtc(lookaheadSeconds));
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
                        reader.GetDateTime(3).ToString(TimeUtility.Format),
                        reader.GetBoolean(4),
                        reader.GetDateTime(5).ToString(TimeUtility.Format)
                    )
                );
            }
            return jobs;
        }
    }

    private DateTimeOffset GetLookbackDateUtc(int seconds)
    {
        return DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds((double)-seconds));
    }

    private DateTimeOffset GetLookaheadDateUtc(int seconds)
    {
        return DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds((double)seconds));
    }
}
