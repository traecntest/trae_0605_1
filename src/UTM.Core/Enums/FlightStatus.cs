namespace UTM.Core.Enums;

/// <summary>
/// 飞行状态枚举
/// </summary>
public enum FlightStatus
{
    /// <summary>计划中</summary>
    Planned = 0,
    /// <summary>已提交</summary>
    Submitted = 1,
    /// <summary>审批中</summary>
    PendingApproval = 2,
    /// <summary>已批准</summary>
    Approved = 3,
    /// <summary>已拒绝</summary>
    Rejected = 4,
    /// <summary>执行中</summary>
    InFlight = 5,
    /// <summary>已完成</summary>
    Completed = 6,
    /// <summary>已取消</summary>
    Cancelled = 7,
    /// <summary>紧急</summary>
    Emergency = 8,
    /// <summary>迫降</summary>
    ForcedLanding = 9
}
