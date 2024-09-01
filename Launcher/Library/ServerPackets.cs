using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Library.Network.ServerPackets
{
    [PacketMark(2001)]
    public sealed class NewAccount : Packet
    {
        public NewAccountResult Result { get; set; }
    }

    [PacketMark(2002)]
    public sealed class ChangePassword : Packet
    {
        public ChangePasswordResult Result { get; set; }

        public string Message { get; set; }
        public TimeSpan Duration { get; set; }
    }

    [PacketMark(2004)]
    public sealed class RequestPasswordReset : Packet
    {
        public RequestPasswordResetResult Result { get; set; }
        public string Message { get; set; }
        public TimeSpan Duration { get; set; }
    }

    [PacketMark(2005)]
    public sealed class ResetPassword : Packet
    {
        public ResetPasswordResult Result { get; set; }
    }

    [PacketMark(2006)]
    public sealed class Activation : Packet
    {
        public ActivationResult Result { get; set; }
    }

    [PacketMark(2007)]
    public sealed class RequestActivationKey : Packet
    {
        public RequestActivationKeyResult Result { get; set; }
        public TimeSpan Duration { get; set; }
    }

    [PacketMark(2008)]
    public sealed class SelectLogout : Packet
    {
    }

    [PacketMark(2009)]
    public sealed class GameLogout : Packet
    {
        public List<SelectInfo> Characters { get; set; }
    }

    [PacketMark(2010)]
    public sealed class NewCharacter : Packet
    {
        public NewCharacterResult Result { get; set; }

        public SelectInfo Character { get; set; }
    }

    [PacketMark(2011)]
    public sealed class DeleteCharacter : Packet
    {
        public DeleteCharacterResult Result { get; set; }

        public int DeletedIndex { get; set; }
    }

    [PacketMark(2179)]
    public sealed class CheckClientDb : Packet
    {
        public bool IsUpgrading { get; set; }
        public int CurrentIndex { get; set; } //从0开始
        public int TotalCount { get; set; }
        public byte[] Datas { get; set; }
    }

    [PacketMark(2180)]
    public sealed class CheckClientHash : Packet
    {
        public List<ClientUpgradeItem> ClientFileHash { get; set; }
    }

    [PacketMark(2181)]
    public sealed class UpgradeClient : Packet
    {
        public string FileKey { get; set; }
        public int TotalSize { get; set; }
        public int StartIndex { get; set; }
        public byte[] Datas { get; set; }
    }

    [PacketMark(2182)]
    public sealed class LoginSimple : Packet
    {
        public LoginResult Result { get; set; }

        public string Message { get; set; }
        public TimeSpan Duration { get; set; }

        public List<SelectInfo> Characters { get; set; }

        public string Address { get; set; }

        public bool TestServer { get; set; }
    }
}

