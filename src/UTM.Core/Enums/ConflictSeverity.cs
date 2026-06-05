namespace UTM.Core.Enums;

/// <summary>
/// 冲突严重程度枚举
/// </summary>
public enum ConflictSeverity
{
    /// <summary>信息级别</summary>
    Info = 0,
    /// <summary>建议级别</summary>
    Advisory = 1,
    /// <summary>警告级别</summary>
    Warning = 2,
    /// <summary>告警级别</summary>
    Alert = 3,
    /// <summary>危急级别</summary>
    Critical = 4
}
