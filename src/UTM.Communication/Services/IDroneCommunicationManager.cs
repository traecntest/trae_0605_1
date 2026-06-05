using UTM.Communication.Protocols;

namespace UTM.Communication.Services;

public interface IDroneCommunicationManager
{
    ValueTask SendAsync(byte systemId, MavLinkMessage message, CancellationToken ct = default);
    ValueTask<MavLinkMessage?> ReceiveAsync(byte systemId, TimeSpan timeout, CancellationToken ct = default);
    ValueTask BroadcastAsync(MavLinkMessage message, CancellationToken ct = default);
    IReadOnlyCollection<byte> GetConnectedSystems();
}
