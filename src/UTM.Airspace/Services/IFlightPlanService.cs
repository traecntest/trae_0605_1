using UTM.Core.Entities;
using UTM.Core.Enums;
using UTM.Core.Interfaces;
using UTM.Airspace.Models;

namespace UTM.Airspace.Services;

public interface IFlightPlanService
{
    ValueTask<FlightPlanApprovalResult> SubmitAsync(FlightPlanRequest request, CancellationToken ct = default);
    ValueTask<FlightPlanApprovalResult> ApproveAsync(Guid flightPlanId, string approverId, CancellationToken ct = default);
    ValueTask RejectAsync(Guid flightPlanId, string reason, CancellationToken ct = default);
    ValueTask CancelAsync(Guid flightPlanId, CancellationToken ct = default);
    ValueTask<FlightPlan?> GetAsync(Guid flightPlanId, CancellationToken ct = default);
    ValueTask<IReadOnlyList<FlightPlan>> GetByOperatorAsync(string operatorId, CancellationToken ct = default);
    ValueTask<IReadOnlyList<FlightPlan>> GetByStatusAsync(FlightStatus status, CancellationToken ct = default);
}
