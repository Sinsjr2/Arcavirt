using System.Diagnostics;
using System.Net;
using System.Text;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

/// <summary>
/// 仕様§9のL3層(実gdbプロセスとの結合テスト)。実gdbをサブプロセスとして
/// 起動し、target remoteで本ライブラリのStubServerへ接続、ブレークポイント
/// 設定とレジスタ読み出しができることを確認する(エピックの受け入れ基準、
/// Arcavirt-o3e.16)。gdbがインストールされていない環境では実行できないため
/// [Category("L3")]を付与し、L1/L2の通常実行(`dotnet test --filter
/// "TestCategory!=L3"`)からは除外できるようにしている(§9.3「固定gdbジョブ」)。
/// </summary>
[TestFixture]
[Category("L3")]
public class RealGdbTest {
    /// <summary>
    /// gdbが引数無しで接続した際の既定アーキテクチャ(このマシンでは i386)
    /// に合わせ、g/G の16レジスタ(4バイト×16=64バイト)構成で応答する
    /// 最小のホスト型エミュレータを構築する。qfThreadInfo/qC/H(SetThread
    /// は組込み既定)で単一スレッド(tid=1)を報告し、他のq系問い合わせは
    /// Commands.Query経由で個別に応答する(未知のものはFW既定の空応答へ
    /// フォールバック)。
    /// </summary>
    [Test]
    public async Task RealGdb_ConnectAndSetBreakpointAndReadRegisters_Succeeds() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        byte[] registers = new byte[64];
        byte[] memory = new byte[0x2000];

        using var server = new StubServerBuilder()
            .Map(Commands.Supported, (cmd, res) => res.Text("PacketSize=4000"u8))
            .Map(Commands.ThreadInfoStart, (cmd, res) => res.Text("m1"u8))
            .Map(Commands.CurrentThread, (cmd, res) => res.Text("QC1"u8))
            .Map(Commands.HaltReason, (cmd, res) => res.Text("S05"u8))
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
            .Map(Commands.InsertBreakpoint, (cmd, res) => res.Ok())
            .Map(Commands.RemoveBreakpoint, (cmd, res) => res.Ok())
            .Map(Commands.Query, (cmd, res) => {
                string name = Encoding.ASCII.GetString(cmd.Name);
                if (name == "sThreadInfo") {
                    res.Text("l"u8);
                } else if (name == "Attached") {
                    res.Text("1"u8);
                } else {
                    res.Empty();
                }
            })
            .UseTransport(transport)
            .Build();
        server.Start();

        string gdbOutput = await RunGdbAsync(transport.Port);

        Assert.Multiple(() => {
            Assert.That(gdbOutput, Does.Contain("Breakpoint 1 at 0x1000"));
            Assert.That(gdbOutput, Does.Contain("eax"));
            Assert.That(gdbOutput, Does.Not.Contain("Remote communication error"));
        });
    }

    private static async Task<string> RunGdbAsync(int port) {
        var startInfo = new ProcessStartInfo {
            FileName = "gdb",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("-nx");
        startInfo.ArgumentList.Add("-batch");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add($"target remote 127.0.0.1:{port}");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("break *0x1000");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("info registers eax");

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await process.WaitForExitAsync(cts.Token);

        string stdout = await stdoutTask;
        string stderr = await stderrTask;
        return stdout + stderr;
    }
}