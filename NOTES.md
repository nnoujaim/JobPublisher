# Summary

This package will connect to a DB to read "jobs" (events) that need to be published into an MQTT topic if it is within a time bound for when the job is supposed to be published

In the context of the application this was designed for, this would mean reading actions that were registered ahead of time (and written into SQL) and slated to be actioned on at a designated time in the future, and publsh them into a topic that a consumer would then pick up and use to make decisions.

In order to keep the code simple there are some baked in assumptions

1) The schema that will be used to feed this library must match the following (Postgres syntax, will work with other DBs in the future)
```sql
CREATE TABLE jobs (
	id SERIAL PRIMARY KEY,
    topic VARCHAR(1000) NOT NULL,
    payload VARCHAR(5000) NOT NULL,
    fire_at TIMESTAMPTZ NOT NULL,
    processed BOOLEAN NOT NULL,
    processed_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_jobs_fire_at ON jobs (fire_at);
CREATE INDEX ix_jobs_processed_fire_at ON jobs (processed, fire_at);
CREATE INDEX ix_jobs_processed_fire_at_id ON jobs (processed, fire_at, id) INCLUDE (topic, payload);
```

ALL TIMESTAMPS NEED TO BE IN UTC!!!

2) Partitioning will be supported by using modulus on the id
This will support many running instances of this publisher running in parallel for scalability

Modulus can be used, for example, to have 3 publishers in parallel each only looking at 1/3rd the data, like this
```sql
-- Reader 0
SELECT * FROM YourTable WHERE Id % 3 = 0;

-- Reader 1
SELECT * FROM YourTable WHERE Id % 3 = 1;

-- Reader 2
SELECT * FROM YourTable WHERE Id % 3 = 2;

ETC...
```
This will prevent more consumers from causing message duplicates
The partitioning key will be injected into the library when initialized
Care has to be taken here to make sure that the count and index of each consumer is setup correctly

2) For fast reads we will need to have a composite index on the fire_at and already_processed columns since those will be queried together
Also it may help to have another index including already_processed, fire_at, and id since we will be paritioning by id but also searching for not yet processed rows within a look ahead of X seconds and a lookback of X minutes

The lookback is to make sure we grab ones we have missed, and the short lookahead is to anticipate firing the next event promptly

These time parameters should be configurable in the library

3) We need to lock the rows that have been read out and will be marked processed to ensure only once delivery of messages

Reads and marking rows processed will be wrapped in a transaction, and the isolation level of the DB will be READ COMMITTED

In order to make sure locks are put in place the library will begin a transaction that updates already_processed to 1. Then it will yield the rows it updated to the caller, who will publish them to MQTT. After successfully publishing to the broker the caller will yield back to the DB code and ONLY then will the transaction be committed and any potential other updaters be allowed to read / update rows

This should be revisited if it causes lots of blocking on the DB server. It may be okay to take dirty reads and have the next reader skip a message that ends up being rolled back, because our lookback window should cause us to immediately catch it

4) Data should be archived from the jobs table periodically in order to keep the service running smoothly
We could build that into this library as well, but for now will wait until a later version.
