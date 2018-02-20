FROM golang:latest

# Install beego and the bee dev tool
RUN go get -u -v github.com/astaxie/beego
RUN go get -u -v github.com/beego/bee
RUN go get -d github.com/Microsoft/ApplicationInsights-Go/appinsights
RUN go get -u -v gopkg.in/mgo.v2
RUN go get -u -v github.com/Azure/go-autorest/autorest/utils
RUN go get -u -v github.com/streadway/amqp

RUN apt-get update 

# download Apache QPID
RUN apt-get install git
WORKDIR /go/src/
RUN git clone --progress --verbose http://git.apache.org/qpid-proton.git

# install the dependencies needed to compile Apache QPID
RUN apt-get install -y gcc=4:6.3.0-4 \
                       g++=4:6.3.0-4 \
                       cmake=3.7.2-1 \
                       cmake-curses-gui=3.7.2-1 \
                       uuid-dev=2.29.2-1

# SSL and Cyrus SASL requirements
RUN apt-get install -y libssl-dev=1.1.0f-3+deb9u1 \
                       libsasl2-2=2.1.27~101-g0780600+dfsg-3 \
                       libsasl2-dev=2.1.27~101-g0780600+dfsg-3 \
                       libsasl2-modules=2.1.27~101-g0780600+dfsg-3
RUN apt-get install -y swig=3.0.10-1.1

# compile Apache QPID proton-c
WORKDIR /go/src/qpid-proton
RUN git fetch
RUN git checkout tags/0.19.0
WORKDIR /go/src/qpid-proton/build
RUN cmake .. -DCMAKE_INSTALL_PREFIX=/usr -DSYSINSTALL_BINDINGS=ON
RUN make install

# Apache QPID Go dependencies
# RUN go get qpid.apache.org/electron
WORKDIR /go/src/qpid-proton/proton-c/bindings/go/src/qpid.apache.org/
RUN cp -r /go/src/qpid-proton/proton-c/bindings/go/src/qpid.apache.org /go/src/

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
COPY eventhub /go/src/hackcaptureorder/eventhub
COPY msauth /go/src/hackcaptureorder/msauth

# Set the working directory to the app directory
WORKDIR /go/src/captureorderfd

# Expose the application on port 8080
EXPOSE 8080

# Set the entry point of the container to the bee command that runs the
# application and watches for changes

CMD ["bee", "run"]



