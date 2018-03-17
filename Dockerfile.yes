FROM golang:latest

# Install beego and the bee dev tool
RUN go get -u -v github.com/astaxie/beego
RUN go get -u -v github.com/beego/bee
RUN go get -d github.com/Microsoft/ApplicationInsights-Go/appinsights
RUN go get -u -v gopkg.in/mgo.v2
RUN go get -u -v github.com/Azure/go-autorest/autorest/utils
RUN go get -u -v github.com/streadway/amqp
RUN go get -u -v pack.ag/amqp

RUN apt-get update 

ENV GOPATH /go
ENV PATH $GOPATH/bin:$PATH
# EH
ENV EVENTURL=
ENV EVENTPOLICYNAME=
ENV EVENTPOLICYKEY=
# ACK Logging
ENV TEAMNAME=
# Mongo/Cosmos
ENV MONGOHOST=
# RabbitMQ
ENV RABBITMQHOST=

# Copy the application files (needed for production)
ADD . /go/src/captureorderfd

# Set the working directory to the app directory
WORKDIR /go/src/captureorderfd

# Expose the application on port 8080
EXPOSE 8080

# Set the entry point of the container to the bee command that runs the
# application and watches for changes

CMD ["bee", "run"]



