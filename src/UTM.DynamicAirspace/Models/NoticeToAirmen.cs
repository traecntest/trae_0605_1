namespace UTM.DynamicAirspace.Models;

/// <summary>
/// NOTAM 类型
/// </summary>
public enum NotamType
{
    /// <summary>
    /// 新 NOTAM
    /// </summary>
    New = 0,

    /// <summary>
    /// 替换
    /// </summary>
    Replace = 1,

    /// <summary>
    /// 取消
    /// </summary>
    Cancel = 2
}

/// <summary>
/// NOTAM 优先级
/// </summary>
public enum NotamPriority
{
    /// <summary>
    /// 一般
    /// </summary>
    General = 0,

    /// <summary>
    /// 重要
    /// </summary>
    Important = 1,

    /// <summary>
    /// 紧急
    /// </summary>
    Critical = 2
}

/// <summary>
/// 航行通告（NOTAM）记录
/// </summary>
/// <param name="NotamId">NOTAM 唯一标识</param>
/// <param name="NotamType">NOTAM 类型</param>
/// <param name="Priority">优先级</param>
/// <param name="Title">标题</param>
/// <param name="Content">内容</param>
/// <param name="AffectedArea">受影响区域</param>
/// <param name="EffectivePeriod">生效时间段</param>
/// <param name="IssuingAuthority">发布机构</param>
/// <param name="IssuedAt">发布时间</param>
/// <param name="ReplacesNotamId">替换的 NOTAM ID</param>
/// <param name="RelatedRestrictionIds">相关的限制 ID 列表</param>
/// <param name="Classification">分类</param>
/// <param name="LowerLimitFeet">下限高度（英尺）</param>
/// <param name="UpperLimitFeet">上限高度（英尺）</param>
/// <param name="IsCancelled">是否已取消</param>
/// <param name="CancelledAt">取消时间</param>
public record NoticeToAirmen(
    string NotamId,
    NotamType NotamType,
    NotamPriority Priority,
    string Title,
    string Content,
    GeoBoundary AffectedArea,
    TimeWindow EffectivePeriod,
    string IssuingAuthority,
    DateTimeOffset IssuedAt,
    string? ReplacesNotamId = null,
    IReadOnlyList<string>? RelatedRestrictionIds = null,
    string? Classification = null,
    double? LowerLimitFeet = null,
    double? UpperLimitFeet = null,
    bool IsCancelled = false,
    DateTimeOffset? CancelledAt = null
)
{
    /// <summary>
    /// 检查 NOTAM 当前是否有效
    /// </summary>
    public bool IsCurrentlyEffective => !IsCancelled && EffectivePeriod.IsCurrentlyActive;

    /// <summary>
    /// 检查指定时间 NOTAM 是否有效
    /// </summary>
    public bool IsEffectiveAt(DateTimeOffset time) => !IsCancelled && EffectivePeriod.Contains(time);
}
