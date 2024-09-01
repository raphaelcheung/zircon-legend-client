using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Network.ClientPackets
{
    [PacketMark(1001)]
    public sealed class NewAccount : Packet
    {
        public string EMailAddress { get; set; }
        public string Password { get; set; }
        public DateTime BirthDate { get; set; }
        public string RealName { get; set; }
        public string Referral { get; set; }
        public string CheckSum { get; set; }
    }

    [PacketMark(1002)]
    public sealed class ChangePassword : Packet
    {
        public string EMailAddress { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string CheckSum { get; set; }
    }

    [PacketMark(1003)]
    public sealed class RequestPasswordReset : Packet
    {
        public string EMailAddress { get; set; }
        public string CheckSum { get; set; }
    }

    [PacketMark(1004)]
    public sealed class ResetPassword : Packet
    {
        public string ResetKey { get; set; }
        public string NewPassword { get; set; }
        public string CheckSum { get; set; }
    }

    [PacketMark(1005)]
    public sealed class Activation : Packet
    {
        public string ActivationKey { get; set; }
        public string CheckSum { get; set; }
    }

    [PacketMark(1006)]
    public sealed class RequestActivationKey : Packet
    {
        public string EMailAddress { get; set; }
        public string CheckSum { get; set; }
    }

    [PacketMark(1007)]
    public sealed class SelectLanguage : Packet
    {
        public string Language { get; set; }
    }

    [PacketMark(1009)]
    public sealed class Logout : Packet { }

    [PacketMark(1010)]
    public sealed class NewCharacter : Packet
    {
        public string CharacterName { get; set; }
        public MirClass Class { get; set; }
        public MirGender Gender { get; set; }
        public int HairType { get; set; }
        public Color HairColour { get; set; }
        public Color ArmourColour { get; set; }
        public string CheckSum { get; set; }
    }

    [PacketMark(1011)]
    public sealed class DeleteCharacter : Packet
    {
        public int CharacterIndex { get; set; }
        public string CheckSum { get; set; }
    }

    [PacketMark(1109)]
    public sealed class CheckClientDb : Packet
    {
        public string Hash { get; set; }
    }

    [PacketMark(1110)]
    public sealed class UpgradeClient : Packet
    {
        public string FileKey { get; set; }
    }

    [PacketMark(1111)]
    public sealed class LoginSimple : Packet
    {
        public string EMailAddress { get; set; }
        public string Password { get; set; }
        public string CheckSum { get; set; }
    }
}
