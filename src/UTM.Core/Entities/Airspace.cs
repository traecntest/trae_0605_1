using UTM.Core.Enums;
using UTM.Core.ValueObjects;

namespace UTM.Core.Entities;

/// <summary>
/// 空域实体
/// </summary>
public sealed record Airspace
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required AirspaceClass Classification { get; init; }
    public required AirspaceVolume Volume { get; init; }
    public required AirspaceRules Rules { get; init; }
    public int Capacity { get; init; } = int.MaxValue;
    public int CurrentOccupancy { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
}

/// <summary>
/// 空域体积（三维空间范围）
/// </summary>
public sealed record AirspaceVolume
{
    public required GeoCoordinate Center { get; init; }
    public required double RadiusMeters { get; init; }
    public required double LowerAltitudeMeters { get; init; }
    public required double UpperAltitudeMeters { get; init; }
    public IReadOnlyList<GeoCoordinate>? BoundaryPolygon { get; init; }

    public BoundingBox GetBoundingBox()
    {
        if (BoundaryPolygon is { Count: > 0 })
        {
            var minLat = BoundaryPolygon.Min(p => p.Latitude);
            var maxLat = BoundaryPolygon.Max(p => p.Latitude);
            var minLon = BoundaryPolygon.Min(p => p.Longitude);
            var maxLon = BoundaryPolygon.Max(p => p.Longitude);
            return new BoundingBox(minLat, minLon, maxLat, maxLon, LowerAltitudeMeters, UpperAltitudeMeters);
        }

        var dLat = RadiusMeters / GeoCoordinate.EarthRadiusMeters * (180.0 / Math.PI);
        var dLon = dLat / Math.Cos(Center.Latitude * Math.PI / 180.0);
        return new BoundingBox(
            Center.Latitude - dLat, Center.Longitude - dLon,
            Center.Latitude + dLat, Center.Longitude + dLon,
            LowerAltitudeMeters, UpperAltitudeMeters);
    }
}

/// <summary>
/// 空域规则
/// </summary>
public sealed record AirspaceRules
{
    public double MinSeparationMeters { get; init; } = 100;
    public double MaxSpeedMps { get; init; } = double.MaxValue;
    public double MinAltitudeMeters { get; init; }
    public double MaxAltitudeMeters { get; init; }
    public bool RequiresAuthorization { get; init; } = true;
    public IReadOnlyList<AircraftType> AllowedAircraftTypes { get; init; } = Array.Empty<AircraftType>();
    public IReadOnlyList<FlightPurpose> AllowedPurposes { get; init; } = Array.Empty<FlightPurpose>();
}
