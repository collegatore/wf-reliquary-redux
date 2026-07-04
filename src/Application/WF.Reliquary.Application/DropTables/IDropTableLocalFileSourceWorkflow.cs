namespace WF.Reliquary.Application.DropTables;

public interface IDropTableLocalFileSourceWorkflow
{
    Task<DropTableFileRegistrationResult> RegisterFromFileAsync(string filePath, CancellationToken cancellationToken = default);

    Task<DropTableFileRegistrationResult> ConfirmSaveAsync(DropTableSourceContent pending, long existingId, CancellationToken cancellationToken = default);
}
