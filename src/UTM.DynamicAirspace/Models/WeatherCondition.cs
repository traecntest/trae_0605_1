namespace UTM.DynamicAirspace.Models;

/// <summary>
/// 降水类型
/// </summary>
public enum PrecipitationType
{
    /// <summary>
    /// 无降水
    /// </summary>
    None = 0,

    /// <summary>
    /// 雨
    /// </summary>
    Rain = 1,

    /// <summary>
    /// 雪
    /// </summary>
    Snow = 2,

    /// <summary>
    /// 雨夹雪
    /// </summary>
    Sleet = 3,

    /// <summary>
    /// 冰雹
    /// </summary>
    Hail = 4,

    /// <summary>
    /// 毛毛雨
    /// </summary>
    Drizzle = 5,

    /// <summary>
    /// 冻雨
    /// </summary>
    FreezingRain = 6
}

/// <summary>
/// 天气现象
/// </summary>
public enum WeatherPhenomenon
{
    /// <summary>
    /// 正常
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 雾
    /// </summary>
    Fog = 1,

    /// <summary>
    /// 雷暴
    /// </summary>
    Thunderstorm = 2,

    /// <summary>
    /// 沙尘暴
    /// </summary>
    Sandstorm = 3,

    /// <summary>
    /// 龙卷风
    /// </summary>
    Tornado = 4,

    /// <summary>
    /// 强风
    /// </summary>
    StrongWind = 5,

    /// <summary>
    /// 风切变
    /// </summary>
    WindShear = 6,

    /// <summary>
    /// 结冰
    /// </summary>
    Icing = 7,

    /// <summary>
    /// 湍流
    /// </summary>
    Turbulence = 8
}

/// <summary>
/// 气象条件记录
/// </summary>
/// <param name="StationId">气象站标识</param>
/// <param name="Location">位置</param>
/// <param name="ObservedAt">观测时间</param>
/// <param name="WindSpeedKnots">风速（节）</param>
/// <param name="WindDirectionDegrees">风向（度，0-360）</param>
/// <param name="VisibilityMeters">能见度（米）</param>
/// <param name="Precipitation">降水类型</param>
/// <param name="PrecipitationIntensity">降水强度（毫米/小时）</param>
/// <param name="TemperatureCelsius">温度（摄氏度）</param>
/// <param name="DewPointCelsius">露点温度（摄氏度）</param>
/// <param name="PressureHpa">气压（百帕）</param>
/// <param name="CloudBaseFeet">云底高度（英尺）</param>
/// <param name="CloudCoverage">云量（%）</param>
/// <param name="Phenomenon">天气现象</param>
/// <param name="HumidityPercent">湿度（%）</param>
/// <param name="RawMetar">原始 METAR 报文</param>
public record WeatherCondition(
    string StationId,
    GeoCoordinate Location,
    DateTimeOffset ObservedAt,
    double WindSpeedKnots,
    double WindDirectionDegrees,
    double VisibilityMeters,
    PrecipitationType Precipitation = PrecipitationType.None,
    double PrecipitationIntensity = 0,
    double? TemperatureCelsius = null,
    double? DewPointCelsius = null,
    double? PressureHpa = null,
    double? CloudBaseFeet = null,
    int? CloudCoverage = null,
    WeatherPhenomenon Phenomenon = WeatherPhenomenon.Normal,
    int? HumidityPercent = null,
    string? RawMetar = null
)
{
    /// <summary>
    /// 检查是否为恶劣天气
    /// </summary>
    public bool IsSevereWeather =>
        WindSpeedKnots > 30 ||
        VisibilityMeters < 1000 ||
        Precipitation == PrecipitationType.Hail ||
        Precipitation == PrecipitationType.FreezingRain ||
        Phenomenon is WeatherPhenomenon.Thunderstorm
            or WeatherPhenomenon.Tornado
            or WeatherPhenomenon.Sandstorm
            or WeatherPhenomenon.WindShear;

    /// <summary>
    /// 检查是否适合飞行
    /// </summary>
    public bool IsFlightPermitted =>
        WindSpeedKnots <= 40 &&
        VisibilityMeters >= 800 &&
        Phenomenon != WeatherPhenomenon.Tornado;
}
