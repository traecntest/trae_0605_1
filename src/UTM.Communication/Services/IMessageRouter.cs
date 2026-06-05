using UTM.Communication.Protocols;

namespace UTM.Communication.Services;

public interface IMessageRouter
{
    void RegisterHandler(MavLinkMessageType messageType, Func<MavLinkMessage, Task> handler);
    void UnregisterHandler(MavLinkMessageType messageType);
    ValueTask RouteAsync(MavLinkMessage message, CancellationToken ct = default);
}
