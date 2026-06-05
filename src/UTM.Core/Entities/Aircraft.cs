using UTM.Core.Enums;
using UTM.Core.ValueObjects;

namespace UTM.Core.Entities;

/// <summary>
/// 航空器实体
/// </summary>
public sealed record Aircraft
{
    public required Guid Id { get; init; }
    public required string Registration { get; init; }
    public required AircraftType Type { get; init; }
    public required string Model { get; init; }
    public required string Manufacturer { get; init; }
    public required string OperatorId { get; init; }
    public required AircraftCapabilities Capabilities { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastSeenAt { get; init; }
}

/// <summary>
/// 航空器性能参数
/// </summary>
public sealed record AircraftCapabilities
{
    public required double MaxAltitudeMeters { get; init; }
    public required double MaxSpeedMps { get; init; }
    public required double MaxEnduranceSeconds { get; init; }
    public required double MaxPayloadKg { get; init; }
    public required double ClimbRateMps { get; init; }
    public required double DescentRateMps { get; init; }
    public required double TurnRateDps { get; init; }
}
