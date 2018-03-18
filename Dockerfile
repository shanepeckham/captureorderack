FROM golang:latest
ENV GOPATH /go
ENV PATH $GOPATH/bin:$PATH
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
RUN go build .

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

CMD ["./captureorderack"]