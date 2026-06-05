using System.Buffers.Binary;
using System.Text;

namespace UTM.Communication.Protocols;

/// <summary>
/// MAVLink协议编解码器
/// </summary>
public static class MavLinkCodec
{
    private const byte MavLinkStx = 0xFD; // MAVLink v2起始标志
    private const int HeaderLength = 10;
    private const int ChecksumLength = 2;

    /// <summary>
    /// 编码MAVLink消息为字节数组
    /// </summary>
    public static byte[] Encode(MavLinkMessage message)
    {
        var payload = EncodePayload(message);
        var length = payload.Length;
        
        var buffer = new byte[HeaderLength + length + ChecksumLength];
        var offset = 0;

        // 包头
        buffer[offset++] = MavLinkStx; // STX
        buffer[offset++] = (byte)length; // 载荷长度
        buffer[offset++] = 0; // 不兼容标志
        buffer[offset++] = 0; // 兼容标志
        buffer[offset++] = 0; // 包序号
        
        // 系统ID和组件ID
        buffer[offset++] = message.SystemId;
        buffer[offset++] = message.ComponentId;
        
        // 消息ID (3字节，小端)
        var messageId = (uint)message.MessageType;
        buffer[offset++] = (byte)(messageId & 0xFF);
        buffer[offset++] = (byte)((messageId >> 8) & 0xFF);
        buffer[offset++] = (byte)((messageId >> 16) & 0xFF);

        // 载荷
        payload.CopyTo(buffer, offset);
        offset += length;

        // CRC (简化处理，实际应计算CRC16)
        var crc = CalculateCrc(buffer, 0, offset);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), crc);

        return buffer;
    }

    /// <summary>
    /// 解码字节数组为MAVLink消息
    /// </summary>
    public static MavLinkMessage? Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderLength + ChecksumLength)
            return null;

        if (data[0] != MavLinkStx)
            return null;

        var length = data[1];
        if (data.Length < HeaderLength + length + ChecksumLength)
            return null;

        var systemId = data[6];
        var componentId = data[7];
        
        // 消息ID (3字节，小端)
        var messageId = (uint)(data[8] | (data[9] << 8) | (data[10] << 16));

        var payload = data.Slice(HeaderLength, length);

        return DecodePayload(systemId, componentId, messageId, payload);
    }

    private static byte[] EncodePayload(MavLinkMessage message)
    {
        return message switch
        {
            HeartbeatMessage msg => EncodeHeartbeat(msg),
            GlobalPositionIntMessage msg => EncodeGlobalPosition(msg),
            StatustextMessage msg => EncodeStatustext(msg),
            _ => Array.Empty<byte>()
        };
    }

    private static byte[] EncodeHeartbeat(HeartbeatMessage msg)
    {
        var buffer = new byte[9];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0), msg.CustomMode);
        buffer[4] = msg.Type;
        buffer[5] = msg.Autopilot;
        buffer[6] = msg.BaseMode;
        buffer[7] = msg.SystemStatus;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(8), msg.MavlinkVersion);
        return buffer;
    }

    private static byte[] EncodeGlobalPosition(GlobalPositionIntMessage msg)
    {
        var buffer = new byte[28];
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(0), msg.TimeBootMs);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(4), msg.Lat);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(8), msg.Lon);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(12), msg.Alt);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(16), msg.RelativeAlt);
        BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(20), msg.Vx);
        BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(22), msg.Vy);
        BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(24), msg.Vz);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(26), msg.Hdg);
        return buffer;
    }

    private static byte[] EncodeStatustext(StatustextMessage msg)
    {
        var buffer = new byte[51];
        buffer[0] = msg.Severity;
        var textBytes = Encoding.UTF8.GetBytes(msg.Text);
        var copyLength = Math.Min(textBytes.Length, 50);
        Array.Copy(textBytes, 0, buffer, 1, copyLength);
        return buffer;
    }

    private static MavLinkMessage? DecodePayload(byte systemId, byte componentId, uint messageId, ReadOnlySpan<byte> payload)
    {
        var messageType = (MavLinkMessageType)messageId;
        
        return messageType switch
        {
            MavLinkMessageType.Heartbeat => DecodeHeartbeat(systemId, componentId, payload),
            MavLinkMessageType.GlobalPositionInt => DecodeGlobalPosition(systemId, componentId, payload),
            MavLinkMessageType.Statustext => DecodeStatustext(systemId, componentId, payload),
            _ => null
        };
    }

    private static HeartbeatMessage DecodeHeartbeat(byte systemId, byte componentId, ReadOnlySpan<byte> payload)
    {
        return new HeartbeatMessage
        {
            SystemId = systemId,
            ComponentId = componentId,
            CustomMode = BinaryPrimitives.ReadUInt32LittleEndian(payload),
            Type = payload[4],
            Autopilot = payload[5],
            BaseMode = payload[6],
            SystemStatus = payload[7],
            MavlinkVersion = BinaryPrimitives.ReadUInt32LittleEndian(payload.Slice(8))
        };
    }

    private static GlobalPositionIntMessage DecodeGlobalPosition(byte systemId, byte componentId, ReadOnlySpan<byte> payload)
    {
        return new GlobalPositionIntMessage
        {
            SystemId = systemId,
            ComponentId = componentId,
            TimeBootMs = BinaryPrimitives.ReadInt32LittleEndian(payload),
            Lat = BinaryPrimitives.ReadInt32LittleEndian(payload.Slice(4)),
            Lon = BinaryPrimitives.ReadInt32LittleEndian(payload.Slice(8)),
            Alt = BinaryPrimitives.ReadInt32LittleEndian(payload.Slice(12)),
            RelativeAlt = BinaryPrimitives.ReadInt32LittleEndian(payload.Slice(16)),
            Vx = BinaryPrimitives.ReadInt16LittleEndian(payload.Slice(20)),
            Vy = BinaryPrimitives.ReadInt16LittleEndian(payload.Slice(22)),
            Vz = BinaryPrimitives.ReadInt16LittleEndian(payload.Slice(24)),
            Hdg = BinaryPrimitives.ReadUInt16LittleEndian(payload.Slice(26))
        };
    }

    private static StatustextMessage DecodeStatustext(byte systemId, byte componentId, ReadOnlySpan<byte> payload)
    {
        var severity = payload[0];
        var text = Encoding.UTF8.GetString(payload.Slice(1, 50)).TrimEnd('\0');
        
        return new StatustextMessage
        {
            SystemId = systemId,
            ComponentId = componentId,
            Severity = severity,
            Text = text
        };
    }

    private static ushort CalculateCrc(byte[] buffer, int offset, int length)
    {
        // 简化的CRC计算，实际应使用CRC16-MCRF4XX
        ushort crc = 0xFFFF;
        for (int i = offset; i < length; i++)
        {
            crc ^= (ushort)(buffer[i] << 8);
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }
}
