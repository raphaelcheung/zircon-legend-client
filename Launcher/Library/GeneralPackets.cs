namespace Library.Network.GeneralPackets
{
    [PacketMark(1)]
    public sealed class Connected : Packet { }

    [PacketMark(2)]
    public sealed class Ping : Packet { }

    [PacketMark(3)]
    public sealed class CheckVersion : Packet
    {
    }

    [PacketMark(4)]
    public sealed class Version : Packet
    {
        public byte[] ClientHash { get; set; }
    }

    [PacketMark(5)]
    public sealed class GoodVersion : Packet { }

    [PacketMark(6)]
    public sealed class PingResponse : Packet
    {
        public int Ping { get; set; }
    }

    [PacketMark(7)]
    public sealed class Disconnect : Packet
    {
        public DisconnectReason Reason { get; set; }
    }
}
