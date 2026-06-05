using Microsoft.Extensions.Logging;
using UTM.Monitoring.Models;

namespace UTM.Monitoring.Services;

public sealed class AircraftStateProcessor(ILogger<AircraftStateProcessor> logger)
{
    public AircraftStateData? Process(AircraftStateData? existing, AircraftStateData incoming)
    {
        if (existing is null) return incoming;
        if (incoming.Timestamp <= existing.Timestamp) return existing;

        logger.LogDebug("更新航空器状态: {AircraftId}", incoming.AircraftId);
        return incoming;
    }
}
