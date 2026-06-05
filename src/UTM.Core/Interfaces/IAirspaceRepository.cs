using UTM.Core.Entities;

namespace UTM.Core.Interfaces;

public interface IAirspaceRepository
{
    ValueTask<Airspace?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<IReadOnlyList<Airspace>> GetAllAsync(CancellationToken ct = default);
    ValueTask<IReadOnlyList<Airspace>> GetActiveAsync(CancellationToken ct = default);
    ValueTask AddAsync(Airspace airspace, CancellationToken ct = default);
    ValueTask UpdateAsync(Airspace airspace, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}
