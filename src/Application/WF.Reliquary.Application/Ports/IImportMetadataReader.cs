namespace WF.Reliquary.Application.Ports;

public interface IImportMetadataReader
{
    Task<string?> GetImportedVersionAsync(CancellationToken ct = default);
}
