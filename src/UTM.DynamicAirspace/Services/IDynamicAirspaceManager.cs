using UTM.DynamicAirspace.Models;

namespace UTM.DynamicAirspace.Services;

/// <summary>
/// 动态空域管理接口
/// </summary>
public interface IDynamicAirspaceManager
{
    /// <summary>
    /// 添加动态空域区域
    /// </summary>
    /// <param name="region">空域区域</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    Task<bool> AddRegionAsync(DynamicAirspaceRegion region, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新动态空域区域
    /// </summary>
    /// <param name="region">空域区域</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    Task<bool> UpdateRegionAsync(DynamicAirspaceRegion region, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除动态空域区域
    /// </summary>
    /// <param name="regionId">区域 ID</param>
    /// <param name="reason">删除原因</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    Task<bool> RemoveRegionAsync(string regionId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有活动区域
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>活动区域列表</returns>
    Task<IReadOnlyList<DynamicAirspaceRegion>> GetActiveRegionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定区域
    /// </summary>
    /// <param name="regionId">区域 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>区域信息</returns>
    Task<DynamicAirspaceRegion?> GetRegionAsync(string regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询与指定位置相交的区域
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>相交区域列表</returns>
    Task<IReadOnlyList<DynamicAirspaceRegion>> GetIntersectingRegionsAsync(GeoCoordinate position, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加临时飞行限制
    /// </summary>
    /// <param name="tfr">TFR 记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    Task<bool> AddTfrAsync(TemporaryFlightRestriction tfr, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除临时飞行限制
    /// </summary>
    /// <param name="tfrId">TFR ID</param>
    /// <param name="reason">移除原因</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    Task<bool> RemoveTfrAsync(string tfrId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有有效 TFR
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>TFR 列表</returns>
    Task<IReadOnlyList<TemporaryFlightRestriction>> GetActiveTfrsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布 NOTAM
    /// </summary>
    /// <param name="notam">NOTAM 记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    Task<bool> PublishNotamAsync(NoticeToAirmen notam, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取消 NOTAM
    /// </summary>
    /// <param name="notamId">NOTAM ID</param>
    /// <param name="reason">取消原因</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    Task<bool> CancelNotamAsync(string notamId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有有效 NOTAM
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>NOTAM 列表</returns>
    Task<IReadOnlyList<NoticeToAirmen>> GetActiveNotamsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查位置是否受限
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="altitude">高度</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否受限</returns>
    Task<bool> IsRestrictedAsync(GeoCoordinate position, double altitude, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取位置所有限制
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>限制列表</returns>
    Task<IReadOnlyList<RestrictionConditions>> GetRestrictionsAtAsync(GeoCoordinate position, CancellationToken cancellationToken = default);
}
