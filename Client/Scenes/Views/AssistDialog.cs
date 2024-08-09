using System;
using System.Drawing;
using System.Linq;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.UserModels;
using Library;
using Library.SystemModels;
using System.Collections.Generic;

using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Scenes.Views
{

    public sealed class AssistDialog : DXWindow
    {
        public override bool AutomaticVisiblity { get { return true; } }
        public override bool CustomSize { get { return false; } }
        public override WindowType Type { get { return WindowType.AssistDialog; } }

        private DXTabControl tabControl;

        //自动喝药
        private DXTab tabPotion;

        //自动捡拾
        private DXTab tabPicker;
        private DXVScrollBar VScrolPicker;
        private DXCheckBox chkPickerGold;
        private DXCheckBox chkPickerHeal;
        private DXCheckBox chkPickerMana;
        private DXCheckBox chkPickerArmour;
        private DXCheckBox chkPickerHelmet;


        //自动技能
        private DXTab tabSpell;
        private DXComboBox ComboShield;
        private DXComboBox ComboPractice;
        private DXNumberBox AutoPracticeInterval;

        private readonly string[] AutoSpellList = { "魔法盾", "神圣战甲术", "阴阳法环"};

        private readonly string[] AutoPracticeList = {
            "野蛮冲撞", "烈火剑法","翔空剑法","莲月剑法","乾坤大挪移","铁布杉","斗转星移","破血狂杀"
            ,"抗拒火环", "诱惑之光","瞬息移动","火墙","圣言术","异形换位","旋风墙"
            ,"治愈术","施毒术","隐身术","集体隐身术","神圣战甲术","困魔咒","群体治愈术","云寂术","妙影无踪","吸星大法"
            , "潜行"};
        

        private static DateTime TimeSpellTime;
        private static DateTime TimePractice;


        public AssistDialog()
        {
            TimeSpellTime = CEnvir.Now;
            TimePractice = CEnvir.Now;
            Size = new Size(280, 280);
            TitleLabel.Text = "辅助功能设置";
            HasFooter = true;

            tabControl = new DXTabControl
            {
                Parent = this,
                Location = ClientArea.Location,
                Size = ClientArea.Size,
            };
            Init();
        }

        public void Init()
        {
            _InitPicker();
            _InitPotion();
            _InitSpell();
        }

        private void _InitSpell()
        {
            if (tabSpell != null)
                tabSpell.Dispose();

            int XAlign = 100;
            int YAlign = 35;


            #region 自动保持状态
            tabSpell = new DXTab
            {
                Parent = tabControl,
                Border = true,
                TabButton = { Label = { Text = "技能" } },
            };

            DXLabel label1 = new DXLabel
            {
                Text = "保持技能状态:",
                Outline = true,
                Parent = tabSpell,
            };
            label1.Location = new Point(XAlign - label1.Size.Width, YAlign);

            ComboShield = new DXComboBox
            {
                Parent = tabSpell,
                Location = new Point(XAlign, YAlign),
                Size = new Size(100, DXComboBox.DefaultNormalHeight),
            };

            YAlign += 30;

            foreach (string tmpspell in AutoSpellList)
            {
                MagicInfo tmpMagic = Globals.MagicInfoList.Binding.FirstOrDefault(m => m.Name == tmpspell);
                if (tmpMagic != null)
                {
                    if (tmpMagic.Class != MapObject.User?.Class || tmpMagic.School == MagicSchool.None)
                        continue;

                    ClientUserMagic tmpCMagic;
                    if (MapObject.User.Magics.TryGetValue(tmpMagic, out tmpCMagic))
                    {
                        
                        new DXListBoxItem
                        {
                            Parent = ComboShield.ListBox,
                            Label = { Text = tmpMagic.Name },
                            Item = tmpMagic
                        };
                    }

                }

            }

            if (ComboShield.ListBox.Controls.Count > 1)
                ComboShield.ListBox.SelectItem(1);
            #endregion

            #region 自动练习
            DXLabel label2 = new DXLabel
            {
                Text = "施放间隔:",
                Outline = true,
                Parent = tabSpell,
            };
            label2.Location = new Point(XAlign - label2.Size.Width, YAlign);

            AutoPracticeInterval = new DXNumberBox
            {
                Parent = tabSpell,
                Location = new Point(XAlign, YAlign),
                Size = new Size(80, 20),
                ValueTextBox = { Size = new Size(40, 18) },
                MaxValue = 999,
                MinValue = 0,
                UpButton = { Location = new Point(63, 1) }
            };

            YAlign += 30;

            DXLabel label3 = new DXLabel
            {
                Text = "自动施放:",
                Outline = true,
                Parent = tabSpell,
            };
            label3.Location = new Point(XAlign - label3.Size.Width, YAlign);

            ComboPractice = new DXComboBox
            {
                Parent = tabSpell,
                Location = new Point(XAlign, YAlign),
                Size = new Size(100, DXComboBox.DefaultNormalHeight),
            };

            foreach (string tmpspell in AutoPracticeList)
            {
                MagicInfo tmpMagic = Globals.MagicInfoList.Binding.FirstOrDefault(m => m.Name == tmpspell);
                if (tmpMagic != null)
                {
                    if (tmpMagic.Class != MapObject.User?.Class || tmpMagic.School == MagicSchool.None)
                        continue;

                    ClientUserMagic tmpCMagic;
                    if (MapObject.User.Magics.TryGetValue(tmpMagic, out tmpCMagic))
                    {

                        new DXListBoxItem
                        {
                            Parent = ComboPractice.ListBox,
                            Label = { Text = tmpMagic.Name },
                            Item = tmpMagic
                        };
                    }

                }

            }

            #endregion
        }

        private void _InitPotion()
        {
            if (tabPotion != null)
                tabPotion.Dispose();

            tabPotion = new DXTab
            {
                Parent = tabControl,
                Border = true,
                TabButton = { Label = { Text = "喝药" } },
            };
        }

        private void _InitPicker()
        {
            if (tabPicker != null)
                tabPicker.Dispose();

            tabPicker = new DXTab
            {
                Parent = tabControl,
                Border = true,
                TabButton = { Label = { Text = "捡拾" } },
            };
            /*VScrolPicker = new DXVScrollBar
            {
                Parent = this,
                Size = new Size(14, ClientArea.Height - 2),
                Location = new Point(ClientArea.Right - 14, ClientArea.Top + 1),
                VisibleSize = ClientArea.Height,
                MaxValue = Rows.Length * 50 - 2

            };
            DXControl panel = new DXControl
            {
                Parent = this,
                Size = new Size(ClientArea.Size.Width - 16, ClientArea.Size.Height),
                Location = ClientArea.Location,
            };
            panel.MouseWheel += ScrollBar.DoMouseWheel;*/

            int XAlign = 180;
            int YAlign = 35;

            chkPickerGold = new DXCheckBox
            {
                Label = { Text = "自动捡拾金币:" },
                Parent = tabControl,
                Checked = true,
            };
            chkPickerGold.Location = new Point(XAlign - chkPickerGold.Size.Width, YAlign);

            YAlign += 30;
            chkPickerHeal = new DXCheckBox
            {
                Label = { Text = "自动捡拾特级以上金疮药:" },
                Parent = tabControl,
                Checked = true,
            };
            chkPickerHeal.Location = new Point(XAlign - chkPickerHeal.Size.Width, YAlign);

            YAlign += 30;
            chkPickerMana = new DXCheckBox
            {
                Label = { Text = "自动捡拾特级以上魔法药:" },
                Parent = tabControl,
                Checked = true,
            };
            chkPickerMana.Location = new Point(XAlign - chkPickerMana.Size.Width, YAlign);

            YAlign += 30;
            chkPickerArmour = new DXCheckBox
            {
                Label = { Text = "自动捡拾33+级和稀有盔甲:" },
                Parent = tabControl,
                Checked = true,
            };
            chkPickerArmour.Location = new Point(XAlign - chkPickerArmour.Size.Width, YAlign);

            YAlign += 30;
            chkPickerHelmet = new DXCheckBox
            {
                Label = { Text = "自动捡拾20+级和稀有头盔:" },
                Parent = tabControl,
                Checked = true,
            };
            chkPickerHelmet.Location = new Point(XAlign - chkPickerHelmet.Size.Width, YAlign);
        }

        public void ProcSpell()
        {
            #region 自动保持状态
            if (ComboShield?.SelectedItem != null)
            {
                TimeSpan time = CEnvir.Now - TimeSpellTime;
                if (time.TotalMilliseconds >= 800)
                {
                    MagicInfo tmpMagic = (MagicInfo)ComboShield.SelectedItem;

                    switch (tmpMagic.Name)
                    {
                        case "魔法盾":
                            if (MapControl.User.MagicShieldEffect != null
                                || MapControl.User.Horse != HorseType.None)
                                return;

                            MapObject.User.AttemptAction(new ObjectAction(
                                MirAction.Spell,
                                MapObject.User.Direction,
                                MapObject.User.CurrentLocation,
                                MagicType.MagicShield,
                                new List<uint> { MapControl.User.ObjectID },
                                new List<Point> { MapControl.User.CurrentLocation },
                                true));
                            /*CEnvir.Enqueue(new C.Magic
                            {
                                Direction = MapObject.User.Direction,
                                Action = MirAction.Spell,
                                Type = MagicType.MagicShield,
                                Target = MapControl.User?.ObjectID ?? 0,
                                Location = MapControl.User?.CurrentLocation ?? Point.Empty
                            });*/

                            break;
                        case "神圣战甲术":

                            break;

                        case "阴阳法环":

                            break;
                    }

                    TimeSpellTime = CEnvir.Now;
                }
            }

            #endregion

            #region 自动练功
            if (ComboPractice?.SelectedItem != null
                && AutoPracticeInterval?.Value > 0)
            {
                TimeSpan time = CEnvir.Now - TimePractice;
                if (time.TotalMilliseconds >= AutoPracticeInterval.Value)
                {
                    MagicInfo tmpMagic = (MagicInfo)ComboPractice.SelectedItem;

                    /*CEnvir.Enqueue(new C.Magic
                    {
                        Direction = MapObject.User.Direction,
                        Action = MirAction.Spell,
                        Type = tmpMagic.Magic,
                        Target = MapControl.User?.AttackTargets[0]?.ObjectID ?? 0,
                        Location = MapControl.User?.CurrentLocation ?? Point.Empty
                    });*/

                    MapObject.User.AttemptAction(new ObjectAction(
                        MirAction.Spell,
                        MapObject.User.Direction,
                        MapObject.User.CurrentLocation,
                        tmpMagic.Magic,
                        new List<uint> { MapControl.User?.AttackTargets[0]?.ObjectID ?? 0 },
                        new List<Point> { MapControl.User?.CurrentLocation ?? Point.Empty },
                        true));

                    TimePractice = CEnvir.Now;
                }

            }
            #endregion
        }

    }
}