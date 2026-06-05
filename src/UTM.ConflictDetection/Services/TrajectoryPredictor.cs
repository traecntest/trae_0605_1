using UTM.Monitoring.Models;

namespace UTM.ConflictDetection.Services;

public class TrajectoryPredictor
{
    public AircraftStateData Predict(AircraftStateData current, double secondsAhead)
    {
        var dt = secondsAhead;
        var dx = current.GroundSpeed * Math.Sin(current.Track * Math.PI / 180) * dt;
        var dy = current.GroundSpeed * Math.Cos(current.Track * Math.PI / 180) * dt;

        var lat = current.Position.Latitude + (dy / 111000);
        var lon = current.Position.Longitude + (dx / (111000 * Math.Cos(current.Position.Latitude * Math.PI / 180)));
        var alt = current.Altitude + current.VerticalRate * dt;

        return new AircraftStateData
        {
            AircraftId = current.AircraftId,
            Position = new UTM.Core.ValueObjects.GeoCoordinate(lat, lon),
            Altitude = Math.Max(0, alt),
            GroundSpeed = current.GroundSpeed,
            Track = current.Track,
            VerticalRate = current.VerticalRate,
            Heading = current.Heading,
            Timestamp = current.Timestamp.AddSeconds(secondsAhead)
        };
    }
}
