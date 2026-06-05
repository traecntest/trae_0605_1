using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UTM.Airspace.Services;
using UTM.Airspace.Models;
using UTM.Core.Entities;
using UTM.Core.Enums;
using UTM.Core.ValueObjects;

namespace UTM.API.Controllers;

public static class FlightPlanEndpoints
{
    public static void MapFlightPlanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/flight-plans").WithTags("FlightPlans");

        group.MapPost("/", async (FlightPlanRequest request, IFlightPlanService service, CancellationToken ct) =>
        {
            var result = await service.SubmitAsync(request, ct);
            return Results.Created($"/api/flight-plans/{result.FlightPlanId}", result);
        });

        group.MapGet("/{id:guid}", async (Guid id, IFlightPlanService service, CancellationToken ct) =>
        {
            var plan = await service.GetAsync(id, ct);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        });

        group.MapPost("/{id:guid}/approve", async (Guid id, string approverId, IFlightPlanService service, CancellationToken ct) =>
        {
            var result = await service.ApproveAsync(id, approverId, ct);
            return result.IsApproved ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/{id:guid}/reject", async (Guid id, string reason, IFlightPlanService service, CancellationToken ct) =>
        {
            await service.RejectAsync(id, reason, ct);
            return Results.Ok();
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, IFlightPlanService service, CancellationToken ct) =>
        {
            await service.CancelAsync(id, ct);
            return Results.Ok();
        });

        group.MapGet("/", async (string? operatorId, FlightStatus? status, IFlightPlanService service, CancellationToken ct) =>
        {
            if (!string.IsNullOrEmpty(operatorId))
            {
                var plans = await service.GetByOperatorAsync(operatorId, ct);
                return Results.Ok(plans);
            }

            if (status.HasValue)
            {
                var plans = await service.GetByStatusAsync(status.Value, ct);
                return Results.Ok(plans);
            }

            return Results.BadRequest("必须提供operatorId或status参数");
        });
    }
}
