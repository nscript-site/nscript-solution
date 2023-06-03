FROM paddlepaddle/paddle:2.4.2-gpu-cuda11.2-cudnn8.2-trt8.0
COPY libpaddle /lib/libpaddle
ENV LD_LIBRARY_PATH=/lib/libpaddle:/usr/local/TensorRT-8.0.3.4/lib:/usr/local/cuda-11.2/targets/x86_64-linux/lib:/usr/local/nvidia/lib:/usr/local/nvidia/lib64