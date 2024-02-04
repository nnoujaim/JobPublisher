# Summary

Highly scalable and performant job and event publisher for IOT systems that need to store events in a DB to fire at a specific time.
Many instances of this publisher can be stood up in parallel next to each other and run in tandem without DB blocking or publishing the same message twice.

# Basic In Code Setup

A simple setup would be similar to what exists in the LocalApp project and is as follows:
```c#
using JobPublisher.Database;
using JobPublisher.Mqtt;
using JobPublisher.Config;
using Microsoft.Extensions.Logging;

...

PostgresConfig pgConfig = new PostgresConfig(dbUser, dbPass, dbPort, dbServer, dbName);
MqttClientConfig mqttConfig = new MqttClientConfig(mqttUser, mqttPass, mqttPort, mqttServer);
PublisherConfig publisherConfig = new PublisherConfig(jobsPerRead, loopFrequencyMs, consumerCount, consumerIndex, maxReads);

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("Job-Publisher");

JobPublisher publisher = new JobPublisher(logger, pgConfig, mqttConfig, publisherConfig);
await publisher.Run();

...
```

These classes can be initialized after downloading the package

# Local Testing

Use the provided dockerfile and sql scripts to boot up an instance of this service and watch it publish messages

Boot up local DB and IOT broker
```bash
docker-compose up postgres mosquitto
```

Boot up a single instance of this service
```bash
docker-compose up job-publisher-single
```

Log into the local DB with any IDE and execute the create table and index queries, and then insert jobs to fire as you wish!

# Testing

# Performance Profiling

In order to stress test the system and see its CPU / MEM usage we can leverage docker

First, boot up local instances of the DB and IOT broker
```bash
docker-compose up postgres mosquitto
```

Second, start up multiple instances of the consumer
```bash
docker-compose up --build job-publisher-parallelized-one job-publisher-parallelized-two job-publisher-parallelized-three job-publisher-parallelized-four job-publisher-parallelized-five
```

Third, hook into container usage using docker
```bash
docker stats job-publisher-parallelized-one job-publisher-parallelized-two job-publisher-parallelized-three job-publisher-parallelized-four job-publisher-parallelized-five
```

Last, pump as many jobs into the database as you see fit by using the provided sql scripts, for example
```sql
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
FROM generate_series(1,20000) AS x(test_data);
```

## Performance stats

### Single Consumer
Jobs per read: 100
Loop interval: 50ms

Results:
Jobs published per minute: 45k
Peak CPU usage of container: 50%
Peak memory usage: 39mb

### 5 Parallelized Consumers
Jobs per read: 100
Loop interval: 50ms

Results:
Jobs published per minute: 160k
Peak CPU usage of container: 42%
Peak memory usage: 39mb
