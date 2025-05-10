using System.Text;

namespace Gdb.Thread.SingleThread;

public interface IGDBSingleThread : IGDBThread {

    ISingleThreadResume? ResumeObject { get; }

    int NumOfRegisters { get; }

    /// <summary>
    /// 指定したレジスタの値を読み込みます。
    /// ASCII形式16進数を追加で書き込んで下さい。
    /// 消すことは想定していません。
    /// 読み込みに失敗した場合は、0を返します。
    /// 読み込めた場合は書き込んだ文字数を返します。
    /// </summary>
    int ReadRegister(int i, StringBuilder result);

    /// <summary>
    /// 指定したレジスタに値を書き込みます。
    /// ASCII形式16進数でレジスタに書き込む値を渡します。
    /// 書き込んだ文字数を返します。
    /// 書き込みに失敗した場合(valueの長さが短い時)は、0を返します。
    /// </summary>
    int WriteRegisters(int i, ReadOnlySpan<char> value);

    bool ReadMemory(StringBuilder result, ulong startAddr, ulong length);

    bool WriteMemory(ulong startAddr, ReadOnlySpan<char> values);
}