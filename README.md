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

## コンテナの実行ユーザー
コンテナはデフォルトで非 root(UID 1000)で動作します。`tool/` のラッパースクリプトを
使うと、ホストの UID/GID を自動で引き継いでコンテナを実行するため、
ビルド成果物はホストユーザーの所有になります。

- `tool/rx-run.sh <コマンド>` : 任意のコマンドをコンテナ内で実行する(例: `tool/rx-run.sh bash`)
- `tool/build-rx.sh [preset]` : RX64M 向け CMake ビルドを一発実行する(preset 省略時は rx64m)

`docker compose run` を直接使う場合、UID は 1000 固定になります。ホストの UID が
1000 以外の環境ではラッパーを使ってください。

### 過去に root コンテナでビルドしていた場合(移行手順)
root 実行時代のビルド成果物が残っていると、非 root ビルドが書き込みに失敗します。
一度削除してからビルドし直してください:
``` bash
sudo rm -rf RenesasRXNative/test/build-rx64m
```

### E2 Lite を非 root で使うための udev ルール
非 root コンテナから e2-server-gdb が E2 Lite(USB)へアクセスするには、ホスト側に
udev ルールが必要です。e2 studio をインストールしたことがあるホストには
`/etc/udev/rules.d/99-renesas-emu.rules` として配置済みの場合があります。
無い場合は以下の内容で `99-renesas-emu.rules` を作成してください:

``` text
ACTION!="add", SUBSYSTEM!="usb_device", GOTO="emu_rules_end"
# Remove sudo access to E2/E2 Lite/E1/E20/IE850A emulator
ATTR{idProduct}=="82a1", ATTR{idVendor}=="045b", MODE="666"
ATTR{idProduct}=="82a0", ATTR{idVendor}=="045b", MODE="666"
ATTR{idProduct}=="823b", ATTR{idVendor}=="045b", MODE="666"
ATTR{idProduct}=="823c", ATTR{idVendor}=="045b", MODE="666"
ATTR{idProduct}=="0250", ATTR{idVendor}=="045b", MODE="666"
# Prevent E2/E2Lite/E1/E20/IE850A from being captured by modem manager service as E2/E2 Lite/E1/E20/IE850A is not a modem
ATTR{idProduct}=="82a1", ATTR{idVendor}=="045b", ENV{ID_MM_DEVICE_IGNORE}="1"
ATTR{idProduct}=="82a0", ATTR{idVendor}=="045b", ENV{ID_MM_DEVICE_IGNORE}="1"
ATTR{idProduct}=="823b", ATTR{idVendor}=="045b", ENV{ID_MM_DEVICE_IGNORE}="1"
ATTR{idProduct}=="823c", ATTR{idVendor}=="045b", ENV{ID_MM_DEVICE_IGNORE}="1"
ATTR{idProduct}=="0250", ATTR{idVendor}=="045b", ENV{ID_MM_DEVICE_IGNORE}="1"
LABEL="emu_rules_end"
```

インストール手順:
``` bash
sudo cp 99-renesas-emu.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

# claude code / beads (bd) をコンテナ内で使う

コンテナは常駐方式(`docker compose up -d`)で動作し、`docker attach` で対話的に
出入りします。`docker compose run --rm` の使い捨て方式(旧`tool/rx-run.sh`)とは異なり、
コンテナ内のプロセス(claude codeを含む)はデタッチしても動き続けます。

## 起動・接続

``` bash
# 初回・以降の起動(既に起動していれば何もしない)
docker compose up -d dev

# コンテナへ入る(衝突しないdetach-keysを指定する)
docker attach --detach-keys='ctrl-q,ctrl-q' $(docker compose ps -q dev)

# コンテナ内で
claude
```

デタッチするには `Ctrl-Q Ctrl-Q` を押す(標準の `Ctrl-P Ctrl-Q` ではない)。中の
claude codeプロセスは動き続けるので、後で同じ `docker attach` コマンドで戻れる。

## 外部から一発コマンドを実行する

``` bash
docker compose exec dev <コマンド>
```

## claude codeの初回ログイン

認証情報は `claude_home` volume(`/home/ubuntu` 相当)に保存され、コンテナの
再作成(`docker compose down` → `up`含む)後も保持される。ホストの `~/.claude` とは
別管理のため、コンテナ内で初回のみ改めてログインが必要:

``` bash
docker attach --detach-keys='ctrl-q,ctrl-q' $(docker compose ps -q dev)
claude
> /login
```

## .beadsの同時利用について

`.beads` はホストとコンテナで共有している(`/work` の一部)。embedded
Doltにはサーバー調停がないため、**ホストとコンテナで同時に `bd` を使わないこと**。

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

ホストから 1 コマンドでビルドできます:
``` bash
tool/build-rx.sh
```
`RenesasRXNative/test/build-rx64m/cmake_test` に elf ファイルが作成されます。
コンテナはホストと同じ UID/GID で実行されるため、成果物はホストユーザーの所有になります。

コンテナに入って手動でビルドする場合:
1. コンテナに入る: `tool/rx-run.sh bash`
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
