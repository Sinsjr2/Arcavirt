# GdbStubDotnet 使用方法ガイド

このガイドは、GdbStubDotnet ライブラリを初めて使う人が自分のプロジェクトへ
組み込むための実践的な手順書です。設計の背景・決定理由は
[GdbStubDotnet-spec.md](spec/GdbStubDotnet/GdbStubDotnet-spec.md) を参照してください
(このガイドとは役割が異なり、重複を避けています)。

## 1. 何をしてくれるライブラリか

GdbStubDotnet は、あなたが実装した CPU エミュレータ/シミュレータへ、実 GDB
から `target remote` で接続してデバッグできるようにする GDB Remote Serial
Protocol (RSP) サーバーです。

ライブラリが提供するもの:

- パケットの送受信・フレーミング(`$...#checksum` の組立・検証)
- 標準的な RSP コマンドの解析(`m`/`M`/`X`/`g`/`G`/`p`/`P`/`Z`/`z`/`vCont`/`H` 等)
- 応答の整形(`OK`/`E NN`/hex エンコード/生テキスト)
- 実行・通知の協調(all-stop の stop reply、non-stop の `%Stop`/`vStopped` ドレイン)

ライブラリが**提供しないもの**(= あなたが実装するもの):

- メモリ・レジスタの実体(あなたのエミュレータが持っている)
- 実行の駆動(resume/step を実際に進める処理)

## 2. インストール

```bash
dotnet add package GdbStubDotnet
```

(社内 NuGet フィード等、配布方法はプロジェクトの運用に従ってください。)

## 3. 最小構成(クイックスタート)

```csharp
using GdbStubDotnet;
using System.Net;

byte[] registers = new byte[64];   // あなたのターゲットのレジスタ実体
byte[] memory = new byte[0x10000]; // あなたのターゲットのメモリ実体

using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 12345));

using var server = new StubServerBuilder()
    .Map(Commands.ReadRegisters, (cmd, res) => res.HexBytes(registers))
    .Map(Commands.WriteRegisters, (cmd, res) => {
        cmd.Data.AsSpan().CopyTo(registers);
        res.Ok();
    })
    .Map(Commands.ReadMemory, (cmd, res) => res.HexBytes(memory.AsSpan((int)cmd.Addr, cmd.Len)))
    .Map(Commands.WriteMemory, (cmd, res) => {
        cmd.Data.AsSpan().CopyTo(memory.AsSpan((int)cmd.Addr));
        res.Ok();
    })
    .Map(Commands.HaltReason, (cmd, res) => res.Text("S05"u8))
    .UseTransport(transport)
    .Build();

server.Start();

// gdb -nx -batch -ex "target remote :12345" -ex "info registers" ...
```

これだけで、gdb から接続してレジスタ・メモリの読み書きができます。

## 4. 唯一の拡張点: `Map`

このライブラリの設計方針は「コアはシンプルに、利用者は `Map` で自由に拡張
できるように」です(仕様書 §6.1)。`IRspMemoryAccess` のような「実装を強いる
インタフェース」は存在しません。

```csharp
stub.Map(Commands.ReadMemory, (cmd, res) => {
    int n = myEmu.ReadMem(cmd.Addr, buf.AsSpan(0, cmd.Len));
    if (n == 0) res.Error(new RspError(14, null)); // 例: EFAULT相当のコード
    else        res.HexBytes(buf.AsSpan(0, n));
});
```

- 第1引数はライブラリ提供のパーサ(`Commands.*`)。ワイヤ解析を自分で書く必要はありません。
- 第2引数のハンドラは、コマンドの種別によって受け取る型が変わります:
  - 同期コマンド(`m`/`g`/`Z` 等) → `ResponseWriter<SyncResponse>`(`Ok()`/`Error()`/`HexBytes()`/`Text()`)
  - 実行コマンド(`c`/`s`/`vCont`) → `ExecutionResponder`(`ReportStop()`/`Reject()`、後述)

同じ入力パターンに一致する `Map` を複数回登録した場合、**最初に登録した
ものが優先**されます(ルーティングは登録順に最初の一致を採用するため)。
`QNonStop`/`vStopped`/`H`/`qSupported` の組込み既定ハンドラは、常に
利用者が明示的に登録した `Map` より後ろに追加されるため、利用者が同じ
コマンドを `Map` すればそちらが優先されます(`H`/`qSupported` は上書き
前提、§6.6)。

## 5. 実行コマンド(`c`/`s`/`vCont`)の扱い

実行コマンドは同期応答では表現できません。ハンドラは即座に return し、
ターゲットの resume は非同期(別スレッド等)で進めます。停止したら
`ExecutionResponder.ReportStop` を呼びます。

```csharp
stub.Map(Commands.VCont, (cmd, exec) => {
    myEmu.Resume(cmd.Actions, stop => exec.ReportStop(in stop));
    // ここでは同期応答を書かない。FW がモード(all-stop/non-stop)に応じて
    // 応答形式を自動で組み立てる。
});
```

- `ExecutionResponder` は長命・スレッドセーフです。ターゲット実行スレッドから
  呼び出しても構いません。
- resume 要求自体が不正な場合は `exec.Reject(new RspError(code, null))` を
  ハンドラ内で**同期的に**呼びます。

## 6. non-stop モードは自動対応(利用者コード不要)

`QNonStop` のネゴシエーション(`qSupported` での `QNonStop+` 広告、
`QNonStop:1/0` 受信でのモード切替、`%Stop` 通知、`vStopped` ドレイン)は
ライブラリが組込み既定で自動的に処理します。**利用者は何も `Map` しなくても
non-stop が動作します**。

- all-stop: `vCont` 実行後、停止したら単一の stop reply(`T05` 等)が返る。
- non-stop: `vCont` 実行後、即座に `OK` が返り、対象スレッドが停止すると
  `%Stop` 通知が非同期に送出され、`vStopped` で残りをドレインする。

ハンドラ側のコード(`exec.ReportStop(...)` を呼ぶ)は all-stop/non-stop で
**同じ**です。モードによる分岐はライブラリ内部で吸収されます。

> **注意**: `Commands.Supported`(`qSupported`)を自分で `Map` して独自の
> 機能一覧(`multiprocess+` 等)を返す場合、組込み既定の `qSupported`
> ハンドラは丸ごと上書きされ、`QNonStop+` の広告も失われます。実 gdb は
> `QNonStop+` が広告されていないと non-stop を有効化しようとしないため、
> `qSupported` を自前で実装する場合は応答テキストに `QNonStop+` を
> 自分で含めてください(例: `res.Text("multiprocess+;QNonStop+"u8)`)。
> この制約はコードレビューで判明した既知の課題であり、恒久的な解決策
> (FW側の機能一覧と利用者側の機能一覧を合成する仕組み)は別途検討中です。

## 7. `H`(スレッド選択)も自動対応

`H`(`Hg<thread-id>`)コマンドもライブラリが組込み既定で処理します。以後、
`Thread` フィールドを持つコマンド(`g`/`G`/`m`/`M`/`p`/`P` 等)には、選択済みの
スレッドが自動的に充填された状態でハンドラが呼ばれます。

```csharp
stub.Map(Commands.ReadRegisters, (cmd, res) => {
    // cmd.Thread には直前の H で選択されたスレッドが入っている
    res.HexBytes(myEmu.GetRegisters(cmd.Thread));
});
```

独自のスレッド選択ロジックが必要な場合は `Commands.SetThread` を自分で
`Map` すれば上書きできますが、その場合この自動充填の恩恵は失われます
(自前でスレッド状態を追跡する必要があります)。

## 8. 割込み(Ctrl-C)

gdb がターゲット実行中に Ctrl-C を送ると、生の `0x03` バイトまたは
`vCtrlC` パケットとして届きます。どちらも同じ経路に集約されます。

```csharp
stub.Map(Commands.Continue, (cmd, exec) => {
    var cts = new CancellationTokenSource();
    myEmu.ResumeAsync(cts.Token, stop => exec.ReportStop(in stop));
    // OnInterrupt は接続全体で1つ。実行中のresumeに割込を伝える手段は
    // 利用者側で用意する(ここではCTSをクロージャで捕まえる等)。
})
.OnInterrupt(() => myEmu.RequestStop());
```

割込み自体への直接応答はありません。ターゲットが実際に停止したら、
通常どおり `exec.ReportStop` を呼べば SIGINT 相当の stop reply が返ります。

## 9. エラーの扱い

- **GDB へ向けたエラー**(予期内): `res.Error(new RspError(code, detail))`。
  `EnableDetailedErrors(true)` を呼ぶと `E.<text>`(人間可読)になります。
  既定は `E NN`(数値コードのみ)。
- **ホストへ向けた診断**(予期しない例外): ハンドラ内の例外は境界で
  catch され、`OnError(StubFault)` へのみ通知されます。GDB へは安全な
  エラー応答が自動送出されます。

```csharp
stub.OnError(fault => myLogger.LogError(fault.Error, fault.Context));
```

- **切断の検知**: TCP 切断(クライアントが接続を閉じた、または読み取りが
  例外になった)は `OnDisconnect(Action)` で検知できます。`D`(detach)
  コマンドとは別の経路です。応答は発生しません。

```csharp
stub.OnDisconnect(() => myEmu.Suspend());
```

## 10. トランスポートの差し替え

既定で `TcpTransport`(TCP 単一クライアント)が同梱されています。別の搬送路
(名前付きパイプ、UART 経由等)を使いたい場合は `ITransport` を実装して
`UseTransport` に渡してください。

```csharp
public interface ITransport {
    ValueTask<int> ReadAsync(Memory<byte> buffer);
    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer);
    void Close();
}
```

## 11. 動作確認(実 gdb で試す)

```bash
gdb -nx -batch \
  -ex "target remote 127.0.0.1:12345" \
  -ex "break *0x1000" \
  -ex "info registers eax" \
  -ex "detach"
```

引数無しで `target remote` した場合、gdb はホスト既定のアーキテクチャ
(64bit Linux 環境では多くの場合 `i386`)を仮定します。レジスタは
`g`/`G` の順序・バイト数がそのアーキテクチャの並びと一致している必要が
あります。実際のターゲットに合わせて `qXfer:features:read`(target
description XML)を `Map` で追加すれば、任意のカスタムアーキテクチャの
レジスタレイアウトを gdb に伝えられます(`拡張`扱い、§5.1)。

具体的な動作例は `GdbStubDotnetTest/RealGdbTest.cs` を参照してください。

## 12. テスト戦略の参考

自分のターゲット組み込みをテストする際は、以下の3層を参考にしてください
(仕様書 §9)。

- **L1**: パーサ・応答整形など個々の関数の単体テスト。
- **L2**: `TcpClient` でスクリプト化したクライアントを使い、`StubServerBuilder`
  で組んだサーバーとの全プロトコル交換を決定的に検証する(このライブラリ
  自体のテストの主戦場。`GdbStubDotnetTest` 内の大半のテストがこの形)。
- **L3**: 実 gdb をサブプロセスとして起動し、`target remote` で接続する
  結合テスト。CI では `gdb` が使える環境に限定し、通常の L1/L2 実行からは
  除外することを推奨します(`dotnet test --filter "TestCategory!=L3"`)。
