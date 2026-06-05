namespace UTM.Monitoring.Models;

/// <summary>
/// 雷达航迹数据
/// </summary>
/// <param name="TrackId">航迹唯一标识</param>
/// <param name="Timestamp">数据时间戳</param>
/// <param name="Position">目标位置</param>
/// <param name="Velocity">目标速度</param>
/// <param name="Heading">航向（度）</param>
/// <param name="RadarId">雷达站标识</param>
/// <param name="RadarType">雷达类型</param>
/// <param name="SignalQuality">信号质量（0-1）</param>
/// <param name="CorrelationId">关联的飞行器ID（如已关联）</param>
/// <param name="TrackAge">航迹持续时间（秒）</param>
/// <param name="UpdateCount">更新次数</param>
/// <param name="IsCooperative">是否为协作目标（如应答机响应）</param>
public record RadarTrack(
    string TrackId,
    DateTimeOffset Timestamp,
    GeoPosition Position,
    Velocity3D Velocity,
    double Heading,
    string RadarId,
    RadarType RadarType,
    double SignalQuality,
    string? CorrelationId = null,
    double TrackAge = 0,
    int UpdateCount = 1,
    bool IsCooperative = false)
{
    /// <summary>
    /// 验证航迹数据是否有效
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(TrackId)
            && !string.IsNullOrEmpty(RadarId)
            && Position.IsValid()
            && SignalQuality >= 0 && SignalQuality <= 1
            && Heading >= 0 && Heading < 360;
    }

    /// <summary>
    /// 预测指定时间后的位置
    /// </summary>
    public GeoPosition PredictPosition(TimeSpan timeAhead)
    {
        var seconds = timeAhead.TotalSeconds;
        
        // 计算水平位移
        var trackRad = Heading * Math.PI / 180;
        var distance = Velocity.HorizontalSpeed * seconds;
        
        // 近似计算（短距离）
        var latChange = distance * Math.Cos(trackRad) / GeoPosition.EarthRadius * 180 / Math.PI;
        var lonChange = distance * Math.Sin(trackRad) / 
                        (GeoPosition.EarthRadius * Math.Cos(Position.Latitude * Math.PI / 180)) * 180 / Math.PI;
        var altChange = Velocity.VerticalSpeed * seconds;

        return new GeoPosition(
            Position.Latitude + latChange,
            Position.Longitude + lonChange,
            Position.AltitudeMeters + altChange
        );
    }

    /// <summary>
    /// 转换为无人机状态记录
    /// </summary>
    public AircraftState ToAircraftState(string? aircraftId = null)
    {
        return new AircraftState(
            AircraftId: aircraftId ?? CorrelationId ?? $"radar_{TrackId}",
            FlightId: null,
            Timestamp: Timestamp,
            Position: Position,
            Velocity: Velocity,
            Heading: Heading,
            Altitude: new Altitude(Position.AltitudeMeters),
            BatteryLevel: null,
            SignalStrength: null,
            Source: RadarType.ToSurveillanceSource(),
            Quality: SignalQuality
        );
    }
}

/// <summary>
/// 雷达类型
/// </summary>
public enum RadarType
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 一次监视雷达（PSR）
    /// </summary>
    PrimarySurveillance = 1,

    /// <summary>
    /// 二次监视雷达（SSR）
    /// </summary>
    SecondarySurveillance = 2,

    /// <summary>
    /// Mode-S雷达
    /// </summary>
    ModeS = 3,

    /// <summary>
    /// 无源雷达
    /// </summary>
    Passive = 4,

    /// <summary>
    /// 多基雷达
    /// </summary>
    Multistatic = 5
}

/// <summary>
/// 雷达类型扩展
/// </summary>
public static class RadarTypeExtensions
{
    /// <summary>
    /// 转换为监视源
    /// </summary>
    public static SurveillanceSource ToSurveillanceSource(this RadarType type)
    {
        return type switch
        {
            RadarType.PrimarySurveillance => SurveillanceSource.PrimaryRadar,
            RadarType.SecondarySurveillance => SurveillanceSource.SecondaryRadar,
            RadarType.ModeS => SurveillanceSource.SecondaryRadar,
            _ => SurveillanceSource.Unknown
        };
    }
}

/// <summary>
/// 雷达原始点迹
/// </summary>
/// <param name="PlotId">点迹标识</param>
/// <param name="Timestamp">时间戳</param>
/// <param name="Position">位置</param>
/// <param name="RadarId">雷达站标识</param>
/// <param name="RadarType">雷达类型</param>
/// <param name="Rcs">雷达截面积（平方米）</param>
/// <param name="Snr">信噪比（dB）</param>
public record RadarPlot(
    string PlotId,
    DateTimeOffset Timestamp,
    GeoPosition Position,
    string RadarId,
    RadarType RadarType,
    double? Rcs = null,
    double? Snr = null)
{
    /// <summary>
    /// 验证点迹是否有效
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(PlotId)
            && !string.IsNullOrEmpty(RadarId)
            && Position.IsValid();
    }
}