namespace UTM.Core.ValueObjects;

/// <summary>
/// 地理坐标值对象（WGS84）
/// </summary>
public readonly record struct GeoCoordinate(double Latitude, double Longitude)
{
    /// <summary>地球半径（米）</summary>
    public const double EarthRadiusMeters = 6371000.0;

    /// <summary>
    /// 计算与另一个坐标的Haversine距离（米）
    /// </summary>
    public double DistanceTo(GeoCoordinate other)
    {
        var dLat = DegreesToRadians(other.Latitude - Latitude);
        var dLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(Latitude)) * Math.Cos(DegreesToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    /// <summary>
    /// 计算方位角（度，0-360）
    /// </summary>
    public double BearingTo(GeoCoordinate other)
    {
        var dLon = DegreesToRadians(other.Longitude - Longitude);
        var lat1 = DegreesToRadians(Latitude);
        var lat2 = DegreesToRadians(other.Latitude);

        var y = Math.Sin(dLon) * Math.Cos(lat2);
        var x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

        var bearing = RadiansToDegrees(Math.Atan2(y, x));
        return (bearing + 360) % 360;
    }

    /// <summary>
    /// 根据距离和方位角计算目标坐标
    /// </summary>
    public GeoCoordinate PointAtDistance(double distanceMeters, double bearingDegrees)
    {
        var d = distanceMeters / EarthRadiusMeters;
        var brng = DegreesToRadians(bearingDegrees);
        var lat1 = DegreesToRadians(Latitude);
        var lon1 = DegreesToRadians(Longitude);

        var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d) + Math.Cos(lat1) * Math.Sin(d) * Math.Cos(brng));
        var lon2 = lon1 + Math.Atan2(Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat1), Math.Cos(d) - Math.Sin(lat1) * Math.Sin(lat2));

        return new GeoCoordinate(RadiansToDegrees(lat2), RadiansToDegrees(lon2));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double RadiansToDegrees(double radians) => radians * 180.0 / Math.PI;

    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";
}
