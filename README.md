# Arcavirt

## 名前の由来

**Arcavirt** （アルカヴァート）は、**Arcadia**（理想郷）と **Virtual**（仮想）を組み合わせた造語です。

実機のCPU、煩雑な配線が必要なステッパーモーター、高価なオシロスコープ、そしてデバッグ用のJTAG。  
これら物理ハードウェアの準備や故障のリスクに縛られることなく、
ソフトウェアの力だけで「エンジニアが真に集中できる理想的な検証空間」を提供したいという願いが込められています。

# dockerのビルド方法
1. `https://llvm-gcc-renesas.com/ja/rx-download-toolchains/` より `GCC for Renesas 14.2.0.202511-GNURX Linux Toolchain (ELF)`を  
  ダウンロードする。
2. `docker/installers`にダウンロードしたファイルを保存する。
3. `docker compose build` を実行する。

# 使用ツール
- vs code
- .Net 8.0  
  いずれかの方法でインストールして下さい  
  ※ Visual Studio 自体は不要です。  
  - Visual Studio からC# をインストールするか
  - 以下から、SDK をインストールする  
    https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0
- e2 studio  
  RX 用の gcc コンパイラーのインストールが必要  
  ※下記説明ではgccを使用するが、ビルドの設定を行うと cc rxでも可  
  RX 用の開発環境 (デバッガープログラムなど)


# はじめかた

### RX64M のプログラムをビルドする
1. e2 studio より RenesasRXNative/test 内のプロジェクトをインポートしてください。
2. e2 studio 内でビルドボタンを押しビルド(HardwareDebug)します。(gcc)
3. フォルダー(HardwareDebug)にtest.elfファイルが作成されることを確認します。

### RX64M用CPUシミュレータを実行する
1. vs code で「実行とデバッグ」ペインを開きます(Ctrl + Shift + D)。
2. Lanch GDB でCPUシミュレータを実行開始します。(TCPサーバーが起動します。)
3. カレントディレクトリをgit リポジトリのルートに移動し、
  RX 用 GDB(rx-elf-gdb)をコンソール上で起動します。  
  ※ rx-elf-gdb はe2 studioでICE デバッガーを使う場合にも起動します。
  ※ どこにインストールされているかは、windows であれば、タスクマネージャー、linux であれば、psを使用して探して下さい。
  linux 版e2 studio では以下にインストールされます。  
  ``` bash
  ~/.local/share/renesas/e2_studio/toolchains/gcc-8.3.0.202411-GNURX-ELF/gcc_8.3.0.202411_rx_elf/bin/rx-elf-gdb
  ```
4. gdb 上で以下のコマンドを実行し、プログラムをCPUシミュレータにロードします。
  ``` txt
  # connect to cpu simulator by tcp port
  target remote localhost:3333
  # load debug symbol
  symbol-file ./RenesasRXNative/test/HardwareDebug/test.elf
  # write binary to cpu simulator
  load ./RenesasRXNative/test/HardwareDebug/test.elf
  # reset CPU simulator
  monitor reset

  ```
5. プログラムを実行します。
  ``` txt
  # continue program
  c
  ```
  ブレークして中断するときは、Ctrl + c を押して下さい。
6. vs code の デバッグコンソール に MCUから出力した文字列が表示されます。

# メモ
GDBStubのAPIデザイン参考
[gdbstub](https://github.com/daniel5151/gdbstub#)
