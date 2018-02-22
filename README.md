# CaptureOrder  - TACK

A containerised Go swagger API to capture orders, write them to MongoDb.

The following environment variables need to be passed to the container:

### ACK Logging
```
ENV TEAMNAME=[YourTeamName]
```
### For Mongo
```
ENV MONGOHOST="mongodb://[mongoinstance].[namespace]"
```

### For RabbitMQ
```
ENV RABBITMQHOST=amqp://[url]:5672
ENV PARTITIONKEY=[0,1,2]
```
### For Event Hubs
```
ENV EVENTURL="https://[youreventhub].servicebus.windows.net/[eventhubname]"

ENV EVENTPOLICYNAME="[policy key name]"

ENV EVENTPOLICYKEY="[policy key]"

ENV PARTITIONKEY=[0,1,2]
```
