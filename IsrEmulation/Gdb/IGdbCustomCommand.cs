using System.Text;

namespace Gdb;

/// <summary>
/// qRcmd コマンドを処理します。
/// </summary>
public interface IGdbCustomCommand {
    void RunCustomCommand(StringBuilder response, string command);
}