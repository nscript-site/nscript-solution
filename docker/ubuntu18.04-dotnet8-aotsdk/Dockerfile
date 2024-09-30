FROM ubuntu:18.04
RUN apt-get update && apt-get install -y clang zlib1g-dev libicu-dev && apt-get install -y wget && \
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x ./dotnet-install.sh && \
    ./dotnet-install.sh --channel 8.0 && \
    rm -rf /var/lib/apt/lists/*
ENV PATH="/root/.dotnet:${PATH}"