namespace UTM.ConflictDetection.Models;

public sealed record ResolutionAdvisory
{
    public required Guid ConflictId { get; init; }
    public required Guid AircraftId { get; init; }
    public required AdvisoryType AdvisoryType { get; init; }
    public required string Instructions { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
}
