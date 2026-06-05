namespace UTM.Core.Exceptions;

public class AirspaceViolationException(string message) : Exception(message)
{
    public Guid? AircraftId { get; init; }
    public Guid? AirspaceId { get; init; }
}

public class FlightPlanConflictException(string message) : Exception(message)
{
    public Guid? ExistingFlightPlanId { get; init; }
    public Guid? NewFlightPlanId { get; init; }
}

public class FlightPlanValidationException(string message) : Exception(message)
{
    public IReadOnlyList<string> Errors { get; init; } = [];
}

public class AircraftNotFoundException(Guid aircraftId)
    : Exception($"Aircraft with ID '{aircraftId}' was not found.");

public class FlightPlanNotFoundException(Guid flightPlanId)
    : Exception($"Flight plan with ID '{flightPlanId}' was not found.");

public class AirspaceCapacityExceededException(string airspaceName, int capacity)
    : Exception($"Airspace '{airspaceName}' has reached maximum capacity of {capacity}.");
