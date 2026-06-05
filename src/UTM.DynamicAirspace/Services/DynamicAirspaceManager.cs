using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using UTM.DynamicAirspace.Events;
using UTM.DynamicAirspace.Models;

namespace UTM.DynamicAirspace.Services;

/// <summary>
/// 动态空域管理核心实现
/// </summary>
public class DynamicAirspaceManager : IDynamicAirspaceManager
{
    private readonly ConcurrentDictionary<string, DynamicAirspaceRegion> _regions = new();
    private readonly ConcurrentDictionary<string, TemporaryFlightRestriction> _tfrs = new();
    private readonly ConcurrentDictionary<string, NoticeToAirmen> _notams = new();
    private readonly ILogger<DynamicAirspaceManager> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// 限制激活事件
    /// </summary>
    public event Func<AirspaceRestrictionActivated, Task>? RestrictionActivated;

    /// <summary>
    /// 限制解除事件
    /// </summary>
    public event Func<AirspaceRestrictionDeactivated, Task>? RestrictionDeactivated;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public DynamicAirspaceManager(ILogger<DynamicAirspaceManager> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> AddRegionAsync(DynamicAirspaceRegion region, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(region);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var added = _regions.TryAdd(region.RegionId, region);
            if (added)
            {
                _logger.LogInformation("Added dynamic airspace region: {RegionId} - {Name}", region.RegionId, region.Name);

                if (region.IsCurrentlyActive)
                {
                    await RaiseRestrictionActivatedAsync(region, "Region added and immediately active");
                }
            }
            else
            {
                _logger.LogWarning("Failed to add region {RegionId}: already exists", region.RegionId);
            }

            return added;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateRegionAsync(DynamicAirspaceRegion region, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(region);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_regions.ContainsKey(region.RegionId))
            {
                _logger.LogWarning("Failed to update region {RegionId}: not found", region.RegionId);
                return false;
            }

            var wasActive = _regions[region.RegionId].IsCurrentlyActive;
            _regions[region.RegionId] = region;

            _logger.LogInformation("Updated dynamic airspace region: {RegionId}", region.RegionId);

            var isActive = region.IsCurrentlyActive;
            if (!wasActive && isActive)
            {
                await RaiseRestrictionActivatedAsync(region, "Region activated");
            }
            else if (wasActive && !isActive)
            {
                await RaiseRestrictionDeactivatedAsync(region.RegionId, region.Type, "Region deactivated");
            }

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveRegionAsync(string regionId, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_regions.TryRemove(regionId, out var region))
            {
                _logger.LogWarning("Failed to remove region {RegionId}: not found", regionId);
                return false;
            }

            _logger.LogInformation("Removed dynamic airspace region: {RegionId}, reason: {Reason}", regionId, reason);

            if (region.IsCurrentlyActive)
            {
                await RaiseRestrictionDeactivatedAsync(regionId, region.Type, reason);
            }

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<DynamicAirspaceRegion>> GetActiveRegionsAsync(CancellationToken cancellationToken = default)
    {
        var active = _regions.Values
            .Where(r => r.IsCurrentlyActive)
            .OrderByDescending(r => r.Priority)
            .ToList();

        return Task.FromResult<IReadOnlyList<DynamicAirspaceRegion>>(active);
    }

    /// <inheritdoc/>
    public Task<DynamicAirspaceRegion?> GetRegionAsync(string regionId, CancellationToken cancellationToken = default)
    {
        _regions.TryGetValue(regionId, out var region);
        return Task.FromResult(region);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<DynamicAirspaceRegion>> GetIntersectingRegionsAsync(GeoCoordinate position, CancellationToken cancellationToken = default)
    {
        var intersecting = _regions.Values
            .Where(r => r.IsCurrentlyActive && IsPointInBoundary(position, r.Boundary))
            .OrderByDescending(r => r.Priority)
            .ToList();

        return Task.FromResult<IReadOnlyList<DynamicAirspaceRegion>>(intersecting);
    }

    /// <inheritdoc/>
    public async Task<bool> AddTfrAsync(TemporaryFlightRestriction tfr, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tfr);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var added = _tfrs.TryAdd(tfr.TfrId, tfr);
            if (added)
            {
                _logger.LogInformation("Added TFR: {TfrId} - {Title}", tfr.TfrId, tfr.Title);

                if (tfr.IsCurrentlyEffective)
                {
                    var region = new DynamicAirspaceRegion(
                        tfr.TfrId,
                        tfr.Title,
                        tfr.Restrictions.RestrictionType,
                        tfr.AffectedArea,
                        tfr.EffectivePeriod,
                        tfr.Restrictions,
                        10,
                        tfr.Description
                    );
                    await RaiseRestrictionActivatedAsync(region, $"TFR activated: {tfr.TfrType}");
                }
            }

            return added;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveTfrAsync(string tfrId, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tfrId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_tfrs.TryRemove(tfrId, out var tfr))
            {
                _logger.LogWarning("Failed to remove TFR {TfrId}: not found", tfrId);
                return false;
            }

            _logger.LogInformation("Removed TFR: {TfrId}, reason: {Reason}", tfrId, reason);

            if (tfr.IsCurrentlyEffective)
            {
                await RaiseRestrictionDeactivatedAsync(tfrId, tfr.Restrictions.RestrictionType, reason);
            }

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TemporaryFlightRestriction>> GetActiveTfrsAsync(CancellationToken cancellationToken = default)
    {
        var active = _tfrs.Values
            .Where(t => t.IsCurrentlyEffective)
            .ToList();

        return Task.FromResult<IReadOnlyList<TemporaryFlightRestriction>>(active);
    }

    /// <inheritdoc/>
    public Task<bool> PublishNotamAsync(NoticeToAirmen notam, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notam);

        var added = _notams.TryAdd(notam.NotamId, notam);
        if (added)
        {
            _logger.LogInformation("Published NOTAM: {NotamId} - {Title}", notam.NotamId, notam.Title);
        }

        return Task.FromResult(added);
    }

    /// <inheritdoc/>
    public Task<bool> CancelNotamAsync(string notamId, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notamId);

        if (!_notams.TryGetValue(notamId, out var notam))
        {
            _logger.LogWarning("Failed to cancel NOTAM {NotamId}: not found", notamId);
            return Task.FromResult(false);
        }

        var cancelled = notam with
        {
            IsCancelled = true,
            CancelledAt = DateTimeOffset.UtcNow
        };
        _notams[notamId] = cancelled;

        _logger.LogInformation("Cancelled NOTAM: {NotamId}, reason: {Reason}", notamId, reason);
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<NoticeToAirmen>> GetActiveNotamsAsync(CancellationToken cancellationToken = default)
    {
        var active = _notams.Values
            .Where(n => n.IsCurrentlyEffective)
            .ToList();

        return Task.FromResult<IReadOnlyList<NoticeToAirmen>>(active);
    }

    /// <inheritdoc/>
    public Task<bool> IsRestrictedAsync(GeoCoordinate position, double altitude, CancellationToken cancellationToken = default)
    {
        var isRestricted = _regions.Values
            .Where(r => r.IsCurrentlyActive)
            .Any(r => IsPointInBoundary(position, r.Boundary) &&
                      altitude >= r.Boundary.MinAltitude &&
                      altitude <= r.Boundary.MaxAltitude);

        return Task.FromResult(isRestricted);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<RestrictionConditions>> GetRestrictionsAtAsync(GeoCoordinate position, CancellationToken cancellationToken = default)
    {
        var restrictions = _regions.Values
            .Where(r => r.IsCurrentlyActive && IsPointInBoundary(position, r.Boundary))
            .Select(r => r.Restrictions)
            .ToList();

        return Task.FromResult<IReadOnlyList<RestrictionConditions>>(restrictions);
    }

    /// <summary>
    /// 检查点是否在边界内
    /// </summary>
    private static bool IsPointInBoundary(GeoCoordinate point, GeoBoundary boundary)
    {
        // 如果有顶点，使用多边形检测
        if (boundary.Vertices is { Count: > 0 })
        {
            return IsPointInPolygon(point, boundary.Vertices);
        }

        // 否则使用圆形边界
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
        const double EarthRadius = 6371000; // 地球半径（米）

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

    /// <summary>
    /// 触发限制激活事件
    /// </summary>
    private async Task RaiseRestrictionActivatedAsync(DynamicAirspaceRegion region, string reason)
    {
        if (RestrictionActivated is null) return;

        var evt = new AirspaceRestrictionActivated(
            region.RegionId,
            region.Type,
            region.Boundary,
            DateTimeOffset.UtcNow,
            reason,
            region.TimeWindow.EndTime - region.TimeWindow.StartTime,
            region.Priority,
            region.Metadata
        );

        try
        {
            await RestrictionActivated(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising RestrictionActivated event for {RegionId}", region.RegionId);
        }
    }

    /// <summary>
    /// 触发限制解除事件
    /// </summary>
    private async Task RaiseRestrictionDeactivatedAsync(string restrictionId, AirspaceRestrictionType type, string reason)
    {
        if (RestrictionDeactivated is null) return;

        var evt = new AirspaceRestrictionDeactivated(
            restrictionId,
            type,
            DateTimeOffset.UtcNow,
            reason
        );

        try
        {
            await RestrictionDeactivated(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising RestrictionDeactivated event for {RestrictionId}", restrictionId);
        }
    }
}
