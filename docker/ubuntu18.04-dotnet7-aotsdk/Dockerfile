FROM ubuntu:18.04
RUN apt-get update && apt-get install -y clang zlib1g-dev && apt-get install -y wget && \
    wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && apt-get install -y dotnet-sdk-7.0 && \
    rm -rf /var/lib/apt/lists/*
