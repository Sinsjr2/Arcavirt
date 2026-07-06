# syntax=docker/dockerfile:1.7
# ============================================================================
# RX 向けデバッグ環境イメージ（マルチステージ）
#   builder 段 : GCC for Renesas RX と rx-elf-gdb をソースからクロスビルド
#   runtime 段 : ツールチェーン成果物（rx-elf-gcc / rx-elf-gdb）と
#                DebugComp（e2-server-gdb）を載せる
# builder の重いビルド依存とソースを最終イメージから排除して軽量化する。
# ============================================================================

ARG UBUNTU_TAG=24.04
ARG TARGET=rx-elf
ARG TOOLCHAIN_PREFIX=/opt/rx-elf

# ============================================================================
# Stage 1: toolchain-builder
# GCC for Renesas RX のソース（GPL 準拠・認証不要）をビルドする。
# ============================================================================
FROM ubuntu:${UBUNTU_TAG} AS toolchain-builder
ARG TARGET
ARG TOOLCHAIN_PREFIX

# ソース配布元とバージョン。d.php は CloudFront 署名 URL へ 302 で
# リダイレクトするため curl -L で追従する。バージョン差し替えは ARG で行う。
ARG GNURX_VERSION=14.2.0.202511
ARG SRC_BASE="https://llvm-gcc-renesas.com/downloads/d.php?s=cdn&f=rx"
ARG BINUTILS_PKG=binutils-2.44
ARG GCC_PKG=gcc-14.2
ARG NEWLIB_PKG=newlib-4.4.0

RUN apt-get update && apt-get install -y --no-install-recommends \
        build-essential \
        libgmp-dev libmpfr-dev libmpc-dev \
        texinfo bison flex \
        curl ca-certificates \
        tar gzip xz-utils \
    && rm -rf /var/lib/apt/lists/*

# binutils → gcc(stage1) → newlib → gcc(stage2) の定石クロスビルド。
# builder 段は最終イメージに含めないため、ソースと中間生成物は残してよい。
# TODO(要初回検証): RX のマルチリブ構成など Renesas 固有の configure フラグは
#   公式ビルド手順 wiki に従う余地がある。初回ビルドで configure ログを確認し、
#   生成された rx-elf-gcc が想定どおり動くか（-mcpu 等）を検証すること。
RUN <<EOF
#!/usr/bin/env bash
set -euo pipefail
export PATH="${TOOLCHAIN_PREFIX}/bin:${PATH}"
mkdir -p /src && cd /src

curl -fSL "${SRC_BASE}/binutils/${GNURX_VERSION}-gnurx/${BINUTILS_PKG}.tar.gz" -o binutils.tar.gz
curl -fSL "${SRC_BASE}/gcc/${GNURX_VERSION}-gnurx/${GCC_PKG}.tar.gz"           -o gcc.tar.gz
curl -fSL "${SRC_BASE}/newlib/${GNURX_VERSION}-gnurx/${NEWLIB_PKG}.tar.gz"     -o newlib.tar.gz
for f in binutils gcc newlib; do
  mkdir -p "$f" && tar -xf "$f.tar.gz" -C "$f"
done

# 1) binutils
mkdir -p /src/build-binutils && cd /src/build-binutils
/src/binutils/configure --target="${TARGET}" --prefix="${TOOLCHAIN_PREFIX}" \
    --disable-nls --disable-werror \
    --disable-gdb --disable-sim --disable-gdbserver --disable-gprofng
make -j"$(nproc)"
make install

# 2) gcc stage1（C のみ・newlib ヘッダ無しで先にコンパイラを作る）
mkdir -p /src/build-gcc1 && cd /src/build-gcc1
/src/gcc/configure --target="${TARGET}" --prefix="${TOOLCHAIN_PREFIX}" \
    --enable-languages=c --without-headers --with-newlib \
    --disable-nls --disable-shared --disable-threads \
    --disable-libssp --disable-libgomp --disable-libmudflap --disable-libquadmath
make -j"$(nproc)" all-gcc
make install-gcc

# 3) newlib（stage1 の rx-elf-gcc でビルド）
mkdir -p /src/build-newlib && cd /src/build-newlib
/src/newlib/configure --target="${TARGET}" --prefix="${TOOLCHAIN_PREFIX}" --disable-nls
make -j"$(nproc)"
make install

# 4) gcc stage2（C・newlib と結合した本番コンパイラ）
# 本プロジェクトの RX ソースは C/asm のみのため C++ は作らない。
# libstdc++ のビルドを省くことで初回ビルドを大幅に短縮する。
mkdir -p /src/build-gcc2 && cd /src/build-gcc2
/src/gcc/configure --target="${TARGET}" --prefix="${TOOLCHAIN_PREFIX}" \
    --enable-languages=c --with-newlib \
    --disable-nls --disable-shared --disable-threads \
    --disable-libssp --disable-libgomp --disable-libmudflap
make -j"$(nproc)"
make install
EOF

# rx-elf-gdb（デバッガ本体）を Renesas GNURX の gdb ソースパッケージからビルドする。
# Renesas 版 gdb は上流に無い RX 固有実装を含むため、上流 gdb では代替できない
# （DebugComp にも rx-elf-gdb は含まれない）。
# binutils 同梱の gdb は opcodes(rx_decode_opcode) と gdb/rx-tdep.c の引数不一致で
# ビルドできないが、Renesas が gdb 単体として配布するこのパッケージ(gdb-16.2)は
# 整合が取れており（全呼び出しが decode_flags 付きの5引数）、パッチ不要でビルド可能。
# 独立した RUN に分けることで、上の重いツールチェーン層のキャッシュを保つ。
# libexpat1-dev は gdb がターゲット XML 記述を扱うために必要。
ARG GNURX_GDB_PKG=gdb-16.2
RUN apt-get update && apt-get install -y --no-install-recommends \
        libexpat1-dev zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*
RUN <<EOF
#!/usr/bin/env bash
set -euo pipefail
export PATH="${TOOLCHAIN_PREFIX}/bin:${PATH}"
# gdb ソースを取得して展開する（統合ツリー形式・ラッパー dir 無しのため
# --strip-components は付けずディレクトリへそのまま展開する）。
mkdir -p /src/gdb-src && cd /src
curl -fSL "${SRC_BASE}/gdb/${GNURX_VERSION}-gnurx/${GNURX_GDB_PKG}.tar.gz" -o gdb.tar.gz
tar -xf gdb.tar.gz -C /src/gdb-src
mkdir -p /src/build-gdb && cd /src/build-gdb
/src/gdb-src/configure --target="${TARGET}" --prefix="${TOOLCHAIN_PREFIX}" \
    --disable-nls --disable-werror \
    --disable-binutils --disable-ld --disable-gas --disable-gprof \
    --disable-sim --disable-gdbserver --disable-gprofng \
    --with-expat --without-python --disable-tui
make -j"$(nproc)" all-gdb
make install-gdb
EOF

# ============================================================================
# Stage 2: runtime
# ============================================================================
FROM ubuntu:${UBUNTU_TAG} AS runtime
ARG TARGET
ARG TOOLCHAIN_PREFIX

# DebugComp（e2 studio のデバッグコンポーネント）取得元。
# e2 studio の p2 リポジトリ（認証不要）から RX 用アーティファクトを取得する。
ARG DEBUGCOMP_BASE_URL="https://tool-support.renesas.com/e2studio/2026/2026-04.2/R20260505-1532/rx/supportfiles"
ARG DEBUGCOMP_DIR=/opt/renesas/DebugComp/RX

# 実行時に必要な依存のみ。cmake は rx-elf-gcc と併用（CMake ビルド）。
# libusb は e2-server-gdb が E2 Lite と USB 通信するために必要。
# usbutils は lsusb によるパススルー確認に使う。
# libgmp10/libmpfr6/libmpc3 は gcc の cc1 が動的リンクするランタイム
#   （これが無いと cc1 が libmpc.so.3 を読めずコンパイル不可）。
# libexpat1 は rx-elf-gdb がターゲット XML 記述の解釈に使う。
RUN apt-get update && apt-get install -y --no-install-recommends \
        cmake \
        libusb-1.0-0 \
        usbutils \
        libgmp10 libmpfr6 libmpc3 \
        libexpat1 \
        curl ca-certificates \
        xz-utils unzip \
    && rm -rf /var/lib/apt/lists/*

# ビルド済みツールチェーンを builder 段から取り込む（ソース・ビルド依存は持ち込まない）
COPY --from=toolchain-builder ${TOOLCHAIN_PREFIX} ${TOOLCHAIN_PREFIX}

# DebugComp/RX（e2-server-gdb / デバイスファイル / 支援ファイル）を
# Eclipse p2 手順で取得する: artifacts.xml.xz → 各アーティファクトの jar → 展開。
#
# 検証済み: jar を展開すると実体は *.tar.xz にまとまっており、下の
#   ループでさらに展開する必要がある（gdb_server.tar.xz / debug_support.tar.xz）。
#   rx-elf-gdb は DebugComp には含まれず、builder 段でソースビルドして
#   ${TOOLCHAIN_PREFIX} 側から供給する。
RUN <<EOF
#!/usr/bin/env bash
set -euo pipefail
ART="com.renesas.ide.supportfiles.rx.gdbserver.linux.x86_64
com.renesas.ide.supportfiles.rx.debug.gdb.linux.x86_64
com.renesas.ide.supportfiles.rx.debug.jlink.linux.x86_64
com.renesas.ide.supportfiles.rx.debug.ffw.linux.x86_64
com.renesas.ide.supportfiles.rx.debug"

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT
mkdir -p "${DEBUGCOMP_DIR}"

curl -fSL "${DEBUGCOMP_BASE_URL}/artifacts.xml.xz" | xz -d > "$work/artifacts.xml"

for id in $ART; do
  ver="$(grep -oE "id='${id}' version='[^']+'" "$work/artifacts.xml" | head -1 \
         | sed -E "s/.*version='([^']+)'.*/\1/")"
  if [ -z "$ver" ]; then
    echo "artifact not found in catalog: $id" >&2
    exit 1
  fi
  curl -fSL "${DEBUGCOMP_BASE_URL}/plugins/${id}_${ver}.jar" -o "$work/a.jar"
  unzip -qo "$work/a.jar" -d "${DEBUGCOMP_DIR}"
done

# jar 内の実体は tar.xz にまとめられている（検証で判明）:
#   gdb_server.tar.xz   → e2-server-gdb 本体・RtosPlugins
#   debug_support.tar.xz → デバイス IO ファイル・system-gdbinit・python 等の支援ファイル
# これらを DEBUGCOMP_DIR 直下へ展開し、e2-server-gdb を PATH 上に置く。
for tb in "${DEBUGCOMP_DIR}"/*.tar.xz; do
  [ -e "$tb" ] || continue
  tar -xJf "$tb" -C "${DEBUGCOMP_DIR}"
  rm -f "$tb"
done

# jar 展開で失われる実行権限を復元する
find "${DEBUGCOMP_DIR}" -type f \
     \( -name 'e2-server-gdb' -o -name 'rx-elf-gdb' -o -name '*.so' -o -name '*.so.*' \) \
     -exec chmod +x {} +
EOF

# ツールチェーンと DebugComp の実行ファイルへパスを通す
ENV DEBUGCOMP_DIR=${DEBUGCOMP_DIR}
ENV PATH="${TOOLCHAIN_PREFIX}/bin:${DEBUGCOMP_DIR}:${PATH}"

# E2 Lite デバッグ用のランタイムヘルパー（ビルドコンテキストは docker/）
COPY e2-server-gdb.sh /usr/local/bin/e2-server-gdb.sh
COPY rx64m.gdbinit    /opt/renesas/rx64m.gdbinit
RUN chmod +x /usr/local/bin/e2-server-gdb.sh

WORKDIR /work
