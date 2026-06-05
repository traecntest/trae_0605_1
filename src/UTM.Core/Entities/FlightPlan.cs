using UTM.Core.Enums;
using UTM.Core.ValueObjects;

namespace UTM.Core.Entities;

/// <summary>
/// 飞行计划实体
/// </summary>
public sealed record FlightPlan
{
    public required Guid Id { get; init; }
    public required string FlightId { get; init; }
    public required Guid AircraftId { get; init; }
    public required string OperatorId { get; init; }
    public FlightStatus Status { get; set; }
    public required FlightRoute Route { get; init; }
    public required TimeInterval PlannedTime { get; init; }
    public required FlightPurpose Purpose { get; init; }
    public required DateTimeOffset SubmittedAt { get; init; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? DepartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; init; }
}

/// <summary>
/// 飞行航线
/// </summary>
public sealed record FlightRoute
{
    public required GeoCoordinate DeparturePoint { get; init; }
    public required GeoCoordinate ArrivalPoint { get; init; }
    public required IReadOnlyList<Waypoint> Waypoints { get; init; }
    public required double CruiseAltitudeMeters { get; init; }
    public required double CruiseSpeedMps { get; init; }
}

/// <summary>
/// 飞行目的
/// </summary>
public enum FlightPurpose
{
    CargoDelivery,
    Inspection,
    Surveying,
    PassengerTransport,
    Emergency,
    Training,
    Recreation,
    Other
}
