using UTM.Core.ValueObjects;
using Xunit;

namespace UTM.Core.Tests;

public class GeoCoordinateTests
{
    [Fact]
    public void DistanceTo_ShouldCalculateCorrectDistance()
    {
        // 北京到上海大约1068公里
        var beijing = new GeoCoordinate(39.9042, 116.4074);
        var shanghai = new GeoCoordinate(31.2304, 121.4737);

        var distance = beijing.DistanceTo(shanghai);

        Assert.InRange(distance, 1000000, 1100000); // 1000-1100公里
    }

    [Fact]
    public void DistanceTo_SamePoint_ShouldReturnZero()
    {
        var point = new GeoCoordinate(39.9042, 116.4074);
        var distance = point.DistanceTo(point);
        Assert.Equal(0, distance, 2);
    }

    [Fact]
    public void BearingTo_ShouldCalculateCorrectBearing()
    {
        var point1 = new GeoCoordinate(39.9042, 116.4074);
        var point2 = new GeoCoordinate(39.9042, 117.4074); // 正东方向

        var bearing = point1.BearingTo(point2);

        Assert.InRange(bearing, 85, 95); // 大约90度（正东）
    }

    [Fact]
    public void PointAtDistance_ShouldCalculateCorrectPosition()
    {
        var start = new GeoCoordinate(39.9042, 116.4074);
        var distance = 1000; // 1公里
        var bearing = 90.0; // 正东

        var result = start.PointAtDistance(distance, bearing);

        // 验证距离大约1公里
        var actualDistance = start.DistanceTo(result);
        Assert.InRange(actualDistance, 990, 1010);
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var point1 = new GeoCoordinate(39.9042, 116.4074);
        var point2 = new GeoCoordinate(39.9042, 116.4074);
        var point3 = new GeoCoordinate(31.2304, 121.4737);

        Assert.Equal(point1, point2);
        Assert.NotEqual(point1, point3);
    }
}
