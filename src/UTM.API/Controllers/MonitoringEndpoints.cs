using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UTM.Monitoring.Services;
using UTM.Monitoring.Models;

namespace UTM.API.Controllers;

public static class MonitoringEndpoints
{
    public static void MapMonitoringEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/monitoring").WithTags("Monitoring");

        group.MapPost("/aircraft-state", async (Guid aircraftId, AircraftStateData state, ITrajectoryTracker tracker, CancellationToken ct) =>
        {
            await tracker.SubmitStateAsync(aircraftId, state, ct);
            return Results.Accepted();
        });

        group.MapGet("/aircraft/{id:guid}/trajectory", (Guid id, ITrajectoryTracker tracker) =>
        {
            var trajectory = tracker.GetTrajectory(id);
            return Results.Ok(trajectory);
        });

        group.MapGet("/aircraft/{id:guid}/state", (Guid id, ITrajectoryTracker tracker) =>
        {
            var state = tracker.GetLatestState(id);
            return state is null ? Results.NotFound() : Results.Ok(state);
        });

        group.MapGet("/active-aircraft", (ITrajectoryTracker tracker) =>
        {
            var ids = tracker.GetActiveAircraftIds();
            return Results.Ok(ids);
        });
    }
}
