using System;
using System.Drawing;
using System.Windows.Forms;
using Client.Envir;
using Client.Scenes;
using Client.Scenes.Views;
using Client.UserModels;
using Library;
using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Controls
{
    public sealed class DXConfigWindow : DXWindow
    {
        #region Properties
        public static DXConfigWindow ActiveConfig;
        public DXKeyBindWindow KeyBindWindow;

        private DXTabControl TabControl;

        //Grpahics
        public DXTab GraphicsTab;
        public DXCheckBox FullScreenCheckBox, VSyncCheckBox, LimitFPSCheckBox, ClipMouseCheckBox, DebugLabelCheckBox;
        private DXComboBox GameSizeComboBox, LanguageComboBox;

        //Sound
        public DXTab SoundTab;
        private DXNumberBox SystemVolumeBox, MusicVolumeBox, SpellVolumeBox, PlayerVolumeBox, MonsterVolumeBox;
        private DXCheckBox BackgroundSoundBox;

        //Game 
        public DXTab GameTab;
        private DXCheckBox ItemNameCheckBox, MonsterNameCheckBox, UserHealthCheckBox, MonsterHealthCheckBox, DamageNumbersCheckBox, EscapeCloseAllCheckBox, ShiftOpenChatCheckBox, RightClickDeTargetCheckBox, MonsterBoxVisibleCheckBox, LogChatCheckBox, DrawEffectsCheckBox;
        public DXButton KeyBindButton;

        //Network
        public DXTab NetworkTab;
        private DXCheckBox UseNetworkConfigCheckBox;
        private DXTextBox IPAddressTextBox;
        private DXNumberBox PortBox;

        //Colours
        //public DXTab ColourTab;
        //public DXColourControl LocalColourBox, GMWhisperInColourBox, WhisperInColourBox, WhisperOutColourBox, GroupColourBox, GuildColourBox, ShoutColourBox, GlobalColourBox, ObserverColourBox, HintColourBox, SystemColourBox, GainsColourBox, AnnouncementColourBox;
        //public DXButton ResetColoursButton;


        private DXButton SaveButton, CancelButton;
        public DXButton ExitButton;

        public override void OnVisibleChanged(bool oValue, bool nValue)
        {
            base.OnVisibleChanged(oValue, nValue);

            if (!IsVisible) return;

            FullScreenCheckBox.Checked = Config.FullScreen;
            GameSizeComboBox.ListBox.SelectItem(Config.GameSize);
            VSyncCheckBox.Checked = Config.VSync;
            LimitFPSCheckBox.Checked = Config.LimitFPS;
            ClipMouseCheckBox.Checked = Config.ClipMouse;
            DebugLabelCheckBox.Checked = Config.DebugLabel;
            LanguageComboBox.ListBox.SelectItem(Config.Language);

            BackgroundSoundBox.Checked = Config.SoundInBackground;
            SystemVolumeBox.ValueTextBox.TextBox.Text = Config.SystemVolume.ToString();
            MusicVolumeBox.ValueTextBox.TextBox.Text = Config.MusicVolume.ToString();
            PlayerVolumeBox.ValueTextBox.TextBox.Text = Config.PlayerVolume.ToString();
            MonsterVolumeBox.ValueTextBox.TextBox.Text = Config.MonsterVolume.ToString();
            SpellVolumeBox.ValueTextBox.TextBox.Text = Config.MagicVolume.ToString();
            UseNetworkConfigCheckBox.Checked = Config.UseNetworkConfig;
            IPAddressTextBox.TextBox.Text = Config.IPAddress;
            PortBox.ValueTextBox.TextBox.Text = Config.Port.ToString();

            ItemNameCheckBox.Checked= Config.ShowItemNames;
            MonsterNameCheckBox.Checked = Config.ShowMonsterNames;
            UserHealthCheckBox.Checked = Config.ShowUserHealth;
            MonsterHealthCheckBox.Checked = Config.ShowMonsterHealth;
            DamageNumbersCheckBox.Checked = Config.ShowDamageNumbers;
            EscapeCloseAllCheckBox.Checked = Config.EscapeCloseAll;
            ShiftOpenChatCheckBox.Checked = Config.ShiftOpenChat;
            RightClickDeTargetCheckBox.Checked = Config.RightClickDeTarget;
            MonsterBoxVisibleCheckBox.Checked = Config.MonsterBoxVisible;
            LogChatCheckBox.Checked = Config.LogChat;
            DrawEffectsCheckBox.Checked = Config.DrawEffects;

            //LocalColourBox.BackColour = CEnvir.LocalTextColour;
            //GMWhisperInColourBox.BackColour = CEnvir.GMWhisperInTextColour;
            //WhisperInColourBox.BackColour = CEnvir.WhisperInTextColour;
            //WhisperOutColourBox.BackColour = CEnvir.WhisperOutTextColour;
            //GroupColourBox.BackColour = CEnvir.GroupTextColour;
            //GuildColourBox.BackColour = CEnvir.GuildTextColour;
            //ShoutColourBox.BackColour = CEnvir.ShoutTextColour;
            //GlobalColourBox.BackColour = CEnvir.GlobalTextColour;
            //ObserverColourBox.BackColour = CEnvir.ObserverTextColour;
            //HintColourBox.BackColour = CEnvir.HintTextColour;
            //SystemColourBox.BackColour = CEnvir.SystemTextColour;
            //GainsColourBox.BackColour = CEnvir.GainsTextColour;
            //AnnouncementColourBox.BackColour = CEnvir.AnnouncementTextColour;
        }
        public override void OnParentChanged(DXControl oValue, DXControl nValue)
        {
            base.OnParentChanged(oValue, nValue);

            KeyBindWindow.Parent = nValue;
        }

        public override WindowType Type => WindowType.ConfigBox;
        public override bool CustomSize => false;
        public override bool AutomaticVisiblity => false;
        #endregion

        public DXConfigWindow()
        {
            ActiveConfig = this;

            Size = new Size(300, 305);
            TitleLabel.Text = "游戏设置";
            HasFooter = true;

            TabControl = new DXTabControl
            {
                Parent = this,
                Location = ClientArea.Location,
                Size = ClientArea.Size,
            };
            GraphicsTab = new DXTab
            {
                Parent = TabControl,
                Border = true,
                TabButton = { Label = { Text = "图形" } },
            };

            SoundTab = new DXTab
            {
                Parent = TabControl,
                Border = true,
                TabButton = { Label = { Text = "声音" } },
            };

            GameTab = new DXTab
            {
                Parent = TabControl,
                Border = true,
                TabButton = { Label = { Text = "游戏" } },
            };

            NetworkTab = new DXTab
            {
                Parent = TabControl,
                Border = true,
                TabButton = { Label = { Text = "网络" } },
            };

            //ColourTab = new DXTab
            //{
            //    Parent = TabControl,
            //    Border = true,
            //    TabButton = { Label = { Text = "颜色" }, Visible = false },
            //};


            KeyBindWindow = new DXKeyBindWindow
            {
                Visible =  false
            };

            #region Graphics
            
            FullScreenCheckBox = new DXCheckBox
            {
                Label = { Text = "全屏:" },
                Parent = GraphicsTab,
                Checked = Config.FullScreen,
            };
            FullScreenCheckBox.Location = new Point(120 - FullScreenCheckBox.Size.Width, 10);

            DXLabel label = new DXLabel
            {
                Text = "分辨率:",
                Outline = true,
                Parent = GraphicsTab,
            };
            label.Location = new Point(104 - label.Size.Width, 35);

            GameSizeComboBox = new DXComboBox
            {
                Parent = GraphicsTab,
                Location = new Point(104, 35),
                Size = new Size(100, DXComboBox.DefaultNormalHeight),
            };

            foreach (Size resolution in Globals.ValidResolutions)
                new DXListBoxItem
                {
                    Parent = GameSizeComboBox.ListBox,
                    Label = { Text = $"{resolution.Width} x {resolution.Height}" },
                    Item = resolution
                };

            VSyncCheckBox = new DXCheckBox
            {
                Label = { Text = "垂直同步:" },
                Parent = GraphicsTab,
            };
            VSyncCheckBox.Location = new Point(120 - VSyncCheckBox.Size.Width, 60);

            LimitFPSCheckBox = new DXCheckBox
            {
                Label = { Text = "FPS限制:" },
                Parent = GraphicsTab,
            };
            LimitFPSCheckBox.Location = new Point(120 - LimitFPSCheckBox.Size.Width, 80);

            ClipMouseCheckBox = new DXCheckBox
            {
                Label = { Text = "裁剪鼠标:" },
                Parent = GraphicsTab,
            };
            ClipMouseCheckBox.Location = new Point(120 - ClipMouseCheckBox.Size.Width, 100);

            DebugLabelCheckBox = new DXCheckBox
            {
                Label = { Text = "Debug信息:" },
                Parent = GraphicsTab,
            };
            DebugLabelCheckBox.Location = new Point(120 - DebugLabelCheckBox.Size.Width, 120);

            label = new DXLabel
            {
                Text = "语言:",
                Outline = true,
                Parent = GraphicsTab,
            };
            label.Location = new Point(104 - label.Size.Width, 140);

            LanguageComboBox = new DXComboBox
            {
                Parent = GraphicsTab,
                Location = new Point(104, 140),
                Size = new Size(100, DXComboBox.DefaultNormalHeight),
            };

            foreach (string language in Globals.Languages)
                new DXListBoxItem
                {
                    Parent = LanguageComboBox.ListBox,
                    Label = { Text = language },
                    Item = language
                };
            #endregion

            #region Sound

            BackgroundSoundBox = new DXCheckBox
            {
                Label = { Text = "背景声音:" },
                Parent = SoundTab,
                Checked = Config.SoundInBackground,
            };
            BackgroundSoundBox.Location = new Point(120 - BackgroundSoundBox.Size.Width, 10);

            label = new DXLabel
            {
                Text = "系统音量:",
                Outline = true,
                Parent = SoundTab,
            };
            label.Location = new Point(104 - label.Size.Width, 35);

            SystemVolumeBox = new DXNumberBox
            {
                Parent = SoundTab,
                MinValue = 0,
                MaxValue = 100,
                Location = new Point(104, 35)
            };

            label = new DXLabel
            {
                Text = "音乐音量:",
                Outline = true,
                Parent = SoundTab,
            };
            label.Location = new Point(104 - label.Size.Width, 60);

            MusicVolumeBox = new DXNumberBox
            {
                Parent = SoundTab,
                MinValue = 0,
                MaxValue = 100,
                Location = new Point(104, 60)
            };

            label = new DXLabel
            {
                Text = "玩家音量:",
                Outline = true,
                Parent = SoundTab,
            };
            label.Location = new Point(104 - label.Size.Width, 85);

            PlayerVolumeBox = new DXNumberBox
            {
                Parent = SoundTab,
                MinValue = 0,
                MaxValue = 100,
                Location = new Point(104, 85)
            };
            label = new DXLabel
            {
                Text = "怪物音量:",
                Outline = true,
                Parent = SoundTab,
            };
            label.Location = new Point(104 - label.Size.Width, 110);

            MonsterVolumeBox = new DXNumberBox
            {
                Parent = SoundTab,
                MinValue = 0,
                MaxValue = 100,
                Location = new Point(104, 110)
            };

            label = new DXLabel
            {
                Text = "技能音量:",
                Outline = true,
                Parent = SoundTab,
            };
            label.Location = new Point(104 - label.Size.Width, 135);

            SpellVolumeBox = new DXNumberBox
            {
                Parent = SoundTab,
                MinValue = 0,
                MaxValue = 100,
                Location = new Point(104, 135)
            };


            #endregion

            #region Game

            ItemNameCheckBox = new DXCheckBox
            {
                Label = { Text = "物品名称:" },
                Parent = GameTab,
            };
            ItemNameCheckBox.Location = new Point(120 - ItemNameCheckBox.Size.Width, 10);

            MonsterNameCheckBox = new DXCheckBox
            {
                Label = { Text = "怪物名称:" },
                Parent = GameTab,
            };
            MonsterNameCheckBox.Location = new Point(120 - MonsterNameCheckBox.Size.Width, 35);

            UserHealthCheckBox = new DXCheckBox
            {
                Label = { Text = "自己血量:" },
                Parent = GameTab,
            };
            UserHealthCheckBox.Location = new Point(120 - UserHealthCheckBox.Size.Width, 60);

            MonsterHealthCheckBox = new DXCheckBox
            {
                Label = { Text = "怪物血量:" },
                Parent = GameTab,
            };
            MonsterHealthCheckBox.Location = new Point(120 - MonsterHealthCheckBox.Size.Width, 85);

            DamageNumbersCheckBox = new DXCheckBox
            {
                Label = { Text = "伤害信息:" },
                Parent = GameTab,
            };
            DamageNumbersCheckBox.Location = new Point(120 - DamageNumbersCheckBox.Size.Width, 110);


            EscapeCloseAllCheckBox = new DXCheckBox
            {
                Label = { Text = "ESC键关闭所有窗口:" },
                Parent = GameTab,
            };
            EscapeCloseAllCheckBox.Location = new Point(270 - EscapeCloseAllCheckBox.Size.Width, 10);

            ShiftOpenChatCheckBox = new DXCheckBox
            {
                Label = { Text = "Shift + 1  打开聊天:" },
                Parent = GameTab,
                Hint = "打开后, 按 Shift + 1 将打开聊天, 否则将使用 快捷栏 1 格物品"
            };
            ShiftOpenChatCheckBox.Location = new Point(270 - ShiftOpenChatCheckBox.Size.Width, 35);

            RightClickDeTargetCheckBox = new DXCheckBox
            {
                Label = { Text = "右键点击取消选择:" },
                Parent = GameTab,
                Hint = "打开后, 右键点击后取消选择的目标."
            };
            RightClickDeTargetCheckBox.Location = new Point(270 - RightClickDeTargetCheckBox.Size.Width, 60);

            MonsterBoxVisibleCheckBox = new DXCheckBox
            {
                Label = { Text = "显示怪物详情:" },
                Parent = GameTab,
            };
            MonsterBoxVisibleCheckBox.Location = new Point(270 - MonsterBoxVisibleCheckBox.Size.Width, 85);

            LogChatCheckBox = new DXCheckBox
            {
                Label = { Text = "聊天日志:" },
                Parent = GameTab,
            };
            LogChatCheckBox.Location = new Point(270 - LogChatCheckBox.Size.Width, 110);

            DrawEffectsCheckBox = new DXCheckBox
            {
                Label = { Text = "绘制效果:" },
                Parent = GameTab,
            };
            DrawEffectsCheckBox.Location = new Point(270 - DrawEffectsCheckBox.Size.Width, 135);

            KeyBindButton = new DXButton
            {
                Parent = GameTab,
                Location = new Point(190, 160),
                Size = new Size(80, SmallButtonHeight),
                ButtonType = ButtonType.SmallButton,
                Label = { Text = "绑定按键" }
            };
            KeyBindButton.MouseClick += (o, e) => KeyBindWindow.Visible = !KeyBindWindow.Visible;

            #endregion

            #region Network

            UseNetworkConfigCheckBox = new DXCheckBox
            {
                Label = { Text = "使用配置:" },
                Parent = NetworkTab,
                Checked = Config.FullScreen,
            };
            UseNetworkConfigCheckBox.Location = new Point(120 - UseNetworkConfigCheckBox.Size.Width, 10);

            label = new DXLabel
            {
                Text = "服务器IP:",
                Outline = true,
                Parent = NetworkTab,
            };
            label.Location = new Point(104 - label.Size.Width, 35);

            IPAddressTextBox = new DXTextBox
            {
                Location = new Point(104, 35),
                Size = new Size(100, 16),
                Parent = NetworkTab,
            };

            label = new DXLabel
            {
                Text = "服务器端口:",
                Outline = true,
                Parent = NetworkTab,
            };
            label.Location = new Point(104 - label.Size.Width, 60);

            PortBox = new DXNumberBox
            {
                Parent = NetworkTab,
                Change = 100,
                MaxValue = ushort.MaxValue,
                Location = new Point(104, 60)
            };
            #endregion

            SaveButton = new DXButton
            {
                Location = new Point(Size.Width - 190, Size.Height - 43),
                Size = new Size(80, DefaultHeight),
                Parent = this,
                Label = { Text = "应用" }
            };
            SaveButton.MouseClick += SaveSettings;

            CancelButton = new DXButton
            {
                Location = new Point(Size.Width - 100, Size.Height - 43),
                Size = new Size(80, DefaultHeight),
                Parent = this,
                Label = { Text = "取消" }
            };
            CancelButton.MouseClick += CancelSettings;

            ExitButton = new DXButton
            {
                Location = new Point(Size.Width - 280, Size.Height - 43),
                Size = new Size(60, DefaultHeight),
                Parent = this,
                Label = { Text = "退出" },
                Visible = false,
            };
            ExitButton.MouseClick += CancelSettings;
        }

        #region Methods
        private void CancelSettings(object o, MouseEventArgs e)
        {
            Visible = false;
        }
        private void SaveSettings(object o, MouseEventArgs e)
        {
            if (Config.FullScreen != FullScreenCheckBox.Checked)
            {
                DXManager.ToggleFullScreen();
            }

            if (GameSizeComboBox.SelectedItem is Size && Config.GameSize != (Size)GameSizeComboBox.SelectedItem)
            {
                Config.GameSize = (Size)GameSizeComboBox.SelectedItem;

                if (ActiveScene is GameScene)
                {
                    ActiveScene.Size = Config.GameSize;
                    DXManager.SetResolution(ActiveScene.Size);
                }
            }

            if (LanguageComboBox.SelectedItem is string && Config.Language != (string)LanguageComboBox.SelectedItem)
            {

                Config.Language = (string) LanguageComboBox.SelectedItem;

                if (CEnvir.Connection != null && CEnvir.Connection.ServerConnected)
                    CEnvir.Enqueue(new C.SelectLanguage { Language = Config.Language });
            }


            if (Config.VSync != VSyncCheckBox.Checked)
            {
                Config.VSync = VSyncCheckBox.Checked;
                DXManager.ResetDevice();
            }

            Config.LimitFPS = LimitFPSCheckBox.Checked;
            Config.ClipMouse = ClipMouseCheckBox.Checked;
            Config.DebugLabel = DebugLabelCheckBox.Checked;

            DebugLabel.IsVisible = Config.DebugLabel;
            PingLabel.IsVisible = Config.DebugLabel;

            if (Config.SoundInBackground != BackgroundSoundBox.Checked)
            {
                Config.SoundInBackground = BackgroundSoundBox.Checked;

                DXSoundManager.UpdateFlags();
            }
            

            bool volumeChanged = false;


            if (Config.SystemVolume != SystemVolumeBox.Value)
            {
                Config.SystemVolume = (int) SystemVolumeBox.Value;
                volumeChanged = true;
            }


            if (Config.MusicVolume != MusicVolumeBox.Value)
            {
                Config.MusicVolume = (int)MusicVolumeBox.Value;
                volumeChanged = true;
            }


            if (Config.PlayerVolume != PlayerVolumeBox.Value)
            {
                Config.PlayerVolume = (int)PlayerVolumeBox.Value;
                volumeChanged = true;
            }

            if (Config.MonsterVolume != MonsterVolumeBox.Value)
            {
                Config.MonsterVolume = (int)MonsterVolumeBox.Value;
                volumeChanged = true;
            }

            if (Config.MagicVolume != SpellVolumeBox.Value)
            {
                Config.MagicVolume = (int)SpellVolumeBox.Value;
                volumeChanged = true;
            }

            Config.ShowItemNames = ItemNameCheckBox.Checked;
            Config.ShowMonsterNames = MonsterNameCheckBox.Checked;
            Config.ShowUserHealth = UserHealthCheckBox.Checked;
            Config.ShowMonsterHealth = MonsterHealthCheckBox.Checked;
            Config.ShowDamageNumbers = DamageNumbersCheckBox.Checked;

            Config.EscapeCloseAll = EscapeCloseAllCheckBox.Checked;
            Config.ShiftOpenChat = ShiftOpenChatCheckBox.Checked;
            Config.RightClickDeTarget = RightClickDeTargetCheckBox.Checked;
            Config.MonsterBoxVisible = MonsterBoxVisibleCheckBox.Checked;
            Config.LogChat = LogChatCheckBox.Checked;
            Config.DrawEffects = DrawEffectsCheckBox.Checked;

            if (volumeChanged)
                DXSoundManager.AdjustVolume();

            Config.UseNetworkConfig = UseNetworkConfigCheckBox.Checked;
            Config.IPAddress = IPAddressTextBox.TextBox.Text;
            Config.Port = (int)PortBox.Value;

        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Visible = false;
                    break;
            }
        }
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (ActiveConfig == this)
                    ActiveConfig = null;

                if (TabControl != null)
                {
                    if (!TabControl.IsDisposed)
                        TabControl.Dispose();

                    TabControl = null;
                }

                if (KeyBindWindow != null)
                {
                    if (!KeyBindWindow.IsDisposed)
                        KeyBindWindow.Dispose();

                    KeyBindWindow = null;
                }

                #region Graphics
                if (GraphicsTab != null)
                {
                    if (!GraphicsTab.IsDisposed)
                        GraphicsTab.Dispose();

                    GraphicsTab = null;
                }

                if (FullScreenCheckBox != null)
                {
                    if (!FullScreenCheckBox.IsDisposed)
                        FullScreenCheckBox.Dispose();

                    FullScreenCheckBox = null;
                }

                if (VSyncCheckBox != null)
                {
                    if (!VSyncCheckBox.IsDisposed)
                        VSyncCheckBox.Dispose();

                    VSyncCheckBox = null;
                }

                if (LimitFPSCheckBox != null)
                {
                    if (!LimitFPSCheckBox.IsDisposed)
                        LimitFPSCheckBox.Dispose();

                    LimitFPSCheckBox = null;
                }

                if (ClipMouseCheckBox != null)
                {
                    if (!ClipMouseCheckBox.IsDisposed)
                        ClipMouseCheckBox.Dispose();

                    ClipMouseCheckBox = null;
                }
                if (DebugLabelCheckBox != null)
                {
                    if (!DebugLabelCheckBox.IsDisposed)
                        DebugLabelCheckBox.Dispose();

                    DebugLabelCheckBox = null;
                }

                if (GameSizeComboBox != null)
                {
                    if (!GameSizeComboBox.IsDisposed)
                        GameSizeComboBox.Dispose();

                    GameSizeComboBox = null;
                }
                if (LanguageComboBox != null)
                {
                    if (!LanguageComboBox.IsDisposed)
                        LanguageComboBox.Dispose();

                    LanguageComboBox = null;
                }
                
                #endregion

                #region Sound
                if (SoundTab != null)
                {
                    if (!SoundTab.IsDisposed)
                        SoundTab.Dispose();

                    SoundTab = null;
                }

                if (SystemVolumeBox != null)
                {
                    if (!SystemVolumeBox.IsDisposed)
                        SystemVolumeBox.Dispose();

                    SystemVolumeBox = null;
                }

                if (MusicVolumeBox != null)
                {
                    if (!MusicVolumeBox.IsDisposed)
                        MusicVolumeBox.Dispose();

                    MusicVolumeBox = null;
                }

                if (PlayerVolumeBox != null)
                {
                    if (!PlayerVolumeBox.IsDisposed)
                        PlayerVolumeBox.Dispose();

                    PlayerVolumeBox = null;
                }

                if (MonsterVolumeBox != null)
                {
                    if (!MonsterVolumeBox.IsDisposed)
                        MonsterVolumeBox.Dispose();

                    MonsterVolumeBox = null;
                }

                if (SpellVolumeBox != null)
                {
                    if (!SpellVolumeBox.IsDisposed)
                        SpellVolumeBox.Dispose();

                    SpellVolumeBox = null;
                }

                if (BackgroundSoundBox != null)
                {
                    if (!BackgroundSoundBox.IsDisposed)
                        BackgroundSoundBox.Dispose();

                    BackgroundSoundBox = null;
                }
                #endregion

                #region Game
                if (GameTab != null)
                {
                    if (!GameTab.IsDisposed)
                        GameTab.Dispose();

                    GameTab = null;
                }

                if (ItemNameCheckBox != null)
                {
                    if (!ItemNameCheckBox.IsDisposed)
                        ItemNameCheckBox.Dispose();

                    ItemNameCheckBox = null;
                }

                if (MonsterNameCheckBox != null)
                {
                    if (!MonsterNameCheckBox.IsDisposed)
                        MonsterNameCheckBox.Dispose();

                    MonsterNameCheckBox = null;
                }

                if (UserHealthCheckBox != null)
                {
                    if (!UserHealthCheckBox.IsDisposed)
                        UserHealthCheckBox.Dispose();

                    UserHealthCheckBox = null;
                }

                if (MonsterHealthCheckBox != null)
                {
                    if (!MonsterHealthCheckBox.IsDisposed)
                        MonsterHealthCheckBox.Dispose();

                    MonsterHealthCheckBox = null;
                }

                if (DamageNumbersCheckBox != null)
                {
                    if (!DamageNumbersCheckBox.IsDisposed)
                        DamageNumbersCheckBox.Dispose();

                    DamageNumbersCheckBox = null;
                }

                if (EscapeCloseAllCheckBox != null)
                {
                    if (!EscapeCloseAllCheckBox.IsDisposed)
                        EscapeCloseAllCheckBox.Dispose();

                    EscapeCloseAllCheckBox = null;
                }

                if (ShiftOpenChatCheckBox != null)
                {
                    if (!ShiftOpenChatCheckBox.IsDisposed)
                        ShiftOpenChatCheckBox.Dispose();

                    ShiftOpenChatCheckBox = null;
                }

                if (RightClickDeTargetCheckBox != null)
                {
                    if (!RightClickDeTargetCheckBox.IsDisposed)
                        RightClickDeTargetCheckBox.Dispose();

                    RightClickDeTargetCheckBox = null;
                }
                
                if (MonsterBoxVisibleCheckBox != null)
                {
                    if (!MonsterBoxVisibleCheckBox.IsDisposed)
                        MonsterBoxVisibleCheckBox.Dispose();

                    MonsterBoxVisibleCheckBox = null;
                }

                if (LogChatCheckBox != null)
                {
                    if (!LogChatCheckBox.IsDisposed)
                        LogChatCheckBox.Dispose();

                    LogChatCheckBox = null;
                }
                

                if (KeyBindButton != null)
                {
                    if (!KeyBindButton.IsDisposed)
                        KeyBindButton.Dispose();

                    KeyBindButton = null;
                }
                #endregion

                #region Network
                if (NetworkTab != null)
                {
                    if (!NetworkTab.IsDisposed)
                        NetworkTab.Dispose();

                    NetworkTab = null;
                }
                
                if (UseNetworkConfigCheckBox != null)
                {
                    if (!UseNetworkConfigCheckBox.IsDisposed)
                        UseNetworkConfigCheckBox.Dispose();

                    UseNetworkConfigCheckBox = null;
                }

                if (IPAddressTextBox != null)
                {
                    if (!IPAddressTextBox.IsDisposed)
                        IPAddressTextBox.Dispose();

                    IPAddressTextBox = null;
                }

                if (PortBox != null)
                {
                    if (!PortBox.IsDisposed)
                        PortBox.Dispose();

                    PortBox = null;
                }
                #endregion

                if (SaveButton != null)
                {
                    if (!SaveButton.IsDisposed)
                        SaveButton.Dispose();

                    SaveButton = null;
                }

                if (CancelButton != null)
                {
                    if (!CancelButton.IsDisposed)
                        CancelButton.Dispose();

                    CancelButton = null;
                }
                
                if (ExitButton != null)
                {
                    if (!ExitButton.IsDisposed)
                        ExitButton.Dispose();

                    ExitButton = null;
                }
            }
        }

        #endregion
    }
}
