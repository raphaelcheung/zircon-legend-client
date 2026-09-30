using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Controls;
using Client.Models;
using Client.Scenes;
using Client.Scenes.Views;
using Client.UserModels;
using Library;
using Library.Network;
using Library.SystemModels;
using MirDB;
using SlimDX.Direct3D9;
using System.IO.IsolatedStorage;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Security.Cryptography.X509Certificates;
using Client.Envir.Translations;
using System.Reflection;
using C = Library.Network.ClientPackets;
using Library.Network.GeneralPackets;

namespace Client.Envir
{
    public static class CEnvir
    {
        public static readonly string[] ChatCommands = { "!!", "!~", "!@", "@!", "!", "@", "#" };
        #region 颜色定义
        public static Color LocalTextColour { get; } = Color.White;
        public static Color GMWhisperInTextColour { get; } = Color.Red;
        public static Color WhisperInTextColour { get;  } = Color.Cyan;
        public static Color WhisperOutTextColour { get;  } = Color.Aquamarine;
        public static Color GroupTextColour { get;  } = Color.Plum;
        public static Color GuildTextColour { get; } = Color.LightPink;
        public static Color ShoutTextColour { get; } = Color.Yellow;
        public static Color GlobalTextColour { get; } = Color.Orange;
        public static Color ObserverTextColour { get; } = Color.Silver;
        public static Color HintTextColour { get; } = Color.AntiqueWhite;
        public static Color SystemTextColour { get; } = Color.Red;
        public static Color GainsTextColour { get; } = Color.GreenYellow;
        public static Color AnnouncementTextColour { get; } = Color.DarkBlue;
        #endregion
        public static TargetForm Target { get; set; }
        public static Random Random = new Random();

        public static string RootPath { get; private set; }

        private static DateTime _FPSTime;
        private static int FPSCounter;
        private static int FPSCount;

        public static int DPSCounter;
        private static int DPSCount;

        public static bool IsQuickGame { get; set; } = false;
        private static bool LauncherUpgrading { get; set; } = false;

        public static int QuickSelectCharacter { get; set; } = -1;

        public static bool Shift, Alt, Ctrl;
        public static DateTime Now;
        public static Point MouseLocation;
        public static bool SafeDisconnected { get; set; } = false;

        public static string LauncherHash { get; set; }
        public static byte[] LauncherDatas { get; set; }

        public static CConnection Connection { get; set; }
        public static bool WrongVersion { get; set; }

        public static Dictionary<LibraryFile, MirLibrary> LibraryList = new Dictionary<LibraryFile, MirLibrary>();

        public static ClientUserItem[] MainStorage;
        public static ClientUserItem[] Storage { get; set; }


        public static List<ClientBlockInfo> BlockList = new List<ClientBlockInfo>();
        public static DBCollection<KeyBindInfo> KeyBinds { get; set; }
        public static DBCollection<WindowSetting> WindowSettings { get; set; }
        public static DBCollection<CastleInfo> CastleInfoList;
        public static Session Session;
        
        public static ConcurrentQueue<string> ChatLog = new ConcurrentQueue<string>();

        public static bool Loaded { get; set; }
        public static string BuyAddress;
        public static string C { get; private set; }

        public static bool TestServer;

        private static bool LoadingDb = false;

        public static bool DbVersionChecked { get; set; } = false;
        public static bool DbVersionChecking { get; set; } = false;

        public static StringMessages Language { get; set; }
        public static string LogonCharacterDesc { get; set; } = "";

        public static int WeaponRefineLevelLimit { get; set; } = 17;
        public static int WeaponRefineRarityStep { get; set; } = 2;

        public static int SkillLevelLimit { get; set; } = 3;
            
        static CEnvir()
        {
            RootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


            Thread workThread = new Thread(SaveChatLoop) { IsBackground = true };
            workThread.Start();

            try
            { A(); }
            catch { }
        }

        public static void LoadLanguage()
        {
            switch (Config.Language.ToUpper())
            {
                case "ENGLISH":
                    Language = (StringMessages)ConfigReader.ConfigObjects[typeof(EnglishMessages)]; //Todo Language Selections
                    break;
                case "CHINESE":
                    Language = (StringMessages)ConfigReader.ConfigObjects[typeof(ChineseMessages)]; //Todo Language Selections
                    break;
            }
        }

        private static void A()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Zircon");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(path + "\\CheckSum.bin"))
            {
                using (BinaryReader E = new BinaryReader(File.OpenRead(path + "\\CheckSum.bin")))
                    C = E.ReadString();
            }
            else
            {
                using (BinaryWriter E = new BinaryWriter(File.Create(path + "\\CheckSum.bin")))
                    E.Write(C = Functions.RandomString(Random, 20));
            }

        }

        public static void CheckLauncherUpgrade()
        {
            if (string.IsNullOrEmpty(LauncherHash))
            { 
                SaveError($"命令行没有发送启动器的 Hash 码，进行强制更新..."); 
                if (!LauncherUpgrading)
                {
                    LauncherUpgrading = true;
                    LauncherDatas = null;
                    Connection.Enqueue(new C.UpgradeClient()
                    {
                        FileKey = "./Launcher.exe",
                    });
                    return;
                }
            }

            if (LauncherUpgrading) return;
            LauncherUpgrading = true;

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "Launcher.exe");
            var datas = File.ReadAllBytes(path);
            if (Functions.CalcMD5(datas) == LauncherHash)
            {
                LauncherUpgrading = false;
                SaveError($"启动器已经是最新，无需更新");
                return;
            }

            SaveError($"启动器发现更新版本，更新中...");
            LauncherDatas = null;
            Connection.Enqueue(new C.UpgradeClient()
            {
                FileKey = "./Launcher.exe",
            });
        }

        public static void Upgrade(string file, int total_size, int index, byte[] datas)
        {
            if (!LauncherUpgrading) return;

            if (total_size <= 0)
            {
                SaveError($"更新 Launcher.exe 时收到 0 大小的异常数据包，更新失败");
                LauncherUpgrading = false;
                LauncherDatas = null;
                return;
            }

            if (LauncherDatas == null)
                LauncherDatas = new byte[total_size];

            try
            {
                datas.CopyTo(LauncherDatas, index);

                if ((index + datas.Length) >= total_size)
                {
                    string path = Path.Combine(RootPath, "Launcher.exe");
                    File.WriteAllBytes(path, LauncherDatas);

                    SaveError($"更新成功 {path}，文件大小 {Functions.BytesToString(total_size)}");

                    LauncherDatas = null;
                    LauncherUpgrading = false;
                }
            }
            catch (Exception ex)
            {
                SaveError($"更新 Launcher.exe 时发现异常");
                SaveError(ex.Message);
                SaveError(ex.StackTrace);
                LauncherDatas = null;
                LauncherUpgrading = false;
            }

            Thread.Sleep(1);
        }

        public static void SaveChatLoop()
        {
            List<string> lines = new List<string>();
            while (true)
            {
                while (ChatLog.IsEmpty)
                    Thread.Sleep(1000);

                while (!ChatLog.IsEmpty)
                {
                    string line;

                    if (!ChatLog.TryDequeue(out line)) continue;

                    lines.Add(line);
                }

                try
                {
                    File.AppendAllLines(@".\Chat Logs.txt", lines);
                    lines.Clear();
                }
                catch
                {
                }
            }
        }

        public static void GameLoop()
        {
            DateTime counter = DateTime.MinValue;

            if (Config.LimitFPS)
                counter = DateTime.Now.AddMilliseconds(1000 / 60);

            UpdateGame();
            RenderGame();

            if (Config.LimitFPS)
                if(DateTime.Now < counter)
                    Thread.Sleep(3);
        }
        private static void UpdateGame()
        {
            Now = Time.Now;
            DXControl.ActiveScene?.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, MouseLocation.X, MouseLocation.Y, 0));

            if (Time.Now >= _FPSTime)
            {
                _FPSTime = Time.Now.AddSeconds(1);
                FPSCount = FPSCounter;
                FPSCounter = 0;
                DPSCount = DPSCounter;
                DPSCounter = 0;
                DXManager.MemoryClear();
            }

            Connection?.Process();
          //  DXControl.ActiveScene?.Process();
            DXControl.ActiveScene?.Process();

            string debugText = $"FPS: {FPSCount}";

            if (DXControl.MouseControl != null)
                debugText += $", Mouse Control: {DXControl.MouseControl.GetType().Name}";

            if (DXControl.FocusControl != null)
                debugText += $", Focus Control: {DXControl.FocusControl.GetType().Name}";

            if (GameScene.Game != null)
            {
                if (DXControl.MouseControl is MapControl)
                    debugText += $", Co Ords: {GameScene.Game.MapControl.MapLocation}";

                debugText += $", Objects: {GameScene.Game.MapControl.Objects.Count}";

                if (MapObject.MouseObject != null)
                    debugText += $", Mouse Object: {MapObject.MouseObject.Name}";
            }
            debugText += $", DPS: {DPSCount}";


            DXControl.DebugLabel.Text = debugText;

            if (Connection != null)
            {
                const decimal KB = 1024;
                const decimal MB = KB*1024;

                string sent, received;


                if (Connection.TotalBytesSent > MB)
                    sent = $"{Connection.TotalBytesSent/MB:#,##0.0}MB";
                else if (Connection.TotalBytesSent > KB)
                    sent = $"{Connection.TotalBytesSent/KB:#,##0}KB";
                else
                    sent = $"{Connection.TotalBytesSent:#,##0}B";

                if (Connection.TotalBytesReceived > MB)
                    received = $"{Connection.TotalBytesReceived/MB:#,##0.0}MB";
                else if (Connection.TotalBytesReceived > KB)
                    received = $"{Connection.TotalBytesReceived/KB:#,##0}KB";
                else
                    received = $"{Connection.TotalBytesReceived:#,##0}B";

                DXControl.PingLabel.Text = $"Ping: {Connection.Ping}, Sent: {sent}, Received: {received}";
                DXControl.PingLabel.Location = new Point(DXControl.DebugLabel.DisplayArea.Right +5, DXControl.DebugLabel.DisplayArea.Y);
            }
            else
            {
                DXControl.PingLabel.Text = String.Empty;
            }


            if (DXControl.MouseControl != null && DXControl.ActiveScene != null)
            {
                DXControl.HintLabel.Text = DXControl.MouseControl.Hint;

                Point location = new Point(MouseLocation.X, MouseLocation.Y + 17);

                if (location.X + DXControl.HintLabel.Size.Width > DXControl.ActiveScene.Size.Width)
                    location.X = DXControl.ActiveScene.Size.Width - DXControl.HintLabel.Size.Width - 1;

                if (location.Y + DXControl.HintLabel.Size.Height > DXControl.ActiveScene.Size.Height)
                    location.Y = DXControl.ActiveScene.Size.Height - DXControl.HintLabel.Size.Height - 1;

                if (location.X < 0) location.X = 0;
                if (location.Y < 0) location.Y = 0;
                
                DXControl.HintLabel.Location = location;
            }
            else
            {
                DXControl.HintLabel.Text = null;
            }
        }
        private static void RenderGame()
        {
            try
            {
                if (Target.ClientSize.Width == 0 || Target.ClientSize.Height == 0)
                {
                    Thread.Sleep(1);
                    return;
                }

                if (DXManager.DeviceLost)
                {
                    DXManager.AttemptReset();
                    Thread.Sleep(1);
                    return;
                }

                DXManager.Device.Clear(ClearFlags.Target, Color.Black, 1, 0);
                DXManager.Device.BeginScene();
                DXManager.Sprite.Begin(SpriteFlags.AlphaBlend);
                
                DXControl.ActiveScene?.Draw();
                
                DXManager.Sprite.End();
                DXManager.Device.EndScene();

                DXManager.Device.Present();
                FPSCounter++;
            }
            catch (Direct3D9Exception)
            {
                DXManager.DeviceLost = true;
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());

                DXManager.AttemptRecovery();
            }
        }

        public static void ReturnToLogin()
        {
            if (DXControl.ActiveScene is LoginScene) return; // handle ?

            DXControl.ActiveScene.Dispose();
            DXSoundManager.StopAllSounds();
            DXControl.ActiveScene = new LoginScene(Config.IntroSceneSize);

            BlockList = new List<ClientBlockInfo>();
        }

        public static void LoadDatabase()
        {
            if (LoadingDb) return;

            LoadingDb = true;

            Task.Run(() =>
            {
                Session = new Session(SessionMode.Users, @".\Data\");

                Globals.ItemInfoList = Session.GetCollection<ItemInfo>();
                Globals.MagicInfoList = Session.GetCollection<MagicInfo>();
                Globals.MapInfoList = Session.GetCollection<MapInfo>();
                Globals.NPCPageList = Session.GetCollection<NPCPage>();
                Globals.MonsterInfoList = Session.GetCollection<MonsterInfo>();
                Globals.StoreInfoList = Session.GetCollection<StoreInfo>();
                Globals.NPCInfoList = Session.GetCollection<NPCInfo>();
                Globals.MovementInfoList = Session.GetCollection<MovementInfo>();
                Globals.QuestInfoList = Session.GetCollection<QuestInfo>();
                Globals.QuestTaskList = Session.GetCollection<QuestTask>();
                Globals.CompanionInfoList = Session.GetCollection<CompanionInfo>();
                Globals.CompanionLevelInfoList = Session.GetCollection<CompanionLevelInfo>();
                
                KeyBinds = Session.GetCollection<KeyBindInfo>();
                WindowSettings = Session.GetCollection<WindowSetting>();
                CastleInfoList = Session.GetCollection<CastleInfo>();

                Globals.GoldInfo = Globals.ItemInfoList.Binding.FirstOrDefault(x => x.Effect == ItemEffect.Gold);

                CheckKeyBinds();

                Loaded = true;
                LoadingDb = false;
                Session.BackUpSpace = TimeSpan.MaxValue;
            });
        }

        public static IEnumerable<KeyBindAction> GetKeyAction(Keys key)
        {
            if (!Loaded) yield break;

            switch (key)
            {
                case Keys.NumPad0:
                    key = Keys.D0;
                    break;
                case Keys.NumPad1:
                    key = Keys.D1;
                    break;
                case Keys.NumPad2:
                    key = Keys.D2;
                    break;
                case Keys.NumPad3:
                    key = Keys.D3;
                    break;
                case Keys.NumPad4:
                    key = Keys.D4;
                    break;
                case Keys.NumPad5:
                    key = Keys.D5;
                    break;
                case Keys.NumPad6:
                    key = Keys.D6;
                    break;
                case Keys.NumPad7:
                    key = Keys.D7;
                    break;
                case Keys.NumPad8:
                    key = Keys.D8;
                    break;
                case Keys.NumPad9:
                    key = Keys.D9;
                    break;
            }

            foreach (KeyBindInfo bind in KeyBinds.Binding)
            {
                if ((bind.Control1 == Ctrl && bind.Alt1 == Alt && bind.Shift1 == Shift && bind.Key1 == key) ||
                    (bind.Control2 == Ctrl && bind.Alt2 == Alt && bind.Shift2 == Shift && bind.Key2 == key))
                    yield return bind.Action;
            }
        }

        public static void FillStorage(List<ClientUserItem> items, bool observer)
        {
            Storage = new ClientUserItem[1000];

            if (!observer)
                MainStorage = Storage;


            foreach (ClientUserItem item in items)
                Storage[item.Slot] = item;
        }
        public static void Enqueue(Packet packet)
        {
            Connection?.Enqueue(packet);
        }

        public static void ResetKeyBinds()
        {
            for (int i = KeyBinds.Count - 1; i >= 0; i--)
                KeyBinds[i].Delete();

            CheckKeyBinds();
        }
        public static void CheckKeyBinds()
        {
            foreach (KeyBindAction action in Enum.GetValues(typeof(KeyBindAction)).Cast<KeyBindAction>())
            {
                switch (action)
                {
                    case KeyBindAction.None:
                        break;
                    default:
                        if (KeyBinds.Binding.Any(x => x.Action == action)) continue;

                        ResetKeyBind(action);
                        break;
                }
            }
        }

        public static void ResetKeyBind(KeyBindAction action)
        {
            KeyBindInfo bind = KeyBinds.CreateNewObject();
            bind.Action = action;

            switch (action)
            {
                case KeyBindAction.ConfigWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.O;
                    break;
                case KeyBindAction.CharacterWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.Q;
                    break;
                case KeyBindAction.InventoryWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.W;
                    break;
                case KeyBindAction.MagicWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.E;
                    break;
                case KeyBindAction.MagicBarWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.E;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.RankingWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.R;
                    break;
                case KeyBindAction.GameStoreWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.Y;
                    break;
                case KeyBindAction.CompanionWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.U;
                    break;
                case KeyBindAction.GroupWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.P;
                    break;
                case KeyBindAction.AutoPotionWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.A;
                    break;
                case KeyBindAction.StorageWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.S;
                    break;
                case KeyBindAction.BlockListWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.F;
                    break;
                case KeyBindAction.GuildWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.G;
                    break;
                case KeyBindAction.QuestLogWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.J;
                    break;
                case KeyBindAction.QuestTrackerWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.L;
                    break;
                case KeyBindAction.BeltWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.Z;
                    break;
                case KeyBindAction.MarketPlaceWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.C;
                    break;
                case KeyBindAction.MapMiniWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.V;
                    break;
                case KeyBindAction.MapBigWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.B;
                    break;
                case KeyBindAction.MailBoxWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.Oemcomma;
                    break;
                case KeyBindAction.MailSendWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.OemPeriod;
                    break;
                case KeyBindAction.ChatOptionsWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.O;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.ExitGameWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.Q;
                    bind.Alt1 = true;
                    bind.Key2 = Keys.X;
                    bind.Alt2 = true;
                    break;
                case KeyBindAction.ChangeAttackMode:
                    bind.Category = "功能";
                    bind.Key1 = Keys.H;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.ChangePetMode:
                    bind.Category = "功能";
                    bind.Key1 = Keys.A;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.GroupAllowSwitch:
                    bind.Category = "功能";
                    bind.Key1 = Keys.P;
                    bind.Alt1 = true;
                    break;
                case KeyBindAction.GroupTarget:
                    bind.Category = "功能";
                    bind.Key1 = Keys.G;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.TradeRequest:
                    bind.Category = "功能";
                    bind.Key1 = Keys.T;
                    break;
                case KeyBindAction.TradeAllowSwitch:
                    bind.Category = "功能";
                    bind.Key1 = Keys.T;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.MountToggle:
                    bind.Category = "功能";
                    bind.Key1 = Keys.M;
                    break;
                case KeyBindAction.AutoRunToggle:
                    bind.Category = "功能";
                    bind.Key1 = Keys.D;
                    break;
                case KeyBindAction.ChangeChatMode:
                    bind.Category = "功能";
                    bind.Key1 = Keys.K;
                    break;
                case KeyBindAction.ItemPickUp:
                    bind.Category = "物品";
                    bind.Key1 = Keys.Tab;
                    break;
                case KeyBindAction.PartnerTeleport:
                    bind.Category = "物品";
                    bind.Key1 = Keys.Z;
                    bind.Shift1 = true;
                    break;
                case KeyBindAction.ToggleItemLock:
                    bind.Category = "物品";
                    bind.Key1 = Keys.Scroll;
                    break;
                case KeyBindAction.UseBelt01:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D1;
                    bind.Key2 = Keys.D1;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt02:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D2;
                    bind.Key2 = Keys.D2;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt03:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D3;
                    bind.Key2 = Keys.D3;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt04:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D4;
                    bind.Key2 = Keys.D4;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt05:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D5;
                    bind.Key2 = Keys.D5;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt06:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D6;
                    bind.Key2 = Keys.D6;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt07:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D7;
                    bind.Key2 = Keys.D7;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt08:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D8;
                    bind.Key2 = Keys.D8;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt09:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D9;
                    bind.Key2 = Keys.D9;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt10:
                    bind.Category = "物品";
                    bind.Key1 = Keys.D0;
                    bind.Key2 = Keys.D0;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellSet01:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F1;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.SpellSet02:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F2;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.SpellSet03:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F3;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.SpellSet04:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F4;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.SpellUse01:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F1;
                    bind.Key2 = Keys.F1;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse02:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F2;
                    bind.Key2 = Keys.F2;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse03:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F3;
                    bind.Key2 = Keys.F3;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse04:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F4;
                    bind.Key2 = Keys.F4;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse05:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F5;
                    bind.Key2 = Keys.F5;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse06:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F6;
                    bind.Key2 = Keys.F6;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse07:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F7;
                    bind.Key2 = Keys.F7;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse08:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F8;
                    bind.Key2 = Keys.F8;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse09:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F9;
                    bind.Key2 = Keys.F9;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse10:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F10;
                    bind.Key2 = Keys.F10;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse11:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F11;
                    bind.Key2 = Keys.F11;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse12:
                    bind.Category = "技能";
                    bind.Key1 = Keys.F12;
                    bind.Key2 = Keys.F12;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse13:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse14:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse15:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse16:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse17:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse18:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse19:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse20:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse21:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse22:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse23:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.SpellUse24:
                    bind.Category = "技能";
                    break;
                case KeyBindAction.FortuneWindow:
                    bind.Category = "窗口";
                    bind.Key1 = Keys.W;
                    bind.Control1 = true;
                    break;
            }
        }

        public static float FontSize(float size)
        {
            return (size - Config.FontSizeMod) * (96F / DXManager.Graphics.DpiX);
        }

        public static int ErrorCount;
        private static string LastError;
        public static void SaveError(string ex)
        {
            try
            {
                if (++ErrorCount > 200 || String.Compare(ex, LastError, StringComparison.OrdinalIgnoreCase) == 0) return;

                const string LogPath = @".\Errors\";

                var now = DateTime.Now.ToLocalTime();

                LastError = $"[{now.Year}-{now.Month}-{now.Day}-{now.Hour}:{now.Minute}:{now.Second}:{now.Millisecond}] {ex}";

                if (!Directory.Exists(LogPath))
                    Directory.CreateDirectory(LogPath);

                File.AppendAllText($"{LogPath}{now.Year}-{now.Month}-{now.Day}.txt", LastError + Environment.NewLine);
            }
            catch
            { }
        }

        public static void Unload()
        {
            CConnection con = Connection;
            Connection = null;

            if (con != null)
            {
                SafeDisconnected = false;
                con.TrySendDisconnect(new Disconnect() { Reason = DisconnectReason.Unknown });
                DateTime timeout = DateTime.Now.AddSeconds(2);

                while (DateTime.Now < timeout && !SafeDisconnected) { Thread.Sleep(300); }

                try { con.Disconnect(); }
                catch { }
            }

        }
        public static KeyBindInfo GetKeyBind(KeyBindAction action)
        {
            return KeyBinds.Binding.FirstOrDefault(x => x.Action == action);
        }
        public static string GetText(Keys key)
        {
            switch (key)
            {
                case Keys.None:
                    return string.Empty;
                case Keys.Back:
                    return "Backspace";
                case Keys.Capital:
                    return "Cap Lock";
                case Keys.Scroll:
                    return "Scroll Lock";
                case Keys.NumLock:
                    return "Num Lock";
                case Keys.Prior:
                    return "Page Up";
                case Keys.Next:
                    return "Page Down";
                case Keys.Multiply:
                    return "Num Pad *";
                case Keys.Add:
                    return "Num Pad +";
                case Keys.Subtract:
                    return "Num Pad -";
                case Keys.Decimal:
                    return "Num Pad .";
                case Keys.Divide:
                    return "Num Pad /";
                case Keys.OemSemicolon:
                    return ";";
                case Keys.Oemplus:
                    return "=";
                case Keys.Oemcomma:
                    return ",";
                case Keys.OemMinus:
                    return "-";
                case Keys.OemPeriod:
                    return ".";
                case Keys.OemQuestion:
                    return "/";
                case Keys.Oemtilde:
                    return "'";
                case Keys.OemOpenBrackets:
                    return "[";
                case Keys.OemCloseBrackets:
                    return "]";
                case Keys.OemQuotes:
                    return "#";
                case Keys.Oem8:
                    return "`";
                case Keys.OemBackslash:
                    return "\\";
                case Keys.D1:
                    return "1";
                case Keys.D2:
                    return "2";
                case Keys.D3:
                    return "3";
                case Keys.D4:
                    return "4";
                case Keys.D5:
                    return "5";
                case Keys.D6:
                    return "6";
                case Keys.D7:
                    return "7";
                case Keys.D8:
                    return "8";
                case Keys.D9:
                    return "9";
                case Keys.D0:
                    return "0";
                default:
                    return key.ToString();
            }
        }
        public static string GetDirName(Point User, Point Item)
        {
            return Item.X >= User.X ? (Item.X != User.X ? (Item.Y >= User.Y ? (Item.Y != User.Y ? "右下↘" : "正右→") : "右上↗") : (Item.Y >= User.Y ? (Item.Y != User.Y ? "正下↓" : "脚下\x3289") : "正上↑")) : (Item.Y >= User.Y ? (Item.Y != User.Y ? "左下↙" : "正左←") : "左上↖");
        }
        public static int GetWeaponLimitLevel(Rarity r)
        {
            return WeaponRefineLevelLimit - (Rarity.Elite - r) * WeaponRefineRarityStep;
        }
        public static bool NeedAmulet(MagicInfo info)
        {
            switch (info.Magic)
            {
                case MagicType.ExplosiveTalisman:
                case MagicType.EvilSlayer:
                case MagicType.Invisibility:
                case MagicType.MagicResistance:
                case MagicType.MassInvisibility:
                case MagicType.Resilience:
                case MagicType.GreaterEvilSlayer:
                case MagicType.TrapOctagon:
                case MagicType.ElementalSuperiority:
                case MagicType.BloodLust:
                case MagicType.Resurrection:
                case MagicType.Purification:
                case MagicType.Transparency:
                case MagicType.CelestialLight:
                case MagicType.ImprovedExplosiveTalisman:
                case MagicType.SummonSkeleton:
                case MagicType.SummonShinsu:
                case MagicType.SummonJinSkeleton:
                case MagicType.StrengthOfFaith:
                case MagicType.SummonDemonicCreature:
                case MagicType.DemonExplosion:
                case MagicType.SummonPuppet:
                    return true;

            }

            return false;
        }
    }
}
