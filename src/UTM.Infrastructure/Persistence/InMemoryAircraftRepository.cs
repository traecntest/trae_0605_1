using System.Collections.Concurrent;
using UTM.Core.Entities;
using UTM.Core.Interfaces;

namespace UTM.Infrastructure.Persistence;

public sealed class InMemoryAircraftRepository : IAircraftRepository
{
    private readonly ConcurrentDictionary<Guid, Aircraft> _store = new();

    public ValueTask<Aircraft?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => ValueTask.FromResult(_store.GetValueOrDefault(id));

    public ValueTask<Aircraft?> GetByRegistrationAsync(string registration, CancellationToken ct = default)
        => ValueTask.FromResult(_store.Values.FirstOrDefault(a => a.Registration == registration));

    public ValueTask<IReadOnlyList<Aircraft>> GetByOperatorAsync(string operatorId, CancellationToken ct = default)
        => ValueTask.FromResult<IReadOnlyList<Aircraft>>(_store.Values.Where(a => a.OperatorId == operatorId).ToList());

    public ValueTask AddAsync(Aircraft aircraft, CancellationToken ct = default)
    {
        _store.TryAdd(aircraft.Id, aircraft);
        return ValueTask.CompletedTask;
    }

    public ValueTask UpdateAsync(Aircraft aircraft, CancellationToken ct = default)
    {
        _store.AddOrUpdate(aircraft.Id, aircraft, (_, _) => aircraft);
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryRemove(id, out _);
        return ValueTask.CompletedTask;
    }
}
