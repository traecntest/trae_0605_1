namespace UTM.Monitoring.Models;

/// <summary>
/// 无人机实时状态记录
/// </summary>
/// <param name="AircraftId">无人机唯一标识</param>
/// <param name="FlightId">飞行任务ID</param>
/// <param name="Timestamp">数据时间戳</param>
/// <param name="Position">地理位置</param>
/// <param name="Velocity">速度信息</param>
/// <param name="Heading">航向角（度，0-360，正北为0）</param>
/// <param name="Altitude">高度信息</param>
/// <param name="BatteryLevel">电池电量百分比（0-100）</param>
/// <param name="SignalStrength">信号强度（dBm）</param>
/// <param name="Source">数据来源</param>
/// <param name="Quality">数据质量指标（0-1）</param>
/// <param name="Status">飞行状态</param>
public record AircraftState(
    string AircraftId,
    string? FlightId,
    DateTimeOffset Timestamp,
    GeoPosition Position,
    Velocity3D Velocity,
    double Heading,
    Altitude Altitude,
    double? BatteryLevel,
    double? SignalStrength,
    SurveillanceSource Source,
    double Quality = 1.0,
    FlightStatus Status = FlightStatus.Normal)
{
    /// <summary>
    /// 验证状态数据是否有效
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(AircraftId)
            && Position.IsValid()
            && Heading >= 0 && Heading < 360
            && BatteryLevel is null or (>= 0 and <= 100)
            && Quality >= 0 && Quality <= 1;
    }

    /// <summary>
    /// 与另一状态计算距离（米）
    /// </summary>
    public double DistanceTo(AircraftState other)
    {
        return Position.DistanceTo(other.Position);
    }

    /// <summary>
    /// 计算与另一状态的时间差
    /// </summary>
    public TimeSpan TimeDelta(AircraftState other)
    {
        return (other.Timestamp - Timestamp).Duration();
    }
}

/// <summary>
/// 地理位置坐标
/// </summary>
/// <param name="Latitude">纬度（度）</param>
/// <param name="Longitude">经度（度）</param>
/// <param name="AltitudeMeters">海拔高度（米）</param>
public record GeoPosition(double Latitude, double Longitude, double AltitudeMeters)
{
    /// <summary>
    /// 地球半径（米）
    /// </summary>
    public const double EarthRadius = 6371000;

    /// <summary>
    /// 验证坐标是否有效
    /// </summary>
    public bool IsValid()
    {
        return Latitude >= -90 && Latitude <= 90
            && Longitude >= -180 && Longitude <= 180;
    }

    /// <summary>
    /// 计算与另一位置的距离（米，Haversine公式）
    /// </summary>
    public double DistanceTo(GeoPosition other)
    {
        var lat1 = Latitude * Math.PI / 180;
        var lat2 = other.Latitude * Math.PI / 180;
        var dLat = (other.Latitude - Latitude) * Math.PI / 180;
        var dLon = (other.Longitude - Longitude) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadius * c;
    }

    /// <summary>
    /// 计算水平距离（忽略高度，米）
    /// </summary>
    public double HorizontalDistanceTo(GeoPosition other)
    {
        var this2D = this with { AltitudeMeters = 0 };
        var other2D = other with { AltitudeMeters = 0 };
        return this2D.DistanceTo(other2D);
    }

    /// <summary>
    /// 计算三维距离（米）
    /// </summary>
    public double DistanceTo3D(GeoPosition other)
    {
        var horizontal = HorizontalDistanceTo(other);
        var vertical = Math.Abs(AltitudeMeters - other.AltitudeMeters);
        return Math.Sqrt(horizontal * horizontal + vertical * vertical);
    }

    /// <summary>
    /// 计算到另一位置的方位角（度）
    /// </summary>
    public double BearingTo(GeoPosition other)
    {
        var lat1 = Latitude * Math.PI / 180;
        var lat2 = other.Latitude * Math.PI / 180;
        var dLon = (other.Longitude - Longitude) * Math.PI / 180;

        var y = Math.Sin(dLon) * Math.Cos(lat2);
        var x = Math.Cos(lat1) * Math.Sin(lat2) -
                Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

        var bearing = Math.Atan2(y, x) * 180 / Math.PI;
        return (bearing + 360) % 360;
    }
}

/// <summary>
/// 三维速度
/// </summary>
/// <param name="HorizontalSpeed">水平速度（米/秒）</param>
/// <param name="VerticalSpeed">垂直速度（米/秒，正值为上升）</param>
/// <param name="TrackAngle">航迹角（度，0-360）</param>
public record Velocity3D(double HorizontalSpeed, double VerticalSpeed, double TrackAngle)
{
    /// <summary>
    /// 计算三维速度大小（米/秒）
    /// </summary>
    public double Speed3D => Math.Sqrt(HorizontalSpeed * HorizontalSpeed + VerticalSpeed * VerticalSpeed);

    /// <summary>
    /// 零速度
    /// </summary>
    public static Velocity3D Zero => new(0, 0, 0);
}

/// <summary>
/// 高度信息
/// </summary>
/// <param name="AltitudeMsl">海拔高度（米）</param>
/// <param name="AltitudeAgl">离地高度（米）</param>
/// <param name="BarometricAltitude">气压高度（米）</param>
public record Altitude(double AltitudeMsl, double? AltitudeAgl = null, double? BarometricAltitude = null)
{
    /// <summary>
    /// 获取最可靠的高度值
    /// </summary>
    public double BestAltitude => AltitudeAgl ?? BarometricAltitude ?? AltitudeMsl;
}

/// <summary>
/// 飞行状态
/// </summary>
public enum FlightStatus
{
    /// <summary>
    /// 未知状态
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 正常飞行
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 起飞中
    /// </summary>
    TakingOff = 2,

    /// <summary>
    /// 降落中
    /// </summary>
    Landing = 3,

    /// <summary>
    /// 悬停
    /// </summary>
    Hovering = 4,

    /// <summary>
    /// 返航中
    /// </summary>
    Returning = 5,

    /// <summary>
    /// 紧急状态
    /// </summary>
    Emergency = 6,

    /// <summary>
    /// 失联
    /// </summary>
    Lost = 7,

    /// <summary>
    /// 已降落
    /// </summary>
    OnGround = 8
}
