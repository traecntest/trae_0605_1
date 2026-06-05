using Microsoft.Extensions.Logging;
using UTM.DynamicAirspace.Models;

namespace UTM.DynamicAirspace.Services;

/// <summary>
/// 气象数据集成服务接口
/// </summary>
public interface IWeatherIntegrationService
{
    /// <summary>
    /// 更新气象条件
    /// </summary>
    /// <param name="condition">气象条件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    Task UpdateWeatherConditionAsync(WeatherCondition condition, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定位置的气象条件
    /// </summary>
    /// <param name="location">位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>气象条件</returns>
    Task<WeatherCondition?> GetWeatherConditionAsync(GeoCoordinate location, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据气象条件生成空域限制建议
    /// </summary>
    /// <param name="condition">气象条件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>限制建议列表</returns>
    Task<IReadOnlyList<RestrictionConditions>> GenerateRestrictionSuggestionsAsync(WeatherCondition condition, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解析 METAR 报文
    /// </summary>
    /// <param name="metar">METAR 报文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>气象条件</returns>
    Task<WeatherCondition?> ParseMetarAsync(string metar, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查气象条件是否适合飞行
    /// </summary>
    /// <param name="condition">气象条件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否适合飞行</returns>
    Task<bool> IsFlightPermittedAsync(WeatherCondition condition, CancellationToken cancellationToken = default);
}

/// <summary>
/// 气象数据集成服务实现
/// </summary>
public class WeatherIntegrationService : IWeatherIntegrationService
{
    private readonly Dictionary<string, WeatherCondition> _weatherConditions = new();
    private readonly ILogger<WeatherIntegrationService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    // 气象限制阈值
    private const double MaxWindSpeedKnots = 35;
    private const double MinVisibilityMeters = 1000;
    private const double MaxCrosswindKnots = 25;
    private const double MinCloudBaseFeet = 500;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public WeatherIntegrationService(ILogger<WeatherIntegrationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task UpdateWeatherConditionAsync(WeatherCondition condition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(condition);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            _weatherConditions[condition.StationId] = condition;
            _logger.LogInformation("Updated weather condition for station {StationId}: Wind {WindSpeed}kts, Visibility {Visibility}m",
                condition.StationId, condition.WindSpeedKnots, condition.VisibilityMeters);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<WeatherCondition?> GetWeatherConditionAsync(GeoCoordinate location, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // 找到最近的气象站
            WeatherCondition? nearest = null;
            var minDistance = double.MaxValue;

            foreach (var condition in _weatherConditions.Values)
            {
                var distance = CalculateDistance(location, condition.Location);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = condition;
                }
            }

            return nearest;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<RestrictionConditions>> GenerateRestrictionSuggestionsAsync(WeatherCondition condition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var suggestions = new List<RestrictionConditions>();

        // 强风限制
        if (condition.WindSpeedKnots > MaxWindSpeedKnots)
        {
            suggestions.Add(new RestrictionConditions(
                AirspaceRestrictionType.WeatherHold,
                MaxSpeedKnots: MaxWindSpeedKnots,
                AdditionalConditions: new Dictionary<string, string>
                {
                    ["Reason"] = "High wind speed",
                    ["WindSpeed"] = condition.WindSpeedKnots.ToString()
                }
            ));

            _logger.LogWarning("High wind detected: {WindSpeed} kts at {StationId}",
                condition.WindSpeedKnots, condition.StationId);
        }

        // 低能见度限制
        if (condition.VisibilityMeters < MinVisibilityMeters)
        {
            suggestions.Add(new RestrictionConditions(
                AirspaceRestrictionType.WeatherHold,
                AdditionalConditions: new Dictionary<string, string>
                {
                    ["Reason"] = "Low visibility",
                    ["Visibility"] = condition.VisibilityMeters.ToString()
                }
            ));

            _logger.LogWarning("Low visibility detected: {Visibility}m at {StationId}",
                condition.VisibilityMeters, condition.StationId);
        }

        // 低云底高限制
        if (condition.CloudBaseFeet.HasValue && condition.CloudBaseFeet < MinCloudBaseFeet)
        {
            suggestions.Add(new RestrictionConditions(
                AirspaceRestrictionType.AltitudeRestriction,
                MinAltitudeFeet: condition.CloudBaseFeet,
                AdditionalConditions: new Dictionary<string, string>
                {
                    ["Reason"] = "Low cloud base",
                    ["CloudBase"] = condition.CloudBaseFeet.Value.ToString()
                }
            ));

            _logger.LogWarning("Low cloud base detected: {CloudBase}ft at {StationId}",
                condition.CloudBaseFeet, condition.StationId);
        }

        // 恶劣天气现象
        if (condition.IsSevereWeather)
        {
            suggestions.Add(new RestrictionConditions(
                AirspaceRestrictionType.NoFlyZone,
                AdditionalConditions: new Dictionary<string, string>
                {
                    ["Reason"] = "Severe weather",
                    ["Phenomenon"] = condition.Phenomenon.ToString()
                }
            ));

            _logger.LogWarning("Severe weather detected: {Phenomenon} at {StationId}",
                condition.Phenomenon, condition.StationId);
        }

        // 降水限制
        if (condition.Precipitation is PrecipitationType.Hail or PrecipitationType.FreezingRain)
        {
            suggestions.Add(new RestrictionConditions(
                AirspaceRestrictionType.NoFlyZone,
                AdditionalConditions: new Dictionary<string, string>
                {
                    ["Reason"] = "Hazardous precipitation",
                    ["PrecipitationType"] = condition.Precipitation.ToString()
                }
            ));
        }

        return Task.FromResult<IReadOnlyList<RestrictionConditions>>(suggestions);
    }

    /// <inheritdoc/>
    public Task<WeatherCondition?> ParseMetarAsync(string metar, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metar);

        try
        {
            // 简化的 METAR 解析示例
            // 实际应用中需要更完整的解析器
            var parts = metar.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                _logger.LogWarning("Invalid METAR format: {Metar}", metar);
                return Task.FromResult<WeatherCondition?>(null);
            }

            var stationId = parts[0];
            var windPart = parts.Length > 2 ? parts[2] : "00000KT";

            // 解析风向风速 (简化)
            var windSpeed = 0.0;
            var windDirection = 0.0;

            if (windPart.EndsWith("KT") && windPart.Length >= 7)
            {
                var windStr = windPart[..^2];
                if (double.TryParse(windStr[..3], out var dir))
                {
                    windDirection = dir;
                }
                if (double.TryParse(windStr[3..], out var spd))
                {
                    windSpeed = spd;
                }
            }

            // 解析能见度 (简化)
            var visibility = 9999.0;
            if (parts.Length > 3)
            {
                var visPart = parts[3];
                if (int.TryParse(visPart, out var vis))
                {
                    visibility = vis * 100; // 转换为米
                }
            }

            var condition = new WeatherCondition(
                stationId,
                new GeoCoordinate(0, 0), // 需要从数据库获取
                DateTimeOffset.UtcNow,
                windSpeed,
                windDirection,
                visibility,
                RawMetar: metar
            );

            _logger.LogInformation("Parsed METAR for {StationId}: Wind {WindSpeed}kts/{WindDirection}°, Visibility {Visibility}m",
                stationId, windSpeed, windDirection, visibility);

            return Task.FromResult<WeatherCondition?>(condition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse METAR: {Metar}", metar);
            return Task.FromResult<WeatherCondition?>(null);
        }
    }

    /// <inheritdoc/>
    public Task<bool> IsFlightPermittedAsync(WeatherCondition condition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var permitted = condition.IsFlightPermitted;

        if (!permitted)
        {
            _logger.LogWarning("Flight not permitted at {StationId}: Wind {WindSpeed}kts, Visibility {Visibility}m, Phenomenon {Phenomenon}",
                condition.StationId, condition.WindSpeedKnots, condition.VisibilityMeters, condition.Phenomenon);
        }

        return Task.FromResult(permitted);
    }

    /// <summary>
    /// 计算两点之间的距离（米）
    /// </summary>
    private static double CalculateDistance(GeoCoordinate a, GeoCoordinate b)
    {
        const double EarthRadius = 6371000;

        var lat1 = a.Latitude * Math.PI / 180;
        var lat2 = b.Latitude * Math.PI / 180;
        var deltaLat = (b.Latitude - a.Latitude) * Math.PI / 180;
        var deltaLon = (b.Longitude - a.Longitude) * Math.PI / 180;

        var sinDLat = Math.Sin(deltaLat / 2);
        var sinDLon = Math.Sin(deltaLon / 2);

        var h = sinDLat * sinDLat + Math.Cos(lat1) * Math.Cos(lat2) * sinDLon * sinDLon;
        var c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));

        return EarthRadius * c;
    }
}
