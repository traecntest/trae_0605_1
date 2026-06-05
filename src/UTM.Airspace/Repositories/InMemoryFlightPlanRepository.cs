using UTM.Core.Entities;
using UTM.Core.Interfaces;

namespace UTM.Airspace.Repositories;

public sealed class InMemoryFlightPlanRepository : IFlightPlanRepository
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, FlightPlan> _store = new();

    public ValueTask<FlightPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => ValueTask.FromResult(_store.GetValueOrDefault(id));

    public ValueTask<FlightPlan?> GetByFlightIdAsync(string flightId, CancellationToken ct = default)
        => ValueTask.FromResult(_store.Values.FirstOrDefault(p => p.FlightId == flightId));

    public ValueTask<IReadOnlyList<FlightPlan>> GetByOperatorAsync(string operatorId, CancellationToken ct = default)
        => ValueTask.FromResult<IReadOnlyList<FlightPlan>>(_store.Values.Where(p => p.OperatorId == operatorId).ToList());

    public ValueTask<IReadOnlyList<FlightPlan>> GetByStatusAsync(Core.Enums.FlightStatus status, CancellationToken ct = default)
        => ValueTask.FromResult<IReadOnlyList<FlightPlan>>(_store.Values.Where(p => p.Status == status).ToList());

    public ValueTask AddAsync(FlightPlan flightPlan, CancellationToken ct = default)
    {
        _store.TryAdd(flightPlan.Id, flightPlan);
        return ValueTask.CompletedTask;
    }

    public ValueTask UpdateAsync(FlightPlan flightPlan, CancellationToken ct = default)
    {
        _store.AddOrUpdate(flightPlan.Id, flightPlan, (_, _) => flightPlan);
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryRemove(id, out _);
        return ValueTask.CompletedTask;
    }
}
