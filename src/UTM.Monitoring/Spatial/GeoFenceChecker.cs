using UTM.Core.ValueObjects;

namespace UTM.Monitoring.Spatial;

public sealed class GeoFenceChecker
{
    public bool IsInsideFence(GeoCoordinate position, double altitude, IReadOnlyList<GeoCoordinate> fencePolygon, double minAlt, double maxAlt)
    {
        if (altitude < minAlt || altitude > maxAlt) return false;
        return IsPointInPolygon(position, fencePolygon);
    }

    public bool IsInsideCircleFence(GeoCoordinate position, GeoCoordinate center, double radiusMeters)
    {
        return position.DistanceTo(center) <= radiusMeters;
    }

    private static bool IsPointInPolygon(GeoCoordinate point, IReadOnlyList<GeoCoordinate> polygon)
    {
        var n = polygon.Count;
        var inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if ((polygon[i].Latitude > point.Latitude) != (polygon[j].Latitude > point.Latitude) &&
                point.Longitude < (polygon[j].Longitude - polygon[i].Longitude) *
                (point.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude)
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
