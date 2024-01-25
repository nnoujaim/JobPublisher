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
	'job/test/topic',
	'{"Job": "TestValue"}',
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
('job/test/topic', '{"Job": "TestValue5"}', now(), false, null),
('job/test/topic', '{"Job": "TestValue6"}', now(), false, null),
('job/test/topic', '{"Job": "TestValue4"}', now() + INTERVAL '1 second', false, null),
('job/test/topic', '{"Job": "TestValue3"}', now() + INTERVAL '2 second', false, null),
('job/test/topic', '{"Job": "TestValue2"}', now() + INTERVAL '4 second', false, null),
('job/test/topic', '{"Job": "TestValue1"}', now() + INTERVAL '3 second', false, null),
('job/test/topic', '{"Job": "7"}', now() + INTERVAL '7 second', false, null);

-- Bulk add for stress test
INSERT INTO jobs (
	topic,
	payload,
	fire_at,
	processed,
	processed_at
)
SELECT
	'job/test/topic',
	'{"Job": "' || x.test_data || '"}',
	now(),
	false,
	null
  FROM generate_series(1,20) AS x(test_data);

------ Read test data -------
SELECT COUNT(*) FROM jobs;
SELECT COUNT(*) FROM jobs WHERE processed = false;

SELECT * FROM jobs;
SELECT * FROM jobs ORDER BY fire_at;
SELECT * FROM jobs ORDER BY id;
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

------ Run partitioned program logic selector -------
-- All
SELECT id
FROM jobs
WHERE processed = false
AND fire_at AT TIME ZONE 'UTC' > now() AT TIME ZONE 'UTC' - INTERVAL '10 min'
AND fire_at AT TIME ZONE 'UTC' < now() AT TIME ZONE 'UTC' + INTERVAL '10 min'
AND id % 1 = 0
ORDER BY fire_at ASC
LIMIT 10;

-- 3 consumers
SELECT id
FROM jobs
WHERE processed = false
AND fire_at AT TIME ZONE 'UTC' > now() AT TIME ZONE 'UTC' - INTERVAL '10 min'
AND fire_at AT TIME ZONE 'UTC' < now() AT TIME ZONE 'UTC' + INTERVAL '10 min'
AND id % 3 = 0
-- AND id % 3 = 1
-- AND id % 3 = 2
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
