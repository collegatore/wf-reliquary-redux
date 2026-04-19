namespace WF.Reliquary.Application.Ports;

public interface IStorageHealthCheck
{
    string ProviderName { get; }

    bool CanCreateConnectionString();
}
