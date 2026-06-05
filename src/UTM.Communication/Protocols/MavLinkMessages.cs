namespace UTM.Communication.Protocols;

/// <summary>
/// MAVLink协议消息类型
/// </summary>
public enum MavLinkMessageType : uint
{
    Heartbeat = 0,
    SysStatus = 1,
    GlobalPositionInt = 33,
    Attitude = 30,
    CommandLong = 76,
    MissionItem = 39,
    MissionRequest = 40,
    MissionAck = 47,
    Statustext = 253,
    ExtendedSysState = 245
}

/// <summary>
/// MAVLink消息基类
/// </summary>
public abstract record MavLinkMessage
{
    public required byte SystemId { get; init; }
    public required byte ComponentId { get; init; }
    public abstract MavLinkMessageType MessageType { get; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 心跳消息
/// </summary>
public sealed record HeartbeatMessage : MavLinkMessage
{
    public override MavLinkMessageType MessageType => MavLinkMessageType.Heartbeat;
    public required uint CustomMode { get; init; }
    public required byte Type { get; init; }
    public required byte Autopilot { get; init; }
    public required byte BaseMode { get; init; }
    public required byte SystemStatus { get; init; }
    public required uint MavlinkVersion { get; init; }
}

/// <summary>
/// 全局位置消息
/// </summary>
public sealed record GlobalPositionIntMessage : MavLinkMessage
{
    public override MavLinkMessageType MessageType => MavLinkMessageType.GlobalPositionInt;
    public required int TimeBootMs { get; init; }
    public required int Lat { get; init; } // 1E7度
    public required int Lon { get; init; } // 1E7度
    public required int Alt { get; init; } // 毫米
    public required int RelativeAlt { get; init; } // 毫米
    public required short Vx { get; init; } // cm/s
    public required short Vy { get; init; } // cm/s
    public required short Vz { get; init; } // cm/s
    public required ushort Hdg { get; init; } // 厘度
}

/// <summary>
/// 状态文本消息
/// </summary>
public sealed record StatustextMessage : MavLinkMessage
{
    public override MavLinkMessageType MessageType => MavLinkMessageType.Statustext;
    public required byte Severity { get; init; }
    public required string Text { get; init; }
}
