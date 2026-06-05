using UTM.Core.Entities;
using UTM.Core.Enums;
using UTM.Core.ValueObjects;

namespace UTM.Airspace.Models;

public sealed record FlightPlanRequest
{
    public required Guid AircraftId { get; init; }
    public required string OperatorId { get; init; }
    public required FlightRoute Route { get; init; }
    public required TimeInterval PlannedTime { get; init; }
    public required FlightPurpose Purpose { get; init; }
    public string? Notes { get; init; }
}

public sealed record FlightPlanApprovalResult(bool IsApproved, Guid FlightPlanId, string? Reason = null);
public sealed record AirspaceAllocationResult(bool IsAllocated, Guid? AirspaceId = null, string? Reason = null);
