namespace UTM.DynamicAirspace.Models;

/// <summary>
/// 地理坐标
/// </summary>
/// <param name="Latitude">纬度</param>
/// <param name="Longitude">经度</param>
/// <param name="Altitude">高度（米）</param>
public record struct GeoCoordinate(double Latitude, double Longitude, double Altitude = 0);

/// <summary>
/// 几何边界
/// </summary>
/// <param name="Center">中心点</param>
/// <param name="RadiusMeters">半径（米）</param>
/// <param name="MinAltitude">最低高度（米）</param>
/// <param name="MaxAltitude">最高高度（米）</param>
/// <param name="Vertices">多边形顶点（可选）</param>
public record GeoBoundary(
    GeoCoordinate Center,
    double RadiusMeters,
    double MinAltitude,
    double MaxAltitude,
    IReadOnlyList<GeoCoordinate>? Vertices = null
);

/// <summary>
/// 限制条件
/// </summary>
/// <param name="RestrictionType">限制类型</param>
/// <param name="MaxSpeedKnots">最大速度（节）</param>
/// <param name="MinAltitudeFeet">最低高度（英尺）</param>
/// <param name="MaxAltitudeFeet">最高高度（英尺）</param>
/// <param name="AdditionalConditions">附加条件</param>
public record RestrictionConditions(
    AirspaceRestrictionType RestrictionType,
    double? MaxSpeedKnots = null,
    double? MinAltitudeFeet = null,
    double? MaxAltitudeFeet = null,
    IReadOnlyDictionary<string, string>? AdditionalConditions = null
);

/// <summary>
/// 时间窗口
/// </summary>
/// <param name="StartTime">开始时间</param>
/// <param name="EndTime">结束时间</param>
/// <param name="IsActive">当前是否激活</param>
public record TimeWindow(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    bool IsActive = true
)
{
    /// <summary>
    /// 检查指定时间是否在窗口内
    /// </summary>
    public bool Contains(DateTimeOffset time) => time >= StartTime && time <= EndTime;

    /// <summary>
    /// 检查当前时间是否在窗口内
    /// </summary>
    public bool IsCurrentlyActive => IsActive && Contains(DateTimeOffset.UtcNow);
}

/// <summary>
/// 动态空域区域记录
/// </summary>
/// <param name="RegionId">区域唯一标识</param>
/// <param name="Name">区域名称</param>
/// <param name="Type">区域类型</param>
/// <param name="Boundary">几何边界</param>
/// <param name="TimeWindow">时间窗口</param>
/// <param name="Restrictions">限制条件</param>
/// <param name="Priority">优先级（1-10，10最高）</param>
/// <param name="Description">描述信息</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="UpdatedAt">更新时间</param>
/// <param name="CreatedBy">创建者</param>
/// <param name="Metadata">扩展元数据</param>
public record DynamicAirspaceRegion(
    string RegionId,
    string Name,
    AirspaceRestrictionType Type,
    GeoBoundary Boundary,
    TimeWindow TimeWindow,
    RestrictionConditions Restrictions,
    int Priority,
    string? Description = null,
    DateTimeOffset? CreatedAt = null,
    DateTimeOffset? UpdatedAt = null,
    string? CreatedBy = null,
    IReadOnlyDictionary<string, string>? Metadata = null
)
{
    /// <summary>
    /// 检查区域当前是否激活
    /// </summary>
    public bool IsCurrentlyActive => TimeWindow.IsCurrentlyActive;

    /// <summary>
    /// 检查指定时间区域是否激活
    /// </summary>
    public bool IsActiveAt(DateTimeOffset time) => TimeWindow.Contains(time);
}
