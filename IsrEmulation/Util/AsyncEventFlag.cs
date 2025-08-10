using System.Threading.Channels;

namespace IsrEmulation.Util;

public class AsyncEventFlag {
    readonly Channel<byte> channel;

    public AsyncEventFlag(bool initialFlag, bool allowSynchronousContinuations = true) {
        channel = Channel.CreateBounded<byte>(new BoundedChannelOptions(1) {
            AllowSynchronousContinuations = allowSynchronousContinuations
        });
        if (initialFlag) {
            Set();
        }
    }

    /// <summary>
    /// イベントフラグを設定します。
    /// </summary>
    /// <returns>
    /// すでにフラグがセットされていた場合はfalse、
    /// セットできた場合はtrue
    /// </returns>
    public bool Set() {
        return channel.Writer.TryWrite(1);
    }

    /// <summary>
    /// イベントフラグをクリアします。
    /// フラグが設定されていない場合は、falseを返し、なにもしません。
    /// </summary>
    public bool TryClear() {
        return channel.Reader.TryRead(out _);
    }

    /// <summary>
    ///  イベントフラグが設定されているかどうかを返します。
    /// </summary>
    public bool HasSet() {
        return 0 < channel.Reader.Count;
    }

    /// <summary>
    /// イベントフラグがセットされるまで待機します。
    /// すでにイベントフラグが設定されている場合は待ちません。
    /// 読み出した時イベントフラグをクリアするかどうかも設定できます。
    /// 通常はクリアします。
    /// </summary>
    public async ValueTask Wait(CancellationToken token, bool autoClear = true) {
        var reader = channel.Reader;
        if (autoClear) {
            await reader.ReadAsync(token);
            return;
        } else {
            await reader.WaitToReadAsync(token);
        }
    }
}