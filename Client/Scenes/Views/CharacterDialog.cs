using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.UserModels;
using Library;
using static System.Net.Mime.MediaTypeNames;
using C = Library.Network.ClientPackets;
using System.Data;

//Cleaned
namespace Client.Scenes.Views
{
    public sealed class CharacterDialog : DXWindow
    {
        #region Properties
        private DXTabControl TabControl;
        private DXTab CharacterTab, StatsTab, HermitTab;
    private DXTab AllStatsTab;
    private DXListView AllStatsList;
        public DXLabel CharacterNameLabel, GuildNameLabel, GuildRankLabel;

        public DXImageControl MarriageIcon;

        public DXItemCell[] Grid { get; set; }

        public DXCheckBox ShowHelmetBox;

        public DXLabel WearWeightLabel, HandWeightLabel;
        public Dictionary<Stat, DXLabel> DisplayStats = new Dictionary<Stat, DXLabel>();
        public Dictionary<Stat, DXLabel> AttackStats = new Dictionary<Stat, DXLabel>();
        public Dictionary<Stat, DXLabel> AdvantageStats = new Dictionary<Stat, DXLabel>();
        public Dictionary<Stat, DXLabel> DisadvantageStats = new Dictionary<Stat, DXLabel>();

        public Dictionary<Stat, DXLabel> HermitDisplayStats = new Dictionary<Stat, DXLabel>();
        public Dictionary<Stat, DXLabel> HermitAttackStats = new Dictionary<Stat, DXLabel>();
        public DXLabel RemainingLabel;

        public override WindowType Type => WindowType.CharacterBox;
        public override bool CustomSize => false;
        public override bool AutomaticVisiblity => true;

        #endregion

        public CharacterDialog()
        { 
            HasTitle = false;
            // enlarged width and height
            SetClientSize(new Size(266 + 12, 371 + 24));


            TabControl = new DXTabControl
            {
                Parent = this,
                Location = ClientArea.Location,
                Size = ClientArea.Size,
            };
            CharacterTab = new DXTab
            {
                Parent = TabControl,
                Border = true,
                TabButton = { Label = { Text = "角色" } },
            };
            CharacterTab.BeforeChildrenDraw += CharacterTab_BeforeChildrenDraw;
            StatsTab = new DXTab
            {
                Parent = TabControl,
                Border = true,
                TabButton = { Label = { Text = "属性" } },
            };
            AllStatsTab = new DXTab
            {
                Parent = TabControl,
                Border = true,
                TabButton = { Label = { Text = "全数值" } },
            };
            HermitTab = new DXTab
            {
                Parent = TabControl,
                Border = true,
                TabButton = { Label = { Text = "修炼" } },
            };
            DXControl namePanel = new DXControl
            {
                Parent = CharacterTab,
                Size = new Size(150, 50),
                Border = true,
                BorderColour = Color.FromArgb(198, 166, 99),
                Location = new Point((CharacterTab.Size.Width - 150) / 2, 10),
            };
            CharacterNameLabel = new DXLabel
            {
                AutoSize = false,
                Size = new Size(150, 20),
                ForeColour = Color.White,
                Outline = true,
                OutlineWeight = 2,
                Parent = namePanel,
                Font = new Font("宋体", CEnvir.FontSize(9F), FontStyle.Bold),
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter
            };
            GuildNameLabel = new DXLabel
            {
                AutoSize = false,
                Size = new Size(150, 15),
                ForeColour = Color.FromArgb(255, 255, 181),
                Outline = false,
                Parent = namePanel,
                Location = new Point(0, CharacterNameLabel.Size.Height - 2),
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter
            };
            GuildRankLabel = new DXLabel
            {
                AutoSize = false,
                Size = new Size(150, 15),
                ForeColour = Color.FromArgb(255, 206, 148),
                Outline = false,
                Parent = namePanel,
                Location = new Point(0, CharacterNameLabel.Size.Height+ GuildNameLabel.Size.Height - 4),
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter
            };

            TabControl.SelectedTab = CharacterTab;
            TabControl.SelectedTabChanged += (o, e) =>
            {
                if (TabControl.SelectedTab == AllStatsTab)
                    UpdateAllStatsList();
            };

            // AllStats list setup
            AllStatsList = new DXListView
            {
                Parent = AllStatsTab,
                Location = new Point(0, 0),
                Size = new Size(Math.Max(0, AllStatsTab.Size.Width), Math.Max(0, AllStatsTab.Size.Height)),
                ItemBorder = false,
                ItemHeight = 18,
                Hspac = 3,
                Vspac = 2,
            };
            int col0 = 75;
            int col2 = 100;
            int col1 = Math.Max(50, AllStatsList.Size.Width - col0 - col2 - 10);
            AllStatsList.InsertColumn(0, "Attribute", col0, AllStatsList.ItemHeight);
            AllStatsList.InsertColumn(1, "属性", col1, AllStatsList.ItemHeight);
            AllStatsList.InsertColumn(2, "数值", col2, AllStatsList.ItemHeight);
            AllStatsList.VScrollBar.Value = 0;
            AllStatsList.UpdateViewRect();
            AllStatsTab.SizeChanged += (o, e) =>
            {
                if (AllStatsList == null || AllStatsList.IsDisposed) return;
                AllStatsList.Location = new Point(0, 0);
                AllStatsList.Size = new Size(Math.Max(0, AllStatsTab.Size.Width), Math.Max(0, AllStatsTab.Size.Height));
                // Recalculate column widths
                if (AllStatsList.Headers.Controls.Count >= 3)
                {
                    int c0 = 75;
                    int c2 = 100;
                    int c1 = Math.Max(50, AllStatsList.Size.Width - col0 - col2 - 10);
                    AllStatsList.Headers.Controls[0].Size = new Size(c0, AllStatsList.Headers.Size.Height);
                    AllStatsList.Headers.Controls[1].Size = new Size(c1, AllStatsList.Headers.Size.Height);
                    AllStatsList.Headers.Controls[2].Size = new Size(c2, AllStatsList.Headers.Size.Height);
                }
                AllStatsList.UpdateViewRect();
            };

            MarriageIcon = new DXImageControl
            {
                Parent = namePanel,
                LibraryFile = LibraryFile.GameInter,
                Index = 1298,
                Location = new Point(5, namePanel.Size.Height - 16),
                Visible = false,
            };

            Grid = new DXItemCell[Globals.EquipmentSize];

            DXItemCell cell;
            Grid[(int)EquipmentSlot.Weapon] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36 - 80, CharacterTab.Size.Height - 15 - 36 - 250),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Weapon,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 35);

            Grid[(int)EquipmentSlot.Shield] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36 - 40, CharacterTab.Size.Height - 15 - 36 - 250),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Shield,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 105);

            Grid[(int)EquipmentSlot.Armour] = cell = new DXItemCell
            {
                Location = new Point(15 + 80, CharacterTab.Size.Height - 15 - 36),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Armour,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 34);

            Grid[(int)EquipmentSlot.Shoes] = cell = new DXItemCell
            {
                Location = new Point(15, CharacterTab.Size.Height - 15 - 36),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Shoes,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 36);

            Grid[(int)EquipmentSlot.Torch] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36, CharacterTab.Size.Height - 15 - 36 - 170),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Torch,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 38);

            Grid[(int)EquipmentSlot.Emblem] = cell = new DXItemCell
            {
                Location = new Point(15 + 40, CharacterTab.Size.Height - 15 - 36),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Emblem,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 104);

            Grid[(int)EquipmentSlot.Helmet] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36, CharacterTab.Size.Height - 15 - 36 - 250),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Helmet,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 37);

            Grid[(int)EquipmentSlot.Necklace] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36, CharacterTab.Size.Height - 15 - 36 - 120),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Necklace,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 33);

            Grid[(int)EquipmentSlot.BraceletL] = cell = new DXItemCell
            {
                Location = new Point(15, CharacterTab.Size.Height - 15 - 36 - 80),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.BraceletL,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 32);

            Grid[(int)EquipmentSlot.BraceletR] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36, CharacterTab.Size.Height - 15 - 36 - 80),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.BraceletR,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 32);

            Grid[(int)EquipmentSlot.RingL] = cell = new DXItemCell
            {
                Location = new Point(15, CharacterTab.Size.Height - 15 - 36 - 40),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.RingL,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 31);

            Grid[(int)EquipmentSlot.RingR] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36, CharacterTab.Size.Height - 15 - 36 - 40),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.RingR,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 31);


            Grid[(int)EquipmentSlot.Poison] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36 - 80, CharacterTab.Size.Height - 15 - 36),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Poison,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 40);

            Grid[(int)EquipmentSlot.Amulet] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36 - 40, CharacterTab.Size.Height - 15 - 36),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Amulet,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 39);


            Grid[(int)EquipmentSlot.HorseArmour] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36, CharacterTab.Size.Height - 15 - 36 - 210),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.HorseArmour,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 82);

            Grid[(int)EquipmentSlot.Flower] = cell = new DXItemCell
            {
                Location = new Point(CharacterTab.Size.Width - 15 - 36, CharacterTab.Size.Height - 15 - 36),
                Parent = CharacterTab,
                FixedBorder = true,
                Border = true,
                ItemGrid = GameScene.Game.Equipment,
                Slot = (int)EquipmentSlot.Flower,
                GridType = GridType.Equipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 81);
            ShowHelmetBox = new DXCheckBox
            {
                AutoSize = true,
                Parent = CharacterTab,
                Hint = "显示头盔",
                ReadOnly = true,
            };
            ShowHelmetBox.Location = new Point(CharacterTab.Size.Width - ShowHelmetBox.Size.Width - 13, 57);
            ShowHelmetBox.MouseClick += (o, e) =>
            {
                CEnvir.Enqueue(new C.HelmetToggle { HideHelmet = ShowHelmetBox.Checked });
            };

            int y = 0;
            int offset = 35;
            int base_width = (StatsTab.Size.Width - 50) / 4;
            DXLabel label = new DXLabel
            {
                Parent = StatsTab,
                Text = "物防:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 10);

            DisplayStats[Stat.MaxAC] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "魔防:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.MaxMR] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "破坏:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            DisplayStats[Stat.MaxDC] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "攻速:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.AttackSpeed] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "精神魔攻:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            DisplayStats[Stat.MaxSC] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "自然魔攻:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.MaxMC] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "准确度:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            DisplayStats[Stat.Accuracy] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "敏捷度:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.Agility] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "腕力:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            HandWeightLabel = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "负重:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            WearWeightLabel = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "舒适度:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            DisplayStats[Stat.Comfort] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "生命窃取:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.LifeSteal] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "幸运:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            DisplayStats[Stat.Luck] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "力量:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.Strength] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "暴击率:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            DisplayStats[Stat.CriticalChance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "暴伤(PvE):"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.CriticalDamage] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "捡拾范围:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            DisplayStats[Stat.PickUpRadius] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "经验增益:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.ExperienceRate] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "金币增益:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, y += 20);

            DisplayStats[Stat.GoldRate] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "掉率增益:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, y);

            DisplayStats[Stat.DropRate] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, y),
                ForeColour = Color.White,
                Text = "0"
            };


            #region Attack


            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "攻击元素:"
            };
            label.Location = new Point(70 - label.Size.Width, 190 + 25);

            DXImageControl icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 600,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     火",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            AttackStats[Stat.FireAttack] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };
            
            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 601,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     冰",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 50, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            AttackStats[Stat.IceAttack] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 602,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     雷",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 100, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            AttackStats[Stat.LightningAttack] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 603,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     风",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 150, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            AttackStats[Stat.WindAttack] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 604,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     神圣",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            AttackStats[Stat.HolyAttack] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 605,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     暗黑",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 50, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            AttackStats[Stat.DarkAttack] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 606,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     幻影",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 100, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            AttackStats[Stat.PhantomAttack] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            #endregion

            #region Resistance


            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "元素抵抗:"
            };
            label.Location = new Point(70  - label.Size.Width, 245 + 25);


            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 600,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     火",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            AdvantageStats[Stat.FireResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 601,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     冰",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 50, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            AdvantageStats[Stat.IceResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 602,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     雷",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 100, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            AdvantageStats[Stat.LightningResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 603,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     风",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 150, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            AdvantageStats[Stat.WindResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 604,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     神圣",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            AdvantageStats[Stat.HolyResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 605,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     暗黑",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 50, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            AdvantageStats[Stat.DarkResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 606,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     幻影",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 100, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            AdvantageStats[Stat.PhantomResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.GameInter,
                Index = 1517,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     物理",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 150, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            AdvantageStats[Stat.PhysicalResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            #endregion

            #region Resistance


            label = new DXLabel
            {
                Parent = StatsTab,
                Text = "元素畏惧:"
            };
            label.Location = new Point(70 - label.Size.Width, 300 + 25);

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 600,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     火",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            DisadvantageStats[Stat.FireResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 601,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     冰",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 50, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            DisadvantageStats[Stat.IceResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 602,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     雷",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 100, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            DisadvantageStats[Stat.LightningResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 603,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     风",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 150, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            DisadvantageStats[Stat.WindResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 604,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     神圣",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            DisadvantageStats[Stat.HolyResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 605,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     暗黑",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 50, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            DisadvantageStats[Stat.DarkResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 606,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     幻影",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 100, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            DisadvantageStats[Stat.PhantomResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text ="0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = StatsTab,
                LibraryFile = LibraryFile.GameInter,
                Index = 1517,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     物理",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 150, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            DisadvantageStats[Stat.PhysicalResistance] = new DXLabel
            {
                Parent = StatsTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            #endregion

            base_width = (HermitTab.Size.Width - 50) / 4;

            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "物防:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, 15);

            HermitDisplayStats[Stat.MaxAC] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, label.Location.Y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "魔防:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, 15);

            HermitDisplayStats[Stat.MaxMR] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, label.Location.Y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "破坏:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, 35);

            HermitDisplayStats[Stat.MaxDC] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, label.Location.Y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "自然魔攻:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, 55);

            HermitDisplayStats[Stat.MaxMC] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, label.Location.Y),
                ForeColour = Color.White,
                Text = "0-0"
            };

            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "精神魔攻:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, 55);

            HermitDisplayStats[Stat.MaxSC] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, label.Location.Y),
                ForeColour = Color.White,
                Text = "0-0"
            };
            
            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "生命:"
            };
            label.Location = new Point(offset + base_width - label.Size.Width, 75);

            HermitDisplayStats[Stat.Health] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, label.Location.Y),
                ForeColour = Color.White,
                Text = "0"
            };

            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "魔法:"
            };
            label.Location = new Point(offset + base_width * 3 - label.Size.Width, 75);

            HermitDisplayStats[Stat.Mana] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, label.Location.Y),
                ForeColour = Color.White,
                Text = "0"
            };


            #region Attack
            
            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "元素攻击:"
            };
            label.Location = new Point(70 - label.Size.Width, 90 + 20);

            icon = new DXImageControl
            {
                Parent = HermitTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 600,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     火",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            HermitAttackStats[Stat.FireAttack] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = HermitTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 601,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     冰",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 50, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            HermitAttackStats[Stat.IceAttack] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = HermitTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 602,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     雷",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 100, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            HermitAttackStats[Stat.LightningAttack] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = HermitTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 603,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     风",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 150, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2);

            HermitAttackStats[Stat.WindAttack] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = HermitTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 604,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     神圣",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            HermitAttackStats[Stat.HolyAttack] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = HermitTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 605,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     暗黑",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 50, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            HermitAttackStats[Stat.DarkAttack] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            icon = new DXImageControl
            {
                Parent = HermitTab,
                LibraryFile = LibraryFile.ProgUse,
                Index = 606,
                ForeColour = Color.FromArgb(60, 60, 60),
                Hint = "     幻影",
            };
            icon.Location = new Point(label.Location.X + label.Size.Width + 100, label.Location.Y + (label.Size.Height - icon.Size.Height) / 2 + 25);

            HermitAttackStats[Stat.PhantomAttack] = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(icon.Location.X + icon.Size.Width, label.Location.Y + 25),
                ForeColour = Color.FromArgb(60, 60, 60),
                Text = "0",
                Tag = icon,
            };

            #endregion


            label = new DXLabel
            {
                Parent = HermitTab,
                Text = "未使用修炼点:"
            };
            label.Location = new Point(HermitTab.Size.Width / 2 - label.Size.Width / 2, 150 + 20);

            RemainingLabel = new DXLabel
            {
                Parent = HermitTab,
                Location = new Point(label.Location.X + label.Size.Width - 5, label.Location.Y),
                ForeColour = Color.White,
                Text = "0"
            };

            DXCheckBox check = new DXCheckBox
            {
                AutoSize = true,
                Parent = HermitTab,
                Text = "显示确认窗口",
                Checked = true,
            };
            check.Location = new Point(HermitTab.Size.Width - check.Size.Width - 5, HermitTab.Size.Height - check.Size.Height - 10);

            DXButton but = new DXButton
            {
                Parent = HermitTab,
                Location = new Point(50, 180 + 20),
                Label = { Text = "物防" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight)
            };
            but.MouseClick += (o, e) =>
            {
                if (MapObject.User.HermitPoints == 0) return;

                if (check.Checked)
                {
                    DXMessageBox box = new DXMessageBox("确定要提升物理防御?", "修炼确认", DXMessageBoxButtons.YesNo);

                    box.YesButton.MouseClick += (o1, e1) =>
                    {
                        CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxAC });
                    };
                }
                else
                {
                    CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxAC });
                }
            };

            but = new DXButton
            {
                Parent = HermitTab,
                Location = new Point(150, but.Location.Y),
                Label = { Text = "魔防" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight),
            };
            but.MouseClick += (o, e) =>
            {
                if (MapObject.User.HermitPoints == 0) return;

                if (check.Checked)
                {
                    DXMessageBox box = new DXMessageBox("确认要提升魔法防御力?", "修炼确认", DXMessageBoxButtons.YesNo);

                    box.YesButton.MouseClick += (o1, e1) =>
                    {
                        CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxMR });
                    };
                }
                else
                {
                    CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxMR });
                }
            };

            but = new DXButton
            {
                Parent = HermitTab,
                Location = new Point(50, but.Location.Y + 25),
                Label = { Text = "生命" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight)
            };
            but.MouseClick += (o, e) =>
            {
                if (MapObject.User.HermitPoints == 0) return;

                if (check.Checked)
                {
                    DXMessageBox box = new DXMessageBox("确认要提升生命值?", "修炼确认", DXMessageBoxButtons.YesNo);

                    box.YesButton.MouseClick += (o1, e1) =>
                    {
                        CEnvir.Enqueue(new C.Hermit { Stat = Stat.Health });
                    };
                }
                else
                {
                    CEnvir.Enqueue(new C.Hermit { Stat = Stat.Health });
                }
            };

            but = new DXButton
            {
                Parent = HermitTab,
                Location = new Point(150, but.Location.Y ),
                Label = { Text = "魔法" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight)
            };
            but.MouseClick += (o, e) =>
            {
                if (MapObject.User.HermitPoints == 0) return;

                if (check.Checked)
                {
                    DXMessageBox box = new DXMessageBox("确认要提升魔法量?", "修炼确认", DXMessageBoxButtons.YesNo);

                    box.YesButton.MouseClick += (o1, e1) =>
                    {
                        CEnvir.Enqueue(new C.Hermit { Stat = Stat.Mana });
                    };
                }
                else
                {
                    CEnvir.Enqueue(new C.Hermit { Stat = Stat.Mana });
                }
            };


            but = new DXButton
            {
                Parent = HermitTab,
                Location = new Point(100, but.Location.Y + 25),
                Label = { Text = "破坏" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight)
            };
            but.MouseClick += (o, e) =>
            {
                if (MapObject.User.HermitPoints == 0) return;

                if (check.Checked)
                {
                    DXMessageBox box = new DXMessageBox("确认要提升物理破坏力?", "修炼确认", DXMessageBoxButtons.YesNo);

                    box.YesButton.MouseClick += (o1, e1) =>
                    {
                        CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxDC });
                    };
                }
                else
                {
                    CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxDC });
                }
            };

            but = new DXButton
            {
                Parent = HermitTab,
                Location = new Point(50, but.Location.Y + 25),
                Label = { Text = "自然魔攻" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight)
            };
            but.MouseClick += (o, e) =>
            {
                if (MapObject.User.HermitPoints == 0) return;

                if (check.Checked)
                {
                    DXMessageBox box = new DXMessageBox("确认要提升自然魔法攻击力?", "修炼确认", DXMessageBoxButtons.YesNo);

                    box.YesButton.MouseClick += (o1, e1) =>
                    {
                        CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxMC });
                    };
                }
                else
                {
                    CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxMC });
                }
            };

            but = new DXButton
            {
                Parent = HermitTab,
                Location = new Point(150, but.Location.Y ),
                Label = { Text = "精神魔攻" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight)
            };
            but.MouseClick += (o, e) =>
            {
                if (MapObject.User.HermitPoints == 0) return;

                if (check.Checked)
                {
                    DXMessageBox box = new DXMessageBox("确认要提升精神魔法攻击力?", "修炼确认", DXMessageBoxButtons.YesNo);

                    box.YesButton.MouseClick += (o1, e1) =>
                    {
                        CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxSC });
                    };
                }
                else
                {
                    CEnvir.Enqueue(new C.Hermit { Stat = Stat.MaxSC });
                }
            };


            but = new DXButton
            {
                Parent = HermitTab,
                Location = new Point(100, but.Location.Y + 25),
                Label = { Text = "元素" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight)
            };
            but.MouseClick += (o, e) =>
            {
                if (MapObject.User.HermitPoints == 0) return;

                if (check.Checked)
                {
                    DXMessageBox box = new DXMessageBox("确认要提升你的元素攻击力?", "修炼确认", DXMessageBoxButtons.YesNo);

                    box.YesButton.MouseClick += (o1, e1) =>
                    {
                        CEnvir.Enqueue(new C.Hermit { Stat = Stat.WeaponElement });
                    };
                }
                else
                {
                    CEnvir.Enqueue(new C.Hermit { Stat = Stat.WeaponElement });
                }
            };


        }

        #region Methods
        public void Draw(DXItemCell cell, int index)
        {
            if (InterfaceLibrary == null) return;

            if (cell.Item != null) return;

            Size s = InterfaceLibrary.GetSize(index);
            int x = (cell.Size.Width - s.Width) / 2 + cell.DisplayArea.X;
            int y = (cell.Size.Height - s.Height) / 2 + cell.DisplayArea.Y;

            InterfaceLibrary.Draw(index, x, y, Color.White, false, 0.2F, ImageType.Image);
        }
        
        private void CharacterTab_BeforeChildrenDraw(object sender, EventArgs e)
        {
            MirLibrary library;

            int x = 120;
            int y = 270 + 29;

            if (!CEnvir.LibraryList.TryGetValue(LibraryFile.Equip, out library)) return;

            if (Grid[(int)EquipmentSlot.Armour].Item != null)
            {
                int index = Grid[(int)EquipmentSlot.Armour].Item.Info.Image;

                MirLibrary effectLibrary;

                if (CEnvir.LibraryList.TryGetValue(LibraryFile.EquipEffect_UI, out effectLibrary))
                {
                    MirImage image = null;
                    switch (index)
                    {
                        //All
                        case 962:
                            image = effectLibrary.CreateImage(1700 + (GameScene.Game.MapControl.Animation % 10), ImageType.Image);
                            break;
                        case 972:
                            image = effectLibrary.CreateImage(1720 + (GameScene.Game.MapControl.Animation % 10), ImageType.Image);
                            break;

                        //War
                        case 963:
                            image = effectLibrary.CreateImage(400 + (GameScene.Game.MapControl.Animation % 15), ImageType.Image);
                            break;
                        case 973:
                            image = effectLibrary.CreateImage(420 + (GameScene.Game.MapControl.Animation % 15), ImageType.Image);
                            break;

                        //Wiz
                        case 964:
                            image = effectLibrary.CreateImage(300 + (GameScene.Game.MapControl.Animation % 15), ImageType.Image);
                            break;
                        case 974:
                            image = effectLibrary.CreateImage(320 + (GameScene.Game.MapControl.Animation % 15), ImageType.Image);
                            break;

                        //Tao
                        case 965:
                            image = effectLibrary.CreateImage(200 + (GameScene.Game.MapControl.Animation % 15), ImageType.Image);
                            break;
                        case 975:
                            image = effectLibrary.CreateImage(220 + (GameScene.Game.MapControl.Animation % 15), ImageType.Image);
                            break;

                        //Ass
                        case 2007:
                            image = effectLibrary.CreateImage(500 + (GameScene.Game.MapControl.Animation % 13), ImageType.Image);
                            break;
                        case 2017:
                            image = effectLibrary.CreateImage(520 + (GameScene.Game.MapControl.Animation % 13), ImageType.Image);
                            break;

                        case 942:
                            image = effectLibrary.CreateImage(700, ImageType.Image);
                            break;
                        case 961:
                            image = effectLibrary.CreateImage(1600, ImageType.Image);
                            break;
                        case 982:
                            image = effectLibrary.CreateImage(800, ImageType.Image);
                            break;
                        case 983:
                            image = effectLibrary.CreateImage(1200, ImageType.Image);
                            break;
                        case 984:
                            image = effectLibrary.CreateImage(1100, ImageType.Image);
                            break;
                        case 1022:
                            image = effectLibrary.CreateImage(900, ImageType.Image);
                            break;
                        case 1023:
                            image = effectLibrary.CreateImage(1300, ImageType.Image);
                            break;
                        case 1002:
                            image = effectLibrary.CreateImage(1000, ImageType.Image);
                            break;
                        case 1003:
                            image = effectLibrary.CreateImage(1400, ImageType.Image);
                            break;

                        case 952:
                            image = effectLibrary.CreateImage(720, ImageType.Image);
                            break;
                        case 971:
                            image = effectLibrary.CreateImage(1620, ImageType.Image);
                            break;
                        case 992:
                            image = effectLibrary.CreateImage(820, ImageType.Image);
                            break;
                        case 993:
                            image = effectLibrary.CreateImage(1220, ImageType.Image);
                            break;
                        case 994:
                            image = effectLibrary.CreateImage(1120, ImageType.Image);
                            break;
                        case 1032:
                            image = effectLibrary.CreateImage(920, ImageType.Image);
                            break;
                        case 1033:
                            image = effectLibrary.CreateImage(1320, ImageType.Image);
                            break;
                        case 1012:
                            image = effectLibrary.CreateImage(1020, ImageType.Image);
                            break;
                        case 1013:
                            image = effectLibrary.CreateImage(1420, ImageType.Image);
                            break;
                    }
                    if (image != null)
                    {

                        bool oldBlend = DXManager.Blending;
                        float oldRate = DXManager.BlendRate;

                        DXManager.SetBlend(true, 0.8F);

                        PresentTexture(image.Image, CharacterTab, new Rectangle(DisplayArea.X + x + image.OffSetX, DisplayArea.Y + y + image.OffSetY, image.Width, image.Height), ForeColour, this);

                        DXManager.SetBlend(oldBlend, oldRate);
                    }
                }
            }

            if (!CEnvir.LibraryList.TryGetValue(LibraryFile.ProgUse, out library)) return;

            if (MapObject.User.Class == MirClass.Assassin && MapObject.User.Gender == MirGender.Female && MapObject.User.HairType == 1 && Grid[(int) EquipmentSlot.Helmet].Item == null)
                library.Draw(1160, DisplayArea.X + x, DisplayArea.Y + y, MapObject.User.HairColour, true, 1F, ImageType.Image);

            switch (MapObject.User.Gender)
            {
                case MirGender.Male:
                    library.Draw(0, DisplayArea.X + x, DisplayArea.Y + y, Color.White, true, 1F, ImageType.Image);
                    break;
                case MirGender.Female:
                    library.Draw(1, DisplayArea.X + x, DisplayArea.Y + y, Color.White, true, 1F, ImageType.Image);
                    break;
            }


            if (CEnvir.LibraryList.TryGetValue(LibraryFile.Equip, out library))
            {
                if (Grid[(int) EquipmentSlot.Armour].Item != null)
                {
                    int index = Grid[(int) EquipmentSlot.Armour].Item.Info.Image;

                    MirLibrary effectLibrary;

                    if (CEnvir.LibraryList.TryGetValue(LibraryFile.EquipEffect_UI, out effectLibrary))
                    {
                        switch (index)
                        {
                            case 942:
                                effectLibrary.Draw(700, DisplayArea.X + x, DisplayArea.Y + y, Color.White, true, 1F, ImageType.Image);
                                break;
                            case 952:
                                effectLibrary.Draw(720, DisplayArea.X + x, DisplayArea.Y + y, Color.White, true, 1F, ImageType.Image);
                                break;
                        }
                    }



                    library.Draw(index, DisplayArea.X + x, DisplayArea.Y + y, Color.White, true, 1F, ImageType.Image);
                    library.Draw(index, DisplayArea.X + x, DisplayArea.Y + y, Grid[(int) EquipmentSlot.Armour].Item.Colour, true, 1F, ImageType.Overlay);
                }

                if (Grid[(int)EquipmentSlot.Weapon].Item != null)
                {
                    int index = Grid[(int)EquipmentSlot.Weapon].Item.Info.Image;

                    library.Draw(index, DisplayArea.X + x, DisplayArea.Y + y, Color.White, true, 1F, ImageType.Image);
                    library.Draw(index, DisplayArea.X + x, DisplayArea.Y + y, Grid[(int)EquipmentSlot.Weapon].Item.Colour, true, 1F, ImageType.Overlay);

                    if (Grid[(int)EquipmentSlot.Weapon].Item.Info.Rarity == Rarity.Elite && CEnvir.LibraryList.TryGetValue(LibraryFile.EquipEffect_UI, out var effectLibrary))
                    {
                        MirImage image = null;

                        switch (index)
                        {
                            case 2529:
                            case 2530:
                                image = effectLibrary.CreateImage(1900 + (GameScene.Game.MapControl.Animation % 12), ImageType.Image);
                                break;
                            case 1075:
                            case 1076:
                                image = effectLibrary.CreateImage(2000 + (GameScene.Game.MapControl.Animation % 10), ImageType.Image);
                                break;
                            case 3422:
                                image = effectLibrary.CreateImage(3183 + (GameScene.Game.MapControl.Animation % 30), ImageType.Image);
                                break;
                            default:
                                //image = effectLibrary.CreateImage(1932 + (GameScene.Game.MapControl.Animation % 30), ImageType.Image);
                                break;
                        }

                        if (image != null)
                        {

                            bool oldBlend = DXManager.Blending;
                            float oldRate = DXManager.BlendRate;
                            DXManager.SetBlend(true, 0.8F);
                            PresentTexture(image.Image, CharacterTab, new Rectangle(DisplayArea.X + x + image.OffSetX, DisplayArea.Y + y + image.OffSetY, image.Width, image.Height), ForeColour, this);
                            DXManager.SetBlend(oldBlend, oldRate);
                        }
                    }
                }
                if (Grid[(int)EquipmentSlot.Shield].Item != null)
                {
                    int index = Grid[(int)EquipmentSlot.Shield].Item.Info.Image;
                    library.Draw(index, DisplayArea.X + x, DisplayArea.Y + y, Color.White, true, 1F, ImageType.Image);
                    library.Draw(index, DisplayArea.X + x, DisplayArea.Y + y, Grid[(int)EquipmentSlot.Shield].Item.Colour, true, 1F, ImageType.Overlay);
                }
            }


            if (Grid[(int) EquipmentSlot.Helmet].Item != null && library != null)
            {
                int index = Grid[(int) EquipmentSlot.Helmet].Item.Info.Image;

                library.Draw(index, DisplayArea.X + x, DisplayArea.Y + y, Color.White, true, 1F, ImageType.Image);
                //library.Draw(index, DisplayArea.X + x, DisplayArea.Y + y, Grid[(int) EquipmentSlot.Helmet].Item.Colour, true, 1F, ImageType.Overlay);
            }
            else if (MapObject.User.HairType > 0)
            {
                library = CEnvir.LibraryList[LibraryFile.ProgUse];

                switch (MapObject.User.Class)
                {
                    case MirClass.Warrior:
                    case MirClass.Wizard:
                    case MirClass.Taoist:
                        switch (MapObject.User.Gender)
                        {
                            case MirGender.Male:
                                library.Draw(60 + MapObject.User.HairType - 1, DisplayArea.X + x, DisplayArea.Y + y, MapObject.User.HairColour, true, 1F, ImageType.Image);
                                break;
                            case MirGender.Female:
                                library.Draw(80 + MapObject.User.HairType - 1, DisplayArea.X + x, DisplayArea.Y + y, MapObject.User.HairColour, true, 1F, ImageType.Image);
                                break;
                        }
                        break;
                    case MirClass.Assassin:
                        switch (MapObject.User.Gender)
                        {
                            case MirGender.Male:
                                library.Draw(1100 + MapObject.User.HairType - 1, DisplayArea.X + x, DisplayArea.Y + y, MapObject.User.HairColour, true, 1F, ImageType.Image);
                                break;
                            case MirGender.Female:
                                library.Draw(1120 + MapObject.User.HairType - 1, DisplayArea.X + x, DisplayArea.Y + y, MapObject.User.HairColour, true, 1F, ImageType.Image);
                                break;
                        }
                        break;
                }
            }
        }

        public void UpdateStats()
        {
            foreach (KeyValuePair<Stat, DXLabel> pair in DisplayStats)
                pair.Value.Text = MapObject.User.Stats.GetFormat(pair.Key);

            
            foreach (KeyValuePair<Stat, DXLabel> pair in AttackStats)
            {

                if (MapObject.User.Stats[pair.Key] > 0)
                {
                    pair.Value.Text = $"+{MapObject.User.Stats[pair.Key]}";
                    pair.Value.ForeColour = Color.DeepSkyBlue;
                    ((DXImageControl)pair.Value.Tag).ForeColour = Color.White;
                }
                else
                {
                    pair.Value.Text = "0";
                    pair.Value.ForeColour = Color.FromArgb(60, 60, 60);
                    ((DXImageControl)pair.Value.Tag).ForeColour = Color.FromArgb(60, 60, 60);
                }
            }

            foreach (KeyValuePair<Stat, DXLabel> pair in AdvantageStats)
            {
                if (MapObject.User.Stats[pair.Key] > 0)
                {
                    pair.Value.Text = $"x{MapObject.User.Stats[pair.Key]}";
                    pair.Value.ForeColour = Color.Lime;
                    ((DXImageControl)pair.Value.Tag).ForeColour = Color.White;
                }
                else
                {
                    pair.Value.Text = "0";
                    pair.Value.ForeColour = Color.FromArgb(60, 60, 60);
                    ((DXImageControl)pair.Value.Tag).ForeColour = Color.FromArgb(60, 60, 60);
                }
            }

            foreach (KeyValuePair<Stat, DXLabel> pair in DisadvantageStats)
            {
                pair.Value.Text = MapObject.User.Stats.GetFormat(pair.Key);

                if (MapObject.User.Stats[pair.Key] < 0)
                {
                    pair.Value.Text = $"x{Math.Abs(MapObject.User.Stats[pair.Key])}";
                    pair.Value.ForeColour = Color.IndianRed;
                    ((DXImageControl)pair.Value.Tag).ForeColour = Color.White;
                }
                else
                {
                    pair.Value.Text = "0";
                    pair.Value.ForeColour = Color.FromArgb(60, 60, 60);
                    ((DXImageControl)pair.Value.Tag).ForeColour = Color.FromArgb(60, 60, 60);
                }
            }


            foreach (KeyValuePair<Stat, DXLabel> pair in HermitDisplayStats)
                pair.Value.Text = MapObject.User.HermitStats.GetFormat(pair.Key);


            foreach (KeyValuePair<Stat, DXLabel> pair in HermitAttackStats)
            {

                if (MapObject.User.HermitStats[pair.Key] > 0)
                {
                    pair.Value.Text = $"+{MapObject.User.HermitStats[pair.Key]}";
                    pair.Value.ForeColour = Color.White;
                    ((DXImageControl)pair.Value.Tag).ForeColour = Color.White;
                }
                else
                {
                    pair.Value.Text = "0";
                    pair.Value.ForeColour = Color.FromArgb(60, 60, 60);
                    ((DXImageControl)pair.Value.Tag).ForeColour = Color.FromArgb(60, 60, 60);
                }
            }

            RemainingLabel.Text = MapObject.User.HermitPoints.ToString();


        }
        private readonly (string Title, Stat[] Stats)[] _statSections = new[]
        {
            ("基础数值", new[] {
                Stat.Health,
                Stat.Mana,
                Stat.BaseHealth,
                Stat.BaseMana,
                Stat.MaxAC,
                Stat.MaxMR,
                Stat.MaxDC,
                Stat.MaxMC,
                Stat.MaxSC,
                Stat.AttackSpeed,
                Stat.Agility,
                Stat.Accuracy,
                Stat.Comfort,
                Stat.BagWeight,
                Stat.WearWeight,
                Stat.HandWeight,
            }),
            ("进阶属性", new[] {
                Stat.Luck,
                Stat.Strength,
                Stat.CriticalChance,
                Stat.CriticalDamage,
                Stat.LifeSteal,
                Stat.PhysicalResistance,
                Stat.ReflectDamage,
                Stat.ParalysisChance,
                Stat.SlowChance,
                Stat.SilenceChance,
                Stat.DamageAdd,
                Stat.DamageReduction,
            }),
            ("收益加成", new[] {
                Stat.ExperienceRate,
                Stat.GoldRate,
                Stat.DropRate,
                Stat.SkillRate
            }),
            ("属性加成", new[] {
                Stat.HealthPercent,
                Stat.ManaPercent,
                Stat.DCPercent,
                Stat.SCPercent,
                Stat.MCPercent,
                Stat.PetHPPercent,
                Stat.PetDCPercent
            }),
            ("其他属性", new[] {
                Stat.PickUpRadius,
                Stat.MagicSpeed,
                Stat.CritReduction,
                Stat.JMCriticalDamage,
                Stat.BlockChance,
                Stat.EvasionChance,
                Stat.PoisonResistance,
                Stat.ProtectionRing,
                Stat.JudgementOfHeaven,
                Stat.Rebirth
            }),
        };

        private void UpdateAllStatsList()
        {
            if (AllStatsList == null || AllStatsList.IsDisposed) return;

            AllStatsList.VScrollBar.Value = 0;
            AllStatsList.RemoveAll();

            for (int i = 0; i < _statSections.Length; i++)
            {
                var section = _statSections[i];
                // Insert section title with custom styling
                uint titleRow = AllStatsList.InsertItem(AllStatsList.ItemCount, "");
                
                // Create custom title label with styling
                DXLabel titleLabel = new DXLabel
                {
                    Text = $"{i + 1}. {section.Title}",
                    ForeColour = Color.FromArgb(122, 103, 61), // Darker brown color
                    AutoSize = false,
                };
                
                AllStatsList.SetItem(titleRow, 0, titleLabel);
                AllStatsList.SetItem(titleRow, 1, "");
                AllStatsList.SetItem(titleRow, 2, "");

                // Insert each stat in the section
                foreach (Stat stat in section.Stats)
                {
                    string value = null;
                    // col0: enum name (Stat), col1: display title (StatDescription if present)
                    string enumName = stat.ToString();
                    string displayName = enumName;
                    var mi = typeof(Stat).GetMember(stat.ToString())[0];
                    var desc = mi.GetCustomAttribute<StatDescription>();
                    if (desc != null && !string.IsNullOrEmpty(desc.Title))
                        displayName = desc.Title;

                    // Use user's stats formatting where applicable
                    if (MapObject.User?.Stats != null)
                        value = MapObject.User.Stats.GetFormat(stat) ?? MapObject.User.Stats[stat].ToString();
                    else
                        value = "0";

                    uint row = AllStatsList.InsertItem(AllStatsList.ItemCount, "");
                    
                    // Set first column with hint
                    DXControl col0Control = AllStatsList.SetItem(row, 0, enumName);
                    if (col0Control is DXLabel col0Label)
                    {
                        col0Label.Hint = enumName;
                    }
                    
                    // Set second column to right align
                    DXControl col1Control = AllStatsList.SetItem(row, 1, displayName);
                    if (col1Control is DXLabel col1Label)
                    {
                        col1Label.DrawFormat = TextFormatFlags.Right;
                    }
                    
                    // Set third column to white color for numeric values
                    DXControl col2Control = AllStatsList.SetItem(row, 2, value);
                    if (col2Control is DXLabel col2Label)
                    {
                        col2Label.ForeColour = Color.White;
                    }
                }
            }

            AllStatsList.UpdateScrollBar();
            AllStatsList.UpdateItems();
        }
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (TabControl != null)
                {
                    if (!TabControl.IsDisposed)
                        TabControl.Dispose();

                    TabControl = null;
                }

                if (CharacterTab != null)
                {
                    if (!CharacterTab.IsDisposed)
                        CharacterTab.Dispose();

                    CharacterTab = null;
                }

                if (StatsTab != null)
                {
                    if (!StatsTab.IsDisposed)
                        StatsTab.Dispose();

                    StatsTab = null;
                }

                if (HermitTab != null)
                {
                    if (!HermitTab.IsDisposed)
                        HermitTab.Dispose();

                    HermitTab = null;
                }

                if (CharacterNameLabel != null)
                {
                    if (!CharacterNameLabel.IsDisposed)
                        CharacterNameLabel.Dispose();

                    CharacterNameLabel = null;
                }

                if (GuildNameLabel != null)
                {
                    if (!GuildNameLabel.IsDisposed)
                        GuildNameLabel.Dispose();

                    GuildNameLabel = null;
                }

                if (GuildRankLabel != null)
                {
                    if (!GuildRankLabel.IsDisposed)
                        GuildRankLabel.Dispose();

                    GuildRankLabel = null;
                }

                if (MarriageIcon != null)
                {
                    if (!MarriageIcon.IsDisposed)
                        MarriageIcon.Dispose();

                    MarriageIcon = null;
                }

                if (Grid != null)
                {
                    for (int i = 0; i < Grid.Length; i++)
                    {
                        if (Grid[i] != null)
                        {
                            if (!Grid[i].IsDisposed)
                                Grid[i].Dispose();

                            Grid[i] = null;
                        }
                    }

                    Grid = null;
                }

                if (WearWeightLabel != null)
                {
                    if (!WearWeightLabel.IsDisposed)
                        WearWeightLabel.Dispose();

                    WearWeightLabel = null;
                }

                if (HandWeightLabel != null)
                {
                    if (!HandWeightLabel.IsDisposed)
                        HandWeightLabel.Dispose();

                    HandWeightLabel = null;
                }

                foreach (KeyValuePair<Stat, DXLabel> pair in DisplayStats)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                DisplayStats.Clear();
                DisplayStats = null;

                foreach (KeyValuePair<Stat, DXLabel> pair in AttackStats)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                AttackStats.Clear();
                AttackStats = null;

                foreach (KeyValuePair<Stat, DXLabel> pair in AdvantageStats)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                AdvantageStats.Clear();
                AdvantageStats = null;

                foreach (KeyValuePair<Stat, DXLabel> pair in DisadvantageStats)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                DisadvantageStats.Clear();
                DisadvantageStats = null;

                foreach (KeyValuePair<Stat, DXLabel> pair in HermitDisplayStats)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                HermitDisplayStats.Clear();
                HermitDisplayStats = null;

                foreach (KeyValuePair<Stat, DXLabel> pair in HermitAttackStats)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                HermitAttackStats.Clear();
                HermitAttackStats = null;

                if (RemainingLabel != null)
                {
                    if (!RemainingLabel.IsDisposed)
                        RemainingLabel.Dispose();

                    RemainingLabel = null;
                }

            }

        }

        #endregion
    }
}
