using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Library.Network;
using Library.Network.ServerPackets;

namespace Library
{
    public static class Globals
    {

        public static Random Random  = new Random();

        public static readonly Regex EMailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.Compiled);
        public static readonly Regex PasswordRegex = new Regex(@"^[\S]{" + MinPasswordLength + "," + MaxPasswordLength + "}$", RegexOptions.Compiled);
        public static readonly Regex CharacterReg = new Regex(@"^[A-Za-z0-9]|[\u4e00-\u9fa5]{" + MinCharacterNameLength + "," + MaxCharacterNameLength + @"}$", RegexOptions.Compiled);
        public static readonly Regex GuildNameRegex = new Regex(@"^[A-Za-z0-9]|[\u4e00-\u9fa5]{" + MinGuildNameLength + "," + MaxGuildNameLength + "}$", RegexOptions.Compiled);

        public static Color NoneColour = Color.White,
                            FireColour = Color.OrangeRed,
                            IceColour = Color.PaleTurquoise,
                            LightningColour = Color.LightSkyBlue,
                            WindColour = Color.LightSeaGreen,
                            HolyColour = Color.DarkKhaki,
                            DarkColour = Color.SaddleBrown,
                            PhantomColour = Color.Purple,

                            BrownNameColour = Color.Brown,
                            RedNameColour = Color.Red;

        public const int
            MinPasswordLength = 5,
            MaxPasswordLength = 15,

            MinRealNameLength = 3,
            MaxRealNameLength = 20,

            MaxEMailLength = 50,

            MinCharacterNameLength = 3,
            MaxCharacterNameLength = 15,
            MaxCharacterCount = 4,

            MinGuildNameLength = 2,
            MaxGuildNameLength = 15,

            MaxChatLength = 120,
            MaxGuildNoticeLength = 4000,

            MaxBeltCount = 10,
            MaxAutoPotionCount = 8,

            MagicRange = 10,

            DuraLossRate = 15,

            GroupLimit = 15,

            CloakRange = 3,
            MarketPlaceFee = 0,
            AccessoryLevelCost = 0,
            AccessoryResetCost = 1000000,

            CraftWeaponPercentCost = 1000000,

            CommonCraftWeaponPercentCost = 30000000,
            SuperiorCraftWeaponPercentCost = 60000000,
            EliteCraftWeaponPercentCost = 80000000;

        public static decimal MarketPlaceTax = 0.07M;  //2.5x Item cost


        public static long
            GuildCreationCost = 7500000,
            GuildMemberCost = 1000000,
            GuildStorageCost = 350000,
            GuildWarCost = 200000;

        public static long
            MasterRefineCost = 50000,
            MasterRefineEvaluateCost = 250000;


        public static List<Size> ValidResolutions = new List<Size>
        {
            new Size(1024, 768),
            new Size(1366, 768),
            new Size(1280, 800),
            new Size(1440, 900),
            new Size(1600, 900),
            new Size(1920, 1080),
        };

        public static List<string> Languages = new List<string>
        {
            "English",
            "Chinese",
        };

        public static List<decimal> ExperienceList = new List<decimal>
        {
            0,
            100,
            200,
            300,
            400,
            600,
            900,
            1200,
            1700,
            2500,
            6000,
            8000,
            10000,
            15000,
            30000,
            40000,
            50000,
            70000,
            100000,
            120000,
            140000,
            250000,
            300000,
            350000,
            400000,
            500000,
            700000,
            1000000,
            1400000,
            1800000,
            2000000,
            2400000,
            2800000,
            3200000,
            3600000,
            4000000,
            4800000,
            5600000,
            8200000,
            9000000,
            11000000,
            14000000,
            18000000,
            22000000,
            25000000,
            30000000,
            35000000,
            40000000,
            50000000,
            60000000,
            70000000,
            85000000,
            110000000,
            135000000,
            145000000,
            150000000,
            175000000,
            180000000,
            200000000,
            220000000,
            230000000,
            240000000,
            250000000,
            260000000,
            270000000,
            280000000,
            300000000,
            320000000,
            340000000,
            360000000,
            380000000,
            400000000,
            800000000,
            1400000000,
            2200000000,
            6530000000,
            12000000000,
            30000000000,
            75000000000,
            150000000000,
            175000000000,
            300000000000,
            430000000000,
            570000000000,
            700000000000,
            800000000000,
            900000000000,
            3000000000000,
            6000000000000,
            9000000000000,
            13000000000000,
            17000000000000,
            1440000000000,
            1460000000000,
            1490000000000,
            1620000000000,
            1660000000000,
            1720000000000,
            1800000000000,
            1880000000000,
            2000000000000,
        };

        public static List<decimal> OldExperienceList = new List<decimal>
        {
            0, // Lv 0
            100,
            200,
            300,
            400,
            600,
            900,
            1200,
            1700,
            2500,
            6000,
            8000,
            10000,
            15000,
            30000,
            40000,
            50000,
            70000,
            100000,
            120000,
            140000,
            250000,
            300000,
            350000,
            400000,
            500000,
            700000,
            1000000,
            1400000,
            1800000,
            2000000,
            2400000,
            2800000,
            3200000,
            3600000,
            4000000,
            4800000,
            5600000,
            8200000,
            9000000,
            11000000,
            14000000,
            25000000,
            45000000,
            70000000,
            90000000,
            110000000,
            130000000,
            150000000,
            170000000,
            210000000,
            230000000,
            250000000,
            270000000,
            310000000,
            330000000,
            350000000,
            370000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            800000000,
            1400000000,
            2200000000,
            3200000000,
            3600000000,
            4000000000,
            4500000000,
            5000000000,
            15000000000,
            45000000000,
            50000000000,
            55000000000,
            60000000000,
            100000000000,
            120000000000,
            135000000000,
            150000000000,
            170000000000,
            300000000000,
            400000000000,
            440000000000,
            460000000000,
            490000000000,
            620000000000,
            660000000000,
            720000000000,
            800000000000,
            880000000000,
            1000000000000,
        };

        public static List<decimal> WeaponExperienceList = new List<decimal>
        {
            0, //0

            300000,
            350000,
            400000,
            450000,
            500000,
            550000,
            600000,
            650000,
            700000,
            750000, //10

            800000,
            850000,
            900000,
            1000000,
            1300000,
            2000000,
        };

        public static List<decimal> AccessoryExperienceList = new List<decimal>
        {
            0,

            5,
            20,
            80,
            350,
            1500,
            6200,
            26500,
            //114000,
            //490000,
            //2090000,
        };


        public const int InventorySize = 49,
                         EquipmentSize = 16,
                         CompanionInventorySize = 40,
                         CompanionEquipmentSize = 4,
                         EquipmentOffSet = 1000,
                         StorageSize = 100;

        public const int AttackDelay = 1500,
                         ASpeedRate = 47,
                         ProjectileSpeed = 48;

        public static TimeSpan TurnTime = TimeSpan.FromMilliseconds(300),
                               HarvestTime = TimeSpan.FromMilliseconds(600),
                               MoveTime = TimeSpan.FromMilliseconds(600),
                               AttackTime = TimeSpan.FromMilliseconds(600),
                               CastTime = TimeSpan.FromMilliseconds(600),
                               MagicDelay = TimeSpan.FromMilliseconds(2000);


        public static bool RealNameRequired = false,
                           BirthDateRequired = false;

        public static Dictionary<RefineQuality, TimeSpan> RefineTimes = new Dictionary<RefineQuality, TimeSpan>
        {
            [RefineQuality.Rush] = TimeSpan.FromSeconds(1),
            [RefineQuality.Quick] = TimeSpan.FromMinutes(5),
            [RefineQuality.Standard] = TimeSpan.FromMinutes(15),
            [RefineQuality.Careful] = TimeSpan.FromMinutes(30),
            [RefineQuality.Precise] = TimeSpan.FromHours(3),
        };
    }

    public sealed class SelectInfo
    {
        public int CharacterIndex { get; set; }
        public string CharacterName { get; set; }
        public int Level { get; set; }
        public MirGender Gender { get; set; }
        public MirClass Class { get; set; }
        public int Location { get; set; }
        public DateTime LastLogin { get; set; }
    }

    public class ClientUpgradeItem
    {
        public string Key { get; set; }
        public string Hash { get; set; }
        public int Size { get; set; }
    }
}


