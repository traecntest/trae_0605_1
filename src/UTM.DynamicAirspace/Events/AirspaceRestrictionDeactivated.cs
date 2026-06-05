using UTM.DynamicAirspace.Models;

namespace UTM.DynamicAirspace.Events;

/// <summary>
/// 空域限制解除事件
/// </summary>
/// <param name="RestrictionId">限制唯一标识</param>
/// <param name="RestrictionType">限制类型</param>
/// <param name="DeactivatedAt">解除时间</param>
/// <param name="Reason">解除原因</param>
/// <param name="ActiveDuration">实际持续时长</param>
/// <param name="Metadata">扩展元数据</param>
public record AirspaceRestrictionDeactivated(
    string RestrictionId,
    AirspaceRestrictionType RestrictionType,
    DateTimeOffset DeactivatedAt,
    string Reason,
    TimeSpan? ActiveDuration = null,
    IReadOnlyDictionary<string, string>? Metadata = null
);
