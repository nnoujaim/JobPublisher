# Usage

Spin up test database and broker
```bash
docker-compose up
```

Connect to broker to see messages being published
```bash
mosquitto_sub -h localhost -p 1883 -q 2 -u test -P test -t '#' -F %m\ %I\ %t\ %p
```

The arguments are as follows: JobsPerRead, LoopFrequency, ConsumerCount, ConsumerIndex, MaxReads
MaxReads == 0 means loop forever

```bash
dotnet run 5 500 1 1 0
```
