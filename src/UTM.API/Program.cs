using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UTM.Airspace.DependencyInjection;
using UTM.Monitoring.Services;
using UTM.ConflictDetection;
using UTM.DynamicAirspace;
using UTM.Communication;
using UTM.Security;
using UTM.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册UTM模块
builder.Services.AddAirspaceServices();
builder.Services.AddSingleton<ITrajectoryTracker, TrajectoryTracker>();
builder.Services.AddSingleton<AircraftStateProcessor>();
builder.Services.AddSingleton<RealTimeDataStream>();
builder.Services.AddConflictDetection();
builder.Services.AddDynamicAirspace();
builder.Services.AddCommunication();
builder.Services.AddSecurity();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// 映射端点
app.MapFlightPlanEndpoints();
app.MapMonitoringEndpoints();
app.MapConflictDetectionEndpoints();
app.MapAirspaceEndpoints();
app.MapSecurityEndpoints();

app.MapGet("/", () => "UTM - 低空经济与无人机交通管理系统 v1.0");
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
