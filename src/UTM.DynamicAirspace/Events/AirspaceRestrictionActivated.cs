using UTM.DynamicAirspace.Models;

namespace UTM.DynamicAirspace.Events;

/// <summary>
/// 空域限制激活事件
/// </summary>
/// <param name="RestrictionId">限制唯一标识</param>
/// <param name="RestrictionType">限制类型</param>
/// <param name="AffectedRegion">受影响区域</param>
/// <param name="ActivatedAt">激活时间</param>
/// <param name="Reason">激活原因</param>
/// <param name="ExpectedDuration">预期持续时间</param>
/// <param name="Severity">严重程度（1-5）</param>
/// <param name="Metadata">扩展元数据</param>
public record AirspaceRestrictionActivated(
    string RestrictionId,
    AirspaceRestrictionType RestrictionType,
    GeoBoundary AffectedRegion,
    DateTimeOffset ActivatedAt,
    string Reason,
    TimeSpan? ExpectedDuration = null,
    int Severity = 1,
    IReadOnlyDictionary<string, string>? Metadata = null
);
