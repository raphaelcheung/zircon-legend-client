﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.UserModels;
using Library;
using Library.SystemModels;

//Cleaned
namespace Client.Scenes.Views
{
    public sealed class BuffDialog : DXWindow
    {
        #region Properties
        private Dictionary<ClientBuffInfo, DXImageControl> Icons = new Dictionary<ClientBuffInfo, DXImageControl>();

        public override WindowType Type => WindowType.BuffBox;
        public override bool CustomSize => false;
        public override bool AutomaticVisiblity => true;
        #endregion

        public BuffDialog()
        {
            HasTitle = false;
            HasFooter = false;
            HasTopBorder = false;
            TitleLabel.Visible = false;
            CloseButton.Visible = false;
            Opacity = 0.6F;
            
            Size = new Size(30, 30);
        }

        #region Methods
        public void BuffsChanged()
        {
            foreach (DXImageControl control in Icons.Values)
                control.Dispose();

            Icons.Clear();

            List<ClientBuffInfo> buffs = MapObject.User.Buffs.ToList();

            Stats permStats = new Stats();

            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                ClientBuffInfo buff = buffs[i];

                switch (buff.Type)
                {
                    case BuffType.ItemBuff:
                        if (buff.RemainingTime != TimeSpan.MaxValue) continue;

                        permStats.Add(Globals.ItemInfoList.Binding.First(x => x.Index == buff.ItemIndex).Stats);

                        buffs.Remove(buff);
                        break;
                    case BuffType.Ranking:
                    case BuffType.Developer:
                        buffs.Remove(buff);
                        break;
                }
            }
            
            if (permStats.Count > 0)
                buffs.Add(new ClientBuffInfo { Index = 0, Stats = permStats, Type = BuffType.ItemBuffPermanent, RemainingTime = TimeSpan.MaxValue });

            buffs.Sort((x1, x2) => x2.RemainingTime.CompareTo(x1.RemainingTime));




            foreach (ClientBuffInfo buff in buffs)
            {
                DXImageControl icon;
                Icons[buff] = icon = new DXImageControl
                {
                    Parent = this,
                    LibraryFile = LibraryFile.CBIcon,
                };

                switch (buff.Type)
                {
                    case BuffType.Heal:
                        icon.Index = 78;
                        break;
                    case BuffType.Invisibility:
                        icon.Index = 74;
                        break;
                    case BuffType.MagicResistance:
                        icon.Index = 92;
                        break;
                    case BuffType.Resilience:
                        icon.Index = 91;
                        break;
                    case BuffType.PoisonousCloud:
                        icon.Index = 98;
                        break;
                    case BuffType.Castle:
                        icon.Index = 242;
                        break;
                    case BuffType.FullBloom:
                        icon.Index = 162;
                        break;
                    case BuffType.WhiteLotus:
                        icon.Index = 163;
                        break;
                    case BuffType.RedLotus:
                        icon.Index = 164;
                        break;
                    case BuffType.MagicShield:
                        icon.Index = 100;
                        break;
                    case BuffType.FrostBite:
                        icon.Index = 221;
                        break;
                    case BuffType.ElementalSuperiority:
                        icon.Index = 93;
                        break;
                    case BuffType.BloodLust:
                        icon.Index = 90;
                        break;
                    case BuffType.Cloak:
                        icon.Index = 160;
                        break;
                    case BuffType.GhostWalk:
                        icon.Index = 160;
                        break;
                    case BuffType.Observable:
                        icon.Index = 172;
                        break;
                    case BuffType.TheNewBeginning:
                        icon.Index = 166;
                        break;
                    case BuffType.Veteran:
                        icon.Index = 171;
                        break;

                    case BuffType.Brown:
                        icon.Index = 229;
                        break;
                    case BuffType.PKPoint:
                        icon.Index = 266;
                        break;
                    case BuffType.Redemption:
                        icon.Index = 258;
                        break;
                    case BuffType.Renounce:
                        icon.Index = 94;
                        break;
                    case BuffType.Defiance:
                        icon.Index = 97;
                        break;
                    case BuffType.Might:
                        icon.Index = 96;
                        break;
                    case BuffType.ReflectDamage:
                        icon.Index = 98;
                        break;
                    case BuffType.Endurance:
                        icon.Index = 95;
                        break;
                    case BuffType.JudgementOfHeaven:
                        icon.Index = 99;
                        break;
                    case BuffType.StrengthOfFaith:
                        icon.Index = 141;
                        break;
                    case BuffType.CelestialLight:
                        icon.Index = 142;
                        break;
                    case BuffType.Transparency:
                        icon.Index = 160;
                        break;
                    case BuffType.LifeSteal:
                        icon.Index = 98;
                        break;
                    case BuffType.DarkConversion:
                        icon.Index = 166;
                        break;
                    case BuffType.DragonRepulse:
                        icon.Index = 165;
                        break;
                    case BuffType.Evasion:
                        icon.Index = 167;
                        break;
                    case BuffType.RagingWind:
                        icon.Index = 168;
                        break;
                    case BuffType.MagicWeakness:
                        icon.Index = 182;
                        break;
                    case BuffType.ItemBuff:
                        icon.Index = Globals.ItemInfoList.Binding.First(x => x.Index == buff.ItemIndex).BuffIcon;
                        break;
                    case BuffType.PvPCurse:
                        icon.Index = 241;
                        break;

                    case BuffType.ItemBuffPermanent:
                        icon.Index = 81;
                        break;
                    case BuffType.HuntGold:
                        icon.Index = 264;
                        break;
                    case BuffType.Companion:
                        icon.Index = 137;
                        break;
                    case BuffType.MapEffect:
                        icon.Index = 76;
                        break;
                    case BuffType.Guild:
                        icon.Index = 140;
                        break;
                    default:
                        icon.Index = 73;
                        break;
                }

                icon.ProcessAction = () =>
                {
                    if (MouseControl == icon)
                        icon.Hint = GetBuffHint(buff);
                };
            }

            for (int i = 0; i < buffs.Count; i++)
                Icons[buffs[i]].Location = new Point(3 + (i%6)*27, 3 + (i/6)*27);

            Size = new Size(3 + Math.Min(6, Math.Max(1, Icons.Count))*27, 3 + Math.Max(1, 1 +  (Icons.Count - 1)/6) * 27);
            
        }
        private string GetBuffHint(ClientBuffInfo buff)
        {
            string text = string.Empty;

            Stats stats = buff.Stats;

            switch (buff.Type)
            {
                case BuffType.Server:
                    text = $"服务器设置\n";
                    break;
                case BuffType.HuntGold:
                    text = $"猎币\n";
                    break;
                case BuffType.Observable:
                    text = $"观众\n\n" +
                           $"你允许观众观看你的游玩.\n";
                    break;
                case BuffType.Veteran:
                    text = $"老兵\n";
                    break;
                case BuffType.Brown:
                    text = $"灰名\n";
                    break;
                case BuffType.PKPoint:
                    text = $"PK 点\n";
                    break;
                case BuffType.Redemption:
                    text = $"救赎之钥石\n";
                    break;
                case BuffType.Castle:
                    text = $"城堡主\n";
                    break;
                case BuffType.Guild:
                    text = $"帮会\n";
                    break;
                case BuffType.MapEffect:
                    text = $"地图效果\n";
                    break;
                case BuffType.ItemBuff:
                    ItemInfo info = Globals.ItemInfoList.Binding.First(x => x.Index == buff.ItemIndex);
                    text = info.ItemName + "\n";
                    stats = info.Stats;
                    break;
                case BuffType.ItemBuffPermanent:
                    text = "永久物品增益\n";
                    break;
                case BuffType.Defiance:
                    text = $"反抗\n";
                    break;
                case BuffType.Might:
                    text = $"力量\n";
                    break;
                case BuffType.Endurance:
                    text = $"忍耐力\n";
                    break;
                case BuffType.ReflectDamage:
                    text = $"反射伤害\n";
                    break;
                case BuffType.Renounce:
                    text = $"凝血\n";
                    break;
                case BuffType.MagicShield:
                    text = $"魔法盾\n";
                    break;
                case BuffType.FrostBite:
                    text = $"冻伤\n";
                    break;
                case BuffType.JudgementOfHeaven:
                    text = $"天神审判\n";
                    break;
                case BuffType.Heal:
                    text = $"恢复\n";
                    break;
                case BuffType.Invisibility:
                    text = $"隐身\n";

                    text += $"视线躲藏.\n";
                    break;
                case BuffType.MagicResistance:
                    text = $"魔法抵抗\n";
                    break;
                case BuffType.Resilience:
                    text = $"快速恢复\n";
                    break;
                case BuffType.ElementalSuperiority:
                    text = $"元素优势\n";
                    break;
                case BuffType.BloodLust:
                    text = $"嗜血\n";
                    break;
                case BuffType.StrengthOfFaith:
                    text = $"信仰的力量\n";
                    break;
                case BuffType.CelestialLight:
                    text = $"天光\n";
                    break;
                case BuffType.Transparency:
                    text = $"秒影无踪\n";
                    break;
                case BuffType.LifeSteal:
                    text = $"生命窃取\n";
                    break;
                case BuffType.PoisonousCloud:
                    text = $"毒云\n";
                    break;
                case BuffType.FullBloom:
                    text = $"盛放\n";
                    break;
                case BuffType.WhiteLotus:
                    text = $"白莲\n";
                    break;
                case BuffType.RedLotus:
                    text = $"红莲\n";
                    break;
                case BuffType.Cloak:
                    text = $"潜行\n";
                    break;
                case BuffType.GhostWalk:
                    text = $"幽灵漫步\n\n" +
                           $"让你在隐形状态下快速移动.";
                    break;
                case BuffType.TheNewBeginning:
                    text = $"新的开始\n";
                    break;
                case BuffType.DarkConversion:
                    text = $"黑暗转换\n";
                    break;
                case BuffType.DragonRepulse:
                    text = $"龙击\n";
                    break;
                case BuffType.Evasion:
                    text = $"回避\n";
                    break;
                case BuffType.RagingWind:
                    text = $"狂风\n";
                    break;
                case BuffType.MagicWeakness:
                    text = $"魔法弱点\n\n" +
                           $"你的魔法抵抗力大幅降低.\n";
                    break;
                case BuffType.Companion:
                    text = $"小伙伴\n";
                    break;
            }
            
            if (stats != null && stats.Count > 0)
            {
                foreach (KeyValuePair<Stat, int> pair in stats.Values)
                {
                    if (pair.Key == Stat.Duration) continue;

                    string temp = stats.GetDisplay(pair.Key);

                    if (temp == null) continue;
                    text += $"\n{temp}";
                }

                if (buff.RemainingTime != TimeSpan.MaxValue)
                    text += $"\n";
            }

            if (buff.RemainingTime != TimeSpan.MaxValue)
                text += $"\n持续：{Functions.ToString(buff.RemainingTime, true)}";

            if (buff.Pause) text += "\n暂停（无效）.";

            return text;
        }

        public override void Process()
        {
            base.Process();

            foreach (KeyValuePair<ClientBuffInfo, DXImageControl> pair in Icons)
            {
                if (pair.Key.Pause)
                {
                    pair.Value.ForeColour = Color.IndianRed;
                    continue;
                }
                    if (pair.Key.RemainingTime == TimeSpan.MaxValue) continue;

                if (pair.Key.RemainingTime.TotalSeconds >= 10)
                {
                    pair.Value.ForeColour = Color.White;
                    continue;
                }
                
                float rate = pair.Key.RemainingTime.Milliseconds / (float)1000;

                pair.Value.ForeColour = Functions.Lerp(Color.White, Color.CadetBlue, rate);
            }

            Hint = Icons.Count > 0 ? null : "增益";


        }
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                foreach (KeyValuePair<ClientBuffInfo, DXImageControl> pair in Icons)
                {
                    if (pair.Value == null) continue;

                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }

                Icons.Clear();
                Icons = null;
            }

        }

        #endregion
    }

}
