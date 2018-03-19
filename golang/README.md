# CaptureOrder  - TACK

A containerised Go swagger API to capture orders, write them to MongoDb and an AMQP message queue.

## Usage
### Swagger

Access the Swagger UI at [http://[host]/swagger]()

### Submitting an order

```
POST /v1/Order HTTP/1.1
Host: [host]:[port]
Content-Type: application/json

{
  "EmailAddress": "test@domain.com",
  "PreferredLanguage": "en"
}
```

## Environment Variables

The following environment variables need to be passed to the container:

### Logging

```
ENV TEAMNAME=[YourTeamName]
ENV APPINSIGHTS_KEY=[YourCustomApplicationInsightsKey]
```

### For MongoDB

```
ENV MONGOURL=mongodb://[mongoinstance].[namespace]
```

### For CosmosDB

```
ENV MONGOURL=mongodb://[CosmosDBInstanceName]:[CosmosDBPrimaryPassword]=@[CosmosDBInstanceName].documents.azure.com:10255/?ssl=true&replicaSet=globaldb
```

### For RabbitMQ

```
ENV AMQPURL=amqp://[url]:5672
```

### For Event Hubs

```
ENV AMQPURL=amqps://[policy name]:[policy key]@[youreventhub].servicebus.windows.net/[eventhubname]
```
