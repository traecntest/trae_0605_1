namespace UTM.Monitoring.Models;

/// <summary>
/// 监视数据源类型枚举
/// </summary>
public enum SurveillanceSource
{
    /// <summary>
    /// 未知来源
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// ADS-B (Automatic Dependent Surveillance-Broadcast)
    /// </summary>
    Adsb = 1,

    /// <summary>
    /// 一次雷达
    /// </summary>
    PrimaryRadar = 2,

    /// <summary>
    /// 二次雷达
    /// </summary>
    SecondaryRadar = 3,

    /// <summary>
    /// MLAT (Multilateration)
    /// </summary>
    Mlat = 4,

    /// <summary>
    /// 北斗卫星导航系统
    /// </summary>
    BeiDou = 5,

    /// <summary>
    /// GPS
    /// </summary>
    Gps = 6,

    /// <summary>
    /// GLONASS
    /// </summary>
    Glonass = 7,

    /// <summary>
    /// Galileo
    /// </summary>
    Galileo = 8,

    /// <summary>
    /// 地面基站定位
    /// </summary>
    Cellular = 9,

    /// <summary>
    /// 视觉追踪
    /// </summary>
    Visual = 10,

    /// <summary>
    /// 融合数据（多源融合）
    /// </summary>
    Fused = 100
}

/// <summary>
/// 监视源扩展方法
/// </summary>
public static class SurveillanceSourceExtensions
{
    /// <summary>
    /// 判断是否为卫星导航源
    /// </summary>
    public static bool IsSatelliteBased(this SurveillanceSource source)
    {
        return source is SurveillanceSource.BeiDou 
            or SurveillanceSource.Gps 
            or SurveillanceSource.Glonass 
            or SurveillanceSource.Galileo;
    }

    /// <summary>
    /// 判断是否为雷达源
    /// </summary>
    public static bool IsRadar(this SurveillanceSource source)
    {
        return source is SurveillanceSource.PrimaryRadar 
            or SurveillanceSource.SecondaryRadar;
    }

    /// <summary>
    /// 获取数据源优先级（数值越小优先级越高）
    /// </summary>
    public static int GetPriority(this SurveillanceSource source)
    {
        return source switch
        {
            SurveillanceSource.Fused => 0,
            SurveillanceSource.Adsb => 1,
            SurveillanceSource.BeiDou => 2,
            SurveillanceSource.Gps => 2,
            SurveillanceSource.SecondaryRadar => 3,
            SurveillanceSource.Mlat => 4,
            SurveillanceSource.PrimaryRadar => 5,
            SurveillanceSource.Glonass => 6,
            SurveillanceSource.Galileo => 6,
            SurveillanceSource.Cellular => 7,
            SurveillanceSource.Visual => 8,
            _ => 99
        };
    }
}
