using System.Collections.Concurrent;
using UTM.Core.ValueObjects;

namespace UTM.ConflictDetection.Spatial;

public class SpatialGrid
{
    private readonly ConcurrentDictionary<(int x, int y), HashSet<Guid>> _cells = new();
    private readonly double _cellSize;

    public SpatialGrid(double cellSize = 1000.0)
    {
        _cellSize = cellSize;
    }

    public void Update(Guid aircraftId, GeoCoordinate position)
    {
        var cell = GetCell(position);
        _cells.AddOrUpdate(cell,
            _ => new HashSet<Guid> { aircraftId },
            (_, set) => { set.Add(aircraftId); return set; });
    }

    public IEnumerable<Guid> GetNearby(GeoCoordinate position, double radius)
    {
        var centerCell = GetCell(position);
        var cellRadius = (int)Math.Ceiling(radius / _cellSize);
        var result = new HashSet<Guid>();

        for (int dx = -cellRadius; dx <= cellRadius; dx++)
        {
            for (int dy = -cellRadius; dy <= cellRadius; dy++)
            {
                var cell = (centerCell.x + dx, centerCell.y + dy);
                if (_cells.TryGetValue(cell, out var aircraft))
                {
                    foreach (var id in aircraft)
                        result.Add(id);
                }
            }
        }

        return result;
    }

    private (int x, int y) GetCell(GeoCoordinate position)
    {
        var x = (int)(position.Longitude * 111000 * Math.Cos(position.Latitude * Math.PI / 180) / _cellSize);
        var y = (int)(position.Latitude * 111000 / _cellSize);
        return (x, y);
    }
}
