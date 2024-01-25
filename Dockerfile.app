# Use Microsoft's official .NET runtime image.
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Set the working directory in the container
WORKDIR /app

# Copy the app's binaries from your host machine to the container
COPY ./JobPublisher.LocalApp/bin/Release/net8.0/publish/ /app/

ARG JOBS_PER_READ
ARG LOOP_FREQUENCY
ARG NUMBER_CONSUMERS
ARG CONSUMER_INDEX
ARG MAX_READS

ENV JR=${JOBS_PER_READ}
ENV LF=${LOOP_FREQUENCY}
ENV NC=${NUMBER_CONSUMERS}
ENV CI=${CONSUMER_INDEX}
ENV MR=${MAX_READS}

# Run
ENTRYPOINT dotnet "JobPublisher.LocalApp.dll" ${JR} ${LF} ${NC} ${CI} ${MR}
