using UTM.Core.ValueObjects;

namespace UTM.Core.Entities;

/// <summary>
/// 航点实体
/// </summary>
public sealed record Waypoint
{
    public required int Sequence { get; init; }
    public required GeoCoordinate Position { get; init; }
    public required double AltitudeMeters { get; init; }
    public double? SpeedMps { get; init; }
    public double? HoldTimeSeconds { get; init; }
    public string? Name { get; init; }
    public WaypointAction Action { get; init; } = WaypointAction.FlyThrough;
}

public enum WaypointAction
{
    FlyThrough,
    Hover,
    Orbit,
    Land,
    Takeoff
}
