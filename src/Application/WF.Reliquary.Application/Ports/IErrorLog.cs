namespace WF.Reliquary.Application.Ports;

public interface IErrorLog
{
    string LogFilePath { get; }

    Task AppendAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default);
}
