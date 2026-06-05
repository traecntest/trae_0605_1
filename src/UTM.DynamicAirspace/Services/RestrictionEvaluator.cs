using Microsoft.Extensions.Logging;
using UTM.DynamicAirspace.Models;

namespace UTM.DynamicAirspace.Services;

/// <summary>
/// 限制评估器接口
/// </summary>
public interface IRestrictionEvaluator
{
    /// <summary>
    /// 评估飞行计划是否违反动态空域限制
    /// </summary>
    /// <param name="flightPlan">飞行计划</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>评估结果</returns>
    Task<RestrictionEvaluationResult> EvaluateAsync(FlightPlan flightPlan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 评估单个点是否受限
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="altitude">高度（米）</param>
    /// <param name="time">时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否受限</returns>
    Task<bool> IsPointRestrictedAsync(GeoCoordinate position, double altitude, DateTimeOffset time, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取路径上的所有限制
    /// </summary>
    /// <param name="waypoints">航点列表</param>
    /// <param name="altitudes">对应高度列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>限制列表</returns>
    Task<IReadOnlyList<RestrictionViolation>> GetViolationsAsync(IReadOnlyList<GeoCoordinate> waypoints, IReadOnlyList<double> altitudes, CancellationToken cancellationToken = default);
}

/// <summary>
/// 飞行计划
/// </summary>
/// <param name="FlightId">航班 ID</param>
/// <param name="AircraftType">航空器类型</param>
/// <param name="Departure">起飞点</param>
/// <param name="Destination">目的地</param>
/// <param name="Waypoints">航点列表</param>
/// <param name="Altitudes">高度列表（米）</param>
/// <param name="DepartureTime">起飞时间</param>
/// <param name="EstimatedArrivalTime">预计到达时间</param>
public record FlightPlan(
    string FlightId,
    string AircraftType,
    GeoCoordinate Departure,
    GeoCoordinate Destination,
    IReadOnlyList<GeoCoordinate> Waypoints,
    IReadOnlyList<double> Altitudes,
    DateTimeOffset DepartureTime,
    DateTimeOffset EstimatedArrivalTime
);

/// <summary>
/// 限制评估结果
/// </summary>
/// <param name="IsCompliant">是否合规</param>
/// <param name="Violations">违规列表</param>
/// <param name="Warnings">警告列表</param>
/// <param name="EvaluatedAt">评估时间</param>
public record RestrictionEvaluationResult(
    bool IsCompliant,
    IReadOnlyList<RestrictionViolation> Violations,
    IReadOnlyList<string> Warnings,
    DateTimeOffset EvaluatedAt
);

/// <summary>
/// 限制违规
/// </summary>
/// <param name="RestrictionId">限制 ID</param>
/// <param name="RestrictionType">限制类型</param>
/// <param name="Location">违规位置</param>
/// <param name="Altitude">违规高度</param>
/// <param name="Time">违规时间</param>
/// <param name="Description">描述</param>
/// <param name="Severity">严重程度（1-5）</param>
public record RestrictionViolation(
    string RestrictionId,
    AirspaceRestrictionType RestrictionType,
    GeoCoordinate Location,
    double Altitude,
    DateTimeOffset Time,
    string Description,
    int Severity
);

/// <summary>
/// 限制评估器实现
/// </summary>
public class RestrictionEvaluator : IRestrictionEvaluator
{
    private readonly IDynamicAirspaceManager _airspaceManager;
    private readonly ILogger<RestrictionEvaluator> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="airspaceManager">空域管理器</param>
    /// <param name="logger">日志记录器</param>
    public RestrictionEvaluator(IDynamicAirspaceManager airspaceManager, ILogger<RestrictionEvaluator> logger)
    {
        _airspaceManager = airspaceManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<RestrictionEvaluationResult> EvaluateAsync(FlightPlan flightPlan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(flightPlan);

        var violations = new List<RestrictionViolation>();
        var warnings = new List<string>();

        _logger.LogInformation("Evaluating flight plan {FlightId} from {Departure} to {Destination}",
            flightPlan.FlightId, flightPlan.Departure, flightPlan.Destination);

        // 检查每个航点
        for (var i = 0; i < flightPlan.Waypoints.Count; i++)
        {
            var waypoint = flightPlan.Waypoints[i];
            var altitude = i < flightPlan.Altitudes.Count ? flightPlan.Altitudes[i] : 0;

            // 计算该航点的预计时间
            var progress = (double)i / Math.Max(1, flightPlan.Waypoints.Count - 1);
            var estimatedTime = flightPlan.DepartureTime + TimeSpan.FromTicks(
                (long)((flightPlan.EstimatedArrivalTime - flightPlan.DepartureTime).Ticks * progress));

            // 获取该位置的限制
            var restrictions = await _airspaceManager.GetRestrictionsAtAsync(waypoint, cancellationToken);

            foreach (var restriction in restrictions)
            {
                var violation = CheckRestrictionViolation(
                    restriction,
                    waypoint,
                    altitude,
                    estimatedTime,
                    flightPlan.AircraftType
                );

                if (violation is not null)
                {
                    violations.Add(violation);
                }
            }

            // 检查 TFR
            var activeTfrs = await _airspaceManager.GetActiveTfrsAsync(cancellationToken);
            foreach (var tfr in activeTfrs)
            {
                if (IsPointInBoundary(waypoint, tfr.AffectedArea) &&
                    altitude >= tfr.AffectedArea.MinAltitude &&
                    altitude <= tfr.AffectedArea.MaxAltitude)
                {
                    violations.Add(new RestrictionViolation(
                        tfr.TfrId,
                        tfr.Restrictions.RestrictionType,
                        waypoint,
                        altitude,
                        estimatedTime,
                        $"TFR violation: {tfr.Title}",
                        5
                    ));
                }
            }
        }

        // 生成警告
        if (violations.Count > 0)
        {
            warnings.Add($"Flight plan has {violations.Count} restriction violation(s)");
        }

        var result = new RestrictionEvaluationResult(
            violations.Count == 0,
            violations,
            warnings,
            DateTimeOffset.UtcNow
        );

        _logger.LogInformation("Flight plan {FlightId} evaluation: {IsCompliant}, {ViolationCount} violations",
            flightPlan.FlightId, result.IsCompliant, violations.Count);

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> IsPointRestrictedAsync(GeoCoordinate position, double altitude, DateTimeOffset time, CancellationToken cancellationToken = default)
    {
        var restrictions = await _airspaceManager.GetRestrictionsAtAsync(position, cancellationToken);

        foreach (var restriction in restrictions)
        {
            if (CheckRestrictionViolation(restriction, position, altitude, time, null) is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RestrictionViolation>> GetViolationsAsync(
        IReadOnlyList<GeoCoordinate> waypoints,
        IReadOnlyList<double> altitudes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(waypoints);
        ArgumentNullException.ThrowIfNull(altitudes);

        var violations = new List<RestrictionViolation>();
        var now = DateTimeOffset.UtcNow;

        for (var i = 0; i < waypoints.Count; i++)
        {
            var waypoint = waypoints[i];
            var altitude = i < altitudes.Count ? altitudes[i] : 0;

            var restrictions = await _airspaceManager.GetRestrictionsAtAsync(waypoint, cancellationToken);

            foreach (var restriction in restrictions)
            {
                var violation = CheckRestrictionViolation(restriction, waypoint, altitude, now, null);
                if (violation is not null)
                {
                    violations.Add(violation);
                }
            }
        }

        return violations;
    }

    /// <summary>
    /// 检查是否违反限制
    /// </summary>
    private static RestrictionViolation? CheckRestrictionViolation(
        RestrictionConditions restriction,
        GeoCoordinate position,
        double altitude,
        DateTimeOffset time,
        string? aircraftType)
    {
        // 禁飞区
        if (restriction.RestrictionType == AirspaceRestrictionType.NoFlyZone)
        {
            return new RestrictionViolation(
                "NoFlyZone",
                AirspaceRestrictionType.NoFlyZone,
                position,
                altitude,
                time,
                "Entered no-fly zone",
                5
            );
        }

        // 高度限制
        if (restriction.RestrictionType == AirspaceRestrictionType.AltitudeRestriction)
        {
            if (restriction.MinAltitudeFeet.HasValue && altitude * 3.28084 < restriction.MinAltitudeFeet)
            {
                return new RestrictionViolation(
                    "AltitudeRestriction",
                    AirspaceRestrictionType.AltitudeRestriction,
                    position,
                    altitude,
                    time,
                    $"Below minimum altitude: {restriction.MinAltitudeFeet}ft",
                    3
                );
            }

            if (restriction.MaxAltitudeFeet.HasValue && altitude * 3.28084 > restriction.MaxAltitudeFeet)
            {
                return new RestrictionViolation(
                    "AltitudeRestriction",
                    AirspaceRestrictionType.AltitudeRestriction,
                    position,
                    altitude,
                    time,
                    $"Above maximum altitude: {restriction.MaxAltitudeFeet}ft",
                    3
                );
            }
        }

        // 速度限制（需要额外信息）
        if (restriction.RestrictionType == AirspaceRestrictionType.SpeedRestriction)
        {
            // 实际应用中需要检查飞行计划中的速度
            // 这里简化处理
        }

        return null;
    }

    /// <summary>
    /// 检查点是否在边界内
    /// </summary>
    private static bool IsPointInBoundary(GeoCoordinate point, GeoBoundary boundary)
    {
        if (boundary.Vertices is { Count: > 0 })
        {
            return IsPointInPolygon(point, boundary.Vertices);
        }

        var distance = CalculateDistance(point, boundary.Center);
        return distance <= boundary.RadiusMeters;
    }

    /// <summary>
    /// 射线法判断点是否在多边形内
    /// </summary>
    private static bool IsPointInPolygon(GeoCoordinate point, IReadOnlyList<GeoCoordinate> polygon)
    {
        var inside = false;
        var j = polygon.Count - 1;

        for (var i = 0; i < polygon.Count; i++)
        {
            if ((polygon[i].Latitude < point.Latitude && polygon[j].Latitude >= point.Latitude) ||
                (polygon[j].Latitude < point.Latitude && polygon[i].Latitude >= point.Latitude))
            {
                if (polygon[i].Longitude + (point.Latitude - polygon[i].Latitude) /
                    (polygon[j].Latitude - polygon[i].Latitude) * (polygon[j].Longitude - polygon[i].Longitude) < point.Longitude)
                {
                    inside = !inside;
                }
            }
            j = i;
        }

        return inside;
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
