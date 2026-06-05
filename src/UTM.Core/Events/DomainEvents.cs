using UTM.Core.Entities;
using UTM.Core.Enums;

namespace UTM.Core.Events;

public sealed record FlightPlanSubmitted(FlightPlan FlightPlan, DateTimeOffset Timestamp);
public sealed record FlightPlanApproved(Guid FlightPlanId, string ApproverId, DateTimeOffset Timestamp);
public sealed record FlightPlanRejected(Guid FlightPlanId, string Reason, DateTimeOffset Timestamp);
public sealed record FlightPlanCancelled(Guid FlightPlanId, DateTimeOffset Timestamp);
public sealed record FlightStarted(Guid FlightPlanId, DateTimeOffset Timestamp);
public sealed record FlightCompleted(Guid FlightPlanId, DateTimeOffset Timestamp);
public sealed record ConflictDetected(Guid AircraftId1, Guid AircraftId2, ConflictSeverity Severity, double Tcpa, double Cpa, DateTimeOffset Timestamp);
public sealed record ConflictResolved(Guid ConflictId, DateTimeOffset Timestamp);
public sealed record AirspaceViolation(Guid AircraftId, Guid AirspaceId, string Reason, DateTimeOffset Timestamp);
public sealed record EmergencyDeclared(Guid AircraftId, string Reason, DateTimeOffset Timestamp);
