using UTM.Airspace.Services;
using UTM.Airspace.Models;
using UTM.Airspace.Repositories;
using UTM.Core.Entities;
using UTM.Core.Enums;
using UTM.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UTM.Integration.Tests;

public class FlightPlanWorkflowTests
{
    [Fact]
    public async Task FlightPlan_CompleteWorkflow_ShouldSucceed()
    {
        var repository = new InMemoryFlightPlanRepository();
        var service = new FlightPlanService(repository);

        var request = CreateTestRequest();

        // 提交飞行计划
        var submitResult = await service.SubmitAsync(request);
        Assert.True(submitResult.IsApproved);
        Assert.NotEqual(Guid.Empty, submitResult.FlightPlanId);

        // 查询飞行计划
        var retrieved = await service.GetAsync(submitResult.FlightPlanId);
        Assert.NotNull(retrieved);
        Assert.Equal(FlightStatus.Submitted, retrieved.Status);

        // 审批飞行计划
        var approvalResult = await service.ApproveAsync(submitResult.FlightPlanId, "approver-001");
        Assert.True(approvalResult.IsApproved);

        // 验证状态
        var approved = await service.GetAsync(submitResult.FlightPlanId);
        Assert.NotNull(approved);
        Assert.Equal(FlightStatus.Approved, approved.Status);
    }

    [Fact]
    public async Task FlightPlan_CancelApproved_ShouldSucceed()
    {
        var repository = new InMemoryFlightPlanRepository();
        var service = new FlightPlanService(repository);

        var request = CreateTestRequest();
        var submitResult = await service.SubmitAsync(request);
        var approvalResult = await service.ApproveAsync(submitResult.FlightPlanId, "approver-001");

        await service.CancelAsync(submitResult.FlightPlanId);

        var cancelled = await service.GetAsync(submitResult.FlightPlanId);
        Assert.NotNull(cancelled);
        Assert.Equal(FlightStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task FlightPlan_Reject_ShouldSucceed()
    {
        var repository = new InMemoryFlightPlanRepository();
        var service = new FlightPlanService(repository);

        var request = CreateTestRequest();
        var submitResult = await service.SubmitAsync(request);

        await service.RejectAsync(submitResult.FlightPlanId, "空域冲突");

        var rejected = await service.GetAsync(submitResult.FlightPlanId);
        Assert.NotNull(rejected);
        Assert.Equal(FlightStatus.Rejected, rejected.Status);
        Assert.Equal("空域冲突", rejected.RejectionReason);
    }

    private FlightPlanRequest CreateTestRequest() => new()
    {
        AircraftId = Guid.NewGuid(),
        OperatorId = "operator-001",
        Route = new FlightRoute
        {
            DeparturePoint = new GeoCoordinate(39.9042, 116.4074),
            ArrivalPoint = new GeoCoordinate(39.9542, 116.4574),
            Waypoints = new List<Waypoint>
            {
                new() { Sequence = 0, Position = new GeoCoordinate(39.9042, 116.4074), AltitudeMeters = 0 },
                new() { Sequence = 1, Position = new GeoCoordinate(39.9542, 116.4574), AltitudeMeters = 120 }
            },
            CruiseAltitudeMeters = 120,
            CruiseSpeedMps = 15
        },
        PlannedTime = new TimeInterval(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2)),
        Purpose = FlightPurpose.CargoDelivery
    };
}
