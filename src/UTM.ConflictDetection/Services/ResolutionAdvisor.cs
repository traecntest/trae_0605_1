using UTM.ConflictDetection.Models;
using UTM.Monitoring.Models;

namespace UTM.ConflictDetection.Services;

public class ResolutionAdvisor
{
    public ResolutionAdvisory? GenerateAdvisory(ConflictAlert conflict, AircraftStateData aircraft)
    {
        if (conflict.AircraftId1 != aircraft.AircraftId && conflict.AircraftId2 != aircraft.AircraftId)
            return null;

        var advisoryType = DetermineAdvisoryType(conflict);
        var instructions = GenerateInstructions(conflict, aircraft);

        return new ResolutionAdvisory
        {
            ConflictId = conflict.Id,
            AircraftId = aircraft.AircraftId,
            AdvisoryType = advisoryType,
            Instructions = instructions,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private static AdvisoryType DetermineAdvisoryType(ConflictAlert conflict)
    {
        return conflict.Severity switch
        {
            ThreatLevel.Critical => AdvisoryType.EmergencyLanding,
            ThreatLevel.Warning or ThreatLevel.Alert => AdvisoryType.AltitudeChange,
            _ => AdvisoryType.HeadingChange
        };
    }

    private static string GenerateInstructions(ConflictAlert conflict, AircraftStateData aircraft)
    {
        return conflict.Severity switch
        {
            ThreatLevel.Critical => $"紧急：立即下降至安全高度，当前高度{aircraft.Altitude:F0}米",
            ThreatLevel.Alert => $"警告：改变航向{90}度，当前航向{aircraft.Heading:F0}度",
            ThreatLevel.Warning => $"建议：调整高度{(aircraft.Altitude > 100 ? -50 : 50):+0;-0}米",
            _ => $"注意：保持当前飞行参数，TCPA={conflict.TcpaSeconds:F0}秒"
        };
    }
}
