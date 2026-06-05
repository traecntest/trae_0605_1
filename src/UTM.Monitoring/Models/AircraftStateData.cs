using UTM.Core.ValueObjects;

namespace UTM.Monitoring.Models;

/// <summary>
/// 航空器实时状态数据
/// </summary>
public sealed record AircraftStateData
{
    public required Guid AircraftId { get; init; }
    public required GeoCoordinate Position { get; init; }
    public required double Altitude { get; init; }
    public required double GroundSpeed { get; init; }
    public required double Track { get; init; }
    public required double VerticalRate { get; init; }
    public required double Heading { get; init; }
    public double? BatteryLevel { get; init; }
    public double? SignalStrength { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}
