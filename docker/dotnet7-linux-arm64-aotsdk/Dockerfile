FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy-arm64v8
RUN sed -i s@/ports.ubuntu.com/@/mirrors.aliyun.com/@g /etc/apt/sources.list
RUN apt-get update && \
    apt-get install -y vim clang zlib1g-dev && \
    rm -rf /var/lib/apt/lists/*