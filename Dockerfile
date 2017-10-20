FROM golang:alpine

# Install beego and the bee dev tool
RUN apk update && apk upgrade && \
    apk add --no-cache bash git openssh
#RUN go get -u -v code.cloudfoundry.org/clock
RUN go get -u -v github.com/astaxie/beego
RUN go get -u -v github.com/beego/bee
RUN go get -u -v gopkg.in/mgo.v2
RUN go get -u -v github.com/Azure/go-autorest/autorest/utils
RUN go get -u -v github.com/Microsoft/ApplicationInsights-Go/appinsights

ENV GOPATH /go
ENV PATH $GOPATH/bin:$PATH
ENV DATABASE=
ENV PASSWORD=
ENV INSIGHTSKEY=
ENV SOURCE=
ENV EVENTURL=
ENV EVENTPOLICYNAME=
ENV EVENTPOLICYKEY=
ENV PARTITIONKEY=

# Copy the application files (needed for production)
ADD . /go/src/captureorderfd

# Set the working directory to the app directory
WORKDIR /go/src/captureorderfd

# Expose the application on port 8080
EXPOSE 8080

# Set the entry point of the container to the bee command that runs the
# application and watches for changes
CMD ["bee", "run"]