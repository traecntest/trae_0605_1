using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using UTM.Core.ValueObjects;
using UTM.Monitoring.Models;

namespace UTM.Monitoring.Services;

/// <summary>
/// 轨迹点记录
/// </summary>
public sealed record TrajectoryPoint(
    GeoCoordinate Position,
    double Altitude,
    double GroundSpeed,
    double Track,
    double VerticalRate,
    DateTimeOffset Timestamp);

/// <summary>
/// 轨迹跟踪服务实现，使用Channel实现高吞吐流式处理
/// </summary>
public sealed class TrajectoryTracker : ITrajectoryTracker, IAsyncDisposable
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<TrajectoryPoint>> _trajectories = new();
    private readonly ConcurrentDictionary<Guid, AircraftStateData> _latestStates = new();
    private readonly Channel<(Guid AircraftId, AircraftStateData State)> _stateChannel;
    private readonly Task _processingTask;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<TrajectoryTracker> _logger;
    private readonly int _maxTrajectoryPoints;

    public TrajectoryTracker(ILogger<TrajectoryTracker> logger, int maxTrajectoryPoints = 10000)
    {
        _logger = logger;
        _maxTrajectoryPoints = maxTrajectoryPoints;

        // 创建有界Channel，支持背压
        _stateChannel = Channel.CreateBounded<(Guid, AircraftStateData)>(
            new BoundedChannelOptions(100000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });

        _processingTask = Task.Run(ProcessStatesAsync);
    }

    public async ValueTask SubmitStateAsync(Guid aircraftId, AircraftStateData state, CancellationToken ct = default)
    {
        await _stateChannel.Writer.WriteAsync((aircraftId, state), ct);
    }

    public IReadOnlyList<TrajectoryPoint> GetTrajectory(Guid aircraftId)
    {
        if (_trajectories.TryGetValue(aircraftId, out var queue))
        {
            return queue.ToArray();
        }
        return Array.Empty<TrajectoryPoint>();
    }

    public AircraftStateData? GetLatestState(Guid aircraftId)
    {
        return _latestStates.TryGetValue(aircraftId, out var state) ? state : null;
    }

    public IReadOnlyList<TrajectoryPoint> GetTrajectoryInRange(Guid aircraftId, DateTimeOffset from, DateTimeOffset to)
    {
        if (_trajectories.TryGetValue(aircraftId, out var queue))
        {
            return queue.Where(p => p.Timestamp >= from && p.Timestamp <= to).ToArray();
        }
        return Array.Empty<TrajectoryPoint>();
    }

    public IReadOnlyCollection<Guid> GetActiveAircraftIds()
    {
        return _latestStates.Keys.ToArray();
    }

    public bool RemoveTrajectory(Guid aircraftId)
    {
        _latestStates.TryRemove(aircraftId, out _);
        return _trajectories.TryRemove(aircraftId, out _);
    }

    private async Task ProcessStatesAsync()
    {
        var reader = _stateChannel.Reader;
        var ct = _cts.Token;

        try
        {
            while (await reader.WaitToReadAsync(ct))
            {
                while (reader.TryRead(out var item))
                {
                    try
                    {
                        ProcessState(item.AircraftId, item.State);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "处理航空器状态失败: AircraftId={AircraftId}", item.AircraftId);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("轨迹跟踪处理任务已取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轨迹跟踪处理任务异常");
        }
    }

    private void ProcessState(Guid aircraftId, AircraftStateData state)
    {
        // 更新最新状态
        _latestStates.AddOrUpdate(aircraftId, state, (_, _) => state);

        // 创建轨迹点
        var point = new TrajectoryPoint(
            state.Position,
            state.Altitude,
            state.GroundSpeed,
            state.Track,
            state.VerticalRate,
            state.Timestamp);

        // 添加到轨迹队列
        var queue = _trajectories.GetOrAdd(aircraftId, _ => new ConcurrentQueue<TrajectoryPoint>());
        queue.Enqueue(point);

        // 限制轨迹点数量
        while (queue.Count > _maxTrajectoryPoints && queue.TryDequeue(out _))
        {
            // 移除最旧的点
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _stateChannel.Writer.Complete();

        try
        {
            await _processingTask;
        }
        catch (OperationCanceledException)
        {
            // 忽略取消异常
        }

        _cts.Dispose();
    }
}
