FROM golang:1.10.0-alpine3.7 AS build-env
ENV GOPATH /go
ENV PATH $GOPATH/bin:$PATH

RUN apk --update add curl git

# Set the working directory to the app directory
WORKDIR /go/src/github.com/shanepeckham/captureorderack/

# Download dep binary to bin folder in $GOPATH
RUN mkdir -p /usr/local/bin \
    && curl -fsSL -o /usr/local/bin/dep https://github.com/golang/dep/releases/download/v0.4.1/dep-linux-amd64 \
    && chmod +x /usr/local/bin/dep


# Add source code. Ignoring local /vendor file (via .dockerignore) to ensure dep
# correctly restores /vendor file
COPY . .
# Restore dependancies with dep 
RUN dep ensure -v
# Build binary
RUN go build -o ./build/captureorder .

# final stage
FROM alpine:3.7
WORKDIR /app
COPY --from=build-env /go/src/github.com/shanepeckham/captureorderack/build /app/
COPY --from=build-env /go/src/github.com/shanepeckham/captureorderack/conf/app.prod.conf /app/conf/app.conf
COPY --from=build-env /go/src/github.com/shanepeckham/captureorderack/swagger /app/swagger
RUN chmod 777 /app/swagger
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
EXPOSE 8080

CMD ["./captureorder"]