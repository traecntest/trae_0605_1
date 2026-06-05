using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using UTM.DynamicAirspace.Services;
using UTM.DynamicAirspace.Models;

namespace UTM.API.Controllers;

public static class AirspaceEndpoints
{
    public static IEndpointRouteBuilder MapAirspaceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/airspace").WithTags("Airspace");

        group.MapGet("/regions", async (IDynamicAirspaceManager manager, CancellationToken ct) =>
        {
            var regions = await manager.GetActiveRegionsAsync(ct);
            return Results.Ok(regions);
        });

        group.MapGet("/regions/{regionId}", async (string regionId, IDynamicAirspaceManager manager, CancellationToken ct) =>
        {
            var region = await manager.GetRegionAsync(regionId, ct);
            return region is not null ? Results.Ok(region) : Results.NotFound();
        });

        group.MapGet("/tfrs", async (IDynamicAirspaceManager manager, CancellationToken ct) =>
        {
            var tfrs = await manager.GetActiveTfrsAsync(ct);
            return Results.Ok(tfrs);
        });

        group.MapGet("/notams", async (IDynamicAirspaceManager manager, CancellationToken ct) =>
        {
            var notams = await manager.GetActiveNotamsAsync(ct);
            return Results.Ok(notams);
        });

        group.MapPost("/check-restriction", async (CheckRestrictionRequest req, IDynamicAirspaceManager manager, CancellationToken ct) =>
        {
            var isRestricted = await manager.IsRestrictedAsync(
                new GeoCoordinate(req.Latitude, req.Longitude),
                req.Altitude, ct);
            return Results.Ok(new { IsRestricted = isRestricted });
        });

        return app;
    }
}

public sealed record CheckRestrictionRequest(double Latitude, double Longitude, double Altitude);
