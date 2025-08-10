namespace IsrEmulation.Util;

public struct ScopedLock : IDisposable {
    readonly AsyncEventFlag? lockObject;
    bool alreadyDisposed;

    public ScopedLock(AsyncEventFlag lockObject) {
        this.lockObject = lockObject;
        alreadyDisposed = false;
    }

    /// <summary>
    /// ロックを開放します。
    /// ロックしていない場合は例外が発生します。
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Dispose() {
        if (alreadyDisposed) {
            throw new InvalidOperationException("already disposed.");
        }
        if (lockObject == null) {
            throw new NullReferenceException($"{nameof(lockObject)} uninitialized");
        }
        if (lockObject.HasSet()) {
            throw new InvalidOperationException("unlocked flag release.");
        }
        lockObject.Set();
        alreadyDisposed = true;
    }

    /// <summary>
    /// 排他制御として使用できるイベントフラグを作ります。
    /// </summary>
    public static AsyncEventFlag Create(bool allowSynchronousContinuations = true) {
        return new AsyncEventFlag(true, allowSynchronousContinuations);
    }

    /// <summary>
    /// ロックを取得します。
    /// </summary>
    public static async ValueTask<ScopedLock> Lock(AsyncEventFlag lockFlag, CancellationToken token) {
        await lockFlag.Wait(token, true);
        return new ScopedLock(lockFlag);
    }
}