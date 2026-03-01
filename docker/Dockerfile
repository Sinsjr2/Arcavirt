FROM ubuntu:24.04

COPY ./installers/gcc-14.2.0.202511-GNURX-ELF.run /installers/

RUN chmod +x  /installers/gcc-14.2.0.202511-GNURX-ELF.run && \
apt update && \
    # rx-elf-gccのインストール時にパッケージを解凍(xz-utils)するために使用する
    apt install -y xz-utils \
    # rx-elf-gcc と一緒に使用する
    cmake && \
    /installers/gcc-14.2.0.202511-GNURX-ELF.run -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
