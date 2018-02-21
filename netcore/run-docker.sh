docker build . -t sabbour/captureorderack-netcore:v1
#docker push sabbour/captureorderack-netcore:v1

docker run -it -p 8000:8080  \
-e MONGOHOST='mongodb://sabbourordercapture:YftliYqKseSNN4cedwLs4o1itApGLs6hMaouPKv4MHVGMbH3lfv9085mOVyZEx0JEUcKsIJUH6yVbPL24f83vQ==@sabbourordercapture.documents.azure.com:10255/?ssl=true&replicaSet=globaldb' \
-e RABBITMQHOST='amqps://TestPolicy:2KHYwfOvBfchurrWXZkbeqh6j5MB1bfh1x1J7KYAiNY=@sabbourcapture.servicebus.windows.net/ehub' \
-e APPINSIGHTS_KEY='db755b02-c972-4f4d-ae93-cccbd98403c9' sabbour/captureorderack-netcore:v1