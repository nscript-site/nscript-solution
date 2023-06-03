# 容器介绍

容器打包了 paddle 4.2 运行环境和 paddleinference 环境，运行时版本：

- paddle: 4.2
- paddleinference: 4.*
- cuda:10.2
- cudnn7.6
- trt7.0

在容器内，可直接使用动态链接库调用 paddleinference 的 api。

可通过docker直接下载：

> docker pull hufei96/paddleinference:2.4.2-gpu-cuda11.2-cudnn8.2-trt8.0

## 打包过程

进入[paddleinference推理库下载页面](https://www.paddlepaddle.org.cn/inference/v2.4/guides/install/download_lib.html)，下载对应版本的 Linux 下 C 推理库。这里下载的是 CUDA11.2/cuDNN8.2/TensorRT8.0 GCC8.2 版本，下载解压后，将其中的 so 和 so* 文件解压到 libpaddle 目录，将 libpaddle 目录放置在本目录下。

目录结构：

```bash
Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
d-----     2023/4/4/周二      8:21                libpaddle
-a----     2023/4/4/周二      8:32            250 Dockerfile
-a----     2023/4/4/周二      8:24             65 README.md
```

libpaddle 目录的相关文件列表如下：

```bash
-a---           2023/2/15    18:44       31131472 libdnnl.so.1
-a---           2023/2/15    18:44       31131472 libdnnl.so.2
-a---           2023/2/15    18:44        1932448 libiomp5.so
-a---           2023/2/15    18:44       31131472 libmkldnn.so.0
-a---           2023/2/15    18:44      130096544 libmklml_intel.so
-a---           2023/2/15    18:44       15086128 libonnxruntime.so
-a---           2023/2/15    18:44       15086128 libonnxruntime.so.1.11.1
-a---           2023/2/15    17:46     1871019528 libpaddle_inference_c.so
-a---           2023/2/15    18:44        6376520 libpaddle2onnx.so
-a---           2023/2/15    18:44        6376520 libpaddle2onnx.so.1.0.0rc2
```

运行下面指令，进行打包：

docker build . --tag hufei96/paddleinference:2.4.2-gpu-cuda11.2-cudnn8.2-trt8.0