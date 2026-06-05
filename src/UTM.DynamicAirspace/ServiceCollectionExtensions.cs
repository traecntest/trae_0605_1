using Microsoft.Extensions.DependencyInjection;
using UTM.DynamicAirspace.Services;

namespace UTM.DynamicAirspace;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加动态空域管理服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDynamicAirspace(this IServiceCollection services)
    {
        // 注册配置选项
        services.AddSingleton(new DynamicAirspaceOptions());

        // 注册动态空域管理器
        services.AddSingleton<IDynamicAirspaceManager, DynamicAirspaceManager>();

        // 注册气象集成服务
        services.AddSingleton<IWeatherIntegrationService, WeatherIntegrationService>();

        // 注册限制评估器
        services.AddSingleton<IRestrictionEvaluator, RestrictionEvaluator>();

        return services;
    }

    /// <summary>
    /// 添加动态空域管理服务（带配置）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDynamicAirspace(this IServiceCollection services, Action<DynamicAirspaceOptions> configure)
    {
        var options = new DynamicAirspaceOptions();
        configure(options);
        services.AddSingleton(options);

        // 注册服务
        services.AddSingleton<IDynamicAirspaceManager, DynamicAirspaceManager>();
        services.AddSingleton<IWeatherIntegrationService, WeatherIntegrationService>();
        services.AddSingleton<IRestrictionEvaluator, RestrictionEvaluator>();

        return services;
    }
}

/// <summary>
/// 动态空域配置选项
/// </summary>
public class DynamicAirspaceOptions
{
    /// <summary>
    /// 默认限制优先级
    /// </summary>
    public int DefaultPriority { get; set; } = 5;

    /// <summary>
    /// 气象数据更新间隔（分钟）
    /// </summary>
    public int WeatherUpdateIntervalMinutes { get; set; } = 30;

    /// <summary>
    /// 是否启用自动 NOTAM 发布
    /// </summary>
    public bool EnableAutoNotamPublishing { get; set; } = true;

    /// <summary>
    /// 是否启用事件通知
    /// </summary>
    public bool EnableEventNotifications { get; set; } = true;

    /// <summary>
    /// 最大并发限制数量
    /// </summary>
    public int MaxConcurrentRestrictions { get; set; } = 1000;

    /// <summary>
    /// 限制过期清理间隔（小时）
    /// </summary>
    public int RestrictionCleanupIntervalHours { get; set; } = 24;
}
