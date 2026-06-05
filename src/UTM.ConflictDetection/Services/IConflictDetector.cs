using UTM.ConflictDetection.Models;
using UTM.Monitoring.Models;

namespace UTM.ConflictDetection.Services;

public interface IConflictDetector
{
    ValueTask<IReadOnlyList<ConflictAlert>> DetectConflictsAsync(
        IReadOnlyDictionary<Guid, AircraftStateData> aircraftStates,
        CancellationToken ct = default);
}
