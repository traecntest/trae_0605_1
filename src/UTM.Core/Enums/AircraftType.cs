namespace UTM.Core.Enums;

/// <summary>
/// 航空器类型枚举
/// </summary>
public enum AircraftType
{
    /// <summary>多旋翼</summary>
    Multirotor = 0,
    /// <summary>固定翼</summary>
    FixedWing = 1,
    /// <summary>直升机</summary>
    Helicopter = 2,
    /// <summary>垂直起降固定翼</summary>
    VTOL = 3,
    /// <summary>载人eVTOL</summary>
    eVTOL = 4,
    /// <summary>飞艇</summary>
    Airship = 5,
    /// <summary>其他</summary>
    Other = 99
}
