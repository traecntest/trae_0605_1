namespace UTM.DynamicAirspace.Models;

/// <summary>
/// 临时飞行限制（TFR）类型
/// </summary>
public enum TemporaryFlightRestrictionType
{
    /// <summary>
    /// 紧急事件
    /// </summary>
    Emergency = 0,

    /// <summary>
    ///  VIP 飞行
    /// </summary>
    VIPMovement = 1,

    /// <summary>
    /// 体育赛事
    /// </summary>
    SportingEvent = 2,

    /// <summary>
    /// 火灾
    /// </summary>
    Fire = 3,

    /// <summary>
    /// 军事活动
    /// </summary>
    MilitaryOperation = 4,

    /// <summary>
    /// 航天活动
    /// </summary>
    SpaceOperation = 5,

    /// <summary>
    /// 特殊安全原因
    /// </summary>
    SpecialSecurityReason = 6,

    /// <summary>
    /// 其他
    /// </summary>
    Other = 99
}

/// <summary>
/// 临时飞行限制（TFR）记录
/// </summary>
/// <param name="TfrId">TFR 唯一标识</param>
/// <param name="TfrType">TFR 类型</param>
/// <param name="Title">标题</param>
/// <param name="Description">详细描述</param>
/// <param name="AffectedArea">受影响区域</param>
/// <param name="EffectivePeriod">生效时间段</param>
/// <param name="Restrictions">限制条件</param>
/// <param name="IssuingAuthority">发布机构</param>
/// <param name="IssuedAt">发布时间</param>
/// <param name="ExpiresAt">过期时间</param>
/// <param name="IsPermanent">是否为永久限制</param>
/// <param name="AffectedAircraftTypes">受影响的航空器类型</param>
/// <param name="Exemptions">豁免条件</param>
/// <param name="ContactInfo">联系信息</param>
/// <param name="RelatedNotams">相关 NOTAM 编号</param>
public record TemporaryFlightRestriction(
    string TfrId,
    TemporaryFlightRestrictionType TfrType,
    string Title,
    string Description,
    GeoBoundary AffectedArea,
    TimeWindow EffectivePeriod,
    RestrictionConditions Restrictions,
    string IssuingAuthority,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    bool IsPermanent = false,
    IReadOnlyList<string>? AffectedAircraftTypes = null,
    IReadOnlyList<string>? Exemptions = null,
    string? ContactInfo = null,
    IReadOnlyList<string>? RelatedNotams = null
)
{
    /// <summary>
    /// 检查 TFR 当前是否有效
    /// </summary>
    public bool IsCurrentlyEffective => !IsPermanent && EffectivePeriod.IsCurrentlyActive;

    /// <summary>
    /// 检查指定时间 TFR 是否有效
    /// </summary>
    public bool IsEffectiveAt(DateTimeOffset time) => IsPermanent || EffectivePeriod.Contains(time);
}
