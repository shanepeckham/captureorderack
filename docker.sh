# Build
docker build -t captureorder . 

# Create network
docker network create challenge

# Run the app with Mongo+RabbitMQ
docker run -d -p 27017:27017 --name chlmongo --network challenge mongo
docker run -d -p 5672:5672 -p 15672:15672 --name chlrabbitmq --network challenge rabbitmq:3-management
docker run -it -p 8080:8080 --env-file env.mongo.rabbit.list --network challenge captureorder

# Run the app with Cosmos+EventHub
docker run -it -p 8080:8080 --env-file env.cosmos.eventhub.list --network challenge captureorder