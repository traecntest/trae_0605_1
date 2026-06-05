using UTM.Core.Entities;

namespace UTM.Core.Interfaces;

public interface IAircraftRepository
{
    ValueTask<Aircraft?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Aircraft?> GetByRegistrationAsync(string registration, CancellationToken ct = default);
    ValueTask<IReadOnlyList<Aircraft>> GetByOperatorAsync(string operatorId, CancellationToken ct = default);
    ValueTask AddAsync(Aircraft aircraft, CancellationToken ct = default);
    ValueTask UpdateAsync(Aircraft aircraft, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}
