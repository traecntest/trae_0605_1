namespace UTM.ConflictDetection.Models;

public sealed record ConflictAlert
{
    public required Guid Id { get; init; }
    public required Guid AircraftId1 { get; init; }
    public required Guid AircraftId2 { get; init; }
    public required ConflictType ConflictType { get; init; }
    public required ThreatLevel Severity { get; init; }
    public required double TcpaSeconds { get; init; }
    public required double CpaMeters { get; init; }
    public required DateTimeOffset DetectedAt { get; init; }
}
