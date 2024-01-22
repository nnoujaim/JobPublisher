------ Prepare schema -------
SHOW timezone;
SET timezone = 'UTC';
ALTER SYSTEM SET timezone='UTC';

DELETE FROM jobs;

DROP TABLE IF EXISTS jobs;

CREATE TABLE jobs (
	id SERIAL PRIMARY KEY,
    topic VARCHAR(1000) NOT NULL,
    payload VARCHAR(5000) NOT NULL,
    fire_at TIMESTAMP WITH TIME ZONE NOT NULL,
    processed BOOLEAN NOT NULL,
    processed_at TIMESTAMP WITH TIME ZONE NULL
);
ALTER TABLE jobs
ALTER COLUMN fire_at TYPE TIMESTAMP WITH TIME ZONE USING fire_at AT TIME ZONE 'UTC',
ALTER COLUMN processed_at TYPE TIMESTAMP WITH TIME ZONE USING processed_at AT TIME ZONE 'UTC';

CREATE INDEX ix_jobs_fire_at ON jobs (fire_at);
CREATE INDEX ix_jobs_processed_fire_at ON jobs (processed, fire_at);
CREATE INDEX ix_jobs_processed_fire_at_id ON jobs (processed, fire_at, id) INCLUDE (topic, payload);


------ Add test data -------
INSERT INTO jobs (
	topic,
	payload,
	fire_at,
	processed,
	processed_at
) VALUES (
	'job1/test/topic',
	'{"Job1": "TestValue4"}',
	now(),
	false,
	null
);

-- Bulk add
INSERT INTO jobs (
	topic,
	payload,
	fire_at,
	processed,
	processed_at
) values
('job/test/topic', '{"Job": "TestValue"}', now(), false, null),
('job/test/topic', '{"Job": "TestValue"}', now(), false, null),
('job/test/topic', '{"Job": "TestValue"}', now(), false, null),
('job/test/topic', '{"Job": "TestValue"}', now(), false, null),
('job/test/topic', '{"Job": "TestValue"}', now(), false, null);

------ Read test data -------
SELECT COUNT(*) FROM jobs;
SELECT COUNT(*) FROM jobs WHERE processed = false;

SELECT * FROM jobs;
SELECT * FROM jobs ORDER BY fire_at;
SELECT
id,
topic,
payload,
fire_at,
fire_at AT TIME ZONE 'UTC' AS fire_at_utc,
processed,
processed_at,
processed_at AT TIME ZONE 'UTC' AS processed_at_utc
FROM jobs;

------ Run program logic selector -------
SELECT id
FROM jobs
WHERE processed = false
AND fire_at AT TIME ZONE 'UTC' > now() AT TIME ZONE 'UTC' - INTERVAL '10 min'
AND fire_at AT TIME ZONE 'UTC' < now() AT TIME ZONE 'UTC' + INTERVAL '10 min'
ORDER BY fire_at ASC
LIMIT 10;

------ Run program logic update -------
UPDATE jobs
SET processed = true,
processed_at = now()
WHERE id IN (SELECT id
    FROM jobs
    WHERE processed = false
    AND fire_at AT TIME ZONE 'UTC' > '2024-01-22 01:30:56.505 +00:00'
    AND fire_at AT TIME ZONE 'UTC' < '2024-01-22 01:54:56.505 +00:00'
    ORDER BY fire_at ASC
    LIMIT 10
)
RETURNING id,
    topic,
    payload,
    fire_at,
    processed,
    processed_at;
