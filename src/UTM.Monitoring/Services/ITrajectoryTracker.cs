using UTM.Core.Entities;
using UTM.Core.ValueObjects;
using UTM.Monitoring.Models;

namespace UTM.Monitoring.Services;

/// <summary>
/// 轨迹跟踪服务接口
/// </summary>
public interface ITrajectoryTracker
{
    /// <summary>
    /// 提交航空器状态更新
    /// </summary>
    ValueTask SubmitStateAsync(Guid aircraftId, AircraftStateData state, CancellationToken ct = default);

    /// <summary>
    /// 获取航空器当前轨迹
    /// </summary>
    IReadOnlyList<TrajectoryPoint> GetTrajectory(Guid aircraftId);

    /// <summary>
    /// 获取航空器最新状态
    /// </summary>
    AircraftStateData? GetLatestState(Guid aircraftId);

    /// <summary>
    /// 获取指定时间范围内的轨迹
    /// </summary>
    IReadOnlyList<TrajectoryPoint> GetTrajectoryInRange(Guid aircraftId, DateTimeOffset from, DateTimeOffset to);

    /// <summary>
    /// 获取所有活跃航空器ID
    /// </summary>
    IReadOnlyCollection<Guid> GetActiveAircraftIds();

    /// <summary>
    /// 移除航空器轨迹（飞行结束后清理）
    /// </summary>
    bool RemoveTrajectory(Guid aircraftId);
}
