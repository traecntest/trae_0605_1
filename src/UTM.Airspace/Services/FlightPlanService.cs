using UTM.Core.Entities;
using UTM.Core.Enums;
using UTM.Core.Interfaces;
using UTM.Airspace.Models;
using UTM.Core.Exceptions;

namespace UTM.Airspace.Services;

public sealed class FlightPlanService(IFlightPlanRepository repository) : IFlightPlanService
{
    public async ValueTask<FlightPlanApprovalResult> SubmitAsync(FlightPlanRequest request, CancellationToken ct = default)
    {
        var flightPlan = new FlightPlan
        {
            Id = Guid.NewGuid(),
            FlightId = $"FLT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..19],
            AircraftId = request.AircraftId,
            OperatorId = request.OperatorId,
            Status = FlightStatus.Submitted,
            Route = request.Route,
            PlannedTime = request.PlannedTime,
            Purpose = request.Purpose,
            SubmittedAt = DateTimeOffset.UtcNow,
            Notes = request.Notes
        };

        await repository.AddAsync(flightPlan, ct);
        return new FlightPlanApprovalResult(true, flightPlan.Id);
    }

    public async ValueTask<FlightPlanApprovalResult> ApproveAsync(Guid flightPlanId, string approverId, CancellationToken ct = default)
    {
        var plan = await repository.GetByIdAsync(flightPlanId, ct)
            ?? throw new FlightPlanNotFoundException(flightPlanId);

        plan.Status = FlightStatus.Approved;
        plan.ApprovedAt = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(plan, ct);

        return new FlightPlanApprovalResult(true, flightPlanId);
    }

    public async ValueTask RejectAsync(Guid flightPlanId, string reason, CancellationToken ct = default)
    {
        var plan = await repository.GetByIdAsync(flightPlanId, ct)
            ?? throw new FlightPlanNotFoundException(flightPlanId);

        plan.Status = FlightStatus.Rejected;
        plan.RejectionReason = reason;
        await repository.UpdateAsync(plan, ct);
    }

    public async ValueTask CancelAsync(Guid flightPlanId, CancellationToken ct = default)
    {
        var plan = await repository.GetByIdAsync(flightPlanId, ct)
            ?? throw new FlightPlanNotFoundException(flightPlanId);

        plan.Status = FlightStatus.Cancelled;
        await repository.UpdateAsync(plan, ct);
    }

    public ValueTask<FlightPlan?> GetAsync(Guid flightPlanId, CancellationToken ct = default)
        => repository.GetByIdAsync(flightPlanId, ct);

    public ValueTask<IReadOnlyList<FlightPlan>> GetByOperatorAsync(string operatorId, CancellationToken ct = default)
        => repository.GetByOperatorAsync(operatorId, ct);

    public ValueTask<IReadOnlyList<FlightPlan>> GetByStatusAsync(FlightStatus status, CancellationToken ct = default)
        => repository.GetByStatusAsync(status, ct);
}
