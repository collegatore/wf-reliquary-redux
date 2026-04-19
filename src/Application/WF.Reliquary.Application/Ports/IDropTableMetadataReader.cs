namespace WF.Reliquary.Application.Ports;

public interface IDropTableMetadataReader
{
    string? ReadLastUpdate(string html);
}
