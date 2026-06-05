using UTM.Core.Entities;

namespace UTM.Core.Interfaces;

public interface IFlightPlanRepository
{
    ValueTask<FlightPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<FlightPlan?> GetByFlightIdAsync(string flightId, CancellationToken ct = default);
    ValueTask<IReadOnlyList<FlightPlan>> GetByOperatorAsync(string operatorId, CancellationToken ct = default);
    ValueTask<IReadOnlyList<FlightPlan>> GetByStatusAsync(Enums.FlightStatus status, CancellationToken ct = default);
    ValueTask AddAsync(FlightPlan flightPlan, CancellationToken ct = default);
    ValueTask UpdateAsync(FlightPlan flightPlan, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}
