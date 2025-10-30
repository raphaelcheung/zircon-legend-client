using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.UserModels;
using SlimDX;
using Library;
using Library.Network;
using Library.Network.ClientPackets;
using Library.SystemModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SqlTypes;
using C = Library.Network.ClientPackets;

namespace Client.Scenes.Views
{
    public sealed class BigPatchDialog : DXWindow
    {
        public DXTabControl TabControl;
        public DXCommonlyTab Commonly;
        public DXPlayerHelperTab Helper;
        public DXProtectionTab Protect;
        public DXAnsweringTab Answering;
        public DXUserNoteBookTab NoteBook;
        public DXSystemMsgRecordTab MsgRecord;
        public DXAutoPickItemTab AutoPick;
        public DXViewRangeObjectTab ViewRange;
        public DXMagicHelperTab Magic { get; set; }
        public DateTime _ProtectTime;

        private ClientUserMagic FlamingSword = null;
        private ClientUserMagic DragonRise = null;
        private ClientUserMagic BladeStorm = null;

        public DateTime AutoSkillsTime { get; set; } = DateTime.MinValue;

        public override WindowType Type
        {
            get
            {
                return WindowType.BigPatchWindow;
            }
        }

        public override bool CustomSize
        {
            get
            {
                return false;
            }
        }

        public override bool AutomaticVisiblity
        {
            get
            {
                return true;
            }
        }

        public static DXCheckBox CreateCheckBox(DXControl parent, string name, int x, int y, EventHandler<EventArgs> Changed, bool Checked = false)
        {
            DXCheckBox dxCheckBox1 = new DXCheckBox();
            dxCheckBox1.AutoSize = true;

            dxCheckBox1.Text = name;
            dxCheckBox1.Parent = parent;
            dxCheckBox1.Checked = Checked;

            //dxCheckBox1.bAlignRight = false;
            //DXCheckBox dxCheckBox2 = dxCheckBox1;
            dxCheckBox1.Location = new Point(x, y);
            dxCheckBox1.CheckedChanged += Changed;
            dxCheckBox1.UpdateControl();
            return dxCheckBox1;
        }

        public BigPatchDialog()
        {
            TitleLabel.Text = "辅助功能";
            HasFooter = false;
            SetClientSize(new Size(550, 428));
            DXTabControl dxTabControl = new DXTabControl();
            dxTabControl.Parent = this;
            dxTabControl.Location = ClientArea.Location;
            dxTabControl.Size = ClientArea.Size;
            TabControl = dxTabControl;
            DXCommonlyTab dxCommonlyTab = new DXCommonlyTab();
            dxCommonlyTab.Parent = TabControl;
            dxCommonlyTab.Border = true;
            dxCommonlyTab.TabButton.Label.Text = "常用";
            dxCommonlyTab.TabButton.Label.Hint = "一些常用的功能设置";
            Commonly = dxCommonlyTab;
            DXPlayerHelperTab dxPlayerHelperTab = new DXPlayerHelperTab();
            dxPlayerHelperTab.Parent = TabControl;
            dxPlayerHelperTab.Border = true;
            dxPlayerHelperTab.TabButton.Label.Text = "辅助";
            dxPlayerHelperTab.TabButton.Label.Hint = "游戏相关的一些自定义调整";
            Helper = dxPlayerHelperTab;
            DXProtectionTab dxProtectionTab = new DXProtectionTab();
            dxProtectionTab.Parent = TabControl;
            dxProtectionTab.Border = true;
            dxProtectionTab.TabButton.Label.Text = "保护";
            dxProtectionTab.TabButton.Label.Hint = "自动使用物品，以及遇到特殊情况的自动应对方案";
            Protect = dxProtectionTab;
            DXAnsweringTab dxAnsweringTab = new DXAnsweringTab();
            dxAnsweringTab.Parent = TabControl;
            dxAnsweringTab.Border = true;
            dxAnsweringTab.TabButton.Label.Text = "聊天";
            dxAnsweringTab.TabButton.Label.Hint = "自动回复以及自动喊话";
            Answering = dxAnsweringTab;
            DXUserNoteBookTab dxUserNoteBookTab = new DXUserNoteBookTab();
            dxUserNoteBookTab.Parent = TabControl;
            dxUserNoteBookTab.Border = true;
            dxUserNoteBookTab.TabButton.Label.Text = "便签";
            dxUserNoteBookTab.TabButton.Label.Hint = "方便用户记录一些文本";
            NoteBook = dxUserNoteBookTab;
            DXSystemMsgRecordTab systemMsgRecordTab = new DXSystemMsgRecordTab();
            systemMsgRecordTab.Parent = TabControl;
            systemMsgRecordTab.Border = true;
            systemMsgRecordTab.TabButton.Label.Text = "记录";
            systemMsgRecordTab.TabButton.Label.Hint = "系统消息记录";
            MsgRecord = systemMsgRecordTab;
            DXAutoPickItemTab dxAutoPickItemTab = new DXAutoPickItemTab();
            dxAutoPickItemTab.Parent = TabControl;
            dxAutoPickItemTab.Border = true;
            dxAutoPickItemTab.TabButton.Label.Text = "拾取";
            dxAutoPickItemTab.TabButton.Label.Hint = "自动拾取的设置";
            AutoPick = dxAutoPickItemTab;
            DXMagicHelperTab dxMagicHelperTab = new DXMagicHelperTab();
            dxMagicHelperTab.Parent = TabControl;
            dxMagicHelperTab.Border = true;
            dxMagicHelperTab.TabButton.Label.Text = "魔法";
            dxMagicHelperTab.TabButton.Label.Hint = "内容会自动刷新";
            Magic = dxMagicHelperTab;
            DXViewRangeObjectTab viewRangeObjectTab = new DXViewRangeObjectTab();
            viewRangeObjectTab.Parent = TabControl;
            viewRangeObjectTab.Border = true;
            viewRangeObjectTab.TabButton.Label.Text = "帮助";
            viewRangeObjectTab.TabButton.Label.Hint = "关于辅助的解释说明";
            ViewRange = viewRangeObjectTab;
        }

        public void UpdateLinks(StartInformation info)
        {
            if (Helper != null)
            {
                switch (info.Class)
                {
                    case MirClass.Warrior:
                        Helper.Warrior.Visible = true;
                        break;
                    case MirClass.Wizard:
                        Helper.Wizard.Visible = true;
                        break;
                    case MirClass.Taoist:
                        Helper.Taoist.Visible = true;
                        break;
                    case MirClass.Assassin:
                        Helper.Assassin.Visible = true;
                        break;
                }
            }
            if (Protect == null)
                return;
            foreach (ClientAutoPotionLink autoPotionLink in info.AutoPotionLinks)
            {
                if (autoPotionLink.Slot >= 0 && autoPotionLink.Slot < Protect.Links.Length)
                    Protect.Links[autoPotionLink.Slot] = autoPotionLink;
            }
        }

        public void OnTimerChanged(long AutoTime)
        {
            TimeSpan timeSpan = new TimeSpan(0, 0, (int)AutoTime);
            if (Helper == null)
                return;
            DXLabel timeLable = Helper.TimeLable;

            timeLable.Text = (timeSpan.Hours + timeSpan.Days * 24).ToString() + " 小时 " + timeSpan.Minutes + " 分钟 " + timeSpan.Seconds + " 秒";
            /*
            string[] strArray = new string[5];
            int index1 = 0;
            int num = timeSpan.Hours + timeSpan.Days * 24;
            string str1 = num.ToString();
            strArray[index1] = str1;
            strArray[1] = ":";
            int index2 = 2;
            num = timeSpan.Minutes;
            string str2 = num.ToString();
            strArray[index2] = str2;
            strArray[3] = ":";
            int index3 = 4;
            num = timeSpan.Seconds;
            string str3 = num.ToString();
            strArray[index3] = str3;
            string str4 = string.Concat(strArray);
            timeLable.Text = str4;
            */
            //if (AutoTime == 0L)
            //    Helper.AndroidPlayer.Checked = false;
        }

        public void CastFourFlowers()
        {
            if (!Config.自动四花 || GameScene.Game.User.Class != MirClass.Assassin) return;
            if (!GameScene.Game.User.Buffs.Any(x =>
            {
                if (x.Type != BuffType.FullBloom && x.Type != BuffType.WhiteLotus)
                    return x.Type == BuffType.RedLotus;
                return true;
            }))
                GameScene.Game.UseMagic(MagicType.FullBloom);
            if (GameScene.Game.User.Buffs.Any(x => x.Type == BuffType.FullBloom))
                GameScene.Game.UseMagic(MagicType.WhiteLotus);
            if (GameScene.Game.User.Buffs.Any(x => x.Type == BuffType.WhiteLotus))
                GameScene.Game.UseMagic(MagicType.RedLotus);
            if (GameScene.Game.User.Buffs.Any(x => x.Type == BuffType.RedLotus))
                GameScene.Game.UseMagic(MagicType.SweetBrier);
        }

        public void ReadySkillInfo()
        {
            if (FlamingSword == null)
            {
                foreach(var pair in MapObject.User.Magics)
                {
                    if (pair.Key.Magic == MagicType.FlamingSword)
                    {
                        FlamingSword = pair.Value;
                        break;
                    }
                }
            }

            if (DragonRise == null)
            {
                foreach (var pair in MapObject.User.Magics)
                {
                    if (pair.Key.Magic == MagicType.DragonRise)
                    {
                        DragonRise = pair.Value;
                        break;
                    }
                }
            }

            if (BladeStorm == null)
            {
                foreach (var pair in MapObject.User.Magics)
                {
                    if (pair.Key.Magic == MagicType.BladeStorm)
                    {
                        BladeStorm = pair.Value;
                        break;
                    }
                }
            }
        }

        public void AutoSkills()
        {
            if (CEnvir.Now < AutoSkillsTime || CEnvir.Now < GameScene.Game.User.NextMagicTime) return;
            if (MapObject.User.Horse != HorseType.None) return;

            AutoSkillsTime = CEnvir.Now.AddMilliseconds(300);

            switch (GameScene.Game.User.Class)
            {
                case MirClass.Warrior:
                    if (Config.自动铁布衫 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.Defiance))
                    {
                        GameScene.Game.UseMagic(MagicType.Defiance);
                        return;
                    }

                    if (Config.自动金刚之躯 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.Endurance))
                    {
                        var clientMagic = GameScene.Game.GetMagic(MagicType.Endurance);
                        if (clientMagic != null && clientMagic.NextCast < CEnvir.Now)
                        {
                            GameScene.Game.UseMagic(clientMagic);
                            return;
                        }
                    }

                    if (Config.自动破血 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.Might))
                    {
                        GameScene.Game.UseMagic(MagicType.Might);
                        return;
                    }

                    if (Config.自动移花接木 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.ReflectDamage))
                    {
                        var clientMagic = GameScene.Game.GetMagic(MagicType.ReflectDamage);
                        if (clientMagic != null && clientMagic.NextCast < CEnvir.Now)
                        {
                            GameScene.Game.UseMagic(clientMagic);
                            return;
                        }
                    }

                    if (Config.自动莲月 && BladeStorm != null && CEnvir.Now > BladeStorm.NextCast)
                        GameScene.Game.UseMagic(MagicType.BladeStorm);
                    else if (Config.自动烈火 && FlamingSword != null && CEnvir.Now > FlamingSword.NextCast)
                        GameScene.Game.UseMagic(MagicType.FlamingSword);
                    else if (Config.自动翔空 && DragonRise != null && CEnvir.Now > DragonRise.NextCast)
                        GameScene.Game.UseMagic(MagicType.DragonRise);

                    if (Config.自动半月弯刀 || Config.自动十方斩)
                    {
                        int halfCount = 0;
                        int surgeCount = 0;
                        var scene = GameScene.Game;
                        if (CanAttack(Functions.Move(scene.User.CurrentLocation, scene.User.Direction)))
                            halfCount++;

                        if (CanAttack(Functions.Move(scene.User.CurrentLocation, Functions.ShiftDirection(scene.User.Direction, -1))))
                            halfCount++;

                        if (CanAttack(Functions.Move(scene.User.CurrentLocation, Functions.ShiftDirection(scene.User.Direction, 1))))
                            halfCount++;

                        if (CanAttack(Functions.Move(scene.User.CurrentLocation, Functions.ShiftDirection(scene.User.Direction, 2))))
                            halfCount++;

                        if (Config.自动十方斩)
                        {
                            if (CanAttack(Functions.Move(scene.User.CurrentLocation, Functions.ShiftDirection(scene.User.Direction, 3))))
                                surgeCount++;

                            if (CanAttack(Functions.Move(scene.User.CurrentLocation, Functions.ShiftDirection(scene.User.Direction, 4))))
                                surgeCount++;

                            if (CanAttack(Functions.Move(scene.User.CurrentLocation, Functions.ShiftDirection(scene.User.Direction, 5))))
                                surgeCount++;

                            if (CanAttack(Functions.Move(scene.User.CurrentLocation, Functions.ShiftDirection(scene.User.Direction, 6))))
                                surgeCount++;
                        }

                        if (Config.自动十方斩 && surgeCount >= 3)
                        {
                            if (!scene.User.CanDestructiveBlow)
                                GameScene.Game.UseMagic(MagicType.DestructiveSurge);
                            else if (scene.User.CanThrusting)
                                GameScene.Game.UseMagic(MagicType.Thrusting);
                        }
                        else
                        {
                            if (scene.User.CanDestructiveBlow)
                                GameScene.Game.UseMagic(MagicType.DestructiveSurge);
                            else if (Config.自动半月弯刀 && halfCount >= 3)
                            {
                                if (!scene.User.CanHalfMoon)
                                    GameScene.Game.UseMagic(MagicType.HalfMoon);
                                else if (scene.User.CanThrusting)
                                    GameScene.Game.UseMagic(MagicType.Thrusting);
                            }
                            else
                            {
                                if (scene.User.CanHalfMoon)
                                    GameScene.Game.UseMagic(MagicType.HalfMoon);
                                else if (!scene.User.CanThrusting)
                                    GameScene.Game.UseMagic(MagicType.Thrusting);
                            }
                        }
                    }

                    break;
                case MirClass.Wizard:
                    if (Config.自动魔法盾 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.MagicShield))
                    {
                        GameScene.Game.UseMagic(MagicType.MagicShield);
                        return;
                    }

                    if (Config.自动凝血 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.Renounce))
                    {
                        GameScene.Game.UseMagic(MagicType.Renounce);
                        return;
                    }

                    if (Config.自动天打雷劈 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.JudgementOfHeaven))
                    {
                        GameScene.Game.UseMagic(MagicType.JudgementOfHeaven);
                        return;
                    }

                    //if (Config.自动魔光盾 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.ShieldOfPreservation))
                    //    GameScene.Game.UseMagic(MagicType.ShieldOfPreservation);

                    if (Config.自动法师连续技能
                    && GameScene.Game.LastMagic != null
                    && (GameScene.Game.CanAttackTarget(GameScene.Game.LastTarget)
                    || GameScene.Game.CanAttackTarget(GameScene.Game.MouseObject)))
                    {
                        var target = GameScene.Game.CanAttackTarget(GameScene.Game.LastTarget) ? GameScene.Game.LastTarget : GameScene.Game.MouseObject;
                        if (!Functions.InRange(GameScene.Game.User.CurrentLocation, target.CurrentLocation, Globals.MagicRange)) break;

                        var helpper = GameScene.Game.GetMagicHelpper(GameScene.Game.LastMagic.Magic);

                        if (helpper == null) break;

                        if (!helpper.LockMonster && target.Race == ObjectType.Monster) break;
                        if (!helpper.LockPlayer && target.Race == ObjectType.Player) break;

                        switch (GameScene.Game.LastMagic.Magic)
                        {
                            case MagicType.FireBall:
                            case MagicType.LightningBall:
                            case MagicType.IceBolt:
                            case MagicType.GustBlast:
                            case MagicType.ElectricShock:
                            case MagicType.AdamantineFireBall:
                            case MagicType.ThunderBolt:
                            case MagicType.IceBlades:
                            case MagicType.Cyclone:
                            case MagicType.ScortchedEarth:
                            case MagicType.LightningBeam:
                            case MagicType.FrozenEarth:
                            case MagicType.BlowEarth:
                            case MagicType.ExpelUndead:
                            case MagicType.FireStorm:
                            case MagicType.LightningWave:
                            case MagicType.IceStorm:
                            case MagicType.DragonTornado:
                            case MagicType.GreaterFrozenEarth:
                            case MagicType.ChainLightning:
                            case MagicType.MeteorShower:
                            case MagicType.Asteroid:
                                GameScene.Game.UseMagic(GameScene.Game.LastMagic.Magic);
                                break;
                        }
                    }
                    break;

                case MirClass.Taoist:
                    AutoHealing();

                    if (Config.自动阴阳盾 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.CelestialLight))
                    {
    					GameScene.Game.UseMagic(MagicType.CelestialLight);
						return;
					}

                    if (Config.自动强魔震法 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.ElementalSuperiority))
                    {
                        GameScene.Game.UseMagic(MagicType.ElementalSuperiority, GameScene.Game.User);
                        return;
                    }

                    if (Config.有宠物时自动移花接玉 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.StrengthOfFaith))
                    {
                        foreach (MapObject ob in GameScene.Game.MapControl.Objects)
                        {
                            if (!(ob is MonsterObject mon) || mon.Dead || mon.PetOwner != GameScene.Game.User.Name) continue;
                            if (mon.CompanionObject != null) continue;

                            GameScene.Game.UseMagic(MagicType.StrengthOfFaith);
                            return;
                        }
                    }

                    if (Config.自动吸星大法 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.LifeSteal))
                    {
                        GameScene.Game.UseMagic(MagicType.LifeSteal);
                        return;
                    }

                    if (Config.自动施放幽灵盾 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.MagicResistance))
                    {
                        GameScene.Game.UseMagic(MagicType.MagicResistance, GameScene.Game.User);
                        return;
                    }

                    if (Config.自动施放神圣战甲术 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.Resilience))
                    {
                        GameScene.Game.UseMagic(MagicType.Resilience, GameScene.Game.User);
                        return;
                    }

                    if (Config.开始挂机)
                        if (GameScene.Game.AutoPoison())
                            return;

                    if (Config.自动施放幽灵盾 || Config.自动给宠物施放猛虎强势 || Config.自动施放神圣战甲术)
                    {
                        foreach (MapObject ob in GameScene.Game.MapControl.Objects)
                        {
                            if (!(ob is MonsterObject mon) || mon.Dead || mon.PetOwner != GameScene.Game.User.Name) continue;
                            if (mon.CompanionObject != null) continue;
                            if (!Functions.InRange(GameScene.Game.User.CurrentLocation, mon.CurrentLocation, Globals.MagicRange)) continue;

                            if (Config.自动施放幽灵盾 && mon.VisibleBuffs.All(x => x != BuffType.MagicResistance))
                            {
                                GameScene.Game.UseMagic(MagicType.MagicResistance, ob);
                                return;
                            }

                            if (Config.自动施放神圣战甲术 && mon.VisibleBuffs.All(x => x != BuffType.Resilience))
                            {
                                GameScene.Game.UseMagic(MagicType.Resilience, ob);
                                return;
                            }

                            if (Config.自动给宠物施放猛虎强势 && mon.VisibleBuffs.All(x => x != BuffType.BloodLust))
                            {
                                GameScene.Game.UseMagic(MagicType.BloodLust, ob);
                                return;
                            }
                        }
                    }

                    if (GameScene.Game.User.Buffs.Any(x => x.Type == BuffType.Transparency || x.Type == BuffType.Invisibility))
                        return;

                    if (Config.自动道士连续技能
                    && GameScene.Game.LastMagic != null
                    && (GameScene.Game.CanAttackTarget(GameScene.Game.LastTarget)
                    || GameScene.Game.CanAttackTarget(GameScene.Game.MouseObject)))
                    {
                        var target = GameScene.Game.CanAttackTarget(GameScene.Game.LastTarget) ? GameScene.Game.LastTarget : GameScene.Game.MouseObject;
                        if (!Functions.InRange(GameScene.Game.User.CurrentLocation, target.CurrentLocation, Globals.MagicRange)) break;
                        
                        var helpper = GameScene.Game.GetMagicHelpper(GameScene.Game.LastMagic.Magic);

                        if (helpper == null) break;

                        if (!helpper.LockMonster && target.Race == ObjectType.Monster) break;
                        if (!helpper.LockPlayer && target.Race == ObjectType.Player) break;

                        switch (GameScene.Game.LastMagic.Magic)
                        {
                            case MagicType.Heal:
                            case MagicType.ExplosiveTalisman:
                            case MagicType.EvilSlayer:
                            case MagicType.GreaterEvilSlayer:
                            //case MagicType.MassHeal:
                            case MagicType.ImprovedExplosiveTalisman:
                            case MagicType.GreaterHolyStrike:
                                GameScene.Game.UseMagic(GameScene.Game.LastMagic.Magic);
                                break;
                        }
                    }
                    break;

                case MirClass.Assassin:
                    if (Config.自动风之闪避 && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.Evasion))
                        GameScene.Game.UseMagic(MagicType.Evasion);

                    if (!Config.自动风之守护 || !GameScene.Game.User.Buffs.All<ClientBuffInfo>(x => x.Type != BuffType.RagingWind))
                        return;

                    GameScene.Game.UseMagic(MagicType.RagingWind);
                    break;
            }
        }
        private void AutoHealing()
        {
            //生命值低于一半时施展秒影后持续用群疗或治愈术直至恢复血量
            if (Config.自动施展秒影恢复血量)
            {
                bool userDangers = GameScene.Game.User.CurrentHP < GameScene.Game.User.Stats[Stat.Health] / 2;
                MapObject target = null;

                if (userDangers) target = GameScene.Game.User;
                else
                {
                    List<MonsterObject> list = new List<MonsterObject>();
                    foreach (var ob in GameScene.Game.MapControl.Objects)
                    {
                        if (ob is MonsterObject mon && !mon.Dead && mon.PetOwner == GameScene.Game.User.Name && mon.CompanionObject == null)
                        {
                            if (!GameScene.Game.DataDictionary.TryGetValue(mon.ObjectID, out var monData)) continue;
                            if (monData.Health > 0 && monData.Health < monData.MaxHealth / 2)
                            {
                                target = mon;
                                break;
                            }
                        }
                    }
                }

                if (target == null) return;

                if (GameScene.Game.User.Buffs.All(x => x.Type != BuffType.Transparency))
                {
                    var invisibleMagic = GameScene.Game.GetMagic(MagicType.Transparency);
                    if (invisibleMagic != null)
                    {
                        GameScene.Game.UseMagic(MagicType.Transparency, null);
                        return;
                    }
                }
                else if ((target == GameScene.Game.User && GameScene.Game.User.Buffs.All(x => x.Type != BuffType.Heal)) || target != GameScene.Game.User)
                {
                    var healMagic = GameScene.Game.GetMagic(MagicType.MassHeal);
                    if (healMagic != null)
                    {
                        GameScene.Game.UseMagic(MagicType.MassHeal, target);
                        return;
                    }

                    healMagic = GameScene.Game.GetMagic(MagicType.Heal);
                    if (healMagic != null)
                        GameScene.Game.UseMagic(MagicType.Heal, target);

                    return;
                }
            }
        }
        public bool CanAttack(Point pi)
        {
            var scene = GameScene.Game;
            if (pi.X < 0 || pi.X >= scene.MapControl.Width || pi.Y < 0 || pi.Y >= scene.MapControl.Height)
                return false;

            if (GameScene.Game.MapControl.Cells[pi.X, pi.Y].Objects != null)
                foreach (var ob in GameScene.Game.MapControl.Cells[pi.X, pi.Y].Objects)
                    if (scene.CanAttackTarget(ob))
                        return true;

            return false;
        }
        public void UpdateAutoAssist()
        {
            CastFourFlowers();

            if (Config.在安全处有效)
                AutoSkills();
            else if (!Config.在安全处有效 && !MapObject.User.InSafeZone)
                AutoSkills();

            if (Config.快速自动拾取)
                PickupItems();

            if (Config.自动关组)
            {
                if (GameScene.Game.GroupBox.AllowGroup)
                    CEnvir.Enqueue(new GroupSwitch { Allow = !GameScene.Game.GroupBox.AllowGroup });
            }

            if (Config.是否开启随机保护)
            {
                float num = (float)Config.血量剩下百分之多少时自动随机 / 100f;
                if ((double)GameScene.Game.User.CurrentHP < (double)GameScene.Game.User.Stats[Stat.Health] * (double)num && CEnvir.Now > _ProtectTime)
                {
                    DXItemCell dxItemCell = GameScene.Game.InventoryBox.Grid.Grid.FirstOrDefault(x => x?.Item?.Info.ItemName == "随机传送卷");
                    if (dxItemCell != null && dxItemCell.UseItem())
                        _ProtectTime = CEnvir.Now.AddSeconds(5.0);
                }
            }
            if (Config.是否开启回城保护)
            {
                float num = (float)Config.血量剩下百分之多少时自动回城 / 100f;
                if ((double)GameScene.Game.User.CurrentHP < (double)GameScene.Game.User.Stats[Stat.Health] * (double)num)
                {
                    DXItemCell dxItemCell = GameScene.Game.InventoryBox.Grid.Grid.FirstOrDefault(x => x?.Item?.Info.ItemName == "回城卷");
                    if (dxItemCell != null && dxItemCell.UseItem())
                        Config.是否开启回城保护 = false;
                }
            }

            if (Config.自动学习技能书 && CEnvir.Now > GameScene.Game.UseItemTime && MapObject.User.Horse == HorseType.None)
            {
                if (!_AutoUseBook(GameScene.Game.InventoryBox.Grid.Grid))
                    _AutoUseBook(GameScene.Game.CompanionBox.InventoryGrid.Grid);
            }

            if (!Config.开始挂机)
                return;

            if (GameScene.Game.User.Dead && Config.死亡回城)
                CEnvir.Enqueue(new TownRevive());

            if (Config.是否开启每间隔自动随机 && CEnvir.Now > _ProtectTime)
            {
                DXItemCell dxItemCell = GameScene.Game.InventoryBox.Grid.Grid.FirstOrDefault(x => x?.Item?.Info.ItemName == "随机传送卷");
                if (dxItemCell != null && dxItemCell.UseItem())
                    _ProtectTime = CEnvir.Now.AddSeconds((double)Config.隔多少秒自动随机一次);
            }


            if (Config.是否开启指定时间无经验或者未杀死目标自动随机)
                Helper.ExpAutoRandoms();
        }
        private bool _AutoUseBook(DXItemCell[] grid)
        {

            foreach (var cell in grid)
                if (cell?.Item?.Info != null && !cell.Locked && cell.Item.Info.ItemType == ItemType.Book && GameScene.Game.CanUseItem(cell.Item))
                {
                    GameScene.Game.UseItemTime = CEnvir.Now.AddMilliseconds(500);
                    cell.Locked = true;
                    CEnvir.Enqueue(new C.ItemUse { Link = new CellLinkInfo { GridType = cell.GridType, Slot = cell.Slot, Count = 1 } });
                    return true;
                }

            return false;
        }
        public void UserChanged()
        {
            Magic.UpdateMagic();
            Helper.UpdateMagic();
        }

        public void ReceiveChat(string message, MessageType type)
        {
            if (type != MessageType.WhisperIn && MessageType.GMWhisperIn != type)
                return;
            string text = Answering?.CombAutoReplayText?.SelectedLabel?.Text;
            if (text != null)
            {
                string[] strArray = message.Split(new char[2]
                {
          '=',
          '>'
                });
                if (strArray.Length == 0)
                    return;
                CEnvir.Enqueue((Packet)new Chat()
                {
                    Text = ("/" + strArray[0] + " " + text)
                });
            }
        }
        public bool NeedPick(ItemInfo item)
        {
            if (AutoPick.ItemFilter.dictItems.TryGetValue(item.Index, out var info))
                return info.pick || info.picks;

            return false;
        }
        public bool PickupItems()
        {
            AutoPick?.SycFilters(false);
            bool flag = false;
            if (AutoPick != null && !GameScene.Game.Observer && (!GameScene.Game.User.Dead && !(CEnvir.Now < GameScene.Game.PickUpTime)))
            {
                GameScene.Game.PickUpTime = CEnvir.Now.AddMilliseconds(500.0);
                int stat = GameScene.Game.User.Stats[Stat.PickUpRadius];
                int x = GameScene.Game.User.CurrentLocation.X;
                int y = GameScene.Game.User.CurrentLocation.Y;

                List<PickItemInfo> user_items = new List<PickItemInfo>();
                List<PickItemInfo> compain_items = new List<PickItemInfo>();

                for (int index1 = 0; index1 <= stat; ++index1)
                {
                    for (int index2 = y - index1; index2 <= y + index1; ++index2)
                    {
                        if (index2 >= 0)
                        {
                            if (index2 < GameScene.Game.MapControl.Height)
                            {
                                int index3 = x - index1;
                                while (index3 <= x + index1)
                                {
                                    if (index3 >= 0)
                                    {
                                        if (index3 < GameScene.Game.MapControl.Width)
                                        {
                                            Cell cell = GameScene.Game.MapControl.Cells[index3, index2];
                                            if (cell?.Objects != null)
                                            {
                                                foreach (MapObject mapObject in cell.Objects)
                                                {
                                                    if (mapObject.Race == ObjectType.Item)
                                                    {
                                                        ItemObject itemObject = (ItemObject)mapObject;
                                                        string str = itemObject.Item.Info.ItemName;
                                                        int num = itemObject.Item.Info.Index;

                                                        if (itemObject.Item.Info.Effect == ItemEffect.Gold)
                                                        {
                                                            if (itemObject.Item.Count + GameScene.Game.User.Gold > Globals.MaxGold) continue;
                                                        }

                                                        if (itemObject.Item.Info.Effect == ItemEffect.ItemPart)
                                                        {
                                                            str = itemObject.Name;
                                                            int part = itemObject.Item.AddedStats[Stat.ItemIndex];
                                                            num = part;
                                                        }

                                                        if (AutoPick.ItemFilter.dictItems.TryGetValue(num, out var item))
                                                        {
                                                            if (item.pick && !item.picks)
                                                            {
                                                                user_items.Add(new PickItemInfo()
                                                                {
                                                                    ItemIndex = itemObject.Item.Info.Index,
                                                                    xPos = index3,
                                                                    yPos = index2
                                                                });
                                                                flag = true;
                                                            }
                                                            else if (item.picks && !item.pick)
                                                            {
                                                                compain_items.Add(new PickItemInfo()
                                                                {
                                                                    ItemIndex = itemObject.Item.Info.Index,
                                                                    xPos = index3,
                                                                    yPos = index2
                                                                });
                                                                flag = true;
                                                            }
                                                            else if (item.picks && item.pick)
                                                            {
                                                                user_items.Add(new PickItemInfo()
                                                                {
                                                                    ItemIndex = itemObject.Item.Info.Index,
                                                                    xPos = index3,
                                                                    yPos = index2
                                                                });
                                                                flag = true;
                                                            }
                                                        }
    
                                                    }
                                                }
                                            }
                                        }
                                        else
                                            break;
                                    }
                                    index3 += Math.Abs(index2 - y) == index1 ? 1 : index1 * 2;
                                }
                            }
                            else
                                break;
                        }
                    }
                }


                if (user_items.Count > 0 || compain_items.Count > 0)
                    CEnvir.Enqueue(new PickUpS()
                    {
                        CompanionItems = compain_items,
                        UserItems = user_items,
                    });
            }


            return flag;
        }

        //public static int GetItemId(int index)
        //{
        //    ItemInfo item = Globals.ItemInfoList.Binding.First(x => x.Index == index);
        //    return item.ItemId;
        //}

        public CItemFilterSet GetFilterItem(int idx)
        {
            BigPatchDialog.CItemFilterSet citemFilterSet = (BigPatchDialog.CItemFilterSet)null;
            if (AutoPick != null && AutoPick.ItemFilter != null && (AutoPick.ItemFilter.dictItems != null && AutoPick.ItemFilter.dictItems.TryGetValue(idx, out var result)))
                citemFilterSet = result;
            return citemFilterSet;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            AutoPick?.ItemFilter.Uninitalize();
            if (!disposing)
                return;
            Commonly?.Dispose();
            Helper?.Dispose();
            Protect?.Dispose();
            Answering?.Dispose();
            NoteBook?.Dispose();
            MsgRecord?.Dispose();
            AutoPick?.Dispose();
            ViewRange?.Dispose();
            Magic?.Dispose();
        }

        public class DXGroupBox : DXControl
        {
            public DXLabel Name;

            public DXGroupBox()
            {
                DXLabel dxLabel = new DXLabel();
                dxLabel.Parent = this;
                dxLabel.Outline = true;
                dxLabel.ForeColour = Color.FromArgb(70, 58, 35);
                dxLabel.Location = new Point(5, 10);
                Name = dxLabel;
                BorderColour = Color.FromArgb(70, 58, 35);
                Border = true;
            }

            protected internal override void UpdateBorderInformation()
            {
                BorderInformation = (Vector2[])null;
                int num1;
                if (Border)
                {
                    Rectangle displayArea = DisplayArea;
                    if (displayArea.Width != 0)
                    {
                        displayArea = DisplayArea;
                        num1 = displayArea.Height == 0 ? 1 : 0;
                        goto label_4;
                    }
                }
                num1 = 1;
            label_4:
                if (num1 != 0)
                    return;
                Vector2[] vector2Array = new Vector2[5];
                vector2Array[0] = new Vector2((float)(Name.Size.Width + Name.Location.X), 20f);
                int index1 = 1;
                Size size = Size;
                Vector2 vector2_1 = new Vector2((float)(size.Width - 5), 20f);
                vector2Array[index1] = vector2_1;
                int index2 = 2;
                size = Size;
                double num2 = (double)(size.Width - 5);
                size = Size;
                double num3 = (double)(size.Height - 5);
                Vector2 vector2_2 = new Vector2((float)num2, (float)num3);
                vector2Array[index2] = vector2_2;
                int index3 = 3;
                double num4 = 5.0;
                size = Size;
                double num5 = (double)(size.Height - 5);
                Vector2 vector2_3 = new Vector2((float)num4, (float)num5);
                vector2Array[index3] = vector2_3;
                vector2Array[4] = new Vector2(5f, 20f);
                BorderInformation = vector2Array;
            }
        }

        public class DXStaticView : DXControl
        {
            public int Vspac = 1;
            public int _First;
            public int _Last;
            public DXControl view;
            public List<string> contents;
            public DXVScrollBar VScrollBar;

            public int ItemCount
            {
                get
                {
                    return view.Controls.Count;
                }
            }

            public DXStaticView()
            {
                _First = 0;
                _Last = 0;
                contents = new List<string>();
                view = new DXControl()
                {
                    Parent = this
                };
            }

            public void UpdateItems()
            {
                int y = 0;
                int num1 = VScrollBar.Value;
                int num2 = -num1;
                if (ItemCount > 0)
                    _First = num1 / VScrollBar.Change;
                for (int index = 0; index < _First; ++index)
                    view.Controls[index].Visible = false;
                for (int first = _First; y < Size.Height && first < ItemCount; ++first)
                {
                    DXControl control = view.Controls[first];
                    control.Location = new Point(0, y);
                    DXControl dxControl = control;
                    Size size1 = view.Size;
                    int width = size1.Width;
                    size1 = control.Size;
                    int height = size1.Height;
                    Size size2 = new Size(width, height);
                    dxControl.Size = size2;
                    y += control.Size.Height + Vspac;
                    control.Visible = true;
                    _Last = first;
                }
                for (int index = _Last + 1; index < ItemCount; ++index)
                    view.Controls[index].Visible = false;
                VScrollBar.MaxValue = ItemCount * VScrollBar.Change;
            }

            public void UpdateScrollBar()
            {
                if (ItemCount == 0)
                {
                    VScrollBar.Visible = false;
                }
                else
                {
                    DXVScrollBar vscrollBar1 = VScrollBar;
                    int x = view.Location.X;
                    Size size1 = view.Size;
                    int width1 = size1.Width;
                    Point point = new Point(x + width1, view.Location.Y + 1);
                    vscrollBar1.Location = point;
                    DXVScrollBar vscrollBar2 = VScrollBar;
                    size1 = view.Size;
                    int height1 = size1.Height;
                    vscrollBar2.VisibleSize = height1;
                    DXVScrollBar vscrollBar3 = VScrollBar;
                    size1 = VScrollBar.Size;
                    int width2 = size1.Width;
                    size1 = view.Size;
                    int height2 = size1.Height;
                    size1 = view.Size;
                    int height3 = size1.Height;
                    int height4 = height2 + height3 + 3;
                    Size size2 = new Size(width2, height4);
                    vscrollBar3.Size = size2;
                    VScrollBar.Visible = true;
                    VScrollBar.Change = DXControl.DefaultHeight + Vspac;
                    int num = VScrollBar.VisibleSize % VScrollBar.Change;
                    if (num <= 0)
                        return;
                    VScrollBar.VisibleSize -= num;
                }
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
                UpdateViewRect();
            }

            public void UpdateViewRect()
            {
                this.view.Location = new Point(5, 5);
                DXControl view = this.view;
                Size size1 = Size;
                int width1 = size1.Width;
                size1 = VScrollBar.Size;
                int width2 = size1.Width;
                int width3 = width1 - width2 - 10;
                size1 = Size;
                int height = size1.Height - 10;
                Size size2 = new Size(width3, height);
                view.Size = size2;
                UpdateScrollBar();
                UpdateItems();
            }

            public void Insert(string str, int pos = -1)
            {
                if (pos >= contents.Count)
                    contents.Add(str);
                else
                    contents.Insert(pos, str);
                UpdateItems();
            }
        }

        public class DXTextView : DXTextBox
        {
            public DXTextView()
            {
                TextBox.Visible = false;
                Border = true;
                Editable = true;
                TextBox.AcceptsReturn = true;
                TextBox.Multiline = true;
                TextBox.WordWrap = false;
                TextBox.ForeColor = Color.DarkOrange;
                TextBox.ScrollBars = ScrollBars.Vertical;
            }
        }

        public class DXCommonlyTab : DXTab
        {
            public BigPatchDialog.DXGroupBox GroupNormal;
            public BigPatchDialog.DXGroupBox GroupWar;
            public DXCheckBox ChkItemObjShining;
            public DXCheckBox ShowItemNames;
            public DXCheckBox ChkAutoPick;
            public DXCheckBox ChkTabPick;
            public DXCheckBox ChkAutoBook;

            public BigPatchDialog.DXGroupBox GroupItem;
            public BigPatchDialog.DXGroupBox GroupWeather;
            public DXComboBox CombWeather;
            //public BigPatchDialog.DXGroupBox GroupAutoAttack;
            public DXCheckBox ChkAutoFire;
            public DXComboBox CombAutoFire;
            public DXLabel LabAutoFire;
            public DXNumberBox NumberAutoFireInterval;
            public BigPatchDialog.DXGroupBox GroupMouseMiddle;
            public DXCheckBox ChkCallMounts;
            public DXCheckBox ChkCastingMagic;
            public DXComboBox CombMiddleMouse;
            public DXButton RefreshBag;
            public DXButton ReloadConfig;

            public DXCommonlyTab()
            {
                Border = true;
                int x1 = 15;
                int num1 = 30;
                int num2 = 25;
                BigPatchDialog.DXGroupBox dxGroupBox1 = new BigPatchDialog.DXGroupBox();
                dxGroupBox1.Parent = this;
                dxGroupBox1.Size = new Size(120, 405);
                dxGroupBox1.Location = new Point(10, 0);
                dxGroupBox1.Name.Text = "常用";
                GroupNormal = dxGroupBox1;
                BigPatchDialog.DXGroupBox dxGroupBox2 = new BigPatchDialog.DXGroupBox();
                dxGroupBox2.Parent = this;
                dxGroupBox2.Size = new Size(120, 405);
                dxGroupBox2.Location = new Point(10, 0);
                dxGroupBox2.Name.Text = "战斗";
                GroupWar = dxGroupBox2;
                BigPatchDialog.DXGroupBox dxGroupBox3 = new BigPatchDialog.DXGroupBox();
                dxGroupBox3.Parent = this;
                dxGroupBox3.Size = new Size(120, 405);
                dxGroupBox3.Location = new Point(10, 0);
                dxGroupBox3.Name.Text = "物品";
                GroupItem = dxGroupBox3;
                BigPatchDialog.DXGroupBox dxGroupBox4 = new BigPatchDialog.DXGroupBox();
                dxGroupBox4.Parent = this;
                dxGroupBox4.Size = new Size(120, 32);
                dxGroupBox4.Location = new Point(10, 0);
                dxGroupBox4.Name.Text = "天气";
                dxGroupBox4.Visible = false;

                GroupWeather = dxGroupBox4;
                //BigPatchDialog.DXGroupBox dxGroupBox5 = new BigPatchDialog.DXGroupBox();
                //dxGroupBox5.Parent = this;
                //dxGroupBox5.Size = new Size(120, 60);
                //dxGroupBox5.Location = new Point(10, 0);
                //dxGroupBox5.Name.Text = "自动练技能";
                //GroupAutoAttack = dxGroupBox5;
                BigPatchDialog.DXGroupBox dxGroupBox6 = new BigPatchDialog.DXGroupBox();
                dxGroupBox6.Parent = this;
                dxGroupBox6.Size = new Size(120, 132);
                dxGroupBox6.Location = new Point(10, 0);
                dxGroupBox6.Name.Text = "鼠标中键";
                GroupMouseMiddle = dxGroupBox6;

                CHK_ITEM_SET[] chkItemSetArray1 = new CHK_ITEM_SET[10];
                int index1 = 0;
                CHK_ITEM_SET chkItemSet1 = new CHK_ITEM_SET();
                //chkItemSet1.name = "数字显血";
                ////chkItemSet1.
                //chkItemSet1.state = Config.数字显血;
                //chkItemSet1.method = ((o, e) =>
                //{
                //    DXCheckBox dxCheckBox = o as DXCheckBox;
                //    Config.数字显血 = dxCheckBox != null && dxCheckBox.Checked;
                //});
                //CHK_ITEM_SET chkItemSet2 = chkItemSet1;
                //chkItemSetArray1[index1] = chkItemSet2;

                //CHK_ITEM_SET Mianzhupao = new CHK_ITEM_SET();
                //Mianzhupao.name = "免助跑";
                //Mianzhupao.state = true;
                //Mianzhupao.method = ((o, e) =>
                //{
                //    DXCheckBox dxCheckBox = o as DXCheckBox;
                //    Config.免助跑 = dxCheckBox != null && dxCheckBox.Checked;
                //});
                //chkItemSetArray1[1] = Mianzhupao;

                CHK_ITEM_SET Mianlazhu = new CHK_ITEM_SET();
                Mianlazhu.name = "免蜡烛";
                Mianlazhu.state = Config.免蜡烛;
                Mianlazhu.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.免蜡烛 = dxCheckBox != null && dxCheckBox.Checked;
                    GameScene.Game?.MapControl?.UpdateLights();
                });
                chkItemSetArray1[index1] = Mianlazhu;
                index1++;

                CHK_ITEM_SET Mingzixianshi = new CHK_ITEM_SET();
                Mingzixianshi.name = "名字显示";
                Mingzixianshi.state = Config.ShowPlayerNames;
                Mingzixianshi.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.ShowPlayerNames = dxCheckBox != null && dxCheckBox.Checked;
                });
                chkItemSetArray1[index1] = Mingzixianshi;
                index1++;

                CHK_ITEM_SET Qinglishiti = new CHK_ITEM_SET();
                Qinglishiti.name = "清理尸体";
                Qinglishiti.state = Config.清理尸体;
                Qinglishiti.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.清理尸体 = dxCheckBox != null && dxCheckBox.Checked;
                });
                chkItemSetArray1[index1] = Qinglishiti;
                index1++;

                CHK_ITEM_SET Zaianquanchuyouxiao = new CHK_ITEM_SET();
                Zaianquanchuyouxiao.name = "在安全处有效";
                Zaianquanchuyouxiao.state = Config.在安全处有效;
                Zaianquanchuyouxiao.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.在安全处有效 = dxCheckBox != null && dxCheckBox.Checked;
                });
                chkItemSetArray1[index1] = Zaianquanchuyouxiao;
                index1++;

                //CHK_ITEM_SET Zuduishuzuxianxue = new CHK_ITEM_SET();
                //Zuduishuzuxianxue.name = "组队数字显血";
                //Zuduishuzuxianxue.state = Config.组队数字显血;
                //Zuduishuzuxianxue.method = ((o, e) =>
                //{
                //    DXCheckBox dxCheckBox = o as DXCheckBox;
                //    Config.组队数字显血 = dxCheckBox != null && dxCheckBox.Checked;
                //});
                //chkItemSetArray1[index1] = Zuduishuzuxianxue;
                //index1++;

                CHK_ITEM_SET Bosstishi = new CHK_ITEM_SET();
                Bosstishi.name = "Boss提示";
                Bosstishi.state = Config.Boss提示;
                Bosstishi.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.Boss提示 = dxCheckBox != null && dxCheckBox.Checked;
                });
                chkItemSetArray1[index1] = Bosstishi;
                index1++;

                CHK_ITEM_SET Zidongtexiu = new CHK_ITEM_SET();
                Zidongtexiu.name = "自动特修物品";
                Zidongtexiu.state = Config.SpecialRepair;
                Zidongtexiu.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.SpecialRepair = dxCheckBox != null && dxCheckBox.Checked;
                });
                chkItemSetArray1[index1] = Zidongtexiu;
                index1++;

                CHK_ITEM_SET Paobuting = new CHK_ITEM_SET();
                Paobuting.name = "跑不停";
                Paobuting.state = Config.跑不停;
                Paobuting.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.跑不停 = dxCheckBox != null && dxCheckBox.Checked;

                    GameScene.Game.AutoRun = Config.跑不停 == false ? false : true;
                });
                chkItemSetArray1[index1] = Paobuting;
                index1++;

                CHK_ITEM_SET Guaiwuxinxi = new CHK_ITEM_SET();
                Guaiwuxinxi.name = "怪物信息";
                Guaiwuxinxi.state = Config.MonsterBoxVisible;
                Guaiwuxinxi.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.MonsterBoxVisible = dxCheckBox != null && dxCheckBox.Checked;
                });
                chkItemSetArray1[index1] = Guaiwuxinxi;
                index1++;

                CHK_ITEM_SET Zidongguanzu = new CHK_ITEM_SET();
                Zidongguanzu.name = "自动关组";
                Zidongguanzu.state = Config.自动关组;
                Zidongguanzu.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.自动关组 = dxCheckBox != null && dxCheckBox.Checked;
                });
                chkItemSetArray1[index1] = Zidongguanzu;
                index1++;

                CHK_ITEM_SET Guaimingzixianshi = new CHK_ITEM_SET();
                Guaimingzixianshi.name = "怪物名字显示";
                Guaimingzixianshi.state = Config.ShowMonsterNames;
                Guaimingzixianshi.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.ShowMonsterNames = dxCheckBox != null && dxCheckBox.Checked;
                });
                chkItemSetArray1[index1] = Guaimingzixianshi;
                index1++;


                CHK_ITEM_SET[] chkItemSetArray2 = chkItemSetArray1;
                for (int index2 = 0; index2 < chkItemSetArray2.Length; ++index2)
                {
                    BigPatchDialog.CreateCheckBox(GroupNormal, chkItemSetArray2[index2].name, x1, num1, chkItemSetArray2[index2].method, chkItemSetArray2[index2].state);
                    num1 += 24;
                }
                BigPatchDialog.DXGroupBox groupNormal = GroupNormal;
                Size size1 = GroupNormal.Size;
                Size size2 = new Size(size1.Width, num1);
                groupNormal.Size = size2;
                BigPatchDialog.DXCommonlyTab.CHK_ITEM_SET[] chkItemSetArray3 = new CHK_ITEM_SET[3];


                index1 = 0;
                chkItemSet1 = new CHK_ITEM_SET();
                chkItemSet1.name = "免SHIFT";
                chkItemSet1.state = Config.免SHIFT;
                chkItemSet1.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.免SHIFT = dxCheckBox != null && dxCheckBox.Checked;
                });
                CHK_ITEM_SET chkItemSet3 = chkItemSet1;
                chkItemSetArray3[index1] = chkItemSet3;
                index1++;

                //chkItemSet1 = new BigPatchDialog.DXCommonlyTab.CHK_ITEM_SET();
                //chkItemSet1.name = "攻击锁定★";
                //chkItemSet1.state = Config.攻击锁定目标;
                //chkItemSet1.method = ((o, e) =>
                //{
                //    DXCheckBox dxCheckBox = o as DXCheckBox;
                //    Config.攻击锁定目标 = dxCheckBox != null && dxCheckBox.Checked;
                //});
                //CHK_ITEM_SET chkItemSet4 = chkItemSet1;
                //chkItemSetArray3[index1] = chkItemSet4;
                //index1++;

                chkItemSet1 = new CHK_ITEM_SET();
                chkItemSet1.name = "数字飘血";
                chkItemSet1.state = Config.ShowDamageNumbers;
                chkItemSet1.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.ShowDamageNumbers = dxCheckBox != null && dxCheckBox.Checked;
                });
                CHK_ITEM_SET shuzipiaoxue = chkItemSet1;
                chkItemSetArray3[index1] = shuzipiaoxue;
                index1++;

                chkItemSet1 = new CHK_ITEM_SET();
                chkItemSet1.name = "关闭经验提示";
                chkItemSet1.state = Config.关闭经验提示;
                chkItemSet1.method = ((o, e) =>
                {
                    DXCheckBox dxCheckBox = o as DXCheckBox;
                    Config.关闭经验提示 = dxCheckBox != null && dxCheckBox.Checked;
                });
                CHK_ITEM_SET jingyantishi = chkItemSet1;
                chkItemSetArray3[index1] = jingyantishi;
                index1++;

                //chkItemSet1 = new CHK_ITEM_SET();
                //chkItemSet1.name = "锁怪效果★";
                //chkItemSet1.state = Config.锁怪效果;
                //chkItemSet1.method = ((o, e) =>
                //{
                //    DXCheckBox dxCheckBox = o as DXCheckBox;
                //    Config.锁怪效果 = dxCheckBox != null && dxCheckBox.Checked;
                //});
                //CHK_ITEM_SET suoguaixiaoguo = chkItemSet1;
                //chkItemSetArray3[index1] = suoguaixiaoguo;
                //index1++;

                //chkItemSet1 = new CHK_ITEM_SET();
                //chkItemSet1.name = "稳如泰山★";
                //chkItemSet1.state = Config.稳如泰山;
                //chkItemSet1.method = ((o, e) =>
                //{
                //    DXCheckBox dxCheckBox = o as DXCheckBox;
                //    Config.稳如泰山 = dxCheckBox != null && dxCheckBox.Checked;
                //});
                //CHK_ITEM_SET wenrutaishan = chkItemSet1;
                //chkItemSetArray3[index1] = wenrutaishan;
                //index1++;

                //chkItemSet1 = new CHK_ITEM_SET();
                //chkItemSet1.name = "转生残影★";
                //chkItemSet1.state = Config.转生残影;
                //chkItemSet1.method = ((o, e) =>
                //{
                //    DXCheckBox dxCheckBox = o as DXCheckBox;
                //    Config.转生残影 = dxCheckBox != null && dxCheckBox.Checked;
                //});
                //CHK_ITEM_SET zhuanshencanying = chkItemSet1;
                //chkItemSetArray3[index1] = zhuanshencanying;
                //index1++;

                //chkItemSet1 = new CHK_ITEM_SET();
                //chkItemSet1.name = "死亡红屏★";
                //chkItemSet1.state = Config.死亡红屏;
                //chkItemSet1.method = ((o, e) =>
                //{
                //    DXCheckBox dxCheckBox = o as DXCheckBox;
                //    Config.死亡红屏 = dxCheckBox != null && dxCheckBox.Checked;
                //});
                //CHK_ITEM_SET siwanghongping = chkItemSet1;
                //chkItemSetArray3[index1] = siwanghongping;
                //index1++;

                CHK_ITEM_SET[] chkItemSetArray4 = chkItemSetArray3;
                int num3 = 30;
                for (int index2 = 0; index2 < chkItemSetArray4.Length; ++index2)
                {
                    CreateCheckBox(GroupWar, chkItemSetArray4[index2].name, x1, num3, chkItemSetArray4[index2].method, chkItemSetArray4[index2].state);
                    num3 += num2;
                }
                DXGroupBox groupWar = GroupWar;
                size1 = GroupWar.Size;
                Size size3 = new Size(size1.Width, num3);
                groupWar.Size = size3;
                int y1 = 30;
                ShowItemNames = CreateCheckBox(GroupItem, "物品名称显示", x1, y1, ((o, e) => Config.ShowItemNames = ShowItemNames.Checked), Config.ShowItemNames);
                int y2 = y1 + num2;
                ChkAutoPick = CreateCheckBox(GroupItem, "快速自动捡取", x1, y2, ((o, e) => Config.快速自动拾取 = ChkAutoPick.Checked), Config.快速自动拾取);
                ChkAutoPick.CheckedChanged += ((o, e) =>
                {
                    ChkAutoPick_CheckedChanged();
                });

                int y4 = y2 + num2;
                ChkTabPick = BigPatchDialog.CreateCheckBox(GroupItem, "Tab捡取", x1, y4, ((o, e) => Config.Tab捡取 = ChkTabPick.Checked), Config.Tab捡取);
                ChkTabPick.CheckedChanged += ((o, e) =>
                {
                    ChkTabPick_CheckedChanged();
                });

                int y5 = y4 + num2;
                ChkAutoBook = BigPatchDialog.CreateCheckBox(GroupItem, "自动学习技能书", x1, y5, ((o, e) => Config.自动学习技能书 = ChkAutoBook.Checked), Config.自动学习技能书);
                ChkAutoBook.CheckedChanged += ((o, e) =>
                {
                    ChkTabPick_CheckedChanged();
                });

                BigPatchDialog.DXGroupBox groupItem = GroupItem;
                size1 = GroupItem.Size;
                Size size4 = new Size(size1.Width, y5 + num2);
                groupItem.Size = size4;
                GroupWeather.Location = new Point(GroupWar.Location.X, y4 + num2 + 12);
                int num4 = 30;
                DXComboBox dxComboBox1 = new DXComboBox();
                dxComboBox1.Parent = GroupWeather;
                CombWeather = dxComboBox1;
                CombWeather.Location = new Point(35, 45);
                int num5 = num4;
                size1 = CombWeather.Size;
                int num6 = size1.Height + num2;
                int num7 = num5 + num6;
                //DXListBoxItem dxListBoxItem1 = new DXListBoxItem();
                //dxListBoxItem1.Parent = CombWeather.ListBox;
                //dxListBoxItem1.Label.Text = "未选择";
                //dxListBoxItem1.Item = (object)WeatherSetting.None;
                //DXListBoxItem dxListBoxItem2 = new DXListBoxItem();
                //dxListBoxItem2.Parent = CombWeather.ListBox;
                //dxListBoxItem2.Label.Text = "晴";
                //dxListBoxItem2.Item = (object)WeatherSetting.Default;
                //DXListBoxItem dxListBoxItem3 = new DXListBoxItem();
                //dxListBoxItem3.Parent = CombWeather.ListBox;
                //dxListBoxItem3.Label.Text = "雾";
                //dxListBoxItem3.Item = (object)WeatherSetting.Fog;
                //DXListBoxItem dxListBoxItem4 = new DXListBoxItem();
                //dxListBoxItem4.Parent = CombWeather.ListBox;
                //dxListBoxItem4.Label.Text = "燃烧的雾";
                //dxListBoxItem4.Item = (object)WeatherSetting.BurningFog;
                //DXListBoxItem dxListBoxItem5 = new DXListBoxItem();
                //dxListBoxItem5.Parent = CombWeather.ListBox;
                //dxListBoxItem5.Label.Text = "雪";
                //dxListBoxItem5.Item = (object)WeatherSetting.Snow;
                //DXListBoxItem dxListBoxItem6 = new DXListBoxItem();
                //dxListBoxItem6.Parent = CombWeather.ListBox;
                //dxListBoxItem6.Label.Text = "花瓣雨";
                //dxListBoxItem6.Item = (object)WeatherSetting.Everfall;
                //DXListBoxItem dxListBoxItem7 = new DXListBoxItem();
                //dxListBoxItem7.Parent = CombWeather.ListBox;
                //dxListBoxItem7.Label.Text = "雨";
                //dxListBoxItem7.Item = (object)WeatherSetting.Rain;
                //CombWeather.ListBox.SelectItem((object)Config.天气效果);
                //CombWeather.SelectedItemChanged += ((o, e) =>
                //{
                //    Config.天气效果 = (int)(WeatherSetting)(o as DXComboBox).ListBox.SelectedItem.Item;
                //    GameScene.Game.MapControl.UpdateWeather();
                //});
                //CombWeather.Size = new Size(100, 18);
                BigPatchDialog.DXGroupBox groupWeather = GroupWeather;
                size1 = GroupWar.Size;
                Size size5 = new Size(size1.Width, num7 + num2);
                groupWeather.Size = size5;
                //BigPatchDialog.DXGroupBox groupAutoAttack1 = GroupAutoAttack;
                //int x2 = GroupItem.Location.X;
                //int y40 = GroupItem.Location.Y;
                //size1 = GroupItem.Size;
                //int height1 = size1.Height;
                //int y5 = y40 + height1 + 10;
                //Point point1 = new Point(x2, y5);
                //groupAutoAttack1.Location = point1;
                //int y6 = 30;
                //ChkAutoFire = CreateCheckBox(GroupAutoAttack, "自动练技能", x1, y6, ((o, e) => Config.是否开启自动练技能 = ChkAutoFire.Checked), Config.是否开启自动练技能);
                //DXComboBox dxComboBox2 = new DXComboBox();
                //dxComboBox2.Parent = GroupAutoAttack;
                //CombAutoFire = dxComboBox2;
                //DXListBoxItem dxListBoxItem8 = new DXListBoxItem();
                //dxListBoxItem8.Parent = CombAutoFire.ListBox;
                //dxListBoxItem8.Label.Text = "空";
                //dxListBoxItem8.Item = (object)0;
                int num8;
                for (int index2 = 0; index2 <= 11; ++index2)
                {
                    //DXListBoxItem dxListBoxItem9 = new DXListBoxItem();
                    //dxListBoxItem9.Parent = CombAutoFire.ListBox;
                    //DXLabel label = dxListBoxItem9.Label;
                    string str1 = "F";
                    num8 = index2 + 1;
                    string str2 = num8.ToString();
                    string str3 = str1 + str2;
                    //label.Text = str3;
                    //dxListBoxItem9.Item = (object)(index2 + 1);
                }
                //CombAutoFire.SelectedItemChanged += ((o, e) => Config.自动练F几技能 = (int)CombAutoFire.ListBox.SelectedItem.Item);
                //CombAutoFire.ListBox.SelectItem((object)Config.自动练F几技能);
                //CombAutoFire.Size = new Size(50, 18);
                //DXComboBox combAutoFire = CombAutoFire;
                //int x3 = ChkAutoFire.Location.X;
                //size1 = ChkAutoFire.Size;
                //int width1 = size1.Width;
                //Point point2 = new Point(x3 + width1 + 5, ChkAutoFire.Location.Y);
                //combAutoFire.Location = point2;
                //int y7 = y6 + num2;
                //DXLabel dxLabel = new DXLabel();
                //dxLabel.Parent = GroupAutoAttack;
                //dxLabel.Text = "间隔:";
                //dxLabel.Location = new Point(x1, y7);
                //LabAutoFire = dxLabel;
                //DXNumberBox dxNumberBox = new DXNumberBox();
                //dxNumberBox.Parent = GroupAutoAttack;
                //int num9 = x1;
                //size1 = LabAutoFire.Size;
                //int width2 = size1.Width;
                //dxNumberBox.Location = new Point(num9 + width2, y7);
                //dxNumberBox.Size = new Size(80, 20);
                //dxNumberBox.ValueTextBox.Size = new Size(40, 18);
                //dxNumberBox.MaxValue = 100L;
                //dxNumberBox.MinValue = 1L;
                //dxNumberBox.Value = Config.隔多少秒自动练技能;
                //dxNumberBox.UpButton.Location = new Point(63, 1);
                //NumberAutoFireInterval = dxNumberBox;
                //NumberAutoFireInterval.ValueTextBox.ValueChanged += ((o, e) => Config.隔多少秒自动练技能 = NumberAutoFireInterval.Value);
                //int num10 = y7 + num2;
                //BigPatchDialog.DXGroupBox groupAutoAttack2 = GroupAutoAttack;
                //size1 = GroupItem.Size;
                //Size size6 = new Size(size1.Width, num10 + 10);
                //groupAutoAttack2.Size = size6;
                int y8 = 30;
                ChkCallMounts = CreateCheckBox(GroupMouseMiddle, "骑马", x1, y8, ((o, e) =>
                {
                    if (ChkCallMounts.Checked)
                        ChkCastingMagic.Checked = !ChkCallMounts.Checked;
                    Config.是否开启鼠标中间按钮自动使用坐骑 = ChkCallMounts.Checked;
                }), Config.是否开启鼠标中间按钮自动使用坐骑);

                int y9 = y8 + num2;
                ChkCastingMagic = BigPatchDialog.CreateCheckBox(GroupMouseMiddle, "魔法", x1, y9, ((o, e) =>
                {
                    if (ChkCastingMagic.Checked)
                        ChkCallMounts.Checked = !ChkCastingMagic.Checked;
                    Config.是否开启鼠标中间按钮自动使用技能 = ChkCastingMagic.Checked;
                }), Config.是否开启鼠标中间按钮自动使用技能);
                DXComboBox dxComboBox3 = new DXComboBox();
                dxComboBox3.Parent = GroupMouseMiddle;
                CombMiddleMouse = dxComboBox3;
                DXListBoxItem dxListBoxItem10 = new DXListBoxItem();
                dxListBoxItem10.Parent = CombMiddleMouse.ListBox;
                dxListBoxItem10.Label.Text = "空";
                dxListBoxItem10.Item = (object)0;
                for (int index2 = 0; index2 <= 11; ++index2)
                {
                    DXListBoxItem dxListBoxItem9 = new DXListBoxItem();
                    dxListBoxItem9.Parent = CombMiddleMouse.ListBox;
                    DXLabel label = dxListBoxItem9.Label;
                    string str1 = "F";
                    num8 = index2 + 1;
                    string str2 = num8.ToString();
                    string str3 = str1 + str2;
                    label.Text = str3;
                    dxListBoxItem9.Item = (object)(index2 + 1);
                }
                CombMiddleMouse.SelectedItemChanged += ((o, e) => Config.鼠标中间按钮使用F几的技能 = (int)CombMiddleMouse.ListBox.SelectedItem.Item);
                CombMiddleMouse.ListBox.SelectItem((object)Config.鼠标中间按钮使用F几的技能);
                CombMiddleMouse.Size = new Size(50, 18);
                CombMiddleMouse.Location = new Point(x1 + ChkCastingMagic.DisplayArea.Right, y9);
                int num11 = y9 + num2;
                BigPatchDialog.DXGroupBox groupMouseMiddle1 = GroupMouseMiddle;
                Point location = GroupItem.Location;
                int x4 = location.X;
                location = groupWar.Location;
                int y11 = location.Y;
                size1 = GroupItem.Size;
                int height2 = size1.Height;
                int y12 = y11 + height2 + 10;
                Point point3 = new Point(x4, y12);
                groupMouseMiddle1.Location = point3;
                BigPatchDialog.DXGroupBox groupMouseMiddle2 = GroupMouseMiddle;
                size1 = GroupItem.Size;
                Size size7 = new Size(size1.Width, num11 + 10);
                groupMouseMiddle2.Size = size7;
                int x5 = GroupMouseMiddle.Location.X;
                int y13 = num11 + 5;
                DXButton dxButton1 = new DXButton();
                dxButton1.Parent = this;
                dxButton1.Size = new Size(70, 18);
                dxButton1.Location = new Point(x5, y13);
                dxButton1.ButtonType = ButtonType.SmallButton;
                dxButton1.Label.Text = "刷新包裹";
                RefreshBag = dxButton1;
                RefreshBag.MouseClick += (EventHandler<MouseEventArgs>)((o, e) =>
                {
                    if (GameScene.Game.Observer)
                        return;
                    CEnvir.Enqueue((Packet)new SortBagItem());
                });
                int y14 = y13 + num2;
                DXButton dxButton2 = new DXButton();
                dxButton2.Parent = this;
                dxButton2.Size = new Size(70, 18);
                dxButton2.Location = new Point(x5, y14);
                dxButton2.ButtonType = ButtonType.SmallButton;
                dxButton2.Label.Text = "重置设置";
                ReloadConfig = dxButton2;
                ReloadConfig.MouseClick += (EventHandler<MouseEventArgs>)((o, e) => { });
            }
            private void ChkAutoPick_CheckedChanged()
            {
                if (ChkAutoPick.Checked)
                    ChkTabPick.Checked = false;
            }

            private void ChkTabPick_CheckedChanged()
            {
                if (ChkTabPick.Checked)
                    ChkAutoPick.Checked = false;
            }
            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
                Size size1;
                if (GroupNormal != null)
                {
                    GroupNormal.Location = new Point(10, 0);
                    BigPatchDialog.DXGroupBox groupNormal = GroupNormal;
                    size1 = Size;
                    int width = (size1.Width - 40) / 3;
                    size1 = GroupNormal.Size;
                    int height = size1.Height;
                    Size size2 = new Size(width, height);
                    groupNormal.Size = size2;
                }
                if (GroupWar != null)
                {
                    BigPatchDialog.DXGroupBox groupWar1 = GroupWar;
                    size1 = Size;
                    int width1 = (size1.Width - 40) / 3;
                    size1 = GroupWar.Size;
                    int height = size1.Height;
                    Size size2 = new Size(width1, height);
                    groupWar1.Size = size2;
                    BigPatchDialog.DXGroupBox groupWar2 = GroupWar;
                    int x = GroupNormal.Location.X;
                    size1 = GroupNormal.Size;
                    int width2 = size1.Width;
                    Point point = new Point(x + width2 + 10, 0);
                    groupWar2.Location = point;
                }
                if (GroupItem != null)
                {
                    BigPatchDialog.DXGroupBox groupItem1 = GroupItem;
                    int x = GroupWar.Location.X;
                    size1 = GroupWar.Size;
                    int width1 = size1.Width;
                    Point point = new Point(x + width1 + 10, 0);
                    groupItem1.Location = point;
                    BigPatchDialog.DXGroupBox groupItem2 = GroupItem;
                    size1 = Size;
                    int width2 = (size1.Width - 40) / 3;
                    size1 = GroupItem.Size;
                    int height = size1.Height;
                    Size size2 = new Size(width2, height);
                    groupItem2.Size = size2;
                }
                Point location;
                if (GroupWeather != null)
                {
                    BigPatchDialog.DXGroupBox groupWeather1 = GroupWeather;
                    int x = GroupWar.Location.X;
                    location = GroupWar.Location;
                    int y1 = location.Y;
                    size1 = GroupWar.Size;
                    int height1 = size1.Height;
                    int y2 = y1 + height1;
                    Point point = new Point(x, y2);
                    groupWeather1.Location = point;
                    BigPatchDialog.DXGroupBox groupWeather2 = GroupWeather;
                    size1 = GroupWar.Size;
                    int width = size1.Width;
                    size1 = GroupWeather.Size;
                    int height2 = size1.Height;
                    Size size2 = new Size(width, height2);
                    groupWeather2.Size = size2;
                }
                //if (GroupAutoAttack != null)
                //{
                //    BigPatchDialog.DXGroupBox groupAutoAttack1 = GroupAutoAttack;
                //    location = GroupItem.Location;
                //    int x = location.X;
                //    location = GroupItem.Location;
                //    int y1 = location.Y;
                //    size1 = GroupItem.Size;
                //    int height1 = size1.Height;
                //    int y2 = y1 + height1 + 10;
                //    Point point = new Point(x, y2);
                //    groupAutoAttack1.Location = point;
                //    BigPatchDialog.DXGroupBox groupAutoAttack2 = GroupAutoAttack;
                //    size1 = GroupItem.Size;
                //    int width = size1.Width;
                //    size1 = GroupAutoAttack.Size;
                //    int height2 = size1.Height;
                //    Size size2 = new Size(width, height2);
                //    groupAutoAttack2.Size = size2;
                //}
                if (GroupMouseMiddle != null)
                {
                    BigPatchDialog.DXGroupBox groupMouseMiddle1 = GroupMouseMiddle;
                    location = GroupItem.Location;
                    int x = location.X;
                    location = GroupItem.Location;
                    int y1 = location.Y;
                    size1 = GroupItem.Size;
                    int height1 = size1.Height;
                    int y2 = y1 + height1 + 10;
                    Point point = new Point(x, y2);
                    groupMouseMiddle1.Location = point;
                    BigPatchDialog.DXGroupBox groupMouseMiddle2 = GroupMouseMiddle;
                    size1 = GroupItem.Size;
                    int width = size1.Width;
                    size1 = GroupMouseMiddle.Size;
                    int height2 = size1.Height;
                    Size size2 = new Size(width, height2);
                    groupMouseMiddle2.Size = size2;
                }
                if (RefreshBag != null)
                {
                    location = GroupMouseMiddle.Location;
                    int x = location.X;
                    location = GroupMouseMiddle.Location;
                    int y1 = location.Y;
                    size1 = GroupMouseMiddle.Size;
                    int height = size1.Height;
                    int y2 = y1 + height + 10;
                    RefreshBag.Location = new Point(x + 5, y2);
                }
                if (ReloadConfig == null)
                    return;
                location = RefreshBag.Location;
                int x1 = location.X;
                size1 = RefreshBag.Size;
                int width3 = size1.Width;
                int x2 = x1 + width3 + 20;
                location = RefreshBag.Location;
                int y = location.Y;
                ReloadConfig.Location = new Point(x2, y);
            }

            public struct CHK_ITEM_SET
            {
                public string name;
                public bool state;
                public EventHandler<EventArgs> method;
            }
        }

        public class DXPlayerHelperTab : DXTab
        {
            public BigPatchDialog.DXGroupBox Warrior;
            public DXCheckBox AutoFlamingSword;
            public DXCheckBox AutoDragobRise;
            public DXCheckBox AutoBladeStorm;
            public DXCheckBox AutoDefiance;
            public DXCheckBox AutoMight;
            public DXCheckBox AutoReflectDamage;
            public DXCheckBox AutoEndurance;
            public DXCheckBox AutoHalfMoon;
            public DXCheckBox AutoDestructiveSurge;


            public DXCheckBox AutoThreeAct;
            public DXComboBox ThreeAct;
            public DXCheckBox AutoFourAct;
            public DXComboBox FourAct;
            public DXCheckBox AutoFiveAct;
            public DXComboBox FiveAct;
            public BigPatchDialog.DXGroupBox Wizard;
            public DXCheckBox AutoMagicShield;
            public DXCheckBox AutoRenounce;
            public DXCheckBox AutoThunder;
            //public DXCheckBox AutoSuperiorMagicShield;
            public DXCheckBox AutoWizardSkill;
            public BigPatchDialog.DXGroupBox Taoist;
            public DXCheckBox AutoPoisonDust;
            public DXCheckBox AutoAmulet;
            public DXCheckBox AutoCelestial;
            public DXCheckBox AutoTaoistSkill;
            public DXCheckBox AutoStrengthOfFaith;
            public DXCheckBox AutoLifeSteal;
            public DXCheckBox AutoMagicResistance;
            public DXCheckBox AutoResilience;
            public DXCheckBox AutoElementalSuperiority;
            public DXCheckBox AutoBloodLust;
            public DXCheckBox AutoKeepLife;


            public BigPatchDialog.DXGroupBox Assassin;
            public DXCheckBox AutoFourFlowers;
            public DXCheckBox AutoEvasion;
            public DXCheckBox AutoRagingWind;
            public BigPatchDialog.DXGroupBox AutoSkill;
            public DXComboBox CombSkill1 { get; set; }
            public DXNumberBox NumbSkill1;
            public DXCheckBox AutoSkill_1;
            public DXComboBox CombSkill2;
            public DXNumberBox NumbSkill2;
            public DXCheckBox AutoSkill_2;
            public DXCheckBox ChkBufTimer;
            public DXCheckBox ChkAutoAddEnemy;
            public DXCheckBox ChkPkMode;
            public DXCheckBox ChkPkDrink;
            public DXLabel LabCommand;
            public DXComboBox CombCmdBox;
            public DXButton BtSubmit;
            public BigPatchDialog.DXGroupBox Android;
            public DXLabel TimeLable;
            public DXCheckBox AndroidPlayer;
            public DXCheckBox AndroidPickUp;
            public DXCheckBox AndroidPoisonDust;
            public DXCheckBox AndroidEluded;
            public DXCheckBox AndroidBackCastle;
            public DXCheckBox AndroidSingleSkill;
            public DXComboBox AndroidSkills;
            public DXNumberBox AndroidCoordX;
            public DXNumberBox AndroidCoordY;
            public DXNumberBox AndroidCoordRange;
            public DXCheckBox AndroidLockRange;
            public DXNumberBox AndroidBackCastleMinPHValue;
            public DXCheckBox AndroidMinPHBackCastle;
            public DXNumberBox AndroidRandomMinPHValue;
            public DXCheckBox AndroidMinPHRandom;
            public DXNumberBox TimeBoxRandom;
            public DXCheckBox ChkAutoRandom;
            public DXNumberBox ExpTimeBoxRandom;
            public DXCheckBox ExpAutoRandom;
            public DXLabel SummerLabelEx, SummerLabelKill, SummerLabelGold, SummerLabelHuntGold, SummerLabelExfen, SummerLabelKillfen, SummerLabelGoldfen, SummerLabelHuntGoldfen, Guajishijian;
            public int skilledCount, LastGainExp;
            public decimal totalExperience, totalGold, totalHuntGold, totalExperiencefen, totalGoldfen, totalHuntGoldfen;
            private static DateTime LastTimes = DateTime.Now;
            private static DateTime LastGainEx = DateTime.Now;
            public DateTime _ProtectTime;


            public DXPlayerHelperTab()
            {
                DXGroupBox dxGroupBox1 = new DXGroupBox();
                dxGroupBox1.Parent = this;
                dxGroupBox1.Size = new Size(120, 32);
                dxGroupBox1.Location = new Point(10, 0);
                dxGroupBox1.Name.Text = "战士";
                dxGroupBox1.Visible = false;
                Warrior = dxGroupBox1;
                DXGroupBox dxGroupBox2 = new DXGroupBox();
                dxGroupBox2.Parent = this;
                dxGroupBox2.Size = new Size(120, 32);
                dxGroupBox2.Location = new Point(10, 0);
                dxGroupBox2.Name.Text = "法师";
                dxGroupBox2.Visible = false;
                Wizard = dxGroupBox2;
                DXGroupBox dxGroupBox3 = new DXGroupBox();
                dxGroupBox3.Parent = this;
                dxGroupBox3.Size = new Size(120, 32);
                dxGroupBox3.Location = new Point(10, 0);
                dxGroupBox3.Name.Text = "道士";
                dxGroupBox3.Visible = false;
                Taoist = dxGroupBox3;
                DXGroupBox dxGroupBox4 = new DXGroupBox();
                dxGroupBox4.Parent = this;
                dxGroupBox4.Size = new Size(120, 32);
                dxGroupBox4.Location = new Point(10, 0);
                dxGroupBox4.Name.Text = "刺客";
                dxGroupBox4.Visible = false;
                Assassin = dxGroupBox4;
                DXGroupBox dxGroupBox5 = new DXGroupBox();
                dxGroupBox5.Parent = this;
                dxGroupBox5.Size = new Size(120, 32);
                dxGroupBox5.Location = new Point(10, 0);
                dxGroupBox5.Name.Text = "自动技能";
                AutoSkill = dxGroupBox5;
                DXGroupBox dxGroupBox6 = new DXGroupBox();
                dxGroupBox6.Parent = this;
                dxGroupBox6.Size = new Size(120, 32);
                dxGroupBox6.Location = new Point(130, 0);
                dxGroupBox6.Name.Text = "挂机";
                dxGroupBox6.Visible = true;

                Android = dxGroupBox6;
   
                int x1 = 15;
                int num1 = 5;
                int y1; 
                AutoFlamingSword = CreateCheckBox(Warrior, "自动烈火", x1, y1 = num1 + 25, ((o, e) => Config.自动烈火 = AutoFlamingSword.Checked), Config.自动烈火);
                AutoFlamingSword.CheckedChanged += ((o, e) =>
                {
                    if (GameScene.Game.Observer)
                        return;
                    CEnvir.Enqueue(new AutoFightConfChanged()
                    {
                        Enabled = AutoFlamingSword.Checked,
                        Slot = AutoSetConf.SetFlamingSwordBox
                    });
                });
                AutoDragobRise = CreateCheckBox(Warrior, "自动翔空", x1 + 120, y1, ((o, e) => Config.自动翔空 = AutoDragobRise.Checked), Config.自动翔空);
                AutoDragobRise.CheckedChanged += ((o, e) =>
                {
                    if (GameScene.Game.Observer)
                        return;
                    CEnvir.Enqueue(new AutoFightConfChanged()
                    {
                        Enabled = AutoDragobRise.Checked,
                        Slot = AutoSetConf.SetDragobRiseBox
                    });
                });
                int y2;
                AutoBladeStorm = CreateCheckBox(Warrior, "自动莲月", x1, y2 = y1 + 25, ((o, e) => Config.自动莲月 = AutoBladeStorm.Checked), Config.自动莲月);
                AutoBladeStorm.CheckedChanged += ((o, e) =>
                {
                    if (GameScene.Game.Observer)
                        return;
                    CEnvir.Enqueue(new AutoFightConfChanged()
                    {
                        Enabled = AutoBladeStorm.Checked,
                        Slot = AutoSetConf.SetBladeStormBox
                    });
                });
                AutoDefiance = CreateCheckBox(Warrior, "自动铁布衫", x1 + 120, y2, ((o, e) => {
                    Config.自动铁布衫 = AutoDefiance.Checked;

                }), Config.自动铁布衫);
                int num2;
                AutoMight = CreateCheckBox(Warrior, "自动破血", x1, num2 = y2 + 25, ((o, e) => {
                    
                    Config.自动破血 = AutoMight.Checked;
                }), Config.自动破血);

                AutoReflectDamage = CreateCheckBox(Warrior, "自动移花接木", x1 + 120, num2, ((o, e) => {

                    Config.自动移花接木 = AutoReflectDamage.Checked;
                }), Config.自动移花接木);

                AutoEndurance = CreateCheckBox(Warrior, "自动金刚之躯", x1, num2 + 25, ((o, e) => {

                    Config.自动金刚之躯 = AutoEndurance.Checked;
                }), Config.自动金刚之躯);

                AutoHalfMoon = CreateCheckBox(Warrior, "智能半月弯刀", x1 + 120, num2 + 25, ((o, e) => {

                    Config.自动半月弯刀 = AutoHalfMoon.Checked;
                }), Config.自动半月弯刀);

                AutoDestructiveSurge = CreateCheckBox(Warrior, "智能十方斩", x1, num2 + 50, ((o, e) => {

                    Config.自动十方斩 = AutoDestructiveSurge.Checked;
                }), Config.自动十方斩);

                int num3 = 5;
                int num4;
                AutoMagicShield = CreateCheckBox(Wizard, "自动魔法盾", x1, num4 = num3 + 25, ((o, e) => Config.自动魔法盾 = AutoMagicShield.Checked), Config.自动魔法盾);
                //AutoMagicShield.CheckedChanged += ((o, e) =>
                //{
                //    AutoMagicShield_CheckedChanged();
                //});
                int num5;
                AutoRenounce = CreateCheckBox(Wizard, "自动凝血", x1, num5 = num4 + 25, ((o, e) => Config.自动凝血 = AutoRenounce.Checked), Config.自动凝血);
                AutoThunder = CreateCheckBox(Wizard, "自动天打雷劈", x1, num2 = num5 + 25, ((o, e) => Config.自动天打雷劈 = AutoThunder.Checked), Config.自动天打雷劈);
                //AutoSuperiorMagicShield = BigPatchDialog.CreateCheckBox(Wizard, "自动魔光盾", x1, num2 = num2 + 25, ((o, e) => Config.自动魔光盾 = AutoSuperiorMagicShield.Checked), Config.自动魔光盾);
                //AutoSuperiorMagicShield.CheckedChanged += ((o, e) =>
                //{
                //    AutoSuperiorMagicShield_CheckedChanged();
                //});

                AutoWizardSkill = CreateCheckBox(Wizard, "自动连续技能", x1, num2 = num2 + 25, ((o, e) => Config.自动法师连续技能 = AutoWizardSkill.Checked), Config.自动法师连续技能);


                int num6 = 5 + 25;
                int num7;
                AutoPoisonDust = CreateCheckBox(Taoist, "自动换毒", x1, num6, ((o, e) => Config.自动换毒 = AutoPoisonDust.Checked), Config.自动换毒);
                AutoAmulet = CreateCheckBox(Taoist, "自动换符", x1 + 120, num6, ((o, e) => Config.自动换符 = AutoAmulet.Checked), Config.自动换符);
                AutoCelestial = CreateCheckBox(Taoist, "自动阴阳盾", x1, num6 += 25, ((o, e) => Config.自动阴阳盾 = AutoCelestial.Checked), Config.自动阴阳盾);
                AutoTaoistSkill = CreateCheckBox(Taoist, "自动连续技能", x1 + 120, num6, ((o, e) => Config.自动道士连续技能 = AutoTaoistSkill.Checked), Config.自动道士连续技能);
                AutoLifeSteal = CreateCheckBox(Taoist, "自动吸星大法", x1, num6 += 25, ((o, e) => Config.自动吸星大法 = AutoLifeSteal.Checked), Config.自动吸星大法);
                AutoMagicResistance = CreateCheckBox(Taoist, "自动施放幽灵盾", x1 + 120, num6, ((o, e) => Config.自动施放幽灵盾 = AutoMagicResistance.Checked), Config.自动施放幽灵盾);

                AutoStrengthOfFaith = CreateCheckBox(Taoist, "有宠物时自动移花接玉", x1, num6 += 25, ((o, e) => Config.有宠物时自动移花接玉 = AutoStrengthOfFaith.Checked), Config.有宠物时自动移花接玉);
                AutoResilience = CreateCheckBox(Taoist, "自动施放神圣战甲", x1, num6 += 25, ((o, e) => Config.自动施放神圣战甲术 = AutoResilience.Checked), Config.自动施放神圣战甲术);
                AutoBloodLust = CreateCheckBox(Taoist, "自动给宠物施放猛虎强势", x1, num6 += 25, ((o, e) => Config.自动给宠物施放猛虎强势 = AutoBloodLust.Checked), Config.自动给宠物施放猛虎强势);
                AutoElementalSuperiority = CreateCheckBox(Taoist, "自动施放强魔震法", x1, num6 += 25, ((o, e) => Config.自动强魔震法 = AutoElementalSuperiority.Checked), Config.自动强魔震法);
                AutoKeepLife = CreateCheckBox(Taoist, "生命低于一半时自动施展秒影并恢复血量", x1, num6 += 25, ((o, e) => Config.自动施展秒影恢复血量 = AutoKeepLife.Checked), Config.自动施展秒影恢复血量);


                int num9 = 5;
                int num10;
                AutoFourFlowers = CreateCheckBox(Assassin, "自动四花", x1, num10 = num9 + 25, ((o, e) => Config.自动四花 = AutoFourFlowers.Checked), Config.自动四花);
                int num11;
                AutoEvasion = CreateCheckBox(Assassin, "自动风之闪避", x1, num11 = num10 + 25, ((o, e) => Config.自动风之闪避 = AutoEvasion.Checked), Config.自动风之闪避);
                AutoRagingWind = CreateCheckBox(Assassin, "自动风之守护", x1, num2 = num11 + 25, ((o, e) => Config.自动风之守护 = AutoRagingWind.Checked), Config.自动风之守护);
                int y3 = 35;
                DXLabel dxLabel1 = new DXLabel();
                dxLabel1.AutoSize = true;
                dxLabel1.Parent = AutoSkill;
                dxLabel1.Text = "技能Ⅰ";
                dxLabel1.Hint = "根据定义的时间间隔(秒单位)自动释放技能";
                DXLabel dxLabel2 = dxLabel1;
                dxLabel2.Location = new Point(x1, y3);
                DXComboBox dxComboBox1 = new DXComboBox();
                dxComboBox1.Parent = AutoSkill;
                dxComboBox1.Size = new Size(90, 18);
                CombSkill1 = dxComboBox1;
                CombSkill1.Location = new Point(dxLabel2.Location.X + dxLabel2.Size.Width + 5, y3);
                CombSkill1.SelectedItemChanged += ((o, e) =>
                {
                    MagicType selectedItem = (MagicType)CombSkill1.SelectedItem;
                    foreach (MagicInfo magicInfo in (IEnumerable<MagicInfo>)Globals.MagicInfoList.Binding)
                    {
                        if (magicInfo.Magic == selectedItem)
                        {
                            Config.自动技能1 = selectedItem;
                            if (NumbSkill1.Value >= (long)(magicInfo.Delay / 1000))
                                break;
                            NumbSkill1.Value = (long)(magicInfo.Delay / 1000);
                            break;
                        }
                    }
                });
                DXNumberBox dxNumberBox1 = new DXNumberBox();
                dxNumberBox1.Parent = AutoSkill;
                dxNumberBox1.Size = new Size(80, 20);
                dxNumberBox1.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox1.MaxValue = 50000L;
                dxNumberBox1.MinValue = 0L;
                dxNumberBox1.Value = Config.自动技能1多长时间使用一次;
                dxNumberBox1.UpButton.Location = new Point(63, 1);
                NumbSkill1 = dxNumberBox1;
                NumbSkill1.ValueTextBox.ValueChanged += ((o, e) => Config.自动技能1多长时间使用一次 = NumbSkill1.Value);
                DXNumberBox numbSkill1 = NumbSkill1;
                Point location1 = CombSkill1.Location;
                int x2 = location1.X + CombSkill1.Size.Width + 5;
                location1 = CombSkill1.Location;
                int y4 = location1.Y;
                Point point1 = new Point(x2, y4);
                numbSkill1.Location = point1;
                DXCheckBox dxCheckBox1 = new DXCheckBox();
                dxCheckBox1.AutoSize = true;
                dxCheckBox1.Parent = AutoSkill;
                dxCheckBox1.Checked = Config.是否开启自动技能1;
                dxCheckBox1.Hint = "自动技能\x2460";
                Point location2 = NumbSkill1.Location;
                int x3 = location2.X + NumbSkill1.Size.Width + 5;
                location2 = NumbSkill1.Location;
                int y5 = location2.Y + 2;
                dxCheckBox1.Location = new Point(x3, y5);
                AutoSkill_1 = dxCheckBox1;
                AutoSkill_1.CheckedChanged += ((o, e) => Config.是否开启自动技能1 = AutoSkill_1.Checked);
                int y6 = y3 + 25;
                DXLabel dxLabel3 = new DXLabel();
                dxLabel3.Parent = AutoSkill;
                dxLabel3.Text = "技能Ⅱ";
                dxLabel3.Hint = "根据定义的时间间隔(秒单位)自动释放技能";
                DXLabel dxLabel4 = dxLabel3;
                dxLabel4.Location = new Point(x1, y6);
                DXComboBox dxComboBox2 = new DXComboBox();
                dxComboBox2.Parent = AutoSkill;
                dxComboBox2.Size = new Size(90, 18);
                CombSkill2 = dxComboBox2;
                DXComboBox combSkill2 = CombSkill2;
                Point location3 = dxLabel4.Location;
                int x4 = location3.X + dxLabel4.Size.Width + 5;
                location3 = dxLabel4.Location;
                int y7 = location3.Y;
                Point point2 = new Point(x4, y7);
                combSkill2.Location = point2;
                CombSkill2.SelectedItemChanged += ((o, e) =>
                {
                    MagicType selectedItem = (MagicType)CombSkill2.SelectedItem;
                    foreach (MagicInfo magicInfo in (IEnumerable<MagicInfo>)Globals.MagicInfoList.Binding)
                    {
                        if (magicInfo.Magic == selectedItem)
                        {
                            Config.自动技能2 = selectedItem;
                            if (NumbSkill2.Value >= (long)(magicInfo.Delay / 1000))
                                break;
                            NumbSkill2.Value = (long)(magicInfo.Delay / 1000);
                            break;
                        }
                    }
                });
                DXNumberBox dxNumberBox2 = new DXNumberBox();
                dxNumberBox2.Parent = AutoSkill;
                dxNumberBox2.Size = new Size(80, 20);
                dxNumberBox2.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox2.MaxValue = 50000L;
                dxNumberBox2.MinValue = 0L;
                dxNumberBox2.Value = Config.自动技能2多长时间使用一次;
                dxNumberBox2.UpButton.Location = new Point(63, 1);
                NumbSkill2 = dxNumberBox2;
                NumbSkill2.ValueTextBox.ValueChanged += ((o, e) => Config.自动技能2多长时间使用一次 = NumbSkill2.Value);
                DXNumberBox numbSkill2 = NumbSkill2;
                Point location4 = CombSkill2.Location;
                int x5 = location4.X + CombSkill2.Size.Width + 5;
                location4 = CombSkill2.Location;
                int y8 = location4.Y;
                Point point3 = new Point(x5, y8);
                numbSkill2.Location = point3;
                DXCheckBox dxCheckBox2 = new DXCheckBox();
                dxCheckBox2.AutoSize = true;
                dxCheckBox2.Parent = AutoSkill;
                dxCheckBox2.Checked = Config.是否开启自动技能2;
                dxCheckBox2.Hint = "自动技能\x2461";
                Point location5 = NumbSkill2.Location;
                int x6 = location5.X + NumbSkill2.Size.Width + 5;
                location5 = NumbSkill2.Location;
                int y9 = location5.Y + 2;
                dxCheckBox2.Location = new Point(x6, y9);
                AutoSkill_2 = dxCheckBox2;
                AutoSkill_2.CheckedChanged += ((o, e) => Config.是否开启自动技能2 = AutoSkill_2.Checked);
                AutoSkill.Size = new Size(125, y6 + 25 + 5);
                DXLabel dxLabel5 = new DXLabel();
                dxLabel5.Parent = this;
                dxLabel5.Text = "特殊命令:";
                LabCommand = dxLabel5;
                DXComboBox dxComboBox3 = new DXComboBox();
                dxComboBox3.Parent = this;
                dxComboBox3.Size = new Size(180, 18);
                CombCmdBox = dxComboBox3;
                DXListBoxItem dxListBoxItem1 = new DXListBoxItem();
                dxListBoxItem1.Parent = CombCmdBox.ListBox;
                dxListBoxItem1.Label.Text = "空";
                dxListBoxItem1.Item = (object)null;
                string[] strArray = new string[]
                {
                   "@允许召唤",
                   "@队伍召唤",
                   "@宠物技能3",
                   "@宠物技能5",
                   "@宠物技能7",
                   "@宠物技能10",
                   "@宠物技能11",
                   "@宠物技能13",
                   "@宠物技能15",
                   "@允许交易",
                   "@允许加入行会",
                   "@退出行会",
                   "@属性提取",
                   "@摇骰子",
                };
                int dijihang = 0;
                foreach (string str in strArray)
                {
                    DXListBoxItem dxListBoxItem2 = new DXListBoxItem();
                    dxListBoxItem2.Parent = CombCmdBox.ListBox;
                    dxListBoxItem2.Label.Text = str;
                    dxListBoxItem2.Item = (object)dijihang++;
                }
                CombCmdBox.ListBox.SelectItem((object)null);
                DXButton dxButton = new DXButton();
                dxButton.Parent = this;
                dxButton.Size = new Size(60, 18);
                dxButton.ButtonType = ButtonType.SmallButton;
                dxButton.Label.Text = "执行";
                BtSubmit = dxButton;
                BtSubmit.MouseClick += (EventHandler<MouseEventArgs>)((o, e) => { Zhixingmingling(); });
                int x7 = 15;
                int y10 = 30;
                DXLabel dxLabel6 = new DXLabel();
                dxLabel6.Parent = Android;
                dxLabel6.Text = "剩时间:";
                dxLabel6.Outline = true;
                dxLabel6.Location = new Point(x7, y10);
                DXLabel dxLabel7 = dxLabel6;
                DXLabel dxLabel8 = new DXLabel();
                dxLabel8.Text = "";
                dxLabel8.Outline = true;
                dxLabel8.Parent = Android;
                dxLabel8.Border = false;
                dxLabel8.BorderColour = Color.Green;
                int x8 = dxLabel7.Location.X;
                Size size = dxLabel7.Size;
                int width1 = size.Width;
                dxLabel8.Location = new Point(x8 + width1 - 8, y10);
                dxLabel8.ForeColour = Color.Cyan;
                TimeLable = dxLabel8;
                AndroidPlayer = CreateCheckBox(Android, "开始挂机", x7 + 170, y10, ((o, e) => Config.开始挂机 = AndroidPlayer.Checked), Config.开始挂机);
                AndroidPlayer.CheckedChanged += ((o, e) =>
                {
                    if (GameScene.Game.Observer)
                        return;

                    if (AndroidPlayer.Checked && GameScene.Game.User.AutoTime == 0L)
                    {
                        AndroidPlayer.Checked = false;
                    }
                    else
                    {
                        CEnvir.Enqueue(new AutoFightConfChanged()
                        {
                            Enabled = AndroidPlayer.Checked,
                            Slot = AutoSetConf.SetAutoOnHookBox
                        });
                    }
                });
                int y11;
                AndroidPoisonDust = CreateCheckBox(Android, "自动上毒", x7, y11 = y10 + 25, ((o, e) => Config.自动上毒 = AndroidPoisonDust.Checked), Config.自动上毒);
                AndroidEluded = CreateCheckBox(Android, "自动躲避", x7 + 85, y11, ((o, e) => Config.自动躲避 = AndroidEluded.Checked), Config.自动躲避);
                int num13;
                AndroidBackCastle = CreateCheckBox(Android, "死亡回城", x7 + 170, num13 = y11, ((o, e) => Config.死亡回城 = AndroidBackCastle.Checked), Config.死亡回城);
                int y12;
                AndroidSingleSkill = CreateCheckBox(Android, "法道自动技能", x7, y12 = num13 + 25, ((o, e) => Config.是否开启挂机自动技能 = AndroidSingleSkill.Checked), Config.是否开启挂机自动技能);
                DXComboBox dxComboBox4 = new DXComboBox();
                dxComboBox4.Parent = Android;
                dxComboBox4.Size = new Size(120, 18);
                int x9 = AndroidSingleSkill.Location.X;
                size = AndroidSingleSkill.Size;
                int width2 = size.Width;
                dxComboBox4.Location = new Point(x9 + width2 + 5, y12);
                AndroidSkills = dxComboBox4;
                AndroidSkills.SelectedItemChanged += ((o, e) => Config.挂机自动技能 = (MagicType)AndroidSkills.ListBox.SelectedItem.Item);
                DXLabel dxLabel9 = new DXLabel();
                dxLabel9.Parent = Android;
                dxLabel9.Text = "X坐标:";
                dxLabel9.Outline = true;
                int y13;
                dxLabel9.Location = new Point(x7, y13 = y12 + 40);
                DXLabel dxLabel10 = dxLabel9;
                DXNumberBox dxNumberBox3 = new DXNumberBox();
                dxNumberBox3.Parent = Android;
                dxNumberBox3.Size = new Size(80, 20);
                dxNumberBox3.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox3.MaxValue = 1024L;
                dxNumberBox3.MinValue = 1L;
                dxNumberBox3.Value = (long)Config.范围挂机坐标.X;
                dxNumberBox3.UpButton.Location = new Point(63, 1);
                int num14 = x7;
                size = dxLabel10.Size;
                int width3 = size.Width;
                dxNumberBox3.Location = new Point(num14 + width3, y13);
                AndroidCoordX = dxNumberBox3;
                AndroidCoordX.ValueTextBox.ValueChanged += ((o, e) => Config.范围挂机坐标 = new Point((int)AndroidCoordX.Value, Config.范围挂机坐标.Y));
                DXLabel dxLabel11 = new DXLabel();
                dxLabel11.Parent = Android;
                dxLabel11.Text = "Y坐标:";
                dxLabel11.Outline = true;
                dxLabel11.Location = new Point(x7 + 120, y13);
                DXLabel dxLabel12 = dxLabel11;
                DXNumberBox dxNumberBox4 = new DXNumberBox();
                dxNumberBox4.Parent = Android;
                dxNumberBox4.Size = new Size(80, 20);
                dxNumberBox4.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox4.MaxValue = 1024L;
                dxNumberBox4.MinValue = 1L;
                dxNumberBox4.Value = (long)Config.范围挂机坐标.Y;
                dxNumberBox4.UpButton.Location = new Point(63, 1);
                int num15 = x7 + 120;
                size = dxLabel12.Size;
                int width4 = size.Width;
                dxNumberBox4.Location = new Point(num15 + width4, y13);
                AndroidCoordY = dxNumberBox4;
                AndroidCoordY.ValueTextBox.ValueChanged += ((o, e) => Config.范围挂机坐标 = new Point(Config.范围挂机坐标.X, (int)AndroidCoordY.Value));
                DXLabel dxLabel13 = new DXLabel();
                dxLabel13.Parent = Android;
                dxLabel13.Text = "范围 :";
                dxLabel13.Outline = true;
                int y14;
                dxLabel13.Location = new Point(x7, y14 = y13 + 25);
                DXLabel dxLabel14 = dxLabel13;
                DXNumberBox dxNumberBox5 = new DXNumberBox();
                dxNumberBox5.Parent = Android;
                dxNumberBox5.Size = new Size(80, 20);
                dxNumberBox5.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox5.MaxValue = 1024L;
                dxNumberBox5.MinValue = 1L;
                dxNumberBox5.Value = Config.范围距离;
                dxNumberBox5.UpButton.Location = new Point(63, 1);
                int num16 = x7;
                size = dxLabel14.Size;
                int width5 = size.Width;
                dxNumberBox5.Location = new Point(num16 + width5 + 5, y14);
                AndroidCoordRange = dxNumberBox5;
                AndroidCoordRange.ValueTextBox.ValueChanged += ((o, e) => Config.范围距离 = AndroidCoordRange.Value);

                AndroidLockRange = CreateCheckBox(Android, "范围挂机", x7 + 170, y14, ((o, e) => Config.范围挂机 = AndroidLockRange.Checked), Config.范围挂机);

                DXLabel dxLabel15 = new DXLabel();
                dxLabel15.Parent = Android;
                dxLabel15.Text = "血量低于 % :";
                dxLabel15.Outline = true;
                dxLabel15.Hint = "　　百分比值";
                int y15;
                dxLabel15.Location = new Point(x7, y15 = y14 + 40);
                DXLabel dxLabel16 = dxLabel15;
                DXNumberBox dxNumberBox6 = new DXNumberBox();
                dxNumberBox6.Parent = Android;
                dxNumberBox6.Size = new Size(80, 20);
                dxNumberBox6.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox6.MaxValue = 100L;
                dxNumberBox6.MinValue = 1L;
                dxNumberBox6.Value = Config.血量剩下百分之多少时自动回城;
                dxNumberBox6.UpButton.Location = new Point(63, 1);
                int num17 = x7;
                size = dxLabel16.Size;
                int width6 = size.Width;
                dxNumberBox6.Location = new Point(num17 + width6, y15);
                AndroidBackCastleMinPHValue = dxNumberBox6;
                AndroidBackCastleMinPHValue.ValueTextBox.ValueChanged += ((o, e) => Config.血量剩下百分之多少时自动回城 = AndroidBackCastleMinPHValue.Value);
                AndroidMinPHBackCastle = CreateCheckBox(Android, "回城保护", x7 + 170, y15, ((o, e) => Config.是否开启回城保护 = AndroidMinPHBackCastle.Checked), Config.是否开启回城保护);
                DXLabel dxLabel17 = new DXLabel();
                dxLabel17.Parent = Android;
                dxLabel17.Text = "血量低于 % :";
                dxLabel17.Outline = true;
                dxLabel17.Hint = "　　百分比值";
                int y16;
                dxLabel17.Location = new Point(x7, y16 = y15 + 20);
                DXLabel dxLabel18 = dxLabel17;
                DXNumberBox dxNumberBox7 = new DXNumberBox();
                dxNumberBox7.Parent = Android;
                dxNumberBox7.Size = new Size(80, 20);
                dxNumberBox7.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox7.MaxValue = 100L;
                dxNumberBox7.MinValue = 1L;
                dxNumberBox7.Value = Config.血量剩下百分之多少时自动随机;
                dxNumberBox7.UpButton.Location = new Point(63, 1);
                int num18 = x7;
                size = dxLabel18.Size;
                int width7 = size.Width;
                dxNumberBox7.Location = new Point(num18 + width7, y16);
                AndroidRandomMinPHValue = dxNumberBox7;
                AndroidRandomMinPHValue.ValueTextBox.ValueChanged += ((o, e) => Config.血量剩下百分之多少时自动随机 = AndroidRandomMinPHValue.Value);
                AndroidMinPHRandom = CreateCheckBox(Android, "随机保护", x7 + 170, y16, ((o, e) => Config.是否开启随机保护 = AndroidMinPHRandom.Checked), Config.是否开启随机保护);

                DXLabel dxLabel19 = new DXLabel();
                dxLabel19.Parent = Android;
                dxLabel19.Text = "每间隔(秒) :";
                dxLabel19.Outline = true;
                int y17;
                dxLabel19.Location = new Point(x7, y17 = y16 + 20);
                DXLabel dxLabel20 = dxLabel19;
                DXNumberBox dxNumberBox8 = new DXNumberBox();
                dxNumberBox8.Parent = Android;
                dxNumberBox8.Size = new Size(80, 20);
                dxNumberBox8.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox8.MaxValue = 10000L;
                dxNumberBox8.MinValue = 5L;
                dxNumberBox8.Value = Config.隔多少秒自动随机一次;
                dxNumberBox8.UpButton.Location = new Point(63, 1);
                int num19 = x7;
                size = dxLabel20.Size;
                int width8 = size.Width;
                dxNumberBox8.Location = new Point(num19 + width8, y17);
                TimeBoxRandom = dxNumberBox8;
                TimeBoxRandom.ValueTextBox.ValueChanged += ((o, e) => Config.隔多少秒自动随机一次 = TimeBoxRandom.Value);
                ChkAutoRandom = CreateCheckBox(Android, "自动随机", x7 + 170, y17, ((o, e) => Config.是否开启每间隔自动随机 = ChkAutoRandom.Checked), Config.是否开启每间隔自动随机);
                ChkAutoRandom.CheckedChanged += ((o, e) =>
                {
                    CheckBox1_CheckedChanged();
                });

                DXLabel dxLabel21 = new DXLabel();
                dxLabel21.Parent = Android;
                dxLabel21.Text = "无经验(秒) :";
                dxLabel21.Outline = true;
                int y18;
                dxLabel21.Location = new Point(x7, y18 = y17 + 20);
                DXLabel dxLabel22 = dxLabel21;
                DXNumberBox dxNumberBox9 = new DXNumberBox();
                dxNumberBox9.Parent = Android;
                dxNumberBox9.Size = new Size(80, 20);
                dxNumberBox9.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox9.MaxValue = 10000L;
                dxNumberBox9.MinValue = 5L;
                dxNumberBox9.Value = Config.多少秒无经验或者未杀死目标自动随机;
                dxNumberBox9.UpButton.Location = new Point(63, 1);
                int num20 = x7;
                size = dxLabel22.Size;
                int width9 = size.Width;
                dxNumberBox9.Location = new Point(num20 + width9, y18);
                ExpTimeBoxRandom = dxNumberBox9;
                ExpTimeBoxRandom.ValueTextBox.ValueChanged += ((o, e) => Config.多少秒无经验或者未杀死目标自动随机 = ExpTimeBoxRandom.Value);
                ExpAutoRandom = CreateCheckBox(Android, "自动随机", x7 + 170, y18, ((o, e) => Config.是否开启指定时间无经验或者未杀死目标自动随机 = ExpAutoRandom.Checked), Config.是否开启指定时间无经验或者未杀死目标自动随机);
                ExpAutoRandom.CheckedChanged += ((o, e) =>
                {
                    CheckBox2_CheckedChanged();
                });


                SummerLabelKill = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(x7, 285),
                    Text = "杀怪数量:0",
                };
                SummerLabelEx = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(110, 285),
                    Text = "获得经验:0",
                };
                SummerLabelHuntGold = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(x7, 305),
                    Text = "获得赏金:0",
                };
                SummerLabelGold = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(110, 305),
                    Text = "获得金币:0",
                };
                SummerLabelKillfen = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(x7, 325),
                    Text = "分钟杀怪:0",
                };
                SummerLabelExfen = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(110, 325),
                    Text = "分钟经验:0",
                };
                SummerLabelHuntGoldfen = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(x7, 345),
                    Text = "分钟赏金:0",
                };
                SummerLabelGoldfen = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(110, 345),
                    Text = "分钟金币:0",
                };
                Guajishijian = new DXLabel()
                {
                    Parent = Android,
                    Location = new Point(x7, 365),
                    Text = "统计时间:0 小时 0 分钟 0 秒",
                };
                DXButton QingliButton = new DXButton
                {
                    Size = new Size(40, 18),
                    Location = new Point(210, 365),
                    Label = { Text = "重置" },
                    ButtonType = ButtonType.SmallButton,
                    Parent = Android,
                };
                QingliButton.MouseClick += (o, e) =>
                {
                    SummerLabelKill.Text = "杀怪数量:0";
                    skilledCount = 0;
                    SummerLabelEx.Text = "获得经验:0";
                    totalExperience = 0;
                    SummerLabelHuntGold.Text = "获得赏金:0";
                    totalHuntGold = 0;
                    SummerLabelGold.Text = "获得金币:0";
                    totalGold = 0;
                    SummerLabelKillfen.Text = "分钟杀怪:0";
                    SummerLabelExfen.Text = "分钟经验:0";
                    SummerLabelHuntGoldfen.Text = "分钟赏金:0";
                    SummerLabelGoldfen.Text = "分钟金币:0";
                    Guajishijian.Text = "统计时间:0 小时 0 分钟 0 秒";
                    LastTimes = DateTime.Now;

                   // GameScene.Game.AttackModeBox.SummerLabelEx.Text = "0";
                };
            }

            public void GainedExperience(decimal amount)
            {
                TimeSpan timeSpan = DateTime.Now - LastTimes;
                int alll = (timeSpan.Minutes + timeSpan.Hours * 60) + 1;
                Guajishijian.Text = "统计时间:" + (timeSpan.Hours + timeSpan.Days * 24).ToString() + " 小时 " + timeSpan.Minutes + " 分钟 " + timeSpan.Seconds + " 秒";
                totalExperience += amount;
                if (amount > 0)
                {
                    skilledCount++;
                    SummerLabelKill.Text = $"杀怪数量:{skilledCount:#,##0}";
                    SummerLabelKillfen.Text = $"分钟杀怪:{skilledCount / alll:#,##0}";

                    GainEx();

                    SummerLabelEx.Text = $"获得经验:{totalExperience:#,##0}";
                    SummerLabelExfen.Text = $"分钟经验:{totalExperience / alll:#,##0}";

                    //GameScene.Game.AttackModeBox.SummerLabelEx.Text = $"{totalExperience:#,##0}【{MapObject.User.Stats[Stat.ExperienceRate]}%】";
                }
            }
            public void GainedHuntGold(decimal amount)
            {
                TimeSpan timeSpan = DateTime.Now - LastTimes;
                int alll = (timeSpan.Minutes + timeSpan.Hours * 60) + 1;

                totalHuntGold += amount;
                SummerLabelHuntGold.Text = $"获得赏金:{totalHuntGold:#,##0}";
                SummerLabelHuntGoldfen.Text = $"分钟赏金:{totalHuntGold / alll:#,##0}";

            }
            public void GainedGold(decimal amount)
            {
                TimeSpan timeSpan = DateTime.Now - LastTimes;
                int alll = (timeSpan.Minutes + timeSpan.Hours * 60) + 1;

                totalGold += amount;
                SummerLabelGold.Text = $"获得金币:{totalGold:#,##0}";
                SummerLabelGoldfen.Text = $"分钟金币:{totalGold / alll:#,##0}";

            }
            public static void GainEx()
            {
                LastGainEx = DateTime.Now;
            }
            public void ExpAutoRandoms()
            {
                if ((DateTime.Now - LastGainEx).TotalSeconds > (double)Config.多少秒无经验或者未杀死目标自动随机)
                {
                    LastGainEx = DateTime.Now.AddSeconds(1);
                    if (skilledCount++ - LastGainExp != 0)
                    {
                        DXItemCell dxItemCell = ((IEnumerable<DXItemCell>)GameScene.Game.InventoryBox.Grid.Grid).FirstOrDefault(x => x?.Item?.Info.ItemName == "随机传送卷");
                        if (dxItemCell != null && dxItemCell.UseItem())
                            _ProtectTime = CEnvir.Now.AddSeconds(5.0);

                        LastGainExp = skilledCount++;
                        LastGainEx = DateTime.Now;
                    }
                }
            }
            private void CheckBox1_CheckedChanged()
            {
                if (ChkAutoRandom.Checked)
                {
                    ExpAutoRandom.Checked = false;
                }
            }
            private void CheckBox2_CheckedChanged()
            {
                if (ExpAutoRandom.Checked)
                {
                    ChkAutoRandom.Checked = false;
                }
            }
            //private void AutoMagicShield_CheckedChanged()
            //{
            //    if (AutoMagicShield.Checked)
            //    {
            //        AutoSuperiorMagicShield.Checked = false;
            //    }
            //}
            //private void AutoSuperiorMagicShield_CheckedChanged()
            //{
            //    if (AutoSuperiorMagicShield.Checked)
            //    {
            //        AutoMagicShield.Checked = false;
            //    }
            //}

            public void Zhixingmingling()
            {
                if (GameScene.Game.Observer) return;

                string text = CombCmdBox?.SelectedLabel?.Text;
                CEnvir.Enqueue(new Chat { Text = text });
            }

            public void UpdateMagic()
            {
                foreach (KeyValuePair<MagicInfo, ClientUserMagic> magic in GameScene.Game.User.Magics)
                { 
                    ClientUserMagic clientUserMagic = magic.Value;
                    if (clientUserMagic.Info.School == MagicSchool.Passive || clientUserMagic.Info.School == MagicSchool.None)
                        continue;

                    DXListBoxItem dxListBoxItem1 = new DXListBoxItem();
                    dxListBoxItem1.Parent = CombSkill1.ListBox;
                    dxListBoxItem1.Label.Text = clientUserMagic.Info.Name;
                    dxListBoxItem1.Item = (object)clientUserMagic.Info.Magic;
                    DXListBoxItem dxListBoxItem2 = new DXListBoxItem();
                    dxListBoxItem2.Parent = CombSkill2.ListBox;
                    dxListBoxItem2.Label.Text = clientUserMagic.Info.Name;
                    dxListBoxItem2.Item = (object)clientUserMagic.Info.Magic;
                    DXListBoxItem dxListBoxItem3 = new DXListBoxItem();
                    dxListBoxItem3.Parent = AndroidSkills.ListBox;
                    dxListBoxItem3.Label.Text = clientUserMagic.Info.Name;
                    dxListBoxItem3.Item = (object)clientUserMagic.Info.Magic;
                }
                CombSkill1.ListBox.SelectItem((object)Config.自动技能1);
                CombSkill2.ListBox.SelectItem((object)Config.自动技能2);
                AndroidSkills.ListBox.SelectItem((object)Config.挂机自动技能);
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
                int x1 = 5;
                int y1 = 5;
                int width = (Size.Width - 15) / 2;
                Size size = Size;
                int num = size.Height - 50;
                size = AutoSkill.Size;
                int height1 = size.Height;
                int height2 = num - height1 - 15;
                Warrior.Size = new Size(width, height2);
                Warrior.Location = new Point(x1, y1);
                Wizard.Size = new Size(width, height2);
                Wizard.Location = new Point(x1, y1);
                Taoist.Size = new Size(width, height2);
                Taoist.Location = new Point(x1, y1);
                Assassin.Size = new Size(width, height2);
                Assassin.Location = new Point(x1, y1);
                Android.Size = new Size(width, Size.Height - 10);
                Android.Location = new Point(x1 + width + 5, y1);
                AutoSkill.Size = new Size(width, AutoSkill.Size.Height);
                AutoSkill.Location = new Point(x1, y1 + height2);
                LabCommand.Location = new Point(AutoSkill.Location.X, AutoSkill.Location.Y + AutoSkill.Size.Height + 5);
                CombCmdBox.Location = new Point(LabCommand.Location.X + 5, LabCommand.Location.Y + LabCommand.Size.Height + 5);
                BtSubmit.Location = new Point(CombCmdBox.Location.X + CombCmdBox.Size.Width + 5, CombCmdBox.Location.Y);
            }
        }

        public sealed class AutoPotionRow : DXControl
        {
            private Library.SystemModels.ItemInfo _UseItem;
            private int _Index;
            public DXLabel IndexLabel;
            public DXLabel HealthLabel;
            public DXLabel ManaLabel;
            public DXItemCell ItemCell;
            public DXNumberBox HealthTargetBox;
            public DXNumberBox ManaTargetBox;
            public DXCheckBox EnabledCheckBox;
            public DXButton UpButton;
            public DXButton DownButton;

            public Library.SystemModels.ItemInfo UseItem
            {
                get
                {
                    return _UseItem;
                }
                set
                {
                    if (_UseItem == value)
                        return;
                    Library.SystemModels.ItemInfo useItem = _UseItem;
                    _UseItem = value;
                    OnUseItemChanged(useItem, value);
                }
            }

            public event EventHandler<EventArgs> UseItemChanged;

            public void OnUseItemChanged(Library.SystemModels.ItemInfo oValue, Library.SystemModels.ItemInfo nValue)
            {

                EventHandler<EventArgs> useItemChanged = UseItemChanged;
                if (useItemChanged == null)
                    return;
                useItemChanged((object)this, EventArgs.Empty);
            }

            public int Index
            {
                get
                {
                    return _Index;
                }
                set
                {
                    if (_Index == value)
                        return;
                    int index = _Index;
                    _Index = value;
                    OnIndexChanged(index, value);
                }
            }

            public event EventHandler<EventArgs> IndexChanged;

            public void OnIndexChanged(int oValue, int nValue)
            {

                EventHandler<EventArgs> indexChanged = IndexChanged;
                if (indexChanged != null)
                    indexChanged((object)this, EventArgs.Empty);
                IndexLabel.Text = (Index + 1).ToString();
                ItemCell.Slot = Index;
                UpButton.Enabled = Index > 0;
                DownButton.Enabled = Index < 7;
            }

            public AutoPotionRow()
            {
                Size = new Size(260, 46);
                Border = true;
                BorderColour = Color.FromArgb(198, 166, 99);
                DXButton dxButton1 = new DXButton();
                dxButton1.Index = 44;
                dxButton1.LibraryFile = LibraryFile.Interface;
                dxButton1.Location = new Point(5, 5);
                dxButton1.Parent = this;
                dxButton1.Enabled = false;
                UpButton = dxButton1;
                UpButton.MouseClick += (EventHandler<MouseEventArgs>)((o, e) =>
                {
                    GameScene.Game.BigPatchBox.Protect.Updating = true;
                    int num1 = (int)HealthTargetBox.Value;
                    int num2 = (int)ManaTargetBox.Value;
                    bool flag = EnabledCheckBox.Checked;
                    Library.SystemModels.ItemInfo quickInfo = ItemCell.QuickInfo;
                    ItemCell.QuickInfo = GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].ItemCell.QuickInfo;
                    HealthTargetBox.Value = GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].HealthTargetBox.Value;
                    ManaTargetBox.Value = GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].ManaTargetBox.Value;
                    EnabledCheckBox.Checked = GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].EnabledCheckBox.Checked;
                    GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].ItemCell.QuickInfo = quickInfo;
                    GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].HealthTargetBox.Value = (long)num1;
                    GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].ManaTargetBox.Value = (long)num2;
                    GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].EnabledCheckBox.Checked = flag;
                    GameScene.Game.BigPatchBox.Protect.Updating = false;
                    SendUpdate();
                    GameScene.Game.BigPatchBox.Protect.Rows[Index - 1].SendUpdate();
                });
                DXButton dxButton2 = new DXButton();
                dxButton2.Index = 46;
                dxButton2.LibraryFile = LibraryFile.Interface;
                dxButton2.Location = new Point(5, 29);
                dxButton2.Parent = this;
                DownButton = dxButton2;
                DownButton.MouseClick += (EventHandler<MouseEventArgs>)((o, e) =>
                {
                    GameScene.Game.BigPatchBox.Protect.Updating = true;
                    int num1 = (int)HealthTargetBox.Value;
                    int num2 = (int)ManaTargetBox.Value;
                    bool flag = EnabledCheckBox.Checked;
                    Library.SystemModels.ItemInfo quickInfo = ItemCell.QuickInfo;
                    ItemCell.QuickInfo = GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].ItemCell.QuickInfo;
                    HealthTargetBox.Value = GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].HealthTargetBox.Value;
                    ManaTargetBox.Value = GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].ManaTargetBox.Value;
                    EnabledCheckBox.Checked = GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].EnabledCheckBox.Checked;
                    GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].ItemCell.QuickInfo = quickInfo;
                    GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].HealthTargetBox.Value = (long)num1;
                    GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].ManaTargetBox.Value = (long)num2;
                    GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].EnabledCheckBox.Checked = flag;
                    GameScene.Game.BigPatchBox.Protect.Updating = false;
                    SendUpdate();
                    GameScene.Game.BigPatchBox.Protect.Rows[Index + 1].SendUpdate();
                });
                DXItemCell dxItemCell = new DXItemCell(38, 38);
                dxItemCell.Parent = this;
                dxItemCell.Location = new Point(20, 5);
                dxItemCell.AllowLink = true;
                dxItemCell.FixedBorder = true;
                dxItemCell.Border = true;
                dxItemCell.GridType = GridType.AutoPotion;
                ItemCell = dxItemCell;
                DXLabel dxLabel1 = new DXLabel();
                dxLabel1.Parent = ItemCell;
                dxLabel1.Text = (Index + 1).ToString();
                dxLabel1.Font = new Font("宋体", CEnvir.FontSize(8f), FontStyle.Italic);
                dxLabel1.IsControl = false;
                dxLabel1.Location = new Point(-2, -1);
                IndexLabel = dxLabel1;
                DXNumberBox dxNumberBox1 = new DXNumberBox();
                dxNumberBox1.Parent = this;
                dxNumberBox1.Location = new Point(105, 5);
                dxNumberBox1.Size = new Size(80, 20);
                dxNumberBox1.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox1.MaxValue = 50000L;
                dxNumberBox1.MinValue = 0L;
                dxNumberBox1.UpButton.Location = new Point(63, 1);
                HealthTargetBox = dxNumberBox1;
                HealthTargetBox.ValueTextBox.ValueChanged += ((o, e) => SendUpdate());
                DXNumberBox dxNumberBox2 = new DXNumberBox();
                dxNumberBox2.Parent = this;
                dxNumberBox2.Location = new Point(105, 25);
                dxNumberBox2.Size = new Size(80, 20);
                dxNumberBox2.ValueTextBox.Size = new Size(40, 18);
                dxNumberBox2.MaxValue = 50000L;
                dxNumberBox2.MinValue = 0L;
                dxNumberBox2.UpButton.Location = new Point(63, 1);
                ManaTargetBox = dxNumberBox2;
                ManaTargetBox.ValueTextBox.ValueChanged += ((o, e) => SendUpdate());
                DXLabel dxLabel2 = new DXLabel();
                dxLabel2.Parent = this;
                dxLabel2.IsControl = false;
                dxLabel2.Text = "生命值:";
                HealthLabel = dxLabel2;
                DXLabel healthLabel = HealthLabel;
                Point location1 = HealthTargetBox.Location;
                int x1 = location1.X - HealthLabel.Size.Width;
                location1 = HealthTargetBox.Location;
                int y1 = location1.Y;
                Size size1 = HealthTargetBox.Size;
                int height1 = size1.Height;
                size1 = HealthLabel.Size;
                int height2 = size1.Height;
                int num3 = (height1 - height2) / 2;
                int y2 = y1 + num3;
                Point point1 = new Point(x1, y2);
                healthLabel.Location = point1;
                DXLabel dxLabel3 = new DXLabel();
                dxLabel3.Parent = this;
                dxLabel3.IsControl = false;
                dxLabel3.Text = "魔法值:";
                ManaLabel = dxLabel3;
                DXLabel manaLabel = ManaLabel;
                Point location2 = ManaTargetBox.Location;
                int x2 = location2.X - ManaLabel.Size.Width;
                location2 = ManaTargetBox.Location;
                int y3 = location2.Y;
                Size size2 = ManaTargetBox.Size;
                int height3 = size2.Height;
                size2 = ManaLabel.Size;
                int height4 = size2.Height;
                int num4 = (height3 - height4) / 2;
                int y4 = y3 + num4;
                Point point2 = new Point(x2, y4);
                manaLabel.Location = point2;
                DXCheckBox dxCheckBox = new DXCheckBox();
                dxCheckBox.AutoSize = true;
                dxCheckBox.Text = "启用";
                dxCheckBox.Parent = this;
                EnabledCheckBox = dxCheckBox;
                EnabledCheckBox.CheckedChanged += ((o, e) => SendUpdate());
                DXCheckBox enabledCheckBox = EnabledCheckBox;
                int width1 = Size.Width;
                int width2 = EnabledCheckBox.Size.Width;
                Point point3 = new Point(width1 - width2 - 5, 5);
                enabledCheckBox.Location = point3;
            }

            public void SendUpdate()
            {
                if (GameScene.Game.Observer || GameScene.Game.BigPatchBox.Protect.Updating)
                    return;
                AutoPotionLinkChanged potionLinkChanged = new AutoPotionLinkChanged();
                potionLinkChanged.Slot = Index;
                ClientUserItem clientUserItem = ItemCell.Item;
                potionLinkChanged.LinkIndex = clientUserItem != null ? clientUserItem.Info.Index : -1;
                potionLinkChanged.Enabled = EnabledCheckBox.Checked;
                potionLinkChanged.Health = (int)HealthTargetBox.Value;
                potionLinkChanged.Mana = (int)ManaTargetBox.Value;
                CEnvir.Enqueue((Packet)potionLinkChanged);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (!disposing)
                    return;
                _UseItem = (Library.SystemModels.ItemInfo)null;

                UseItemChanged = null;
                _Index = 0;

                IndexChanged = null;
                if (IndexLabel != null)
                {
                    if (!IndexLabel.IsDisposed)
                        IndexLabel.Dispose();
                    IndexLabel = (DXLabel)null;
                }
                if (HealthLabel != null)
                {
                    if (!HealthLabel.IsDisposed)
                        HealthLabel.Dispose();
                    HealthLabel = (DXLabel)null;
                }
                if (ManaLabel != null)
                {
                    if (!ManaLabel.IsDisposed)
                        ManaLabel.Dispose();
                    ManaLabel = (DXLabel)null;
                }
                if (ItemCell != null)
                {
                    if (!ItemCell.IsDisposed)
                        ItemCell.Dispose();
                    ItemCell = (DXItemCell)null;
                }
                if (HealthTargetBox != null)
                {
                    if (!HealthTargetBox.IsDisposed)
                        HealthTargetBox.Dispose();
                    HealthTargetBox = (DXNumberBox)null;
                }
                if (ManaTargetBox != null)
                {
                    if (!ManaTargetBox.IsDisposed)
                        ManaTargetBox.Dispose();
                    ManaTargetBox = (DXNumberBox)null;
                }
                if (EnabledCheckBox != null)
                {
                    if (!EnabledCheckBox.IsDisposed)
                        EnabledCheckBox.Dispose();
                    EnabledCheckBox = (DXCheckBox)null;
                }
                if (UpButton != null)
                {
                    if (!UpButton.IsDisposed)
                        UpButton.Dispose();
                    UpButton = (DXButton)null;
                }
                if (DownButton != null)
                {
                    if (!DownButton.IsDisposed)
                        DownButton.Dispose();
                    DownButton = (DXButton)null;
                }
            }
        }

        public class DXProtectionTab : DXTab
        {
            public ClientAutoPotionLink[] Links;
            public BigPatchDialog.AutoPotionRow[] Rows;
            public new bool Updating;

            public DXProtectionTab()
            {
                Links = new ClientAutoPotionLink[8];
                Rows = new BigPatchDialog.AutoPotionRow[8];
                for (int index1 = 0; index1 < Links.Length; ++index1)
                {
                    BigPatchDialog.AutoPotionRow[] rows = Rows;
                    int index2 = index1;
                    BigPatchDialog.AutoPotionRow autoPotionRow = new BigPatchDialog.AutoPotionRow();
                    autoPotionRow.Parent = this;
                    autoPotionRow.Location = new Point(5, 5 + 50 * index1);
                    autoPotionRow.Index = index1;
                    rows[index2] = autoPotionRow;
                }
            }

            public void UpdateLinks()
            {
                Updating = true;
                foreach (ClientAutoPotionLink link1 in Links)
                {
                    ClientAutoPotionLink link = link1;
                    if (link != null && link.Slot >= 0 && link.Slot < Rows.Length)
                    {
                        Rows[link.Slot].ItemCell.QuickInfo = Globals.ItemInfoList.Binding.FirstOrDefault<Library.SystemModels.ItemInfo>((Func<Library.SystemModels.ItemInfo, bool>)(x => x.Index == link.LinkInfoIndex));
                        Rows[link.Slot].HealthTargetBox.Value = (long)link.Health;
                        Rows[link.Slot].ManaTargetBox.Value = (long)link.Mana;
                        Rows[link.Slot].EnabledCheckBox.Checked = link.Enabled;
                    }
                }
                Updating = false;
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
            }
        }

        public class DXAnsweringTab : DXTab
        {
            public BigPatchDialog.DXTextView InputBox;
            public DXCheckBox ChkMsgNotify;
            public DXCheckBox ChkAutoReplay;
            public DXComboBox CombAutoReplayText;
            public DXCheckBox ChkAutoSayWords;
            public DXNumberBox AutoSayInterval;
            public System.Timers.Timer SayWordTimer;
            public DXLabel IntervalLeft;
            public DXLabel IntervalRight;
            public DXCheckBox ChkSaveSayRecord;
            public DXCheckBox ChkShieldNpcWords;
            public DXCheckBox ChkShieldMonsterWords;

            public DXAnsweringTab()
            {
                DXCheckBox dxCheckBox1 = new DXCheckBox();
                dxCheckBox1.AutoSize = true;
                dxCheckBox1.bAlignRight = false;
                dxCheckBox1.Parent = this;
                dxCheckBox1.Text = "自动回复";
                dxCheckBox1.Location = new Point(10, 10);
                dxCheckBox1.Checked = Config.自动回复;
                ChkAutoReplay = dxCheckBox1;
                ChkAutoReplay.CheckedChanged += ((o, e) => Config.自动回复 = ChkAutoReplay.Checked);
                DXComboBox dxComboBox = new DXComboBox();
                dxComboBox.Parent = this;
                dxComboBox.Location = new Point(10, 10);
                dxComboBox.Size = new Size(32, 18);
                CombAutoReplayText = dxComboBox;
                CombAutoReplayText.SelectedItemChanged += ((o, e) => Config.自动回复时用第几行句子 = (int)CombAutoReplayText.SelectedItem);
                DXListBoxItem dxListBoxItem1 = new DXListBoxItem();
                dxListBoxItem1.Parent = CombAutoReplayText.ListBox;
                dxListBoxItem1.Label.Text = "您好，我有事不在";
                dxListBoxItem1.Item = (object)0;
                DXListBoxItem dxListBoxItem2 = new DXListBoxItem();
                dxListBoxItem2.Parent = CombAutoReplayText.ListBox;
                dxListBoxItem2.Label.Text = "我去吃饭了....";
                dxListBoxItem2.Item = (object)1;
                DXListBoxItem dxListBoxItem3 = new DXListBoxItem();
                dxListBoxItem3.Parent = CombAutoReplayText.ListBox;
                dxListBoxItem3.Label.Text = "挂机中.....";
                dxListBoxItem3.Item = (object)2;
                DXListBoxItem dxListBoxItem4 = new DXListBoxItem();
                dxListBoxItem4.Parent = CombAutoReplayText.ListBox;
                dxListBoxItem4.Label.Text = "挂机练功 请勿打扰";
                dxListBoxItem4.Item = (object)3;
                CombAutoReplayText.ListBox.SelectItem((object)Config.自动回复时用第几行句子);
                DXCheckBox dxCheckBox2 = new DXCheckBox();
                dxCheckBox2.Parent = this;
                dxCheckBox2.Label.Text = "来消息声音提示";
                //dxCheckBox2.bAlignRight = false;
                dxCheckBox2.Location = new Point(10, 10);
                ChkMsgNotify = dxCheckBox2;
                DXCheckBox dxCheckBox3 = new DXCheckBox();
                dxCheckBox3.Parent = this;
                dxCheckBox3.Label.Text = "自动喊话";
                //dxCheckBox3.bAlignRight = false;
                dxCheckBox3.Location = new Point(10, 10);
                dxCheckBox3.Checked = false;
                ChkAutoSayWords = dxCheckBox3;
                DXLabel dxLabel1 = new DXLabel();
                dxLabel1.Parent = this;
                dxLabel1.Text = "间隔:";
                IntervalLeft = dxLabel1;
                DXNumberBox dxNumberBox = new DXNumberBox();
                dxNumberBox.Parent = this;
                dxNumberBox.ValueTextBox.Size = new Size(50, 16);
                dxNumberBox.MaxValue = (long)ushort.MaxValue;
                dxNumberBox.Value = Config.多少秒一次自动喊话;
                dxNumberBox.MinValue = 5000L;
                AutoSayInterval = dxNumberBox;
                DXLabel dxLabel2 = new DXLabel();
                dxLabel2.Parent = this;
                dxLabel2.Text = "毫秒";
                IntervalRight = dxLabel2;
                SayWordTimer = new System.Timers.Timer()
                {
                    Interval = 10000.0,
                    Enabled = false,
                    AutoReset = true
                };
                SayWordTimer.Elapsed += new ElapsedEventHandler(BigPatchDialog.DXAnsweringTab.OnSayWordsTimer);
                AutoSayInterval.ValueTextBox.ValueChanged += ((o, e) =>
                {
                    Config.多少秒一次自动喊话 = AutoSayInterval.Value;
                    SayWordTimer.Interval = (double)Config.多少秒一次自动喊话;
                });
                ChkAutoSayWords.CheckedChanged += ((o, e) =>
                {
                    SayWordTimer.Enabled = ChkAutoSayWords.Checked;
                    SayWordTimer.Interval = (double)AutoSayInterval.Value;
                    if (SayWordTimer.Enabled)
                        SayWordTimer.Start();
                    else
                        SayWordTimer.Stop();
                });
                DXCheckBox dxCheckBox4 = new DXCheckBox();
                dxCheckBox4.Parent = this;
                dxCheckBox4.Label.Text = "保存喊话内容";
                //dxCheckBox4.bAlignRight = false;
                dxCheckBox4.Location = new Point(10, 10);
                dxCheckBox4.Checked = false;
                ChkSaveSayRecord = dxCheckBox4;
                ChkSaveSayRecord.CheckedChanged += ((o, e) =>
                {
                    if (!ChkSaveSayRecord.Checked)
                        return;
                    SaveSaywords();
                });
                DXCheckBox dxCheckBox5 = new DXCheckBox();
                dxCheckBox5.Parent = this;
                dxCheckBox5.Label.Text = "屏蔽NPC白字";
                //dxCheckBox5.bAlignRight = false;
                dxCheckBox5.Location = new Point(10, 10);
                dxCheckBox5.Checked = Config.是否关闭NPC话;
                ChkShieldNpcWords = dxCheckBox5;
                ChkShieldNpcWords.CheckedChanged += ((o, e) => Config.是否关闭NPC话 = ChkShieldNpcWords.Checked);
                DXCheckBox dxCheckBox6 = new DXCheckBox();
                dxCheckBox6.Parent = this;
                dxCheckBox6.Label.Text = "屏蔽怪物白字";
                //dxCheckBox6.bAlignRight = false;
                dxCheckBox6.Location = new Point(10, 10);
                dxCheckBox6.Checked = Config.是否关闭怪物话;
                ChkShieldMonsterWords = dxCheckBox6;
                ChkShieldMonsterWords.CheckedChanged += ((o, e) => Config.是否关闭怪物话 = ChkShieldMonsterWords.Checked);
                BigPatchDialog.DXTextView dxTextView = new BigPatchDialog.DXTextView();
                dxTextView.Parent = this;
                dxTextView.ReadOnly = false;
                dxTextView.Visible = false;
                InputBox = dxTextView;
                for (int index = 0; index < Config.自动喊话内内容.Count; ++index)
                {
                    DXTextBox.MirTextBox textBox = InputBox.TextBox;
                    textBox.Text = textBox.Text + Config.自动喊话内内容[index] + "\r\n";
                }
                InputBox.TextBox.ScrollBars = ScrollBars.None;
                InputBox.TextBox.MaxLength = 115;
                InputBox.TextBox.Disposed += (EventHandler)((o, e) => { });
            }

            public static void OnSayWordsTimer(object o, ElapsedEventArgs e)
            {
                string text = GameScene.Game?.BigPatchBox?.Answering?.InputBox?.TextBox?.Text;
                if (text == null || text.Length == 0)
                    return;
                CEnvir.Enqueue((Packet)new Chat()
                {
                    Text = text
                });
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
                if (InputBox == null)
                    return;
                ChkAutoReplay.Location = new Point(10, 15);
                DXComboBox combAutoReplayText1 = CombAutoReplayText;
                Size size1 = Size;
                int num1 = size1.Width - 20;
                size1 = ChkAutoReplay.Size;
                int width1 = size1.Width;
                int num2 = num1 - width1;
                size1 = ChkMsgNotify.Size;
                int width2 = size1.Width;
                int width3 = num2 - width2 - 40;
                size1 = CombAutoReplayText.Size;
                int height1 = size1.Height;
                Size size2 = new Size(width3, height1);
                combAutoReplayText1.Size = size2;
                DXComboBox combAutoReplayText2 = CombAutoReplayText;
                int x1 = ChkAutoReplay.Location.X;
                size1 = ChkAutoReplay.Size;
                int width4 = size1.Width;
                Point point1 = new Point(x1 + width4 + 20, 15);
                combAutoReplayText2.Location = point1;
                DXCheckBox chkMsgNotify = ChkMsgNotify;
                size1 = Size;
                int num3 = size1.Width - 10;
                size1 = ChkMsgNotify.Size;
                int width5 = size1.Width;
                Point point2 = new Point(num3 - width5, 15);
                chkMsgNotify.Location = point2;
                int x2 = ChkAutoReplay.Location.X;
                int y1 = ChkAutoReplay.Location.Y;
                size1 = ChkAutoReplay.Size;
                int height2 = size1.Height;
                int y2 = y1 + height2 + 10;
                ChkAutoSayWords.Location = new Point(x2, y2);
                int num4 = x2;
                size1 = ChkAutoSayWords.Size;
                int num5 = size1.Width + 5;
                int x3 = num4 + num5;
                IntervalLeft.Location = new Point(x3, y2);
                int num6 = x3;
                size1 = IntervalLeft.Size;
                int width6 = size1.Width;
                int x4 = num6 + width6;
                AutoSayInterval.Location = new Point(x4, y2);
                int num7 = x4;
                size1 = AutoSayInterval.Size;
                int width7 = size1.Width;
                int x5 = num7 + width7;
                IntervalRight.Location = new Point(x5, y2);
                int num8 = x5;
                size1 = IntervalRight.Size;
                int num9 = size1.Width + 5;
                int x6 = num8 + num9;
                ChkSaveSayRecord.Location = new Point(x6, y2);
                int num10 = x6;
                size1 = ChkSaveSayRecord.Size;
                int num11 = size1.Width + 5;
                int x7 = num10 + num11;
                ChkShieldNpcWords.Location = new Point(x7, y2);
                int num12 = x7;
                size1 = ChkShieldNpcWords.Size;
                int num13 = size1.Width + 5;
                ChkShieldMonsterWords.Location = new Point(num12 + num13, y2);
                int x8 = ChkAutoReplay.Location.X;
                int num14 = y2;
                size1 = ChkShieldNpcWords.Size;
                int num15 = size1.Height + 10;
                int num16 = num14 + num15;
                size1 = ChkShieldMonsterWords.Size;
                int num17 = size1.Height + 10;
                int y3 = num16 + num17;
                BigPatchDialog.DXTextView inputBox = InputBox;
                size1 = Size;
                int width8 = size1.Width - 20;
                size1 = Size;
                int height3 = size1.Height - y3 - 10;
                Size size3 = new Size(width8, height3);
                inputBox.Size = size3;
                InputBox.Location = new Point(10, y3);
                InputBox.Visible = true;
            }

            public void SaveSaywords()
            {
                Config.自动喊话内内容.Clear();
                string text = InputBox?.TextBox?.Text;
                if (text == null)
                    return;
                string[] separator = new string[1] { "\r\n" };
                foreach (string str in text.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                    Config.自动喊话内内容.Add(str);
            }
        }

        public class DXUserNoteBookTab : DXTab
        {
            public BigPatchDialog.DXTextView NoteView;

            public DXUserNoteBookTab()
            {
                BigPatchDialog.DXTextView dxTextView = new BigPatchDialog.DXTextView();
                dxTextView.Parent = this;
                dxTextView.ReadOnly = false;
                dxTextView.Visible = false;
                NoteView = dxTextView;
                NoteView.TextBox.ScrollBars = ScrollBars.None;
                NoteView.TextBox.MaxLength = 1024;
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
                if (NoteView == null)
                    return;
                BigPatchDialog.DXTextView noteView = NoteView;
                Size size1 = Size;
                int width = size1.Width - 20;
                size1 = Size;
                int height = size1.Height - 20;
                Size size2 = new Size(width, height);
                noteView.Size = size2;
                NoteView.Location = new Point(10, 10);
                NoteView.Visible = true;
            }
        }

        public class DXSystemMsgRecordTab : DXTab
        {
            public DXListBox LogList;

            [DllImport("kernel32.dll")]
            public static extern int WinExec(string programPath, int operType);

            public DXSystemMsgRecordTab()
            {
                DXListBox dxListBox = new DXListBox();
                dxListBox.Parent = this;
                dxListBox.Size = new Size(120, DXControl.DefaultHeight);
                dxListBox.Location = new Point(5, 5);
                LogList = dxListBox;
                DirectoryInfo directoryInfo = new DirectoryInfo(".\\SysLogs\\");
                if (!directoryInfo.Exists)
                    return;
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    DXListBoxItem dxListBoxItem1 = new DXListBoxItem();
                    dxListBoxItem1.Parent = LogList;
                    dxListBoxItem1.Label.Text = file.Name;
                    dxListBoxItem1.Hint = "双击 打开记录文件";
                    dxListBoxItem1.Tag = (object)file.FullName;
                    dxListBoxItem1.MouseDoubleClick += (EventHandler<MouseEventArgs>)((o, e) =>
                    {
                        DXListBoxItem dxListBoxItem = o as DXListBoxItem;
                        if (dxListBoxItem == null)
                            return;
                        BigPatchDialog.DXSystemMsgRecordTab.WinExec("notepad.exe " + (dxListBoxItem.Tag as string), 5);
                    });
                }
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
                DXListBox logList = LogList;
                Size size1 = Size;
                int width = size1.Width - 10;
                size1 = Size;
                int height = size1.Height - 10;
                Size size2 = new Size(width, height);
                logList.Size = size2;
            }
        }

        public class CItemFilterSet
        {
            public int idx { get; set; }
            public string name {  get; set; }
            public ItemType type { get; set; }
            public bool hint;
            public bool pick;
            public bool picks { get; set; }
            public bool show
            {
                get => _show;
                set
                {
                    if (value == _show) return;
                    _show = value;
                    ShowChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            private bool _show = true;

            public bool sell;
            public bool buy;

            public bool OldPicks { get; set; }

            public event EventHandler<EventArgs> ShowChanged;

            public void SetValue(int idx, bool value)
            {
                switch (idx)
                {
                    case 0:
                        hint = value;
                        break;
                    case 1:
                        pick = value;
                        break;
                    case 2:
                        picks = value;
                        break;
                    case 3:
                        show = value;
                        break;
                    case 4:
                        sell = value;
                        break;
                    case 5:
                        buy = value;
                        break;
                }
            }
        }

        public class CItemFilter
        {
            public bool Inited = false;
            public string FileName;

            public List<CItemFilterSet> Items { get; private set; }
            public Dictionary<int, CItemFilterSet> dictItems { get; private set; }

            public CItemFilter()
            {
                Items = new List<CItemFilterSet>();
                dictItems = new Dictionary<int, CItemFilterSet>();
                foreach (ItemInfo itemInfo in Globals.ItemInfoList.Binding)
                {
                    if (itemInfo.BlockMonsterDrop) continue;

                    CItemFilterSet tmp = new CItemFilterSet()
                    {
                        name = itemInfo.ItemName,
                        idx = itemInfo.Index,
                        type = itemInfo.ItemType,
                    };

                    Items.Add(tmp);

                    if (dictItems.ContainsKey(itemInfo.Index))
                    {
                        CEnvir.SaveError($"道具有重复的索引号: NAME={itemInfo.ItemName} INDEX={itemInfo.Index}");
                        continue;
                    }

                    dictItems.Add(itemInfo.Index, tmp);
                }
            }

            public void Initialize(string file = "./PickupFilter.ini")
            {
                FileName = file;
                if (!File.Exists(FileName))
                    File.Create(FileName)?.Close();

                foreach (string readAllLine in File.ReadAllLines(FileName))
                {
                    if (!string.IsNullOrEmpty(readAllLine) && readAllLine[0] != ';')
                    {
                        string[] strArray = readAllLine.Split(',');
                        if (strArray.Length >= 6)
                        {
                            int result1 = 0;

                            if (int.TryParse(strArray[0].Trim(), out result1))
                            {
                                if (!dictItems.TryGetValue(result1, out CItemFilterSet citemFilterSet)) continue;

                                bool result2 = false;
                                if (bool.TryParse(strArray[2].Trim(), out result2))
                                    citemFilterSet.hint = result2;
                                bool result3 = false;
                                if (bool.TryParse(strArray[3].Trim(), out result3))
                                    citemFilterSet.pick = result3;
                                bool result4 = false;
                                if (bool.TryParse(strArray[4].Trim(), out result4))
                                    citemFilterSet.picks = result4;
                                bool result5 = false;
                                if (bool.TryParse(strArray[5].Trim(), out result5))
                                    citemFilterSet.show = result5;
                            }
                        }
                    }
                }
                Inited = true;
            }

            public void Uninitalize()
            {
                if (!File.Exists(FileName))
                    File.Create(FileName);
                string[] contents = new string[Items.Count];
                for (int index = 0; index < Items.Count; ++index)
                {
                    CItemFilterSet citemFilterSet = Items[index];

                        contents[index] = string.Format("{0}, {1}, {2}, {3}, {4}, {5}"
                            , Items[index].idx,
                            Items[index].name,
                            Items[index].hint,
                            Items[index].pick,
                            Items[index].picks,
                            Items[index].show
                        );

                }
                File.WriteAllLines(FileName, contents);
            }
        }


        public class DXAutoPickItemTab : DXTab
        {
            public DXButton Search;
            public DXComboBox CombTypeBox;
            public DXTextBox sTextBox;
            public DXButton AddButton;
            public DXButton DelButton;
            public CItemFilter ItemFilter { get; set; }
            public DXListView ItemView { get; set; }

            public bool NeedSycFilters { get; set; } = true;

            public DXAutoPickItemTab()
            {
                DXComboBox dxComboBox = new DXComboBox();
                dxComboBox.Parent = this;
                dxComboBox.Size = new Size(100, 18);
                dxComboBox.Location = new Point(10, 5);
                dxComboBox.DropDownHeight = 198;
                CombTypeBox = dxComboBox;

                CombTypeBox.SelectedItemChanged += ((o, e) => {
                    RefreshItems(CombTypeBox.SelectedItem as ItemType?);
                });

                DXListBoxItem dxListBoxItem1 = new DXListBoxItem();
                dxListBoxItem1.Parent = CombTypeBox.ListBox;
                dxListBoxItem1.Text = $"全部";
                dxListBoxItem1.Item = null;
                Type type = typeof(ItemType);
                for (ItemType itemType = ItemType.Nothing; itemType <= ItemType.Fabao; ++itemType)
                {
                    DescriptionAttribute customAttribute = type.GetMember(itemType.ToString())[0].GetCustomAttribute<DescriptionAttribute>();
                    DXListBoxItem dxListBoxItem2 = new DXListBoxItem();
                    dxListBoxItem2.Parent = CombTypeBox.ListBox;
                    dxListBoxItem2.Label.Text = customAttribute?.Description ?? itemType.ToString();
                    dxListBoxItem2.Item = (object)itemType;
                }
                CombTypeBox.ListBox.SelectItem(null);
                DXTextBox dxTextBox = new DXTextBox();
                dxTextBox.Parent = this;
                dxTextBox.Size = new Size(120, 20);
                int x1 = CombTypeBox.Location.X + CombTypeBox.Size.Width + 5;
                Point location = CombTypeBox.Location;
                int y1 = location.Y;
                dxTextBox.Location = new Point(x1, y1);
                dxTextBox.TextBox.KeyPress += (o, e)=>{
                    if (e.KeyChar == (char)Keys.Enter)
                    {
                        SearchItem();
                        e.Handled = true;
                    }
                    else
                        e.Handled = false;
                };
                sTextBox = dxTextBox;
                DXButton dxButton1 = new DXButton();
                dxButton1.Parent = this;
                dxButton1.Size = new Size(64, 18);
                dxButton1.Label.Text = "搜索";
                dxButton1.ButtonType = ButtonType.SmallButton;
                location = sTextBox.Location;
                int x2 = location.X + sTextBox.Size.Width + 5;
                location = sTextBox.Location;
                int y2 = location.Y;
                dxButton1.Location = new Point(x2, y2);
                Search = dxButton1;
                Search.MouseClick += (EventHandler<MouseEventArgs>)((o, e) =>
                {
                    SearchItem();
                });
                DXButton dxButton2 = new DXButton();
                dxButton2.Parent = this;
                dxButton2.Size = new Size(64, 18);
                dxButton2.Label.Text = "重置";
                dxButton2.ButtonType = ButtonType.SmallButton;
                location = Search.Location;
                int x3 = location.X + Search.Size.Width + 5;
                location = Search.Location;
                int y3 = location.Y;
                dxButton2.Location = new Point(x3, y3);
                AddButton = dxButton2;
                AddButton.MouseClick += (EventHandler<MouseEventArgs>)((o, e) =>
                {
                    if (ItemView.ItemCount == 0U)
                        return;

                    foreach (ItemInfo itemInfo in Globals.ItemInfoList.Binding)
                    {
                        ItemFilter.Items.Add(new CItemFilterSet()
                        {
                            name = itemInfo.ItemName,
                            type = itemInfo.ItemType,
                        });
                    }
                    ItemFilter.Inited = true;
                    Initialize();
                });

                DXButton dxButton3 = new DXButton();
                dxButton3.Parent = this;
                dxButton3.Size = new Size(64, 18);
                dxButton3.Label.Text = "清空";
                dxButton3.ButtonType = ButtonType.SmallButton;
                location = AddButton.Location;
                int x4 = location.X + AddButton.Size.Width + 5;
                location = AddButton.Location;
                int y4 = location.Y;
                dxButton3.Location = new Point(x4, y4);
                DelButton = dxButton3;
                DelButton.MouseClick += (EventHandler<MouseEventArgs>)((o, e) => ItemView.RemoveAll());
                DXListView dxListView = new DXListView();
                dxListView.Parent = this;
                dxListView.Size = new Size(410, 405);
                int x5 = 5;
                location = Search.Location;
                int y5 = location.Y + Search.Size.Height + 5;
                dxListView.Location = new Point(x5, y5);
                dxListView.ItemBorder = false;
                ItemView = dxListView;
                int num1 = (int)ItemView.InsertColumn(0U, "物品名称", 158, 24, "  物品的名字");


                var ckb = new DXCheckBox()
                {
                    Size = new Size(80, 24),
                    //AutoSize = true,
                    Checked = false,
                    Text = "提示",
                    BackColour = Color.Green,
                };

                ckb.CheckedChanged += (o, e) =>
                {
                    if (o is DXCheckBox box)
                        foreach (DXControl row in ItemView.Controls[1].Controls)
                        {
                            if (row.Controls[1] is DXCheckBox tmp)
                                tmp.Checked = box.Checked;
                        }
                };

                int num2 = (int)ItemView.InsertColumn(1U, ckb);


                ckb = new DXCheckBox()
                {
                    Size = new Size(80, 24),
                    //AutoSize = true,
                    Checked = false,
                    Text = "角色拾取",
                    BackColour = Color.Empty,
                };

                ckb.CheckedChanged += (o, e) =>
                {
                    if (o is DXCheckBox box)
                        foreach (DXControl row in ItemView.Controls[1].Controls)
                        {
                            if (row.Controls[2] is DXCheckBox tmp)
                                tmp.Checked = box.Checked;
                        }
                };

                int num3 = (int)ItemView.InsertColumn(2U, ckb);

                ckb = new DXCheckBox()
                {
                    Size = new Size(80, 24),
                    //AutoSize = true,
                    Checked = false,
                    Text = "宠物拾取",
                    BackColour = Color.Empty,
                };
                ckb.CheckedChanged += (o, e) =>
                {
                    if (o is DXCheckBox box)
                        foreach (DXControl row in ItemView.Controls[1].Controls)
                        {
                            if (row.Controls[3] is DXCheckBox tmp)
                                tmp.Checked = box.Checked;
                        }
                };

                int num4 = (int)ItemView.InsertColumn(3U, ckb);


                ckb = new DXCheckBox()
                {
                    Size = new Size(80, 24),
                    //AutoSize = true,
                    Checked = false,
                    Text = "显示",
                    BackColour = Color.Empty,
                };
                ckb.CheckedChanged += (o, e) =>
                {
                    if (o is DXCheckBox box)
                        foreach (DXControl row in ItemView.Controls[1].Controls)
                        {
                            if (row.Controls[4] is DXCheckBox tmp)
                                tmp.Checked = box.Checked;
                        }
                };

                int num5 = (int)ItemView.InsertColumn(4U, ckb);

                ItemFilter = new CItemFilter();

                string old_name = "./PickupFilter.ini";
                string new_name = $"./{CEnvir.LogonCharacterDesc}-PickupFilter.ini";

                if (File.Exists(old_name) && !File.Exists(new_name))
                {
                    try { File.Move(old_name, new_name); }
                    catch { }
                }

                ItemFilter.Initialize(new_name);
                Initialize();

                NeedSycFilters = true;
                SycFilters(true);
            }
            private void SearchItem()
            {
                string text = sTextBox.TextBox.Text;
                if (text == null || text.Length <= 0)
                    return;
                ItemView.SortByName(text);
            }
            public void SycFilters(bool first_syc)
            {
                if (!NeedSycFilters) return;

                List<string> filters = new List<string>();
                foreach (var pair in ItemFilter.dictItems)
                {
                    if (first_syc)
                    {
                        pair.Value.OldPicks = pair.Value.picks;
                        if (pair.Value.picks)
                            filters.Add($"{pair.Key},1");
                    }
                    else if (pair.Value.picks != pair.Value.OldPicks)
                    {
                        pair.Value.OldPicks = pair.Value.picks;
                        filters.Add($"{pair.Key},{(pair.Value.picks ? 1 : 0)}");
                    }
                }

                CEnvir.Enqueue(new PktFilterItem()
                {
                    FilterStr = filters,
                });
                NeedSycFilters = false;
            }

            private void RefreshItems(ItemType? type = null)
            {
                ItemView.RemoveAll();

                for (int index = 0; index < ItemFilter.Items.Count; ++index)
                {
                    CItemFilterSet citemFilterSet = ItemFilter.Items[index];
                    if (type != null && citemFilterSet.type != type) continue;
                    
                    if (citemFilterSet.name != null && citemFilterSet.name.Length != 0)
                    {
                        uint nItem = ItemView.InsertItem(uint.MaxValue, citemFilterSet.name);
                        DXCheckBox dxCheckBox1 = new DXCheckBox();
                        dxCheckBox1.AutoSize = false;
                        dxCheckBox1.Checked = citemFilterSet.hint;
                        dxCheckBox1.Tag = (object)index;
                        DXCheckBox dxCheckBox2 = dxCheckBox1;
                        dxCheckBox2.CheckedChanged += (o, e) =>
                        {
                            DXCheckBox dxCheckBox = o as DXCheckBox;
                            ItemFilter.Items[(int)dxCheckBox.Tag]?.SetValue(0, dxCheckBox.Checked);
                        };
                        ItemView.SetItem(nItem, 1U, dxCheckBox2);
                        DXCheckBox dxCheckBox4 = new DXCheckBox();
                        dxCheckBox4.AutoSize = false;
                        dxCheckBox4.Checked = citemFilterSet.pick;
                        dxCheckBox4.Tag = (object)index;
                        DXCheckBox dxCheckBox5 = dxCheckBox4;
                        dxCheckBox5.CheckedChanged += (o, e) =>
                        {
                            DXCheckBox dxCheckBox = o as DXCheckBox;
                            ItemFilter.Items[(int)dxCheckBox.Tag]?.SetValue(1, dxCheckBox.Checked);
                        };
                        ItemView.SetItem(nItem, 2U, dxCheckBox5);


                        DXCheckBox DXCheckBox16 = new DXCheckBox();
                        DXCheckBox16.AutoSize = false;
                        DXCheckBox16.Checked = citemFilterSet.picks;
                        DXCheckBox16.Tag = (object)index;
                        DXCheckBox DXCheckBox17 = DXCheckBox16;
                        DXCheckBox17.CheckedChanged += (o, e) =>
                        {
                            DXCheckBox dxCheckBox = o as DXCheckBox;
                            ItemFilter.Items[(int)dxCheckBox.Tag]?.SetValue(2, dxCheckBox.Checked);
                            NeedSycFilters = true;
                        };
                        ItemView.SetItem(nItem, 3U, DXCheckBox17);

                        DXCheckBox dxCheckBox6 = new DXCheckBox();
                        dxCheckBox6.AutoSize = false;
                        dxCheckBox6.Checked = citemFilterSet.show;
                        dxCheckBox6.Tag = (object)index;
                        DXCheckBox dxCheckBox7 = dxCheckBox6;
                        dxCheckBox7.CheckedChanged += (o, e) =>
                        {
                            DXCheckBox dxCheckBox = o as DXCheckBox;
                            ItemFilter.Items[(int)dxCheckBox.Tag]?.SetValue(3, dxCheckBox.Checked);
                        };
                        ItemView.SetItem(nItem, 4U, dxCheckBox7);

                        DXCheckBox dxCheckBox8 = new DXCheckBox();
                        dxCheckBox8.AutoSize = false;
                        dxCheckBox8.Checked = citemFilterSet.sell;
                        dxCheckBox8.Tag = (object)index;
                        DXCheckBox dxCheckBox9 = dxCheckBox8;
                        dxCheckBox9.CheckedChanged += (o, e) =>
                        {
                            DXCheckBox dxCheckBox = o as DXCheckBox;
                            ItemFilter.Items[(int)dxCheckBox.Tag]?.SetValue(4, dxCheckBox.Checked);
                        };
                        ItemView.SetItem(nItem, 5U, dxCheckBox9);
                        DXCheckBox dxCheckBox10 = new DXCheckBox();
                        dxCheckBox10.AutoSize = false;
                        dxCheckBox10.Checked = citemFilterSet.buy;
                        dxCheckBox10.Tag = (object)index;
                        DXCheckBox dxCheckBox11 = dxCheckBox10;
                        dxCheckBox11.CheckedChanged += (o, e) =>
                        {
                            DXCheckBox dxCheckBox = o as DXCheckBox;
                            ItemFilter.Items[(int)dxCheckBox.Tag]?.SetValue(5, dxCheckBox.Checked);
                        };
                        ItemView.SetItem(nItem, 6U, dxCheckBox11);
                        DXCheckBox dxCheckBox12 = new DXCheckBox();
                        dxCheckBox12.AutoSize = false;
                        dxCheckBox12.Checked = citemFilterSet.buy;
                        dxCheckBox12.Enabled = false;
                        dxCheckBox12.Tag = (object)index;
                        DXCheckBox dxCheckBox13 = dxCheckBox12;
                        dxCheckBox13.CheckedChanged += (o, e) =>
                        {
                            DXCheckBox dxCheckBox = o as DXCheckBox;
                            ItemFilter.Items[(int)dxCheckBox.Tag]?.SetValue(6, dxCheckBox.Checked);
                        };
                        ItemView.SetItem(nItem, 7U, dxCheckBox13);
                    }
                }

                ItemView.UpdateItems();
            }
            public void Initialize()
            {
                RefreshItems();
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
                if (!ItemView.Visible)
                    return;
                DXListView itemView1 = ItemView;
                int x = 5;
                int y1 = Search.Location.Y;
                Size size1 = Search.Size;
                int height1 = size1.Height;
                int y2 = y1 + height1;
                Point point = new Point(x, y2);
                itemView1.Location = point;
                DXListView itemView2 = ItemView;
                size1 = Size;
                int width = size1.Width - 10;
                size1 = Size;
                int height2 = size1.Height - ItemView.Location.Y;
                Size size2 = new Size(width, height2);
                itemView2.Size = size2;
            }
        }

        public class DXViewRangeObjectTab : DXTab
        {
            public List<MapObject> Objects;

            public DXViewRangeObjectTab()
            {
                Objects = new List<MapObject>();
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
            }
        }

        public class DXMagicHelperTab : DXTab
        {
            private static List<ItemInfo> Amulets = null;
            private static List<ItemInfo> Poisons = null;
            private static void FillAmulets()
            {
                if (Amulets != null) return;

                Amulets = new List<ItemInfo>();
                foreach (var item in Globals.ItemInfoList.Binding)
                {
                    if (item.ItemType != ItemType.Amulet || item.BlockMonsterDrop) continue;
                    Amulets.Add(item);
                }
            }
            private static void FillPoison()
            {
                if (Poisons != null) return;

                Poisons = new List<ItemInfo>();
                foreach (var item in Globals.ItemInfoList.Binding)
                {
                    if (item.ItemType != ItemType.Poison || item.BlockMonsterDrop) continue;
                    Poisons.Add(item);
                }
            }

            public DXListView MagicView;

            public DXMagicHelperTab()
            {
                DXListView dxListView = new DXListView();
                dxListView.Parent = this;
                dxListView.Size = new Size(410, 405);
                dxListView.Location = new Point(140, 0);
                MagicView = dxListView;
                int num1 = (int)MagicView.InsertColumn(0U, "魔法名称", 90, 24, "只显示已学过的技能");
                int num2 = (int)MagicView.InsertColumn(1U, "等级", 35, 24, "技能等级");
                int num3 = (int)MagicView.InsertColumn(2U, "快捷键", 80, 24, "选中技能，按下F1-F12注册一个快捷键");
                int num4 = (int)MagicView.InsertColumn(3U, "扩展", 70, 24, "保留");
                int num5 = (int)MagicView.InsertColumn(4U, "锁人", 35, 24, "设置自动锁定人");
                int num6 = (int)MagicView.InsertColumn(5U, "锁怪", 35, 24, "设置自动锁定怪");
                int num7 = (int)MagicView.InsertColumn(6U, "毒符设定", 150, 24, "道士的自动换符设置");
            }

            public override void OnSizeChanged(Size oValue, Size nValue)
            {
                base.OnSizeChanged(oValue, nValue);
                DXListView magicView = MagicView;
                Size size1 = Size;
                int width = size1.Width - 20;
                size1 = Size;
                int height = size1.Height - 20;
                Size size2 = new Size(width, height);
                magicView.Size = size2;
                MagicView.Location = new Point(10, 10);
            }

            public void SetShortcutKey(ClientUserMagic magic, Keys KeyCode)
            {
                SpellKey spellKey = SpellKey.None;
                foreach (KeyBindAction keyBindAction in CEnvir.GetKeyAction(KeyCode))
                {
                    switch (keyBindAction)
                    {
                        case KeyBindAction.SpellUse01:
                            spellKey = SpellKey.Spell01;
                            break;
                        case KeyBindAction.SpellUse02:
                            spellKey = SpellKey.Spell02;
                            break;
                        case KeyBindAction.SpellUse03:
                            spellKey = SpellKey.Spell03;
                            break;
                        case KeyBindAction.SpellUse04:
                            spellKey = SpellKey.Spell04;
                            break;
                        case KeyBindAction.SpellUse05:
                            spellKey = SpellKey.Spell05;
                            break;
                        case KeyBindAction.SpellUse06:
                            spellKey = SpellKey.Spell06;
                            break;
                        case KeyBindAction.SpellUse07:
                            spellKey = SpellKey.Spell07;
                            break;
                        case KeyBindAction.SpellUse08:
                            spellKey = SpellKey.Spell08;
                            break;
                        case KeyBindAction.SpellUse09:
                            spellKey = SpellKey.Spell09;
                            break;
                        case KeyBindAction.SpellUse10:
                            spellKey = SpellKey.Spell10;
                            break;
                        case KeyBindAction.SpellUse11:
                            spellKey = SpellKey.Spell11;
                            break;
                        case KeyBindAction.SpellUse12:
                            spellKey = SpellKey.Spell12;
                            break;
                        case KeyBindAction.SpellUse13:
                            spellKey = SpellKey.Spell13;
                            break;
                        case KeyBindAction.SpellUse14:
                            spellKey = SpellKey.Spell14;
                            break;
                        case KeyBindAction.SpellUse15:
                            spellKey = SpellKey.Spell15;
                            break;
                        case KeyBindAction.SpellUse16:
                            spellKey = SpellKey.Spell16;
                            break;
                        case KeyBindAction.SpellUse17:
                            spellKey = SpellKey.Spell17;
                            break;
                        case KeyBindAction.SpellUse18:
                            spellKey = SpellKey.Spell18;
                            break;
                        case KeyBindAction.SpellUse19:
                            spellKey = SpellKey.Spell19;
                            break;
                        case KeyBindAction.SpellUse20:
                            spellKey = SpellKey.Spell20;
                            break;
                        case KeyBindAction.SpellUse21:
                            spellKey = SpellKey.Spell21;
                            break;
                        case KeyBindAction.SpellUse22:
                            spellKey = SpellKey.Spell22;
                            break;
                        case KeyBindAction.SpellUse23:
                            spellKey = SpellKey.Spell23;
                            break;
                        case KeyBindAction.SpellUse24:
                            spellKey = SpellKey.Spell24;
                            break;
                        default:
                            continue;
                    }
                }
                if (spellKey == SpellKey.None)
                    return;
                switch (GameScene.Game.MagicBarBox.SpellSet)
                {
                    case 1:
                        magic.Set1Key = spellKey;
                        break;
                    case 2:
                        magic.Set2Key = spellKey;
                        break;
                    case 3:
                        magic.Set3Key = spellKey;
                        break;
                    case 4:
                        magic.Set4Key = spellKey;
                        break;
                }
                foreach (KeyValuePair<MagicInfo, ClientUserMagic> magic1 in GameScene.Game.User.Magics)
                {
                    if (magic1.Key == magic.Info)
                        GameScene.Game.MagicBox.Magics[magic1.Key].Refresh();
                    else if (magic1.Value.Set1Key == magic.Set1Key && (uint)magic.Set1Key > 0U)
                    {
                        magic1.Value.Set1Key = SpellKey.None;
                        GameScene.Game.MagicBox.Magics[magic1.Key].Refresh();
                    }
                    else if (magic1.Value.Set2Key == magic.Set2Key && (uint)magic.Set2Key > 0U)
                    {
                        magic1.Value.Set2Key = SpellKey.None;
                        GameScene.Game.MagicBox.Magics[magic1.Key].Refresh();
                    }
                    else if (magic1.Value.Set3Key == magic.Set3Key && (uint)magic.Set3Key > 0U)
                    {
                        magic1.Value.Set3Key = SpellKey.None;
                        GameScene.Game.MagicBox.Magics[magic1.Key].Refresh();
                    }
                    else if (magic1.Value.Set4Key == magic.Set4Key && (uint)magic.Set4Key > 0U)
                    {
                        magic1.Value.Set4Key = SpellKey.None;
                        GameScene.Game.MagicBox.Magics[magic1.Key].Refresh();
                    }
                }
                CEnvir.Enqueue((Packet)new MagicKey()
                {
                    Magic = magic.Info.Magic,
                    Set1Key = magic.Set1Key,
                    Set2Key = magic.Set2Key,
                    Set3Key = magic.Set3Key,
                    Set4Key = magic.Set4Key
                });
                GameScene.Game.MagicBarBox.UpdateIcons();
            }

            public void UpdateMagic()
            {
                if (GameScene.Game.User == null) return;

                FillAmulets();
                FillPoison();

                uint nItem = 0;
                MagicView.RemoveAll();
                Type type = typeof(SpellKey);
                foreach (KeyValuePair<MagicInfo, ClientUserMagic> magic in GameScene.Game.User.Magics)
                {
                    if (magic.Value.Info.School == MagicSchool.Passive 
                        || magic.Value.Info.School == MagicSchool.None
                        || magic.Value.Info.Magic == MagicType.None) continue;

                    ClientUserMagic clientUserMagic = magic.Value;

                    MagicHelper magicHelper = null;
                    for (int index = 0; index < Config.magics.Count; ++index)
                    {
                        if (Config.magics[index].TypeID == clientUserMagic.Info.Magic)
                        {
                            magicHelper = Config.magics[index];
                            break;
                        }
                    }
                    if (magicHelper == null)
                    {
                        magicHelper = new MagicHelper()
                        {
                            TypeID = clientUserMagic.Info.Magic,
                            Name = clientUserMagic.Info.Name,
                            Key = clientUserMagic.Set1Key,
                            LockPlayer = false,
                            LockMonster = false,
                            Amulet = -1
                        };
                        Config.magics.Add(magicHelper);
                    }

                    magicHelper.obj = (object)clientUserMagic;
                    magicHelper.Name = clientUserMagic.Info.Name;
                    nItem = MagicView.InsertItem(nItem, clientUserMagic.Info.Name);
                    DXControl control = MagicView.Items.Controls[(int)nItem];
                    control.Tag = (object)magicHelper;
                    MagicView.SetItem(nItem, 1U, clientUserMagic.Level.ToString() ?? "");

                    string text1 = "";
                    if ((uint)clientUserMagic.Set1Key > 0U)
                        text1 = Functions.GetEnumDesc(clientUserMagic.Set1Key);// type.GetMember(clientUserMagic.Set1Key.ToString())[0].GetCustomAttribute<DescriptionAttribute>().Description;

                    text1 = text1.Replace('\n', ' ');

                    (MagicView.SetItem(nItem, 2U, text1) as DXLabel).KeyUp += (EventHandler<KeyEventArgs>)((o, e) =>
                    {
                        DXControl dxControl = o as DXControl;
                        if (dxControl == null)
                            return;
                        MagicHelper tag = dxControl.Parent.Tag as MagicHelper;
                        if (tag == null)
                            return;
                        DescriptionAttribute customAttribute = typeof(SpellKey).GetMember(tag.Key.ToString())[0].GetCustomAttribute<DescriptionAttribute>();
                        dxControl.Text = customAttribute?.Description ?? "";
                        SetShortcutKey(tag.obj as ClientUserMagic, e.KeyCode);
                        e.Handled = true;
                    });
                    string text2 = "";
                    MagicView.SetItem(nItem, 3U, text2);
                    MagicView.SetItem(nItem, 4U, BigPatchDialog.CreateCheckBox(control, "", 0, 0, ((o, e) =>
                    {
                        DXCheckBox dxCheckBox = o as DXCheckBox;
                        if (dxCheckBox == null)
                            return;
                        MagicHelper tag = dxCheckBox.Parent.Tag as MagicHelper;
                        if (tag == null)
                            return;
                        tag.LockPlayer = dxCheckBox.Checked;
                    }), magicHelper.LockPlayer));
                    MagicView.SetItem(nItem, 5U, BigPatchDialog.CreateCheckBox(control, "", 0, 0, ((o, e) =>
                    {
                        DXCheckBox dxCheckBox = o as DXCheckBox;
                        if (dxCheckBox == null)
                            return;
                        MagicHelper tag = dxCheckBox.Parent.Tag as MagicHelper;
                        if (tag == null)
                            return;
                        tag.LockMonster = dxCheckBox.Checked;
                    }), magicHelper.LockMonster));
                    DXComboBox dxComboBox1 = new DXComboBox();
                    MagicView.SetItem(nItem, 6U, dxComboBox1);
                    DXListBoxItem dxListBoxItem1 = new DXListBoxItem();
                    dxListBoxItem1.Parent = dxComboBox1.ListBox;
                    dxListBoxItem1.Label.Text = "未选择";
                    dxListBoxItem1.Item = (object)-1;
                    dxComboBox1.SelectedItemChanged += ((o, e) =>
                    {
                        if (GameScene.Game.Observer)
                            return;
                        DXComboBox dxComboBox = o as DXComboBox;
                        if (dxComboBox == null)
                            return;
                        DXControl parent = dxComboBox.Parent;
                        if (parent == null)
                            return;
                        MagicHelper tag = parent.Tag as MagicHelper;
                        if (tag == null)
                            return;
                        tag.Amulet = (int)dxComboBox.SelectedItem;
                    });
                    if (clientUserMagic.Info.Magic == MagicType.PoisonDust)
                    {
                        DXListBoxItem listItem = new DXListBoxItem();
                        listItem.Parent = dxComboBox1.ListBox;
                        listItem.Label.Text = "红绿毒交替";
                        listItem.Item = 0;

                        foreach (var item in Poisons)
                        {
                            listItem = new DXListBoxItem();
                            listItem.Parent = dxComboBox1.ListBox;
                            listItem.Label.Text = item.ItemName;
                            listItem.Item = item.Index;
                        }
                    }
                    else if (clientUserMagic.Info.Class == MirClass.Taoist)
                    { 
                        if (CEnvir.NeedAmulet(clientUserMagic.Info))
                        {
                            foreach (var amulet in Amulets)
                            {
                                DXListBoxItem dxListBoxItem2 = new DXListBoxItem();
                                dxListBoxItem2.Parent = dxComboBox1.ListBox;
                                dxListBoxItem2.Label.Text = amulet.ItemName;
                                dxListBoxItem2.Item = amulet.Index;
                            }
                        }
                        else
                        {
                            dxListBoxItem1.Label.Text = "";
                            dxListBoxItem1.Enabled = false;
                            dxComboBox1.Enabled = false;
                        }
                    }

                    dxComboBox1.ListBox.SelectItem((object)magicHelper.Amulet);
                }
                MagicView.UpdateItems();
            }
        }
    }
}
