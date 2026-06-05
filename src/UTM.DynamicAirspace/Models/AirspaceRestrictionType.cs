namespace UTM.DynamicAirspace.Models;

/// <summary>
/// 空域限制类型枚举
/// </summary>
public enum AirspaceRestrictionType
{
    /// <summary>
    /// 禁飞区
    /// </summary>
    NoFlyZone = 0,

    /// <summary>
    /// 速度限制
    /// </summary>
    SpeedRestriction = 1,

    /// <summary>
    /// 高度限制
    /// </summary>
    AltitudeRestriction = 2,

    /// <summary>
    /// 气象等待
    /// </summary>
    WeatherHold = 3,

    /// <summary>
    /// 临时禁飞区
    /// </summary>
    TemporaryFlightRestriction = 4,

    /// <summary>
    /// 活动空域
    /// </summary>
    ActiveAirspace = 5,

    /// <summary>
    /// 进近限制
    /// </summary>
    ApproachRestriction = 6,

    /// <summary>
    /// 离场限制
    /// </summary>
    DepartureRestriction = 7
}
