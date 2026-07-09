# Arcavirt

## 名前の由来

**Arcavirt** （アルカヴァート）は、**Arcadia**（理想郷）と **Virtual**（仮想）を組み合わせた造語です。

実機のCPU、煩雑な配線が必要なステッパーモーター、高価なオシロスコープ、そしてデバッグ用のJTAG。  
これら物理ハードウェアの準備や故障のリスクに縛られることなく、
ソフトウェアの力だけで「エンジニアが真に集中できる理想的な検証空間」を提供したいという願いが込められています。

# dockerのビルド方法
1. `docker compose build` を実行する。
   - RX 用ツールチェーン（gcc/binutils/newlib）をソースからビルドし、
     デバッグ用の DebugComp/RX（e2-server-gdb 等）も取得するため、
     手動でのダウンロードや e2 studio のインストールは不要。
   - 初回ビルドはソースビルドのため数十分かかる。
2. E2 Lite で実機デバッグする場合は `doc/renesas-rx-e2lite-debug.md` を参照する。

# 使用ツール
- vs code
- .Net 8.0  
  いずれかの方法でインストールして下さい  
  ※ Visual Studio 自体は不要です。  
  - Visual Studio からC# をインストールするか
  - 以下から、SDK をインストールする  
    https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0


# はじめかた

### RX64M のプログラムをビルドする
RX 用ツールチェーンや DebugComp は Docker イメージに同梱されているため、
e2 studio のインストールは不要です。
1. コンテナに入る: `docker compose run --rm dev bash`
2. CMake preset でビルドする:
   ``` bash
   cd RenesasRXNative/test
   cmake --preset rx64m
   cmake --build --preset rx64m
   ```
3. `RenesasRXNative/test/build-rx64m/cmake_test` に elf ファイルが作成されることを確認します。

e2 studio を過去に使っていた場合、`RenesasRXNative/test/HardwareDebug/`・`trash/` に
旧プロジェクトのビルド成果物が残っていることがあります（`.gitignore` 済みで
git 管理外）。不要であれば手動で削除してください。
なお `RenesasRXNative/test/test HardwareDebug.launch` は e2 studio + E2 Lite で
デバッグする際の起動構成として残しています（ビルドは上記の CMake で行う）。

### RX64M用CPUシミュレータを実行する
1. vs code で「実行とデバッグ」ペインを開きます(Ctrl + Shift + D)。
2. Lanch GDB でCPUシミュレータを実行開始します。(TCPサーバーが起動します。)
3. カレントディレクトリをgit リポジトリのルートに移動し、
  RX 用 GDB(rx-elf-gdb)をコンソール上で起動します。  
  ※ この手順はホスト側で動作する CPU シミュレータ（localhost:3333）に
  接続するため、gdb もホスト側で起動する必要があります（Docker コンテナ内の
  gdb では到達できません）。
  ※ どこにインストールされているかは、windows であれば、タスクマネージャー、linux であれば、psを使用して探して下さい。
  e2 studio をインストール済みであれば、linux 版では以下にあります。  
  ``` bash
  ~/.local/share/renesas/e2_studio/toolchains/gcc-8.3.0.202411-GNURX-ELF/gcc_8.3.0.202411_rx_elf/bin/rx-elf-gdb
  ```
  e2 studio をインストールしない場合は、ホスト上で RX 用 gdb を別途用意してください。
4. gdb 上で以下のコマンドを実行し、プログラムをCPUシミュレータにロードします。
  ``` txt
  # connect to cpu simulator by tcp port
  target remote localhost:3333
  # load debug symbol
  symbol-file ./RenesasRXNative/test/build-rx64m/cmake_test
  # write binary to cpu simulator
  load ./RenesasRXNative/test/build-rx64m/cmake_test
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
