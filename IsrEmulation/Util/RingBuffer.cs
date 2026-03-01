using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Util;

/// <summary>
/// リングバッファー
/// 容量が空のときは、何も追加できず、何も値を取り出せません。
/// </summary>
public class RingBuffer<T> : IEnumerable<T> {
    readonly Queue<T> buf;

    public int Count => buf.Count;

    public int Capacity { get; }

    public RingBuffer(int capacity) {
        Capacity = capacity;
        buf = new Queue<T>(capacity + 1);
    }

    /// <summary>
    /// 空きがある場合は追加し、trueを返します。
    /// 満タンの場合は、falseを返し、追加しません。
    /// </summary>
    public bool TryEnqueue(T item) {
        if (Count < Capacity) {
            buf.Enqueue(item);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 指定した容量以上の場合は、初めに追加したデータを上書きします。
    /// </summary>
    public void Enqueue(T item) {
        buf.Enqueue(item);

        if (Count > Capacity) {
            buf.Dequeue();
        }
    }

    public bool TryDequeue([MaybeNullWhen(false)] out T result) => buf.TryDequeue(out result);
    public T Dequeue() => buf.Dequeue();

    public bool TryPeek([MaybeNullWhen(false)] out T result) => buf.TryPeek(out result);
    public T Peek() => buf.Peek();

    public void Clear() {
        buf.Clear();
    }

    public IEnumerator<T> GetEnumerator() => buf.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        buf.GetEnumerator();
}
