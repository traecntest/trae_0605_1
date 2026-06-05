using UTM.Core.ValueObjects;

namespace UTM.Core.ValueObjects;

/// <summary>
/// 高度值对象
/// </summary>
public readonly record struct Altitude(double Meters)
{
    public double Feet => Meters * 3.28084;
    public static Altitude FromFeet(double feet) => new(feet / 3.28084);
    public static Altitude FromMeters(double meters) => new(meters);
}

/// <summary>
/// 时间区间值对象
/// </summary>
public readonly record struct TimeInterval(DateTimeOffset Start, DateTimeOffset End)
{
    public TimeSpan Duration => End - Start;

    public bool Overlaps(TimeInterval other) =>
        Start < other.End && End > other.Start;

    public bool Contains(DateTimeOffset time) =>
        time >= Start && time <= End;

    public bool Contains(TimeInterval other) =>
        Start <= other.Start && End >= other.End;
}

/// <summary>
/// 边界框值对象
/// </summary>
public readonly record struct BoundingBox(
    double MinLatitude, double MinLongitude,
    double MaxLatitude, double MaxLongitude,
    double MinAltitude = 0, double MaxAltitude = double.MaxValue)
{
    public bool Contains(GeoCoordinate point, double altitude) =>
        point.Latitude >= MinLatitude && point.Latitude <= MaxLatitude &&
        point.Longitude >= MinLongitude && point.Longitude <= MaxLongitude &&
        altitude >= MinAltitude && altitude <= MaxAltitude;

    public bool Contains(GeoCoordinate point) =>
        point.Latitude >= MinLatitude && point.Latitude <= MaxLatitude &&
        point.Longitude >= MinLongitude && point.Longitude <= MaxLongitude;

    public bool Overlaps(BoundingBox other) =>
        MinLatitude <= other.MaxLatitude && MaxLatitude >= other.MinLatitude &&
        MinLongitude <= other.MaxLongitude && MaxLongitude >= other.MinLongitude;

    public GeoCoordinate Center => new(
        (MinLatitude + MaxLatitude) / 2,
        (MinLongitude + MaxLongitude) / 2);
}
