using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library;

namespace Client.Envir
{
    [ConfigPath(@".\Legend.ini")]
    public static class Config
    {
        public static readonly Size IntroSceneSize = new Size(1366, 768);

        [ConfigSection("Network")]
        public static string IPAddress { get; set; } = "mir.ibaboo.xyz";
        public static int Port { get; set; } = 53536;
        public static TimeSpan TimeOutDuration { get; set; } = TimeSpan.FromSeconds(15);


        [ConfigSection("Graphics")]
        public static bool FullScreen { get; set; } = false;
        public static bool VSync { get; set; }
        public static bool LimitFPS { get; set; } = true;
        public static Size GameSize { get; set; } = IntroSceneSize;
        public static TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(30);
        public static string MapPath { get; set; } = @".\Map\";
        public static bool ClipMouse { get; set; } = false;
        public static bool DebugLabel { get; set; } = false;
        public static float FontSizeMod { get; set; } = 0.0F;
        public static string Language { get; set; } = "Chinese";
        public static bool Borderless { get; set; } = false;


        [ConfigSection("Sound")]
        public static bool SoundInBackground { get; set; } = true;
        public static int SoundOverLap { get; set; } = 5;
        public static int SystemVolume { get; set; } = 25;
        public static int MusicVolume { get; set; } = 25;
        public static int PlayerVolume { get; set; } = 25;
        public static int MonsterVolume { get; set; } = 25;
        public static int MagicVolume { get; set; } = 25;

        [ConfigSection("Login")]
        public static bool RememberDetails { get; set; } = false;
        public static string RememberedEMail { get; set; } = string.Empty;
        public static string RememberedPassword { get; set; } = string.Empty;

        [ConfigSection("Game")]
        public static bool DrawEffects { get; set; } = true;
        public static bool ShowItemNames { get; set; } = true;
        public static bool ShowMonsterNames { get; set; } = true;
        public static bool ShowPlayerNames { get; set; } = true;
        public static bool ShowUserHealth { get; set; } = true;
        public static bool ShowMonsterHealth { get; set; } = true;
        public static bool ShowDamageNumbers { get; set; } = true;
        public static bool EscapeCloseAll { get; set; } = false;
        public static bool ShiftOpenChat { get; set; } = true;
        public static bool SpecialRepair { get; set; } = true;
        public static bool RightClickDeTarget { get; set; } = true;

        public static bool MonsterBoxExpanded { get; set; } = true;
        public static bool MonsterBoxVisible { get; set; } = true;
        public static bool QuestTrackerVisible { get; set; } = true;

        public static bool LogChat { get; set; } = true;

        public static int RankingClass { get; set; } = (int)RequiredClass.All;
        public static bool RankingOnline { get; set; } = true;

        [ConfigSection("BigPatch")]
        public static bool 自动烈火 { get; set; } = false;

        public static bool 自动翔空 { get; set; } = false;

        public static bool 自动莲月 { get; set; } = false;

        public static bool 自动铁布衫 { get; set; } = false;

        public static bool 自动破血 { get; set; } = false;

        public static bool 是否开启自动技能三连击 { get; set; } = false;

        public static int 技能三连击 { get; set; } = 0;

        public static bool 是否开启自动技能四连击 { get; set; } = false;

        public static int 技能四连击 { get; set; } = 0;

        public static bool 是否开启自动技能五连击 { get; set; } = false;

        public static int 技能五连击 { get; set; } = 0;

        public static bool 自动魔法盾 { get; set; } = false;

        public static bool 自动凝血 { get; set; } = false;

        public static bool 自动天打雷劈 { get; set; } = false;
        public static bool 自动魔光盾 { get; set; } = false;

        public static bool 自动换毒 { get; set; } = false;

        public static bool 自动换符 { get; set; } = false;

        public static bool 自动阴阳盾 { get; set; } = false;

        public static bool 自动四花 { get; set; } = false;

        public static bool 自动风之闪避 { get; set; } = false;

        public static bool 自动风之守护 { get; set; } = false;

        public static long 自动技能1多长时间使用一次 { get; set; } = 0;

        public static MagicType 自动技能1 { get; set; } = MagicType.None;

        public static bool 是否开启自动技能1 { get; set; } = false;

        public static long 自动技能2多长时间使用一次 { get; set; } = 0;

        public static MagicType 自动技能2 { get; set; } = MagicType.None;

        public static bool 是否开启自动技能2 { get; set; } = false;

        public static bool 是否开启Buff显示时间 { get; set; } = false;

        public static bool 是否开启自动添加敌人 { get; set; } = false;

        public static bool 是否开启自动PK模式 { get; set; } = false;

        public static bool 是否开启自动吃PK药水 { get; set; } = false;

        public static bool 开始挂机 { get; set; } = false;

        public static bool 自动上毒 { get; set; } = false;

        public static bool 自动躲避 { get; set; } = false;

        public static bool 死亡回城 { get; set; } = false;

        public static bool 是否开启挂机自动技能 { get; set; } = false;

        public static MagicType 挂机自动技能 { get; set; } = MagicType.None;

        public static Point 范围挂机坐标 { get; set; } = Point.Empty;

        public static long 范围距离 { get; set; } = 0;

        public static bool 范围挂机 { get; set; } = false;

        public static long 血量剩下百分之多少时自动回城 { get; set; } = 0;

        public static bool 是否开启回城保护 { get; set; } = false;

        public static long 血量剩下百分之多少时自动随机 { get; set; } = 0;

        public static bool 是否开启随机保护 { get; set; } = false;

        public static long 隔多少秒自动随机一次 { get; set; } = 0;

        public static bool 是否开启每间隔自动随机 { get; set; } = false;

        public static long 多少秒无经验或者未杀死目标自动随机 { get; set; } = 0;

        public static bool 是否开启指定时间无经验或者未杀死目标自动随机 { get; set; } = false;

        public static bool 免助跑 { get; set; } = false;
        public static bool 数字显血 { get; set; } = false;
        public static bool 在安全处有效 { get; set; } = false;
        public static bool 组队数字显血 { get; set; } = false;
        public static bool Boss提示 { get; set; } = false;
        public static bool 金币自动换金票 { get; set; } = false;
        public static bool 快速小退 { get; set; } = false;
        public static bool 跑不停 { get; set; } = false;
        public static bool 自动关组 { get; set; } = false;
        public static bool 免蜡烛 { get; set; } = false;

        public static bool 显示他人 { get; set; } = false;

        public static bool 快速选择 { get; set; } = false;
        public static bool 显示目标 { get; set; } = false;
        public static bool 装备持久警告 { get; set; } = false;
        public static bool 清理尸体 { get; set; } = false;
        public static bool 怪物等级显示 { get; set; } = false;

        public static bool 免SHIFT { get; set; } = false;

        public static bool 攻击锁定目标 { get; set; } = false;
        public static bool 关闭经验提示 { get; set; } = false;
        public static bool 锁怪效果 { get; set; } = false;
        public static bool 稳如泰山 { get; set; } = false;
        public static bool 转生残影 { get; set; } = false;
        public static bool 死亡红屏 { get; set; } = false;

        public static bool 显示目标信息 { get; set; } = false;

        public static bool 退出战争 { get; set; } = false;

        public static bool 躲避石化 { get; set; } = false;
        public static bool 攻击目标改颜色 { get; set; } = false;

        public static bool 好友改颜色 { get; set; } = false;
        public static bool 快速自动拾取 { get; set; } = false;

        public static bool Tab捡取 { get; set; } = false;

        public static int 天气效果 { get; set; } = 0;

        public static bool 是否开启自动练技能 { get; set; } = false;

        public static int 自动练F几技能 { get; set; } = 0;

        public static long 隔多少秒自动练技能 { get; set; } = 10;

        public static bool 是否开启鼠标中间按钮自动使用坐骑 { get; set; } = false;

        public static bool 是否开启鼠标中间按钮自动使用技能 { get; set; } = false;

        public static int 鼠标中间按钮使用F几的技能 { get; set; } = 0;

        public static bool 自动回复 { get; set; } = false;

        public static long 多少秒一次自动喊话 { get; set; } = 10000;

        public static int 自动回复时用第几行句子 { get; set; } = 0;

        public static bool 是否关闭NPC话 { get; set; } = false;

        public static bool ChkSaveSayRecord { get; set; } = false;

        public static bool 是否关闭怪物话 { get; set; } = false;

        public static List<string> 自动喊话内内容 { get; set; } = new List<string>();

        public static List<MagicHelper> magics { get; set; } = new List<MagicHelper>();

        public static int 自动登录延迟时间 { get; set; } = 30;
    }
}
