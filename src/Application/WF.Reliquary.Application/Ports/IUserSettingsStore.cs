using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Application.Ports;

public interface IUserSettingsStore
{
    Task<UserSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(UserSettings settings, CancellationToken cancellationToken = default);
}
