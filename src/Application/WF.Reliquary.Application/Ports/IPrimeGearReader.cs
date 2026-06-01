using WF.Reliquary.Application.PrimeGear;

namespace WF.Reliquary.Application.Ports;

public interface IPrimeGearReader
{
    Task<IReadOnlyList<PrimeGearEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}
