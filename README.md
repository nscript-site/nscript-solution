# NScript DotNet 人工智能解决方案

## 综述

NScript 是一整套完整的，基于 .net 的人工智能解决方案。

## 推理引擎

- onnxruntime
- paddleinference
- libtorch

## 技术开发


## Docker 部署

### linux dotnetsdk 编译环境

X64:

[dotnet7-linux-aotsdk](./docker/dotnet7-linux-aotsdk/README.md): 因为 libc 的版本问题，这个镜像打包的程序不支持 ubuntu18.04 下运行，若需要在 ubuntu18.04 下跑，需要使用下面的镜像。

[dotnet7-aotsdk-ubuntu18.04](./docker/ubuntu18.04-dotnet7-aotsdk/README.md): 该镜像打包的程序具备最大的兼容性。

ARM64:

[dotnet7-linux-arm64-aotsdk](./docker/dotnet7-linux-arm64-aotsdk/README.md): 需要在 Arm64 环境下打包（比如，进行国产化适配）请采用此镜像。运行时，需要 arm64 环境下的 docker。

### paddle 部署

[paddle4.2-cuda11.2](./docker/paddle/paddle4.2-cuda11.2/README.md)

[paddle4.2-cuda11.7](./docker/paddle/paddle4.2-cuda11.7/README.md) 