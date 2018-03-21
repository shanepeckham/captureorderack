docker run --rm --name mongolocal -d mongo
docker run --rm --name rabbitlocal -d rabbitmq
docker build -t captureorder .
docker run --rm -it -e MONGOHOST="mongodb://mongolocal:27017" -e RABBITMQHOST="amqp://rabbitlocal:5672" -p 8081:8080 --link mongolocal:mongo --link rabbitlocal:rabbitmq captureorder:latest