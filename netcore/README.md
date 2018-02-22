# CaptureOrder  - The Azure Kubernetes Challenge

A containerised .Net Core 2.0 swagger API to capture orders, write them to MongoDb.

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
ENV AMQPHOST=amqp://[url]:5672
ENV PARTITIONKEY=[0,1,2]
```
### For Event Hubs
```
ENV AMQPHOST="amqps://[policy name]:[policy key]@[youreventhub].servicebus.windows.net/[eventhubname]"
ENV EVENTHUBNAME="[eventhubname]"