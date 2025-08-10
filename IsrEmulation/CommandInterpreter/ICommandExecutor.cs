namespace IsrEmulation.CommandInterpreter;

public interface ICommandExecutor {
    ValueTask Run(CancellationToken token);
}