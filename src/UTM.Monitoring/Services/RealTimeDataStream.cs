using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using UTM.Monitoring.Models;

namespace UTM.Monitoring.Services;

public sealed class RealTimeDataStream : IAsyncDisposable
{
    private readonly Channel<AircraftStateData> _channel;
    private readonly ILogger<RealTimeDataStream> _logger;
    private readonly CancellationTokenSource _cts = new();

    public RealTimeDataStream(ILogger<RealTimeDataStream> logger, int capacity = 50000)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<AircraftStateData>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false
        });
    }

    public async ValueTask WriteAsync(AircraftStateData data, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(data, ct);
    }

    public IAsyncEnumerable<AircraftStateData> ReadAllAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _channel.Writer.Complete();
        _cts.Dispose();
    }
}
