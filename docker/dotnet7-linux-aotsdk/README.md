# 容器介绍

容器打包了 dotnet7 native aot 编译环境：

> docker pull hufei96/dotnet7aotsdk:linux-x64

该版本打包的 AOT 程序，不支持在 ubuntu18.04 下运行

## 打包过程

docker build . --tag hufei96/dotnet7aotsdk:linux-x64