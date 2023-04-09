FROM paddlepaddle/paddle:2.4.2-gpu-cuda11.7-cudnn8.4-trt8.4
COPY libpaddle /lib/libpaddle
ENV LD_LIBRARY_PATH=/lib/libpaddle:/usr/local/TensorRT-8.4.2.4/lib:/usr/local/cuda-11.7/targets/x86_64-linux/lib:/usr/local/nvidia/lib:/usr/local/nvidia/lib64