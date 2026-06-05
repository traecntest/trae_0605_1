using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using UTM.ConflictDetection.Services;
using UTM.Monitoring.Services;

namespace UTM.API.Controllers;

public static class ConflictDetectionEndpoints
{
    public static IEndpointRouteBuilder MapConflictDetectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conflict-detection").WithTags("ConflictDetection");

        group.MapPost("/detect", async (IConflictDetector detector, ITrajectoryTracker tracker, CancellationToken ct) =>
        {
            var aircraftIds = tracker.GetActiveAircraftIds();
            var states = new Dictionary<Guid, UTM.Monitoring.Models.AircraftStateData>();

            foreach (var id in aircraftIds)
            {
                var state = tracker.GetLatestState(id);
                if (state != null)
                {
                    states[id] = state;
                }
            }

            var conflicts = await detector.DetectConflictsAsync(states, ct);
            return Results.Ok(conflicts);
        });

        group.MapGet("/predict/{aircraftId:guid}", (Guid aircraftId, TrajectoryPredictor predictor, ITrajectoryTracker tracker) =>
        {
            var state = tracker.GetLatestState(aircraftId);
            if (state == null)
            {
                return Results.NotFound();
            }

            // Predict 60 seconds ahead
            var prediction = predictor.Predict(state, 60.0);
            return Results.Ok(prediction);
        });

        return app;
    }
}
