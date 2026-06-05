using Microsoft.Extensions.Logging;
using UTM.ConflictDetection.Models;
using UTM.Monitoring.Models;

namespace UTM.ConflictDetection.Services;

public class ConflictDetector : IConflictDetector
{
    private readonly ILogger<ConflictDetector> _logger;
    private const double MinSeparationHorizontal = 200.0; // meters
    private const double MinSeparationVertical = 50.0; // meters
    private const double LookaheadSeconds = 60.0;

    public ConflictDetector(ILogger<ConflictDetector> logger)
    {
        _logger = logger;
    }

    public ValueTask<IReadOnlyList<ConflictAlert>> DetectConflictsAsync(
        IReadOnlyDictionary<Guid, AircraftStateData> aircraftStates,
        CancellationToken ct = default)
    {
        var conflicts = new List<ConflictAlert>();
        var states = aircraftStates.Values.ToList();

        for (int i = 0; i < states.Count; i++)
        {
            for (int j = i + 1; j < states.Count; j++)
            {
                var conflict = CheckPair(states[i], states[j]);
                if (conflict != null)
                    conflicts.Add(conflict);
            }
        }

        _logger.LogDebug("检测到 {Count} 个冲突", conflicts.Count);
        return ValueTask.FromResult<IReadOnlyList<ConflictAlert>>(conflicts);
    }

    private ConflictAlert? CheckPair(AircraftStateData s1, AircraftStateData s2)
    {
        var hDist = s1.Position.DistanceTo(s2.Position);
        var vDist = Math.Abs(s1.Altitude - s2.Altitude);

        if (hDist < MinSeparationHorizontal && vDist < MinSeparationVertical)
        {
            var severity = hDist < 50 && vDist < 20 ? ThreatLevel.Critical : ThreatLevel.Warning;
            return new ConflictAlert
            {
                Id = Guid.NewGuid(),
                AircraftId1 = s1.AircraftId,
                AircraftId2 = s2.AircraftId,
                ConflictType = ConflictType.LossOfSeparation,
                Severity = severity,
                TcpaSeconds = 0,
                CpaMeters = hDist,
                DetectedAt = DateTimeOffset.UtcNow
            };
        }

        // Predictive check using velocity vectors
        var tcpa = EstimateTcpa(s1, s2);
        if (tcpa > 0 && tcpa < LookaheadSeconds)
        {
            var cpa = EstimateCpa(s1, s2, tcpa);
            if (cpa < MinSeparationHorizontal)
            {
                var severity = tcpa < 30 ? ThreatLevel.Alert : ThreatLevel.Warning;
                return new ConflictAlert
                {
                    Id = Guid.NewGuid(),
                    AircraftId1 = s1.AircraftId,
                    AircraftId2 = s2.AircraftId,
                    ConflictType = ConflictType.Converging,
                    Severity = severity,
                    TcpaSeconds = tcpa,
                    CpaMeters = cpa,
                    DetectedAt = DateTimeOffset.UtcNow
                };
            }
        }

        return null;
    }

    private static double EstimateTcpa(AircraftStateData s1, AircraftStateData s2)
    {
        var dx = (s2.Position.Longitude - s1.Position.Longitude) * 111000 * Math.Cos(s1.Position.Latitude * Math.PI / 180);
        var dy = (s2.Position.Latitude - s1.Position.Latitude) * 111000;
        var dvx = s2.GroundSpeed * Math.Sin(s2.Track * Math.PI / 180) - s1.GroundSpeed * Math.Sin(s1.Track * Math.PI / 180);
        var dvy = s2.GroundSpeed * Math.Cos(s2.Track * Math.PI / 180) - s1.GroundSpeed * Math.Cos(s1.Track * Math.PI / 180);

        var dot = dx * dvx + dy * dvy;
        var speedSq = dvx * dvx + dvy * dvy;
        if (speedSq < 0.001) return double.MaxValue;
        return -dot / speedSq;
    }

    private static double EstimateCpa(AircraftStateData s1, AircraftStateData s2, double tcpa)
    {
        var dx = (s2.Position.Longitude - s1.Position.Longitude) * 111000 * Math.Cos(s1.Position.Latitude * Math.PI / 180);
        var dy = (s2.Position.Latitude - s1.Position.Latitude) * 111000;
        var dvx = s2.GroundSpeed * Math.Sin(s2.Track * Math.PI / 180) - s1.GroundSpeed * Math.Sin(s1.Track * Math.PI / 180);
        var dvy = s2.GroundSpeed * Math.Cos(s2.Track * Math.PI / 180) - s1.GroundSpeed * Math.Cos(s1.Track * Math.PI / 180);

        var cx = dx + dvx * tcpa;
        var cy = dy + dvy * tcpa;
        return Math.Sqrt(cx * cx + cy * cy);
    }
}
