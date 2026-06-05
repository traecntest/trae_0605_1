namespace UTM.Core.Enums;

/// <summary>
/// 空域类别枚举
/// </summary>
public enum AirspaceClass
{
    /// <summary>A类空域 - 高空管制区</summary>
    ClassA = 0,
    /// <summary>B类空域 - 终端管制区</summary>
    ClassB = 1,
    /// <summary>C类空域 - 进近管制区</summary>
    ClassC = 2,
    /// <summary>D类空域 - 塔台管制区</summary>
    ClassD = 3,
    /// <summary>E类空域 - 通用空域</summary>
    ClassE = 4,
    /// <summary>G类空域 - 非管制空域</summary>
    ClassG = 5,
    /// <summary>禁飞区</summary>
    Restricted = 6,
    /// <summary>危险区</summary>
    Danger = 7,
    /// <summary>限制区</summary>
    Prohibited = 8
}
