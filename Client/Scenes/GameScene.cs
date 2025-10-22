using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.Scenes.Views;
using Client.UserModels;
using Library;
using Library.SystemModels;
using MirDB;
using C = Library.Network.ClientPackets;
using UserObject = Client.Models.UserObject;
//using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using Library.Network.ClientPackets;
using Library.Network;
using Client.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//Cleaned
namespace Client.Scenes
{
    public sealed class GameScene : DXScene
    {
        #region Properties
        public static GameScene Game;

        public DXItemCell SelectedCell
        {
            get => _SelectedCell;
            set
            {
                if (_SelectedCell == value) return;

                if (_SelectedCell != null) _SelectedCell.Selected = false;

                _SelectedCell = value;

                if (_SelectedCell != null) _SelectedCell.Selected = true;
            }
        }
        private DXItemCell _SelectedCell;
        
        #region User

        public UserObject User
        {
            get => _User;
            set
            {
                if (_User == value) return;

                _User = value;

                UserChanged();
            }
        }
        private UserObject _User;


        #endregion
        //public int PatchGridSize
        //{
        //    get
        //    {
        //        return _PatchGridSize;
        //    }
        //    set
        //    {
        //        if (_PatchGridSize == value)
        //            return;
        //        int patchGridSize = _PatchGridSize;
        //        _PatchGridSize = value;
        //        OnPatchGridSizeChanged(patchGridSize, value);
        //    }
        //}
        //private int _PatchGridSize = 0;
        //public void OnPatchGridSizeChanged(int oValue, int nValue)
        //{
        //    InventoryBox.RefreshPatchGrid();
        //}
        #region Observer

        public bool Observer
        {
            get => _Observer;
            set
            {
                if (_Observer == value) return;

                bool oldValue = _Observer;
                _Observer = value;

                OnObserverChanged(oldValue, value);
            }
        }
        private bool _Observer;
        public event EventHandler<EventArgs> ObserverChanged;
        public void OnObserverChanged(bool oValue, bool nValue)
        {
            ObserverChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public bool GoldPickedUp { get; set; }

        public MapObject MagicObject, TargetObject, FocusObject;
        public MapObject MouseObject { get; set; }
        public DXControl ItemLabel, MagicLabel;
        private DateTime skillTime1 { get; set; }
        private DateTime skillTime2 { get; set; }

        public MagicInfo LastMagic { get; private set; } = null;
        public MapObject LastTarget { get; private set; } = null;

        private DateTime AutoPoisonTime { get; set; } = DateTime.MinValue;
        #region MouseItem

        public ClientUserItem MouseItem
        {
            get => _MouseItem;
            set
            {
                if (_MouseItem == value) return;

                ClientUserItem oldValue = _MouseItem;
                _MouseItem = value;

                OnMouseItemChanged(oldValue, value);
            }
        }
        private ClientUserItem _MouseItem;
        public event EventHandler<EventArgs> MouseItemChanged;
        public void OnMouseItemChanged(ClientUserItem oValue, ClientUserItem nValue)
        {
            MouseItemChanged?.Invoke(this, EventArgs.Empty);

            CreateItemLabel();
        }

        #endregion

        public int PatchGridSize
        {
            get
            {
                return _PatchGridSize;
            }
            set
            {
                if (_PatchGridSize == value)
                    return;
                int patchGridSize = _PatchGridSize;
                _PatchGridSize = value;
                OnPatchGridSizeChanged(patchGridSize, value);
            }
        }
        private int _PatchGridSize;
        public void OnPatchGridSizeChanged(int oValue, int nValue)
        {
            InventoryBox.RefreshPatchGrid();
        }
        #region MouseMagic

        public MagicInfo MouseMagic
        {
            get => _MouseMagic;
            set
            {
                if (_MouseMagic == value) return;

                MagicInfo oldValue = _MouseMagic;
                _MouseMagic = value;

                OnMouseMagicChanged(oldValue, value);
            }
        }
        private MagicInfo _MouseMagic;
        public event EventHandler<EventArgs> MouseMagicChanged;
        public void OnMouseMagicChanged(MagicInfo oValue, MagicInfo nValue)
        {
            MouseMagicChanged?.Invoke(this, EventArgs.Empty);

            if (MagicLabel != null && !MagicLabel.IsDisposed) MagicLabel.Dispose();
            MagicLabel = null;
            CreateMagicLabel();
        }

        #endregion
        public BigPatchDialog BigPatchBox { get; set; }
        public ClientUserItem[] PatchGrid = new ClientUserItem[98];

        public MapControl MapControl;
        public MainPanel MainPanel;

        public DXConfigWindow ConfigBox;
        public InventoryDialog InventoryBox;
        public CharacterDialog CharacterBox;
        public ExitDialog ExitBox;
        public ChatTextBox ChatTextBox { get; set; }
        public BeltDialog BeltBox;
        public ChatOptionsDialog ChatOptionsBox;
        public NPCDialog NPCBox;
        public NPCGoodsDialog NPCGoodsBox;
        public NPCSellDialog NPCSellBox;
        public NPCRepairDialog NPCRepairBox;
        public NPCRefinementStoneDialog NPCRefinementStoneBox;
        public NPCRefineDialog NPCRefineBox;
        public NPCRefineRetrieveDialog NPCRefineRetrieveBox;
        public NPCQuestDialog NPCQuestBox;
        public NPCAdoptCompanionDialog NPCAdoptCompanionBox;
        public NPCCompanionStorageDialog NPCCompanionStorageBox;
        public NPCWeddingRingDialog NPCWeddingRingBox;
        public NPCItemFragmentDialog NPCItemFragmentBox;
        public NPCAccessoryUpgradeDialog NPCAccessoryUpgradeBox;
        public NPCAccessoryLevelDialog NPCAccessoryLevelBox;
        public NPCAccessoryResetDialog NPCAccessoryResetBox;
        public NPCMasterRefineDialog NPCMasterRefineBox;
        private DXTabControl ChatTabControl;
        private DXButton HideChat;
        private DXButton ShowChat;
        private DXButton BigPatch;
        private DXButton Ranking;
        public MiniMapDialog MiniMapBox { get; set; }
        public BigMapDialog BigMapBox;
        public MagicDialog MagicBox;
        public GroupDialog GroupBox;
        public BuffDialog BuffBox;
        public StorageDialog StorageBox { get; set; }

        public InspectDialog InspectBox;
        public RankingDialog RankingBox { get; set; }
        public MarketPlaceDialog MarketPlaceBox;
        public MailDialog MailBox;
        public ReadMailDialog ReadMailBox;
        public SendMailDialog SendMailBox;
        public TradeDialog TradeBox;
        public GuildDialog GuildBox;
        public GuildMemberDialog GuildMemberBox;
        public QuestDialog QuestBox;
        public QuestTrackerDialog QuestTrackerBox;
        public CompanionDialog CompanionBox;
        public CompanionHealthPanel CompanionHealth;
        public BlockDialog BlockBox;
        public MonsterDialog MonsterBox;
        public MagicBarDialog MagicBarBox { get; set; }
        public EditCharacterDialog EditCharacterBox;
        public FortuneCheckerDialog FortuneCheckerBox;
        public NPCWeaponCraftWindow NPCWeaponCraftBox;

        public ClientUserItem[] Inventory { get; set; } = new ClientUserItem[Globals.InventorySize];
        public ClientUserItem[] Equipment { get;private set; } = new ClientUserItem[Globals.EquipmentSize];

        public List<ClientUserQuest> QuestLog { get; set; } = new List<ClientUserQuest>();

        public HashSet<string> GuildWars = new HashSet<string>();
        public HashSet<CastleInfo> ConquestWars = new HashSet<CastleInfo>();

        public SortedDictionary<uint, ClientObjectData> DataDictionary = new SortedDictionary<uint, ClientObjectData>();

        public Dictionary<ItemInfo, ClientFortuneInfo> FortuneDictionary = new Dictionary<ItemInfo, ClientFortuneInfo>();

        public Dictionary<CastleInfo, string> CastleOwners = new Dictionary<CastleInfo, string>();

        public bool MoveFrame { get; set; }
        private DateTime MoveTime { get; set; }
        private DateTime OutputTime { get; set; }
        private DateTime ItemRefreshTime { get; set; }
        public bool CanRun { get; set; }

        public bool AutoRun
        {
            get => _AutoRun;
            set
            {
                if (_AutoRun == value) return;
                _AutoRun = value;
                
                ReceiveChat(value ? "[自动奔跑: 打开]" : "[自动奔跑: 关闭]", MessageType.Hint);
            }
        }
        private bool _AutoRun;

        private KeyBindInfo LastAttackModeKey { get; set; }
        private KeyBindInfo LastPetModeKey { get; set; }

        // Track last MaxHP/MaxMP to avoid unnecessary auto-potion updates
        private int _LastMaxHP = 0;
        private int _LastMaxMP = 0;

        #region StorageSize

        public int StorageSize
        {
            get { return _StorageSize; }
            set
            {
                if (_StorageSize == value) return;

                int oldValue = _StorageSize;
                _StorageSize = value;

                OnStorageSizeChanged(oldValue, value);
            }
        }
        private int _StorageSize;
        public void OnStorageSizeChanged(int oValue, int nValue)
        {
            StorageBox.RefreshStorage();
        }

        #endregion

        

        #region NPCID

        public uint NPCID
        {
            get => _NPCID;
            set
            {
                if (_NPCID == value) return;

                uint oldValue = _NPCID;
                _NPCID = value;

                OnNPCIDChanged(oldValue, value);
            }
        }
        private uint _NPCID;
        public void OnNPCIDChanged(uint oValue, uint nValue)
        {
            if (MapControl?.Objects == null || NPCQuestBox == null) return;

            foreach (MapObject ob in MapControl.Objects)
            {
                if (ob.Race != ObjectType.NPC || ob.ObjectID != NPCID) continue;

                NPCQuestBox.NPCInfo = ((NPCObject) ob).NPCInfo;
                return;
            }
            NPCQuestBox.NPCInfo = null;
        }

        #endregion

        #region Companion

        public ClientUserCompanion Companion
        {
            get => _Companion;
            set
            {
                if (_Companion == value) return;
                
                _Companion = value;

                CompanionChanged();
            }
        }
        private ClientUserCompanion _Companion;

        #endregion

        public ClientPlayerInfo Partner
        {
            get => _Partner;
            set
            {
                if (_Partner == value) return;
                
                _Partner = value;

                MarriageChanged();
            }
        }
        private ClientPlayerInfo _Partner;
        

        public uint InspectID;
        public DateTime PickUpTime, UseItemTime, NPCTime, ToggleTime, InspectTime, ItemTime = CEnvir.Now;
        public DateTime AutoPickUpTime = CEnvir.Now;
        public DateTime ReincarnationPillTime { get; set; }
        public DateTime ItemReviveTime { get; set; }
        public float DayTime
        {
            get => _DayTime;
            set
            {
                if (_DayTime == value) return;

                _DayTime = value;
                MapControl.UpdateLights();
            }
        }
        private float _DayTime;
        
        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);
            
            SetDefaultLocations();

            foreach (DXWindow window in DXWindow.Windows)
                window.LoadSettings();

            LoadChatTabs();
        }

        #endregion

        public GameScene(Size size) : base(size)
        {
            DrawTexture = false;
            Game = this;

            foreach (NPCInfo info in Globals.NPCInfoList.Binding)
                info.CurrentIcon = QuestIcon.None;

            #region 创建组件
            MapControl = new MapControl
            {
                Parent = this,
                Size = Size,
            };
            MapControl.MouseWheel += (o, e) =>
            {
                foreach (ChatTab tab in ChatTab.Tabs)
                {
                    if (!tab.DisplayArea.Contains(e.Location) || !tab.Visible) continue;

                    tab.ScrollBar.DoMouseWheel(tab.ScrollBar, e);
                }
            };

            MainPanel = new MainPanel { Parent = this };

            ConfigBox = new DXConfigWindow
            {
                Parent = this,
                Visible = false,
                NetworkTab = { Enabled = false, TabButton = { Visible = false } },
                //ColourTab = { TabButton = { Visible = true } },
                ExitButton = { Visible = true },
            };
            ConfigBox.ExitButton.MouseClick += (o, e) => ExitBox.Visible = true;

            ExitBox = new ExitDialog
            {
                Parent = this,
                Visible = false,
            };
            InventoryBox = new InventoryDialog
            {
                Parent = this,
                Visible = false,
            };

            CharacterBox = new CharacterDialog
            {
                Parent = this,
                Visible = false,
            };

            ChatTextBox = new ChatTextBox
            {
                Visible = true,
                Parent = this,
            };
            ChatOptionsBox = new ChatOptionsDialog
            {
                Parent = this,
                Visible = false,
            };
            BeltBox = new BeltDialog
            {
                Parent = this,
                Visible = true,
            };
            NPCBox = new NPCDialog
            {
                Parent = this,
                Visible = false
            };
            NPCGoodsBox = new NPCGoodsDialog
            {
                Parent = this,
                Visible = false
            };
            NPCSellBox = new NPCSellDialog
            {
                Parent = this,
                Visible = false,
            };

            NPCRepairBox = new NPCRepairDialog
            {
                Parent = this,
                Visible = false,
            };
            NPCQuestBox = new NPCQuestDialog
            {
                Parent = this,
                Visible = false,
            };
            NPCAdoptCompanionBox = new NPCAdoptCompanionDialog
            { 
                Parent = this,
                Visible = false,
            };
            NPCCompanionStorageBox = new NPCCompanionStorageDialog
            {
                Parent = this,
                Visible = false,
            };
            NPCWeddingRingBox = new NPCWeddingRingDialog
            {
                Parent = this,
                Visible = false,
            };

            MiniMapBox = new MiniMapDialog
            {
                Parent = this,
            };
            CompanionHealth = new CompanionHealthPanel
            {
                Parent = this,
                Location = new Point(0, Size.Height - 150), // left bottom (height adjusted to visible area)
                Size = new Size(54, 100), // narrow panel width
                IsControl = false,
            };
            MagicBox = new MagicDialog()
            {
                Parent = this,
                Visible = false,
            };
            GroupBox = new GroupDialog()
            {
                Parent = this,
                Visible = false,
            };

            BigMapBox = new BigMapDialog
            {
                Parent = this,
                Visible = false,
            };
            BuffBox = new BuffDialog
            {
                Parent = this,
            };
            StorageBox = new StorageDialog
            {
                Parent = this,
                Visible = false
            };
            BigPatchBox = new BigPatchDialog
            {
                Parent = this,
                Visible = false
            };
            NPCRefinementStoneBox = new NPCRefinementStoneDialog
            {
                Parent = this,
                Visible = false,
            };
            NPCItemFragmentBox = new NPCItemFragmentDialog()
            {
                Parent = this,
                Visible = false,
            };
            NPCAccessoryUpgradeBox = new NPCAccessoryUpgradeDialog
            {
                Parent = this,
                Visible = false,
            };
            NPCAccessoryLevelBox = new NPCAccessoryLevelDialog
            {
                Parent = this,
                Visible = false,
            };
            NPCAccessoryResetBox = new NPCAccessoryResetDialog
            {
                Parent = this,
                Visible = false,
            };
            NPCRefineBox = new NPCRefineDialog
            {
                Parent = this,
                Visible = false
            };
            NPCRefineRetrieveBox = new NPCRefineRetrieveDialog
            {
                Parent = this,
                Visible = false
            };
            NPCMasterRefineBox = new NPCMasterRefineDialog
            {
                Parent = this,
                Visible = false
            };

            InspectBox = new InspectDialog
            {
                Parent = this,
                Visible = false
            };
            RankingBox = new RankingDialog
            {
                Parent = this,
                Visible = false
            };
            MarketPlaceBox = new MarketPlaceDialog
            {
                Parent = this,
                Visible = false,
            };
            EditCharacterBox = new EditCharacterDialog
            {
                Parent = this,
                Visible = false
            };
            MailBox = new MailDialog
            {
                Parent = this,
                Visible = false
            };
            ReadMailBox = new ReadMailDialog
            {
                Parent = this,
                Visible = false
            };
            SendMailBox = new SendMailDialog
            {
                Parent = this,
                Visible = false
            };

            TradeBox = new TradeDialog
            {
                Parent = this,
                Visible = false
            };
            GuildBox = new GuildDialog
            {
                Parent = this,
                Visible = false
            };
            GuildMemberBox = new GuildMemberDialog
            {
                Parent = this,
                Visible = false
            };

            QuestBox = new QuestDialog
            {
                Parent = this,
                Visible = false
            };
            QuestTrackerBox = new QuestTrackerDialog
            {
                Parent = this,
                Visible = false
            };
            CompanionBox = new CompanionDialog
            {
                Parent = this,
                Visible = false,
            };

            BlockBox = new BlockDialog
            {
                Parent = this,
                Visible = false
            };

            MonsterBox = new MonsterDialog
            {
                Parent = this,
                Visible = false,
            };
            MagicBarBox = new MagicBarDialog
            {
                Parent = this,
                Visible = true,
            };

            FortuneCheckerBox = new FortuneCheckerDialog
            {
                Parent = this,
                Visible = false,
            };

            NPCWeaponCraftBox = new NPCWeaponCraftWindow
            {
                Visible = false,
                Parent = this,
            };

            HideChat = new DXButton()
            {
                Visible = true,
                ButtonType = ButtonType.Default,
                Label = { Text = "隐藏" },
                Parent = this,
                Size = new Size(50, SmallButtonHeight),
                Opacity = 0.2f,
                //Location = new Point(Game.ChatTextBox.Location.X + Game.ChatTextBox.Size.Width - HideChat.Size.Width, Game.ChatTextBox.Location.Y - 150),
            };

            HideChat.MouseClick += (a, b) =>
            {
                if (ChatTabControl == null) return;
                ChatTabControl.Visible = false;
                Game.ChatTextBox.Visible = false;
                ShowChat.Visible = true;
                HideChat.Visible = false;
                BigPatch.Visible = true;
                Ranking.Visible = true;
            };

            ShowChat = new DXButton()
            {
                Visible = false,
                ButtonType = ButtonType.Default,
                Label = { Text = "显示聊天" },
                Parent = this,
                Size = new Size(80, SmallButtonHeight),
                //Location = new Point(Game.MainPanel.Location.X, Game.MainPanel.Location.Y - ShowChat.Size.Height),
            };
            ShowChat.MouseClick += (a, b) =>
            {
                if (ChatTabControl == null) return;
                ChatTabControl.Visible = true;
                Game.ChatTextBox.Visible = true;
                ShowChat.Visible = false;
                HideChat.Visible = true;
                BigPatch.Visible = false;
                Ranking.Visible = false;
            };
            BigPatch = new DXButton()
            {
                Visible = false,
                ButtonType = ButtonType.Default,
                Label = { Text = "辅助工具" },
                Parent = this,
                Size = new Size(80, SmallButtonHeight),
                //Location = new Point(Game.MainPanel.Location.X, Game.MainPanel.Location.Y - ShowChat.Size.Height),
            };
            BigPatch.MouseClick += (a, b) =>
            {
                BigPatchBox.Visible = !BigPatchBox.Visible;

            };

            Ranking = new DXButton()
            {
                Visible = false,
                ButtonType = ButtonType.Default,
                Label = { Text = "排行榜" },
                Parent = this,
                Size = new Size(70, SmallButtonHeight),
                //Location = new Point(Game.MainPanel.Location.X, Game.MainPanel.Location.Y - ShowChat.Size.Height),
            };
            Ranking.MouseClick += (a, b) =>
            {
                RankingBox.Visible = !RankingBox.Visible;

            };
            #endregion

            SetDefaultLocations();

            LoadChatTabs();

            foreach (DXWindow window in DXWindow.Windows)
                window.LoadSettings();

            CEnvir.CheckLauncherUpgrade();

            CEnvir.IsQuickGame = false;
            CEnvir.Target.Location = new Point(0, 0);

            ChatTextBox.Visible = true;
        }

        #region Methods
        private void SetDefaultLocations()
        {
            if (ConfigBox == null) return;

            ConfigBox.Location = new Point((Size.Width - ConfigBox.Size.Width)/2, (Size.Height - ConfigBox.Size.Height)/2);

            ChatOptionsBox.Location = new Point((Size.Width - ChatOptionsBox.Size.Width)/2, (Size.Height - ChatOptionsBox.Size.Height)/2);
            
            ExitBox.Location = new Point((Size.Width - ExitBox.Size.Width) / 2, (Size.Height - ExitBox.Size.Height) / 2);

            TradeBox.Location = new Point((Size.Width - TradeBox.Size.Width) / 2, (Size.Height - TradeBox.Size.Height) / 2);

            GuildBox.Location = new Point((Size.Width - GuildBox.Size.Width) / 2, (Size.Height - GuildBox.Size.Height) / 2);

            GuildMemberBox.Location = new Point((Size.Width - GuildMemberBox.Size.Width) / 2, (Size.Height - GuildMemberBox.Size.Height) / 2);

            InventoryBox.Location = new Point(Size.Width - InventoryBox.Size.Width, 0);
            
            CharacterBox.Location = Point.Empty;

            MapControl.Size = Size;

            MainPanel.Location = new Point((Size.Width - MainPanel.Size.Width)/2
                , Size.Height - MainPanel.Size.Height);

            //MagicBarBox.Location = new Point(MainPanel.Location.X + MainPanel.Size.Width - MagicBarBox.Size.Width
            //    , MainPanel.Location.Y - MagicBarBox.Size.Height);

            //BeltBox.Location = new Point(MagicBarBox.Location.X + MagicBarBox.Size.Width - BeltBox.Size.Width
            //    , MagicBarBox.Location.Y - BeltBox.Size.Height);

            ChatTextBox.Location = new Point(MainPanel.Location.X, MainPanel.Location.Y - ChatTextBox.Size.Height);

            //BeltBox.Location = new Point(MainPanel.Location.X + MainPanel.Size.Width - BeltBox.Size.Width, MainPanel.Location.Y - BeltBox.Size.Height);
            
            NPCBox.Location = Point.Empty;

            NPCGoodsBox.Location = new Point(0, NPCBox.Size.Height);

            NPCSellBox.Location = new Point(NPCGoodsBox.Size.Width, NPCBox.Size.Height);

            NPCRepairBox.Location = new Point(0, NPCBox.Size.Height);

            MiniMapBox.Location = new Point(Size.Width - MiniMapBox.Size.Width, 0);

            QuestTrackerBox.Location = new Point(Size.Width - QuestTrackerBox.Size.Width, MiniMapBox.Size.Height + 5);

            BuffBox.Location = new Point(Size.Width - MiniMapBox.Size.Width - BuffBox.Size.Width - 5, 0);

            MagicBox.Location = new Point(Size.Width - MagicBox.Size.Width, 0);

            GroupBox.Location = new Point((Size.Width - GroupBox.Size.Width)/2, (Size.Height - GroupBox.Size.Height)/2);

            StorageBox.Location = new Point(Size.Width - StorageBox.Size.Width - InventoryBox.Size.Width, 0);

            BigPatchBox.Location = new Point((Size.Width - BigPatchBox.Size.Width) / 2, (Size.Height - BigPatchBox.Size.Height) / 2);

            InspectBox.Location = new Point(CharacterBox.Size.Width, 0);

            RankingBox.Location = new Point((Size.Width - RankingBox.Size.Width) / 2, (Size.Height - RankingBox.Size.Height) / 2);

            MarketPlaceBox.Location = new Point((Size.Width - MarketPlaceBox.Size.Width) / 2, (Size.Height - MarketPlaceBox.Size.Height) / 2);

            MailBox.Location = new Point((Size.Width - MailBox.Size.Width) / 2, (Size.Height - MailBox.Size.Height) / 2);

            ReadMailBox.Location = new Point((Size.Width - ReadMailBox.Size.Width) / 2, (Size.Height - ReadMailBox.Size.Height) / 2);

            SendMailBox.Location = new Point((Size.Width - SendMailBox.Size.Width) / 2, (Size.Height - SendMailBox.Size.Height) / 2);

            CompanionBox.Location = new Point((Size.Width - CompanionBox.Size.Width) / 2, (Size.Height - CompanionBox.Size.Height) / 2);

            BlockBox.Location = new Point((Size.Width - BlockBox.Size.Width) / 2, (Size.Height - BlockBox.Size.Height) / 2);

            MonsterBox.Location = new Point((Size.Width - MonsterBox.Size.Width) / 2, 50);

            EditCharacterBox.Location = new Point((Size.Width - EditCharacterBox.Size.Width) / 2, (Size.Height - EditCharacterBox.Size.Height) / 2);

            FortuneCheckerBox.Location = new Point((Size.Width - FortuneCheckerBox.Size.Width) / 2, (Size.Height - FortuneCheckerBox.Size.Height) / 2);

            NPCWeaponCraftBox.Location = new Point((Size.Width - NPCWeaponCraftBox.Size.Width) / 2, (Size.Height - NPCWeaponCraftBox.Size.Height) / 2);

            // Companion health panel anchored bottom-center, offset left by 512 + panel width
            if (CompanionHealth != null)
            {
                CompanionHealth.Location = new Point(Size.Width / 2 - 512 - CompanionHealth.Size.Width, Size.Height - CompanionHealth.Size.Height);
            }

            HideChat.Location = new Point(Game.ChatTextBox.Location.X + Game.ChatTextBox.Size.Width - HideChat.Size.Width, Game.ChatTextBox.Location.Y - 148);
            ShowChat.Location = new Point(Game.MainPanel.Location.X, Game.MainPanel.Location.Y - ShowChat.Size.Height);
            BigPatch.Location = new Point(Game.ShowChat.Location.X + Game.ShowChat.Size.Width + 3, Game.ShowChat.Location.Y);
            Ranking.Location = new Point(Game.BigPatch.Location.X + Game.BigPatch.Size.Width + 3, Game.BigPatch.Location.Y);

        }

        public void SaveChatTabs()
        {
            DBCollection<ChatTabControlSetting> controlSettings = CEnvir.Session.GetCollection<ChatTabControlSetting>();
            DBCollection<ChatTabPageSetting> pageSettings = CEnvir.Session.GetCollection<ChatTabPageSetting>();

            for (int i = controlSettings.Binding.Count - 1; i >= 0; i--)
                controlSettings.Binding[i].Delete();

            foreach (DXControl temp1 in Controls)
            {
                DXTabControl tabControl = temp1 as DXTabControl;

                if (tabControl == null) continue;

                ChatTabControlSetting cSetting = controlSettings.CreateNewObject();

                cSetting.Resolution = Config.GameSize;
                cSetting.Location = tabControl.Location;
                cSetting.Size = tabControl.Size;

                foreach (DXControl tempC in tabControl.Controls)
                {
                    ChatTab tab = tempC as ChatTab;
                    if (tab == null) continue;

                    ChatTabPageSetting pSetting = pageSettings.CreateNewObject();

                    pSetting.Parent = cSetting;

                    if (tabControl.SelectedTab == tab)
                        cSetting.SelectedPage = pSetting;

                    pSetting.Name = tab.Panel.NameTextBox.TextBox.Text;
                    pSetting.Transparent = tab.Panel.TransparentCheckBox.Checked;
                    pSetting.Alert = tab.Panel.AlertCheckBox.Checked;
                    pSetting.LocalChat = tab.Panel.LocalCheckBox.Checked;
                    pSetting.WhisperChat = tab.Panel.WhisperCheckBox.Checked;
                    pSetting.GroupChat = tab.Panel.GroupCheckBox.Checked;
                    pSetting.GuildChat = tab.Panel.GuildCheckBox.Checked;
                    pSetting.ShoutChat = tab.Panel.ShoutCheckBox.Checked;
                    pSetting.GlobalChat = tab.Panel.GlobalCheckBox.Checked;
                    pSetting.ObserverChat = tab.Panel.ObserverCheckBox.Checked;
                    pSetting.HintChat = tab.Panel.HintCheckBox.Checked;
                    pSetting.SystemChat = tab.Panel.SystemCheckBox.Checked;
                    pSetting.GainsChat = tab.Panel.GainsCheckBox.Checked;
                    pSetting.AnnouncementChat = tab.Panel.AnnouncementCheckBox.Checked;
                }
            }
        }

        public void LoadChatTabs()
        {
            if (ConfigBox == null) return;
            for (int i = ChatTab.Tabs.Count - 1; i >= 0; i--)
                ChatTab.Tabs[i].Panel.RemoveButton.InvokeMouseClick();

            // 先尝试从数据库读取已保存的聊天配置
            DBCollection<ChatTabPageSetting> savedSettings = CEnvir.Session.GetCollection<ChatTabPageSetting>();
            
            List<ChatTabPageSetting> settings = new List<ChatTabPageSetting>();
            
            // 如果数据库中有保存的设置，直接使用
            if (savedSettings.Binding.Count > 0)
            {
                settings.AddRange(savedSettings.Binding);
                _LoadChatTabs(settings);
                return;
            }
            
            // 否则使用默认设置
            settings.Add(new ChatTabPageSetting
            {
                Name = "全部",
                Transparent = false,

                LocalChat = true,
                Alert = true,
                GlobalChat = true,
                GroupChat = true,
                GuildChat = true,
                ObserverChat = true,
                WhisperChat = true,
                ShoutChat = true,
                HintChat = true,
                SystemChat = true,
                GainsChat = true,
                AnnouncementChat = true,
            });

            settings.Add(new ChatTabPageSetting
            {
                Name = "私聊",
                Transparent = false,

                LocalChat = false,
                Alert = false,
                GlobalChat = false,
                GroupChat = false,
                GuildChat = false,
                ObserverChat = true,
                WhisperChat = true,
                ShoutChat = false,
                HintChat = false,
                SystemChat = false,
                GainsChat = false,
                AnnouncementChat = false
            });

            settings.Add(new ChatTabPageSetting
            {
                Name = "队伍",
                Transparent = false,

                LocalChat = false,
                Alert = false,
                GlobalChat = false,
                GroupChat = true,
                GuildChat = false,
                ObserverChat = true,
                WhisperChat = true,
                ShoutChat = false,
                HintChat = false,
                SystemChat = true,
                GainsChat = false,
                AnnouncementChat = false
            });

            settings.Add(new ChatTabPageSetting
            {
                Name = "行会",
                Transparent = false,

                LocalChat = false,
                Alert = false,
                GlobalChat = false,
                GroupChat = false,
                GuildChat = true,
                ObserverChat = false,
                WhisperChat = true,
                ShoutChat = false,
                HintChat = false,
                SystemChat = true,
                GainsChat = false,
                AnnouncementChat = false
            });

            settings.Add(new ChatTabPageSetting
            {
                Name = "公共",
                Transparent = false,

                LocalChat = true,
                Alert = false,
                GlobalChat = true,
                GroupChat = false,
                GuildChat = false,
                ObserverChat = true,
                WhisperChat = false,
                ShoutChat = true,
                HintChat = false,
                SystemChat = false,
                GainsChat = false,
                AnnouncementChat = true
            });

            settings.Add(new ChatTabPageSetting
            {
                Name = "系统",
                Transparent = false,

                LocalChat = true,
                Alert = true,
                GlobalChat = false,
                GroupChat = false,
                GuildChat = false,
                ObserverChat = false,
                WhisperChat = false,
                ShoutChat = false,
                HintChat = false,
                SystemChat = true,
                GainsChat = false,
                AnnouncementChat = false
            });

            settings.Add(new ChatTabPageSetting
            {
                Name = "提示",
                Transparent = false,

                LocalChat = false,
                Alert = false,
                GlobalChat = false,
                GroupChat = false,
                GuildChat = false,
                ObserverChat = false,
                WhisperChat = false,
                ShoutChat = false,
                HintChat = true,
                SystemChat = false,
                GainsChat = true,
                AnnouncementChat = false,
            });

            _LoadChatTabs(settings);
        }
        private void _LoadChatTabs(List<ChatTabPageSetting> controlSettings)
        {
            if (ConfigBox == null || ChatTabControl != null) return;

            for (int i = ChatTab.Tabs.Count - 1; i >= 0; i--)
                ChatTab.Tabs[i].Panel.RemoveButton.InvokeMouseClick();

            ChatTabControl = new DXTabControl
            {
                Location = new Point(Game.ChatTextBox.Location.X, Game.ChatTextBox.Location.Y - 150),
                Size = new Size(Game.ChatTextBox.Size.Width, 150),
                Parent = this,
                AllowDragOut = false,
                AllowResize = false,
            };

            ChatTabControl.SelectedTabChanged += OnChatTabChanged;
            ChatTab selected = null;
            foreach (ChatTabPageSetting pSetting in controlSettings)
            {
                ChatTab tab = ChatOptionsBox.AddNewTab();

                tab.Parent = ChatTabControl;
                tab.AllowDragOut = false;
                tab.AllowResize = false;
                

                tab.Panel.NameTextBox.TextBox.Text = pSetting.Name;
                tab.Panel.TransparentCheckBox.Checked = pSetting.Transparent;
                tab.Panel.AlertCheckBox.Checked = pSetting.Alert;
                tab.Panel.LocalCheckBox.Checked = pSetting.LocalChat;
                tab.Panel.WhisperCheckBox.Checked = pSetting.WhisperChat;
                tab.Panel.GroupCheckBox.Checked = pSetting.GroupChat;
                tab.Panel.GuildCheckBox.Checked = pSetting.GuildChat;
                tab.Panel.ShoutCheckBox.Checked = pSetting.ShoutChat;
                tab.Panel.GlobalCheckBox.Checked = pSetting.GlobalChat;
                tab.Panel.ObserverCheckBox.Checked = pSetting.ObserverChat;

                tab.Panel.HintCheckBox.Checked = pSetting.HintChat;
                tab.Panel.SystemCheckBox.Checked = pSetting.SystemChat;
                tab.Panel.GainsCheckBox.Checked = pSetting.GainsChat;
                tab.Panel.AnnouncementCheckBox.Checked = pSetting.AnnouncementChat;

                // 立即应用透明状态
                tab.TransparencyChanged();

                if (selected == null)
                    selected = tab;
            }

            ChatTabControl.SelectedTab = selected;
        }
        private void OnChatTabChanged(object sender, EventArgs e)
        {
            if (!(sender is DXTabControl tab) || !(tab.SelectedTab is ChatTab chattab)) return;

            switch(chattab.Panel.NameTextBox.TextBox.Text)
            {
                case "队伍":
                    //ChatTextBox.SetText(ChatTextBox.ChangeCommand("!!", CEnvir.ChatCommands));
                    ChatTextBox.Mode = ChatMode.队伍;
                    break;
                case "行会":
                    //ChatTextBox.SetText(ChatTextBox.ChangeCommand("!~", CEnvir.ChatCommands));
                    ChatTextBox.Mode = ChatMode.行会;
                    break;
                case "公共":
                    //ChatTextBox.SetText(ChatTextBox.ChangeCommand("!@", CEnvir.ChatCommands));
                    ChatTextBox.Mode = ChatMode.全服;
                    break;
                case "系统":
                    //ChatTextBox.SetText(ChatTextBox.ChangeCommand("!@", CEnvir.ChatCommands));
                    ChatTextBox.Mode = ChatMode.全服;
                    break;
                case "私聊":
                    //ChatTextBox.SetText(ChatTextBox.ChangeCommand("/", CEnvir.ChatCommands));
                    ChatTextBox.Mode = ChatMode.本地;
                    break;
                case "全部":
                    //ChatTextBox.SetText(ChatTextBox.ChangeCommand("", CEnvir.ChatCommands));
                    ChatTextBox.Mode = ChatMode.本地;
                    break;
            }
        }
        public override void Process()
        {
            base.Process();

            UpdateAttackModeTips();
            UpdatePetModeTips();

            if (CEnvir.Now >= MoveTime)
            {
                MoveTime = CEnvir.Now.AddMilliseconds(100);
                MapControl.Animation++;
                MoveFrame = true;
            }
            else
                MoveFrame = false;

            if (MouseControl == MapControl)
                MapControl.CheckCursor();

            if (MouseControl == MapControl)
            {
                if (CEnvir.Ctrl && MapObject.MouseObject?.Race == ObjectType.Item)
                    MouseItem = ((ItemObject) MapObject.MouseObject).Item;
                else
                    MouseItem = null;
            }

            StorageBox?.Proccess();

            TimeSpan ticks = CEnvir.Now - ItemTime;
            ItemTime = CEnvir.Now;

            if (!User.InSafeZone)
            {
                foreach (ClientUserItem item in Equipment)
                {
                    if ((item?.Flags & UserItemFlags.Expirable) != UserItemFlags.Expirable) continue;

                    item.ExpireTime -= ticks;
                }

                foreach (ClientUserItem item in Inventory)
                {
                    if ((item?.Flags & UserItemFlags.Expirable) != UserItemFlags.Expirable) continue;

                    item.ExpireTime -= ticks;
                }

                if (Companion != null)
                {
                    foreach (ClientUserItem item in Companion.InventoryArray)
                    {
                        if ((item?.Flags & UserItemFlags.Expirable) != UserItemFlags.Expirable) continue;

                        item.ExpireTime -= ticks;
                    }
                    foreach (ClientUserItem item in Companion.EquipmentArray)
                    {
                        if ((item?.Flags & UserItemFlags.Expirable) != UserItemFlags.Expirable) continue;

                        item.ExpireTime -= ticks;
                    }
                }
            }

            if (MouseItem != null && CEnvir.Now > ItemRefreshTime)
            {
                CreateItemLabel();
            }


            MapControl.ProcessInput();

            foreach (MapObject ob in MapControl.Objects)
                ob.Process();

            for (int i = MapControl.Effects.Count - 1; i >= 0; i--)
                MapControl.Effects[i].Process();

            if (ItemLabel != null && !ItemLabel.IsDisposed)
            {
                int x = CEnvir.MouseLocation.X + 15, y = CEnvir.MouseLocation.Y;

                if (x + ItemLabel.Size.Width > Size.Width + Location.X)
                    x = Size.Width - ItemLabel.Size.Width + Location.X;

                if (y + ItemLabel.Size.Height > Size.Height + Location.Y)
                    y = Size.Height - ItemLabel.Size.Height + Location.Y;

                if (x < Location.X)
                    x = Location.X;

                if (y <= Location.Y)
                    y = Location.Y;


                ItemLabel.Location = new Point(x, y);
            }

            if (MagicLabel != null && !MagicLabel.IsDisposed)
            {
                int x = CEnvir.MouseLocation.X + 15, y = CEnvir.MouseLocation.Y;

                if (x + MagicLabel.Size.Width > Size.Width + Location.X)
                    x = Size.Width - MagicLabel.Size.Width + Location.X;

                if (y + MagicLabel.Size.Height > Size.Height + Location.Y)
                    y = Size.Height - MagicLabel.Size.Height + Location.Y;

                if (x < Location.X)
                    x = Location.X;

                if (y <= Location.Y)
                    y = Location.Y;


                MagicLabel.Location = new Point(x, y);
            }


            MonsterObject mob = MouseObject as MonsterObject;
            bool isWizardOrTaoist = User.Class == MirClass.Wizard || User.Class == MirClass.Taoist;


            if (Config.开始挂机 
                && ((Config.是否开启挂机自动技能 && isWizardOrTaoist)
                || !isWizardOrTaoist))
            {
                var pick = MapControl.FindNearstItem();


                if (TargetObject == null || TargetObject.Dead)
                {
                    if (Game.User.XunzhaoGuaiwuMoshi01)
                        TargetObject = MapControl.LaoSelectMonster();
                    else if (Game.User.XunzhaoGuaiwuMoshi02)
                        TargetObject = MapControl.SelectMonster();
                    else if (!Game.User.XunzhaoGuaiwuMoshi01 && !Game.User.XunzhaoGuaiwuMoshi02)
                        TargetObject = MapControl.LaoSelectMonster();

                    if (TargetObject != null)
                        Game.MapControl.AutoPath = false;
                    mob = TargetObject as MonsterObject;
                }
                else
                    mob = TargetObject as MonsterObject;

                MouseObject = TargetObject;
            }


            if (mob != null && mob.CompanionObject == null)
                MonsterBox.Monster = mob;
            else
            {
                mob = FocusObject as MonsterObject;
                if (mob != null && mob.CompanionObject == null && !FocusObject.Dead)
                    MonsterBox.Monster = mob;
            }

            AutoZidongGuajiChanged();

            InventoryBox.InventoryDialogProcess();


            if (!Observer)
                BigPatchBox?.UpdateAutoAssist();

            ProcessSkills();
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled) return;

            // Hardcode: 宠物模式快捷键 Ctrl+1 到 Ctrl+5
            if (e.Control && !Observer)
            {
                PetMode? targetMode = null;
                bool enabled = false;

                switch (e.KeyCode)
                {
                    case Keys.D1:
                        if (Config.启用Ctrl1宠物休息)
                        {
                            targetMode = PetMode.None;
                            enabled = true;
                        }
                        break;
                    case Keys.D2:
                        if (Config.启用Ctrl2宠物移动攻击)
                        {
                            targetMode = PetMode.Both;
                            enabled = true;
                        }
                        break;
                    case Keys.D3:
                        if (Config.启用Ctrl3宠物移动)
                        {
                            targetMode = PetMode.Move;
                            enabled = true;
                        }
                        break;
                    case Keys.D4:
                        if (Config.启用Ctrl4宠物攻击)
                        {
                            targetMode = PetMode.Attack;
                            enabled = true;
                        }
                        break;
                    case Keys.D5:
                        if (Config.启用Ctrl5宠物PvP)
                        {
                            targetMode = PetMode.PvP;
                            enabled = true;
                        }
                        break;
                }

                if (enabled && targetMode.HasValue)
                {
                    User.PetMode = targetMode.Value;
                    CEnvir.Enqueue(new C.ChangePetMode { Mode = User.PetMode });
                    e.Handled = true;
                    return;
                }
            }

            // Ctrl + Enter: Toggle chat visibility
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                if (ChatTabControl != null)
                {
                    if (ChatTabControl.Visible)
                    {
                        // Hide chat
                        ChatTabControl.Visible = false;
                        ChatTextBox.Visible = false;
                        ShowChat.Visible = true;
                        HideChat.Visible = false;
                        BigPatch.Visible = true;
                        Ranking.Visible = true;
                    }
                    else
                    {
                        // Show chat
                        ChatTabControl.Visible = true;
                        ChatTextBox.Visible = true;
                        ShowChat.Visible = false;
                        HideChat.Visible = true;
                        BigPatch.Visible = false;
                        Ranking.Visible = false;
                    }
                }
                e.Handled = true;
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    MonsterBox.Monster = null;
                    LastTarget = null;
                    LastMagic = null;
                    e.Handled = true;
                    break;
            }

            foreach (KeyBindAction action in CEnvir.GetKeyAction(e.KeyCode))
            {
                switch (action)
                {
                    case KeyBindAction.ConfigWindow:
                        ConfigBox.Visible = !ConfigBox.Visible;
                        break;
                    case KeyBindAction.RankingWindow:
                        RankingBox.Visible = !RankingBox.Visible && CEnvir.Connection != null;
                        break;
                    case KeyBindAction.CharacterWindow:
                        CharacterBox.Visible = !CharacterBox.Visible;
                        break;
                    case KeyBindAction.InventoryWindow:
                        InventoryBox.Visible = !InventoryBox.Visible;
                        break;
                    case KeyBindAction.FortuneWindow:
                        FortuneCheckerBox.Visible = !FortuneCheckerBox.Visible;
                        break;
                    case KeyBindAction.MagicWindow:
                        MagicBox.Visible = !MagicBox.Visible;
                        break;
                    case KeyBindAction.MagicBarWindow:
                        MagicBarBox.Visible = !MagicBarBox.Visible;
                        break;
                    case KeyBindAction.GameStoreWindow:
                        if (MarketPlaceBox.StoreTab.IsVisible)
                            MarketPlaceBox.Visible = false;
                        else
                        {
                            MarketPlaceBox.Visible = true;
                            MarketPlaceBox.StoreTab.TabButton.InvokeMouseClick();
                        }
                        break;
                    case KeyBindAction.CompanionWindow:
                        CompanionBox.Visible = !CompanionBox.Visible;
                        break;
                    case KeyBindAction.GroupWindow:
                        GroupBox.Visible = !GroupBox.Visible;
                        break;
                    case KeyBindAction.AutoPotionWindow:
                        BigPatchBox.Visible = !BigPatchBox.Visible;
                        break;
                    case KeyBindAction.StorageWindow:
                        StorageBox.Visible = !StorageBox.Visible;
                        break;
                    case KeyBindAction.BlockListWindow:
                        BlockBox.Visible = !BlockBox.Visible;
                        break;
                    case KeyBindAction.GuildWindow:
                        GuildBox.Visible = !GuildBox.Visible;
                        break;
                    case KeyBindAction.QuestLogWindow:
                        QuestBox.Visible = !QuestBox.Visible;
                        break;
                    case KeyBindAction.QuestTrackerWindow:
                        QuestBox.CurrentTab.ShowTrackerBox.Checked = !QuestBox.CurrentTab.ShowTrackerBox.Checked;
                        break;
                    case KeyBindAction.BeltWindow:
                        BeltBox.Visible = !BeltBox.Visible;
                        break;
                    case KeyBindAction.MarketPlaceWindow:
                        if (MarketPlaceBox.ConsignTab.IsVisible || MarketPlaceBox.SearchTab.IsVisible)
                            MarketPlaceBox.Visible = false;
                        else
                        {
                            MarketPlaceBox.Visible = true;
                            MarketPlaceBox.SearchTab.TabButton.InvokeMouseClick();
                        }
                        break;
                    case KeyBindAction.MapMiniWindow:
                        if (!MiniMapBox.Visible)
                        {
                            MiniMapBox.Opacity = 1F;
                            MiniMapBox.Visible = true;
                            return;
                        }

                        if (MiniMapBox.Opacity == 1F)
                        {
                            MiniMapBox.Opacity = 0.5F;
                            return;
                        }

                        MiniMapBox.Visible = false;
                        break;
                    case KeyBindAction.MapBigWindow:
                        if (!BigMapBox.Visible)
                        {
                            BigMapBox.Opacity = 1F;
                            BigMapBox.Visible = true;
                            return;
                        }

                        if (BigMapBox.Opacity == 1F)
                        {
                            BigMapBox.Opacity = 0.5F;
                            return;
                        }

                        BigMapBox.Visible = false;
                        break;
                    case KeyBindAction.MailBoxWindow:
                        if (Observer) continue;
                        MailBox.Visible = !MailBox.Visible;
                        break;
                    case KeyBindAction.MailSendWindow:
                        if (Observer) continue;
                        SendMailBox.Visible = !SendMailBox.Visible;
                        break;
                    case KeyBindAction.ChatOptionsWindow:
                        ChatOptionsBox.Visible = !ChatOptionsBox.Visible;
                        break;
                    case KeyBindAction.ExitGameWindow:
                        ExitBox.Visible = true;
                        ExitBox.BringToFront();
                        break;
                    case KeyBindAction.ChangeAttackMode:
                        if (Observer) continue;
                        User.AttackMode = (AttackMode) (((int) User.AttackMode + 1) % 5);
                        CEnvir.Enqueue(new C.ChangeAttackMode { Mode = User.AttackMode });
                        break;
                    case KeyBindAction.ChangePetMode:
                        if (Observer) continue;

                        User.PetMode = (PetMode) (((int) User.PetMode + 1) % 5);
                        CEnvir.Enqueue(new C.ChangePetMode { Mode = User.PetMode });
                        break;
                    case KeyBindAction.GroupAllowSwitch:
                        if (Observer) continue;

                        GroupBox.AllowGroupButton.InvokeMouseClick();
                        break;
                    case KeyBindAction.GroupTarget:
                        if (Observer) continue;

                        if (MouseObject == null || MouseObject.Race != ObjectType.Player) continue;

                        CEnvir.Enqueue(new C.GroupInvite { Name = MouseObject.Name });
                        break;
                    case KeyBindAction.TradeRequest:
                        if (Observer) continue;

                        CEnvir.Enqueue(new C.TradeRequest());
                        break;
                    case KeyBindAction.TradeAllowSwitch:
                        if (Observer) continue;

                        CEnvir.Enqueue(new C.Chat { Text = "@AllowTrade" });
                        break;
                    case KeyBindAction.ChangeChatMode:
                        ChatTextBox.ChatModeButton.InvokeMouseClick();
                        break;
                    case KeyBindAction.ItemPickUp:
                        if (Observer) continue;

                        if (CEnvir.Now > PickUpTime)
                        {
                            CEnvir.Enqueue(new C.PickUp() { PickType = (byte)PickType.Sequence });
                            PickUpTime = CEnvir.Now.AddMilliseconds(250);
                        }
                        break;
                    case KeyBindAction.PartnerTeleport:
                        if (Observer) continue;

                        CEnvir.Enqueue(new C.MarriageTeleport());
                        break;
                    case KeyBindAction.MountToggle:
                        if (Observer) continue;

                        if (CEnvir.Now < User.NextActionTime || User.ActionQueue.Count > 0) return;
                        if (CEnvir.Now < User.ServerTime) return; //Next Server response Time.

                        User.ServerTime = CEnvir.Now.AddSeconds(5);
                        CEnvir.Enqueue(new C.Mount());
                        break;
                    case KeyBindAction.AutoRunToggle:
                        if (Observer) continue;

                        AutoRun = !AutoRun;
                        break;
                    case KeyBindAction.UseBelt01:
                        if (Observer) continue;

                        if (BeltBox.Grid.Grid.Length > 0)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[0]);
                            else
                                BeltBox.Grid.Grid[0].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt02:
                        if (Observer) continue;

                        if (BeltBox.Grid.Grid.Length > 1)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[1]);
                            else
                                BeltBox.Grid.Grid[1].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt03:
                        if (Observer) continue;

                        if (BeltBox.Grid.Grid.Length > 2)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[2]);
                            else
                                BeltBox.Grid.Grid[2].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt04:
                        if (Observer) continue;


                        if (BeltBox.Grid.Grid.Length > 3)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[3]);
                            else
                                BeltBox.Grid.Grid[3].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt05:
                        if (Observer) continue;


                        if (BeltBox.Grid.Grid.Length > 4)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[4]);
                            else
                                BeltBox.Grid.Grid[4].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt06:
                        if (Observer) continue;


                        if (BeltBox.Grid.Grid.Length > 5)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[5]);
                            else
                                BeltBox.Grid.Grid[5].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt07:
                        if (Observer) continue;


                        if (BeltBox.Grid.Grid.Length > 6)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[6]);
                            else
                                BeltBox.Grid.Grid[6].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt08:
                        if (Observer) continue;


                        if (BeltBox.Grid.Grid.Length > 7)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[7]);
                            else
                                BeltBox.Grid.Grid[7].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt09:
                        if (Observer) continue;


                        if (BeltBox.Grid.Grid.Length > 8)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[8]);
                            else
                                BeltBox.Grid.Grid[8].UseItem();
                        }
                        break;
                    case KeyBindAction.UseBelt10:
                        if (Observer) continue;


                        if (BeltBox.Grid.Grid.Length > 9)
                        {
                            if (SelectedCell != null)
                                SelectedCell.MoveItem(BeltBox.Grid.Grid[9]);
                            else
                                BeltBox.Grid.Grid[9].UseItem();
                        }
                        break;

                    case KeyBindAction.SpellSet01:
                        MagicBarBox.SpellSet = 1;
                        break;
                    case KeyBindAction.SpellSet02:
                        MagicBarBox.SpellSet = 2;
                        break;
                    case KeyBindAction.SpellSet03:
                        MagicBarBox.SpellSet = 3;
                        break;
                    case KeyBindAction.SpellSet04:
                        MagicBarBox.SpellSet = 4;
                        break;

                    case KeyBindAction.SpellUse01:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell01);
                        break;
                    case KeyBindAction.SpellUse02:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell02);
                        break;
                    case KeyBindAction.SpellUse03:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell03);
                        break;
                    case KeyBindAction.SpellUse04:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell04);
                        break;
                    case KeyBindAction.SpellUse05:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell05);
                        break;
                    case KeyBindAction.SpellUse06:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell06);
                        break;
                    case KeyBindAction.SpellUse07:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell07);
                        break;
                    case KeyBindAction.SpellUse08:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell08);
                        break;
                    case KeyBindAction.SpellUse09:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell09);
                        break;
                    case KeyBindAction.SpellUse10:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell10);
                        break;
                    case KeyBindAction.SpellUse11:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell11);
                        break;
                    case KeyBindAction.SpellUse12:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell12);
                        break;
                    case KeyBindAction.SpellUse13:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell13);
                        break;
                    case KeyBindAction.SpellUse14:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell14);
                        break;
                    case KeyBindAction.SpellUse15:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell15);
                        break;
                    case KeyBindAction.SpellUse16:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell16);
                        break;
                    case KeyBindAction.SpellUse17:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell17);
                        break;
                    case KeyBindAction.SpellUse18:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell18);
                        break;
                    case KeyBindAction.SpellUse19:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell19);
                        break;
                    case KeyBindAction.SpellUse20:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell20);
                        break;
                    case KeyBindAction.SpellUse21:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell21);
                        break;
                    case KeyBindAction.SpellUse22:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell22);
                        break;
                    case KeyBindAction.SpellUse23:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell23);
                        break;
                    case KeyBindAction.SpellUse24:
                        if (Observer) continue;

                        UseMagic(SpellKey.Spell24);
                        break;
                    case KeyBindAction.Guaji:
                        //GameScene.Game.ReceiveChat("挂机功能暂未开放，敬请期待...", MessageType.System);
                        // if (!MapControl.MapInfo.AllowRT)
                        //     GameScene.Game.ReceiveChat("目前您在不允许使用自动打怪功能的地图，因此不能挂机", MessageType.System);
                        // else if (Game.User.Zdgjgongneng)
                        //    GameScene.Game.ReceiveChat("每天18 : 00 点至 22 : 00 点不允许自动挂机的时间，因此不能挂机", MessageType.System);
                        // else
                        Game.BigPatchBox.Helper.AndroidPlayer.Checked = !Game.BigPatchBox.Helper.AndroidPlayer.Checked;
                        break;
                    default:
                        continue;
                }

                e.Handled = true;
            }
        }

        private void CreateItemLabel()
        {
            if (ItemLabel != null && !ItemLabel.IsDisposed) ItemLabel.Dispose();

            if (MouseItem == null) return;

            ItemRefreshTime = CEnvir.Now.AddSeconds(1);

            Stats stats = new Stats();
            stats.Add(MouseItem.Info.Stats);
            stats.Add(MouseItem.AddedStats);

            ItemLabel = new DXControl
            {
                BackColour = Color.FromArgb(200, 0, 24, 48),
                Border = true,
                BorderColour = Color.FromArgb(148, 148, 49), // Color.FromArgb(144, 148, 48),
                DrawTexture = true,
                IsControl = false,
                IsVisible = true,
            };

            ItemInfo displayInfo = MouseItem.Info;

            if (MouseItem.Info.Effect == ItemEffect.ItemPart)
                displayInfo = Globals.ItemInfoList.Binding.First(x => x.Index == MouseItem.AddedStats[Stat.ItemIndex]);
            

            DXLabel label = new DXLabel
            {
                ForeColour = Color.Yellow,
                Location = new Point(4, 4),
                Parent = ItemLabel,
                Text = $"▼ {displayInfo.ItemName}" 
            };

            if (MouseItem.Info.Effect == ItemEffect.ItemPart)
                label.Text += " - [碎片]";
            ItemLabel.Size = new Size(label.DisplayArea.Right + 4, label.DisplayArea.Bottom);

            Type type = displayInfo.ItemType.GetType();
            MemberInfo[] infos = type.GetMember(displayInfo.ItemType.ToString());
            DescriptionAttribute description = infos[0].GetCustomAttribute<DescriptionAttribute>();

            string itemtype = description?.Description ?? "";
            itemtype = $"{(displayInfo.ItemType != ItemType.Nothing ? $"{itemtype} " : "")}";
            bool needSpacer = false;


            if (displayInfo.Rarity > Rarity.Common || displayInfo.ItemType != ItemType.Nothing)
            {
                Type type2 = displayInfo.Rarity.GetType();
                MemberInfo[] infos2 = type2.GetMember(displayInfo.Rarity.ToString());
                DescriptionAttribute description2 = infos2[0].GetCustomAttribute<DescriptionAttribute>();
                string rarity = description2?.Description ?? "普通物品";

                label = new DXLabel
                {
                    ForeColour = displayInfo.Rarity == Rarity.Common ? Color.White : (displayInfo.Rarity == Rarity.Superior ? Color.FromArgb(0, 180, 0) : Color.FromArgb(0, 255, 0)),
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom + 2),
                    Parent = ItemLabel,
                    Text = displayInfo.Rarity > Rarity.Common ? $"{itemtype}{rarity}" : itemtype,
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 15);

            if (needSpacer)
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            if (MouseItem.Info.Weight > 0)
            {
                label = new DXLabel
                {
                    ForeColour = Color.White,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"重量: {MouseItem.Info.Weight}",
                };

                switch (MouseItem.Info.ItemType)
                {
                    case ItemType.Weapon:
                    case ItemType.Shield:
                    case ItemType.Torch:
                        if (User.HandWeight - (Equipment[(int)EquipmentSlot.Weapon]?.Info.Weight ?? 0) + MouseItem.Info.Weight > User.Stats[Stat.HandWeight])
                            label.ForeColour = Color.Red;
                        break;
                    case ItemType.Armour:
                    case ItemType.Helmet:
                    case ItemType.Necklace:
                    case ItemType.Bracelet:
                    case ItemType.Ring:
                    case ItemType.Shoes:
                    case ItemType.Poison:
                    case ItemType.Amulet:
                        if (User.WearWeight - (Equipment[(int)EquipmentSlot.Armour]?.Info.Weight ?? 0) + MouseItem.Info.Weight > User.Stats[Stat.WearWeight])
                            label.ForeColour = Color.Red;
                        break;
                }

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                needSpacer = true;
            }

            if (MouseItem.Info.Effect == ItemEffect.Gold || MouseItem.Info.Effect == ItemEffect.Experience)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(ItemLabel.DisplayArea.Right, 4),
                    Parent = ItemLabel,
                    Text = $"数额: {MouseItem.Count}"
                };
                ItemLabel.Size = new Size(label.DisplayArea.Right + 4, ItemLabel.Size.Height + 4);
                return;
            }


            if (MouseItem.Info.Effect == ItemEffect.ItemPart)
            {
                label = new DXLabel
                {
                    ForeColour = Color.LightSeaGreen,
                    Location = new Point(ItemLabel.DisplayArea.Right, 4),
                    Parent = ItemLabel,
                    Text = $"组件: {MouseItem.Count}/{displayInfo.PartCount}.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4, ItemLabel.Size.Height);
            }
            else if (MouseItem.Info.StackSize > 1)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(ItemLabel.DisplayArea.Right, 4),
                    Parent = ItemLabel,
                    Text = $"数量: {MouseItem.Count}/{MouseItem.Info.StackSize}"
                };
                ItemLabel.Size = new Size(label.DisplayArea.Right + 4, ItemLabel.Size.Height);
            }

            switch (displayInfo.ItemType)
            {
                case ItemType.Consumable:
                case ItemType.Scroll:
                    if (MouseItem.Info.Effect == ItemEffect.StatExtractor || MouseItem.Info.Effect == ItemEffect.RefineExtractor)
                        EquipmentItemInfo();
                    else
                        CreatePotionLabel();
                    break;
                case ItemType.Book:
                    if (MouseItem.Info.Durability > 0)
                    {
                        label = new DXLabel
                        {
                            ForeColour = Color.White,
                            Location = new Point(ItemLabel.DisplayArea.Right, 4),
                            Parent = ItemLabel,
                            Text = $"页数: {MouseItem.CurrentDurability}/{MouseItem.MaxDurability}",
                        };

                        ItemLabel.Size = new Size(label.DisplayArea.Right + 4, ItemLabel.Size.Height);
                    }
                    break;
                case ItemType.Meat:
                    if (MouseItem.Info.Durability > 0)
                    {
                        label = new DXLabel
                        {
                            ForeColour = MouseItem.CurrentDurability == 0 ? Color.Red : Color.White,
                            Location = new Point(ItemLabel.DisplayArea.Right, 4),
                            Parent = ItemLabel,
                            Text = $"品质: {Math.Round(MouseItem.CurrentDurability/1000M)}/{Math.Round(MouseItem.MaxDurability/1000M)}",
                        };

                        ItemLabel.Size = new Size(label.DisplayArea.Right + 4, ItemLabel.Size.Height);
                    }
                    break;
                case ItemType.Ore:
                    if (MouseItem.Info.Durability > 0)
                    {
                        label = new DXLabel
                        {
                            ForeColour = MouseItem.CurrentDurability == 0 ? Color.Red : Color.White,
                            Location = new Point(ItemLabel.DisplayArea.Right, 4),
                            Parent = ItemLabel,
                            Text = $"纯度: {Math.Round(MouseItem.CurrentDurability/1000M)}",
                        };

                        ItemLabel.Size = new Size(label.DisplayArea.Right + 4, ItemLabel.Size.Height);
                    }
                    break;
                default:
                    EquipmentItemInfo();
                    break;
            }


            if (displayInfo.RequiredGender != RequiredGender.None)
            {
                Color colour = Color.White;
                switch (User.Gender)
                {
                    case MirGender.Male:
                        if (!displayInfo.RequiredGender.HasFlag(RequiredGender.Male))
                            colour = Color.Red;
                        break;
                    case MirGender.Female:
                        if (!displayInfo.RequiredGender.HasFlag(RequiredGender.Female))
                            colour = Color.Red;
                        break;
                }

                Type type4 = displayInfo.RequiredGender.GetType();

                MemberInfo[] infos4 = type4.GetMember(displayInfo.RequiredGender.ToString());

                DescriptionAttribute description4 = infos4.Length > 0 ? infos4[0].GetCustomAttribute<DescriptionAttribute>() : null;


                label = new DXLabel
                {
                    ForeColour = colour,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"性别: {description4?.Description ?? ""}",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }


            if (displayInfo.RequiredClass != RequiredClass.All)
            {
                Color colour = Color.White;
                switch (User.Class)
                {
                    case MirClass.Warrior:
                        if (!MouseItem.Info.RequiredClass.HasFlag(RequiredClass.Warrior))
                            colour = Color.Red;
                        break;
                    case MirClass.Wizard:
                        if (!MouseItem.Info.RequiredClass.HasFlag(RequiredClass.Wizard))
                            colour = Color.Red;
                        break;
                    case MirClass.Taoist:
                        if (!MouseItem.Info.RequiredClass.HasFlag(RequiredClass.Taoist))
                            colour = Color.Red;
                        break;
                    case MirClass.Assassin:
                        if (!MouseItem.Info.RequiredClass.HasFlag(RequiredClass.Assassin))
                            colour = Color.Red;
                        break;
                }

                Type type3 = displayInfo.RequiredClass.GetType();

                MemberInfo[] infos3 = type3.GetMember(displayInfo.RequiredClass.ToString());

                DescriptionAttribute description3 = infos3[0].GetCustomAttribute<DescriptionAttribute>();


                label = new DXLabel
                {
                    ForeColour = colour,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"职业: {description3?.Description ?? displayInfo.RequiredClass.ToString()}",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }



            if (displayInfo.RequiredAmount > 0)
            {
                string text;
                Color colour = displayInfo.Rarity == Rarity.Common ? Color.White : Color.FromArgb(0, 204, 0);
                switch (displayInfo.RequiredType)
                {
                    case RequiredType.Level:
                        text = $"等级: {MouseItem.Info.RequiredAmount}";
                        if (User.Level < MouseItem.Info.RequiredAmount && User.Stats[Stat.Rebirth] == 0)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxLevel:
                        text = $"最大等级: {MouseItem.Info.RequiredAmount}";
                        if (User.Level > MouseItem.Info.RequiredAmount || User.Stats[Stat.Rebirth] > 0)
                            colour = Color.Red;
                        break;
                    case RequiredType.AC:
                        text = $"物防: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.MaxAC] < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MR:
                        text = $"魔防: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.MaxMR] < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.DC:
                        text = $"破坏力: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.MaxDC] < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MC:
                        text = $"自然魔法力: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.MaxMC] < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.SC:
                        text = $"精神力: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.MaxSC] < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.Health:
                        text = $"生命值: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.Health] < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.Mana:
                        text = $"魔法值: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.Mana] < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.CompanionLevel:
                        text = $"伙伴等级: {MouseItem.Info.RequiredAmount}";
                        if (Companion == null || Companion.Level < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxCompanionLevel:
                        text = $"最大伙伴等级: {MouseItem.Info.RequiredAmount}";
                        if (Companion == null || Companion.Level > MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.RebirthLevel:
                        text = $"重生等级: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.Rebirth] < MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxRebirthLevel:
                        text = $"重生等级: {MouseItem.Info.RequiredAmount}";
                        if (User.Stats[Stat.Rebirth] > MouseItem.Info.RequiredAmount)
                            colour = Color.Red;
                        break;
                    default:
                        text = "未知条件";
                        break;
                }


                //if (displayInfo.Rarity > Rarity.Common)
                //    text += $" ({rarity})";


                label = new DXLabel
                {
                    ForeColour = colour,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = text,
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }



            bool spacer = false;
            long sale = MouseItem.Price(Math.Max(1, MouseItem.Count));
            if (sale > 0)
            {
                label = new DXLabel
                {
                    ForeColour = Color.LightGoldenrodYellow,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom + 2),
                    Parent = ItemLabel,
                    Text = $"售价: {sale}",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }
            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);


            if (MouseItem.Info.Durability > 0 && !MouseItem.Info.CanRepair && MouseItem.Info.StackSize == 1)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "不可修理.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }

            if (!MouseItem.Info.CanSell || (MouseItem.Flags & UserItemFlags.Worthless) == UserItemFlags.Worthless)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "只能0金币出售给商店.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }

            if (!MouseItem.Info.CanStore)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "不可寄售.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }
            
            if (!MouseItem.Info.CanTrade || (MouseItem.Flags & UserItemFlags.Bound) == UserItemFlags.Bound)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "不能交易.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }

            if (!MouseItem.Info.CanDrop)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "不可丢弃.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }

            if (!MouseItem.Info.CanDeathDrop || (MouseItem.Flags & UserItemFlags.Worthless) == UserItemFlags.Worthless || (MouseItem.Flags & UserItemFlags.Bound) == UserItemFlags.Bound)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "死亡不会掉落.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }

            if ((MouseItem.Flags & UserItemFlags.Bound) == UserItemFlags.Bound)
            {
                label = new DXLabel
                {
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "绑定物品.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }

            if ((MouseItem.Flags & UserItemFlags.NonRefinable) == UserItemFlags.NonRefinable)
            {
                label = new DXLabel
                {
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                };

                switch (MouseItem.Info.ItemType)
                {
                    case ItemType.Book:
                        label.ForeColour = Color.Red;
                        label.Text = "没有包含高等级书页.";
                        break;
                    default:
                        label.ForeColour = Color.Yellow;
                        label.Text = "不可精炼或升级.";
                        break;
                }

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }
            else if (MouseItem.Info.ItemType == ItemType.Book)
            {
                label = new DXLabel
                {
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    ForeColour = Color.Green,
                    Text = "包含了高等级技能书页.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }

            if (!string.IsNullOrEmpty(displayInfo.Description))
            {
                label = new DXLabel
                {
                    ForeColour = Color.Wheat,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom + 10),
                    Parent = ItemLabel,
                    Text = displayInfo.Description,
                };

                if (displayInfo.Effect == ItemEffect.FootBallWhistle)
                    label.ForeColour = Color.Red;

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);
                spacer = true;
            }

            if (spacer)
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);


            if (CEnvir.Now < MouseItem.NextSpecialRepair && MouseItem.Info.Durability > 0 && MouseItem.Info.CanRepair && MouseItem.Info.StackSize == 1 && MouseItem.Info.ItemType != ItemType.Book)
            {
                label = new DXLabel
                {
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                };

                label.Text = $"特殊修理还需等待 {Functions.ToString(MouseItem.NextSpecialRepair - CEnvir.Now, true)}";
                label.ForeColour = Color.Red;

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            }

            if ((MouseItem.Flags & UserItemFlags.Expirable) == UserItemFlags.Expirable)
            {
                label = new DXLabel
                {
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"将于 {Functions.ToString(MouseItem.ExpireTime, true)}后过期",
                    ForeColour = Color.Chocolate,
                };
                

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            }

            if (stats[Stat.ItemReviveTime] > 0)
            {
                label = new DXLabel
                {
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                };

                DateTime value = MouseItem.Info.Effect == ItemEffect.PillOfReincarnation ? ReincarnationPillTime : ItemReviveTime;

                if (CEnvir.Now >= value)
                {
                    label.Text = "可以归还";
                    label.ForeColour = Color.LimeGreen;
                }
                else
                {
                    label.Text = $"将于 {Functions.ToString(value - CEnvir.Now, true)}后归还";
                    label.ForeColour = Color.Red;
                }


                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            }


            if (MouseItem.Info.Set != null)
                SetItemInfo(MouseItem.Info.Set);

            if ((MouseItem.Flags & UserItemFlags.Marriage) == UserItemFlags.Marriage)
            {
                label = new DXLabel
                {
                    ForeColour = Color.MediumOrchid,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "结婚戒指.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            }

            if ((MouseItem.Flags & UserItemFlags.GameMaster) == UserItemFlags.GameMaster)
            {
                label = new DXLabel
                {
                    ForeColour = Color.LightSeaGreen,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "游戏管理员创建.",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            }

            if (NPCItemFragmentBox.IsVisible && MouseItem.CanFragment())
            {
                label = new DXLabel
                {
                    ForeColour = Color.MediumAquamarine,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"碎片 消耗: {MouseItem.FragmentCost():#,##0}",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

                label = new DXLabel
                {
                    ForeColour = Color.MediumAquamarine,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"碎片: {(MouseItem.Info.Rarity == Rarity.Common ? "碎片" : "碎片 (II)")} x{MouseItem.FragmentCount():#,##0}",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            }

            if (CEnvir.Now < MouseItem.NextReset)
            {
                label = new DXLabel
                {
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"需等待 {Functions.ToString(MouseItem.NextReset - CEnvir.Now, true)}后可以重置",
                    ForeColour = Color.Red,
                };


                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            }

            if ((MouseItem.Flags & UserItemFlags.Locked) == UserItemFlags.Locked)
            {
                label = new DXLabel
                {
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"锁定: 防止意外出售或丢弃\n" +
                           $"解锁请按 [鼠标滚轮键] 或者 [Scroll Lock].",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height);

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            }


        }
        private void EquipmentItemInfo()
        {
            Stats stats = new Stats();

            ItemInfo displayInfo = MouseItem.Info;

            if (MouseItem.Info.Effect == ItemEffect.ItemPart)
                displayInfo = Globals.ItemInfoList.Binding.First(x => x.Index == MouseItem.AddedStats[Stat.ItemIndex]);

            stats.Add(displayInfo.Stats, displayInfo.ItemType != ItemType.Weapon);
            stats.Add(MouseItem.AddedStats, MouseItem.Info.ItemType != ItemType.Weapon);
            
            if (displayInfo.ItemType == ItemType.Weapon)
            {
                Stat ele = MouseItem.AddedStats.GetWeaponElement();

                if (ele == Stat.None)
                    ele = displayInfo.Stats.GetWeaponElement();

                if (ele != Stat.None)
                    stats[ele] += MouseItem.AddedStats.GetWeaponElementValue() + displayInfo.Stats.GetWeaponElementValue();
            }

            DXLabel label;
            if (MouseItem.Info.Durability > 0)
            {
                label = new DXLabel
                {
                    ForeColour = MouseItem.CurrentDurability == 0 ? Color.Red : Color.FromArgb(132, 255, 255),
                    Location = new Point(ItemLabel.DisplayArea.Right, 4),
                    Parent = ItemLabel,
                    Text = $"持久: {Math.Round(MouseItem.CurrentDurability/1000M)}/{Math.Round(MouseItem.MaxDurability/1000M)}",
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4, ItemLabel.Size.Height);
            }

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 5);

            bool firstele = stats.HasElementalWeakness();
            foreach (KeyValuePair<Stat, int> pair in stats.Values)
            {
                string text = stats.GetDisplay(pair.Key);

                if (text == null) continue;

                string added = MouseItem.AddedStats.GetFormat(pair.Key);

                label = new DXLabel
                {
                    ForeColour = Color.White,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = text
                };

                switch (pair.Key)
                {
                    case Stat.Luck:
                        label.ForeColour = Color.Yellow;
                        break;
                    case Stat.Strength:
                        label.ForeColour = Color.FromArgb(148, 255, 206);
                        break;
                    case Stat.DropRate:
                    case Stat.ExperienceRate:
                    case Stat.SkillRate:
                    case Stat.GoldRate:
                        label.ForeColour = Color.Yellow;

                        if (added == null) break;
                        label.Text += $" ({added})";
                        break;
                    case Stat.FireAttack:
                    case Stat.IceAttack:
                    case Stat.LightningAttack:
                    case Stat.WindAttack:
                    case Stat.HolyAttack:
                    case Stat.DarkAttack:
                    case Stat.PhantomAttack:
                        label.ForeColour = Color.DeepSkyBlue;
                        break;
                    case Stat.FireResistance:
                    case Stat.IceResistance:
                    case Stat.LightningResistance:
                    case Stat.WindResistance:
                    case Stat.HolyResistance:
                    case Stat.DarkResistance:
                    case Stat.PhantomResistance:
                    case Stat.PhysicalResistance:
                        label.ForeColour = !firstele ? Color.Lime : Color.IndianRed;
                        firstele = true;
                        break;
                    default:
                        if (MouseItem.AddedStats[pair.Key] == 0) break;
                        label.Text += $"   ({added})";
                        label.ForeColour = Color.FromArgb(148, 255, 206);
                        break;
                }

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }
            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 5);

            int limit_level = Globals.WeaponExperienceList.Count;
            limit_level -= (2 - (int)displayInfo.Rarity) * 3;

            Type type = displayInfo.ItemType.GetType();

            MemberInfo[] infos = type.GetMember(displayInfo.ItemType.ToString());

            DescriptionAttribute description = infos[0].GetCustomAttribute<DescriptionAttribute>();

            string itemtype = description?.Description ?? "";

            switch (displayInfo.ItemType)
            {
                case ItemType.Weapon:
                    if ((MouseItem.Flags & UserItemFlags.NonRefinable) == UserItemFlags.NonRefinable) break;

                    label = new DXLabel
                    {
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                        Parent = ItemLabel,
                        Text = $"{itemtype} 等级: " + MouseItem.Level.ToString() + (MouseItem.Level < limit_level ? "" : " 满级"),
                    };

                    ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                        label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

                    if (MouseItem.Level <= limit_level)
                    {
                        label = new DXLabel
                        {
                            Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                            Parent = ItemLabel,
                        };

                        if ((MouseItem.Flags & UserItemFlags.Refinable) == UserItemFlags.Refinable)
                        {
                            label.Text = "可以精炼";
                            label.ForeColour = Color.LightGreen;
                        }
                        else if(MouseItem.Level != limit_level)
                        {
                            label.Text = $"{itemtype} 训练点: {MouseItem.Experience / Globals.WeaponExperienceList[MouseItem.Level]:0.##%}";
                            label.ForeColour = Color.White;
                        }



                        ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                            label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                    }
                    ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 5);
                    break;
                case ItemType.Necklace:
                case ItemType.Bracelet:
                case ItemType.Ring:

                    if ((MouseItem.Flags & UserItemFlags.NonRefinable) == UserItemFlags.NonRefinable) break;

                    label = new DXLabel
                    {
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                        Parent = ItemLabel,
                        Text = $"{itemtype} 等级: " + (MouseItem.Level < Globals.AccessoryExperienceList.Count - (2 - (int)MouseItem.Info.Rarity) * 2 ? MouseItem.Level.ToString() : $"{MouseItem.Level.ToString()} 满级")
                    };

                    ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                        label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

                    if (MouseItem.Level < Globals.AccessoryExperienceList.Count)
                    {
                        label = new DXLabel
                        {
                            Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                            Parent = ItemLabel,
                        };

                        if ((MouseItem.Flags & UserItemFlags.Refinable) == UserItemFlags.Refinable)
                        {
                            label.Text = "可以精炼";
                            label.ForeColour = Color.LightGreen;
                        }
                        else
                        {
                            label.Text = $"{itemtype} 修炼点: {MouseItem.Experience / Globals.AccessoryExperienceList[MouseItem.Level]:0.##%}";
                            label.ForeColour = Color.White;
                        }



                        ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                            label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
                    }
                    ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 5);
                    break;

            }
        }
        private void CreatePotionLabel()
        {
            if (MouseItem == null) return;

            Stats stats = new Stats();
            
            stats.Add(MouseItem.Info.Stats);
            

            DXLabel label;
            foreach (KeyValuePair<Stat, int> pair in stats.Values)
            {
                string text = stats.GetDisplay(pair.Key);

                if (text == null) continue;

                label = new DXLabel
                {
                    ForeColour = Color.White,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = text
                };

                switch (pair.Key)
                {
                    case Stat.Luck:
                    case Stat.DropRate:
                    case Stat.ExperienceRate:
                    case Stat.SkillRate:
                    case Stat.GoldRate:
                        label.ForeColour = Color.Yellow;
                        break;
                    case Stat.DeathDrops:
                        label.ForeColour = Color.OrangeRed;
                        break;
                }

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }
            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 5);

            if (MouseItem.Info.Durability > 0)
            {
                label = new DXLabel
                {
                    ForeColour = Color.White,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = $"冷却: {MouseItem.Info.Durability/1000M:#,##0.#} 秒"
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }
        }
        private void CreateMagicLabel()
        {
            if (MouseMagic == null) return;

            MagicLabel = new DXControl
            {
                BackColour = Color.FromArgb(255, 0, 24, 48),
                Border = true,
                BorderColour = Color.Yellow, // Color.FromArgb(144, 148, 48),
                DrawTexture = true,
                IsControl = false,
                IsVisible = true,
            };


            DXLabel label = new DXLabel
            {
                ForeColour = Color.Yellow,
                Location = new Point(4, 4),
                Parent = MagicLabel,
                Text = MouseMagic.Name
            };
            MagicLabel.Size = new Size(label.DisplayArea.Right + 4, label.DisplayArea.Bottom + 4);

            ClientUserMagic magic;


            int width;
            if (User.Magics.TryGetValue(MouseMagic, out magic))
            {
                int level = magic.Level;
                label = new DXLabel
                {
                    ForeColour = Color.Green,
                    Location = new Point(4, MagicLabel.DisplayArea.Bottom),
                    Parent = MagicLabel,
                    Text = $"当前等级: {level}",
                };
                string text = null;
                switch (magic.Level)
                {
                    case 0:
                        text = $"{magic.Experience}/{magic.Info.Experience1}";
                        break;
                    case 1:
                        text = $"{magic.Experience}/{magic.Info.Experience2}";
                        break;
                    case 2:
                        text = $"{magic.Experience}/{magic.Info.Experience3}";
                        break;
                    default:
                        text = $"{magic.Experience}/{(magic.Level - 2) * 500}";
                        break;
                }

                width = label.DisplayArea.Right;
                label = new DXLabel
                {
                    ForeColour = Color.Green,
                    Location = new Point(width + 4, MagicLabel.DisplayArea.Bottom),
                    Parent = MagicLabel,
                    Text = $"经验: {text}",
                };
            }
            else
            {
                label = new DXLabel
                {
                    ForeColour = Color.Red,
                    Location = new Point(4, MagicLabel.DisplayArea.Bottom),
                    Parent = MagicLabel,
                    Text = $"未学习.",
                };
            }
            MagicLabel.Size = new Size(label.DisplayArea.Right + 4 > MagicLabel.Size.Width ? label.DisplayArea.Right + 4 : MagicLabel.Size.Width, label.DisplayArea.Bottom);

            label = new DXLabel
            {
                ForeColour = User.Level < MouseMagic.NeedLevel1 ? Color.Red : Color.White,
                Location = new Point(4, MagicLabel.DisplayArea.Bottom),
                Parent = MagicLabel,
                Text = $"修炼 1 级条件: 等级 {MouseMagic.NeedLevel1}",
            };
            width = label.DisplayArea.Right + 10;
            label = new DXLabel
            {
                ForeColour = Color.White,
                Location = new Point(width, MagicLabel.DisplayArea.Bottom),
                Parent = MagicLabel,
                Text = $"经验: {MouseMagic.Experience1:#,##0}",
            };

            MagicLabel.Size = new Size(label.DisplayArea.Right + 4 > MagicLabel.Size.Width ? label.DisplayArea.Right + 4 : MagicLabel.Size.Width, label.DisplayArea.Bottom);

            new DXLabel
            {
                ForeColour = User.Level < MouseMagic.NeedLevel2 ? Color.Red : Color.White,
                Location = new Point(4, MagicLabel.DisplayArea.Bottom),
                Parent = MagicLabel,
                Text = $"修炼 2 级条件: 等级 {MouseMagic.NeedLevel2}",
            };

            label = new DXLabel
            {
                ForeColour = Color.White,
                Location = new Point(width , MagicLabel.DisplayArea.Bottom),
                Parent = MagicLabel,
                Text = $"经验: {MouseMagic.Experience2:#,##0}",
            };

            MagicLabel.Size = new Size(label.DisplayArea.Right + 4 > MagicLabel.Size.Width ? label.DisplayArea.Right + 4 : MagicLabel.Size.Width, label.DisplayArea.Bottom);

            new DXLabel
            {
                ForeColour = User.Level < MouseMagic.NeedLevel3 ? Color.Red : Color.White,
                Location = new Point(4, MagicLabel.DisplayArea.Bottom),
                Parent = MagicLabel,
                Text = $"修炼 3 级条件: 等级 {MouseMagic.NeedLevel3}",
            };

            label = new DXLabel
            {
                ForeColour = Color.White,
                Location = new Point(width, MagicLabel.DisplayArea.Bottom),
                Parent = MagicLabel,
                Text = $"经验: {MouseMagic.Experience3:#,##0}",
            };
            MagicLabel.Size = new Size(label.DisplayArea.Right + 4 > MagicLabel.Size.Width ? label.DisplayArea.Right + 4 : MagicLabel.Size.Width, label.DisplayArea.Bottom);


            label = new DXLabel
            {
                ForeColour = magic?.Level < 3 ? Color.Red : Color.White,
                Location = new Point(4, MagicLabel.DisplayArea.Bottom),
                Parent = MagicLabel,
                Text = $"修炼 4+ 条件: 掉落书籍",
            };
            MagicLabel.Size = new Size(label.DisplayArea.Right + 4 > MagicLabel.Size.Width ? label.DisplayArea.Right + 4 : MagicLabel.Size.Width, label.DisplayArea.Bottom);
            

            label = new DXLabel
            {
                AutoSize = false,
                ForeColour = Color.Wheat,
                Location = new Point(4, MagicLabel.DisplayArea.Bottom),
                Parent = MagicLabel,
                Text = MouseMagic.Description,
            };
            label.Size = DXLabel.GetHeight(label, MagicLabel.Size.Width );
            MagicLabel.Size = new Size(label.DisplayArea.Right + 4 > MagicLabel.Size.Width ? label.DisplayArea.Right + 4 : MagicLabel.Size.Width, label.DisplayArea.Bottom + 4);
            

        }
        private void SetItemInfo(SetInfo set)
        {
            DXLabel label = new DXLabel
            {
                ForeColour = Color.LimeGreen,
                Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                Parent = ItemLabel,
                Text = $"套装:"
            };

            ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

            label = new DXLabel
            {
                ForeColour = Color.LimeGreen,
                Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                Parent = ItemLabel,
                Text = $"    {set.SetName}"
            };

            ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            
            label = new DXLabel
            {
                ForeColour = Color.LimeGreen,
                Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                Parent = ItemLabel,
                Text = "套件:"
            };

            ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

            bool hasFullSet = true;
            List<int> counted = new List<int>();

            Stats setBonus = new Stats();

            int l;
            MirClass c;
            ClientUserItem[] equip;

            DXItemCell cell = MouseControl as DXItemCell;
            if (cell?.GridType == GridType.Inspect)
            {
                l = InspectBox.Level;
                c = InspectBox.Class;
                equip = InspectBox.Equipment;
            }
            else
            {
                l = User.Level;
                c = User.Class;
                equip = Equipment;
            }

            foreach (ItemInfo info in set.Items)
            {
                bool hasPart = false;
                for (int j = 0; j < equip.Length; j++)
                {
                    if (counted.Contains(j)) continue;
                    if (equip[j] == null) continue;
                    if (equip[j].Info != info) continue;
                    if (equip[j].CurrentDurability == 0 && equip[j].Info.Durability > 0) continue;
                    
                    counted.Add(j);

                    hasPart = true;
                    break;
                }

                if (!hasPart)
                    hasFullSet = false;

                label = new DXLabel
                {
                    ForeColour = hasPart ? Color.LimeGreen : Color.Gray,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "    " + info.ItemName
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                    label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);
            }
            label = new DXLabel
            {
                ForeColour = Color.LimeGreen,
                Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                Parent = ItemLabel,
                Text = $"套装属性:"
            };

            ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);


            foreach (SetInfoStat stat in set.SetStats)
            {
                if (l < stat.Level) continue;

                switch (c)
                {
                    case MirClass.Warrior:
                        if ((stat.Class & RequiredClass.Warrior) != RequiredClass.Warrior) continue;
                        break;
                    case MirClass.Wizard:
                        if ((stat.Class & RequiredClass.Wizard) != RequiredClass.Wizard) continue;
                        break;
                    case MirClass.Taoist:
                        if ((stat.Class & RequiredClass.Taoist) != RequiredClass.Taoist) continue;
                        break;
                    case MirClass.Assassin:
                        if ((stat.Class & RequiredClass.Assassin) != RequiredClass.Assassin) continue;
                        break;
                }

                setBonus[stat.Stat] += stat.Amount;
            }



            foreach (KeyValuePair<Stat, int> pair in setBonus.Values)
            {
                string text = setBonus.GetDisplay(pair.Key);

                if (text == null) continue;

                label = new DXLabel
                {
                    ForeColour = hasFullSet ? Color.LimeGreen : Color.Gray,
                    Location = new Point(4, ItemLabel.DisplayArea.Bottom),
                    Parent = ItemLabel,
                    Text = "    " + text
                };

                ItemLabel.Size = new Size(label.DisplayArea.Right + 4 > ItemLabel.Size.Width ? label.DisplayArea.Right + 4 : ItemLabel.Size.Width,
                                          label.DisplayArea.Bottom > ItemLabel.Size.Height ? label.DisplayArea.Bottom : ItemLabel.Size.Height);

            }


            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
        }

        public ClientUserMagic GetMagic(MagicType type)
        {
            foreach (KeyValuePair<MagicInfo, ClientUserMagic> magic in User.Magics)
            {
                if (magic.Value.Info.Magic == type)
                    return magic.Value;
            }

            return null;
        }

        public void UseMagic(MagicType type, MapObject map_ob = null)
        {
            if (Game.Observer || User == null || User.Horse != HorseType.None || MagicBarBox == null)
                return;
            ClientUserMagic clientUserMagic = GetMagic(type);
            UseMagic(clientUserMagic, map_ob);
        }
        public MagicHelper GetMagicHelpper(MagicType magic)
        {
            for (int index = 0; index < Config.magics.Count; ++index)
            {
                if (Config.magics[index].TypeID == magic)
                    return Config.magics[index];
            }

            return null;
        }
        public MagicHelper TakeAmulet(ClientUserMagic magic)
        {
            MagicHelper magicHelper = null;
            if (!CEnvir.NeedAmulet(magic.Info)) return magicHelper;


            magicHelper = GetMagicHelpper(magic.Info.Magic);
            if (!Config.自动换符)
                return magicHelper;
            if (magicHelper == null)
                return magicHelper;
            int amulet = magicHelper.Amulet;
            if (amulet == -1)
                return magicHelper;
            ClientUserItem clientUserItem1 = CharacterBox?.Grid[11]?.Item;

            if ((clientUserItem1?.Info?.Index ?? -1) == magicHelper.Amulet)
                return magicHelper;

            long bestCount = 0;
            int bestIndex = -1;

            for (int index = 0; index < Inventory.Length; ++index)
            {
                ClientUserItem clientUserItem2 = Inventory[index];
                if ((clientUserItem2?.Info.Index ?? -1) == magicHelper.Amulet)
                {
                    if (clientUserItem2.Count > 0 && clientUserItem2.Count > bestCount)
                    {
                        bestCount = clientUserItem2.Count;
                        bestIndex = index;
                    }
                }
            }

            if (bestIndex < 0)
            {
                ReceiveChat("你的符用完了，释放失败", MessageType.Hint);
                return magicHelper;
            }

            CharacterBox.Grid[11].ToEquipment(InventoryBox.Grid.Grid[bestIndex]);
            return magicHelper;
        }

        public void UseMagic(ClientUserMagic magic, MapObject map_ob = null)
        {
            if (magic == null || User.Level < magic.Info.NeedLevel1)
                return;
            MapObject mapObject = map_ob;
            MagicHelper helpper = TakeAmulet(magic);
            switch (magic.Info.Magic)
            {
                case MagicType.Swordsmanship:
                    break;
                case MagicType.Thrusting:
                    if (CEnvir.Now < ToggleTime)
                        break;
                    ToggleTime = CEnvir.Now.AddSeconds(1.0);
                    CEnvir.Enqueue(new MagicToggle()
                    {
                        Magic = magic.Info.Magic,
                        CanUse = !User.CanThrusting
                    });
                    break;
                case MagicType.HalfMoon:
                    if (CEnvir.Now < ToggleTime)
                        break;
                    ToggleTime = CEnvir.Now.AddSeconds(1.0);
                    CEnvir.Enqueue(new MagicToggle()
                    {
                        Magic = magic.Info.Magic,
                        CanUse = !User.CanHalfMoon
                    });
                    break;
                case MagicType.FlamingSword:
                    if (CEnvir.Now < magic.NextCast || magic.Cost > User.CurrentMP)
                        break;
                    magic.NextCast = CEnvir.Now.AddSeconds(0.5);
                    CEnvir.Enqueue(new MagicToggle()
                    {
                        Magic = magic.Info.Magic
                    });
                    break;
                case MagicType.DragonRise:
                    if (CEnvir.Now < magic.NextCast || magic.Cost > User.CurrentMP)
                        break;
                    magic.NextCast = CEnvir.Now.AddSeconds(0.5);
                    if (CanAttackTarget(MagicObject))
                    {
                        MapObject magicObject = MagicObject;
                    }
                    if (CanAttackTarget(MouseObject))
                    {
                        MapObject mouseObject = MouseObject;
                        MapObject.MagicObject = MouseObject.Race != ObjectType.Monster || ((MonsterObject)MouseObject).MonsterInfo.AI < 0 ? (MouseObject.Race != ObjectType.Player ? (MapObject)null : mouseObject) : mouseObject;
                    }
                    CEnvir.Enqueue(new MagicToggle()
                    {
                        Magic = magic.Info.Magic
                    });
                    break;
                case MagicType.BladeStorm:
                case MagicType.DemonicRecovery:
                    if (CEnvir.Now < magic.NextCast || magic.Cost > User.CurrentMP)
                        break;
                    magic.NextCast = CEnvir.Now.AddSeconds(0.5);
                    CEnvir.Enqueue(new MagicToggle()
                    {
                        Magic = magic.Info.Magic
                    });
                    break;
                case MagicType.DestructiveSurge:
                    if (CEnvir.Now < ToggleTime)
                        break;
                    ToggleTime = CEnvir.Now.AddSeconds(1.0);
                    CEnvir.Enqueue(new MagicToggle()
                    {
                        Magic = magic.Info.Magic,
                        CanUse = !User.CanDestructiveBlow
                    });
                    break;
                case MagicType.Endurance:
                    if (CEnvir.Now < magic.NextCast || magic.Cost > User.CurrentMP)
                        break;
                    magic.NextCast = CEnvir.Now.AddSeconds(0.5);
                    CEnvir.Enqueue(new MagicToggle()
                    {
                        Magic = magic.Info.Magic
                    });
                    break;
                case MagicType.SpiritSword:
                    break;
                case MagicType.WillowDance:
                    break;
                case MagicType.VineTreeDance:
                    break;
                case MagicType.FullBloom:
                case MagicType.WhiteLotus:
                case MagicType.RedLotus:
                case MagicType.SweetBrier:
                    if (CEnvir.Now < ToggleTime || CEnvir.Now < magic.NextCast || User.AttackMagic == magic.Info.Magic)
                        break;
                    ReceiveChat(magic.Info.Name + " 准备就绪", MessageType.Hint);
                    int num1 = Math.Max(800, Globals.AttackDelay - MapObject.User.Stats[Stat.AttackSpeed] * Globals.ASpeedRate);
                    ToggleTime = CEnvir.Now + TimeSpan.FromMilliseconds((double)(num1 + 200));
                    User.AttackMagic = magic.Info.Magic;
                    break;
                case MagicType.Karma:
                    if (CEnvir.Now < ToggleTime || CEnvir.Now < magic.NextCast || User.Buffs.All((x => x.Type != BuffType.Cloak)) || User.AttackMagic == magic.Info.Magic)
                        break;
                    ReceiveChat(magic.Info.Name + " 准备就绪", MessageType.Hint);
                    ToggleTime = CEnvir.Now + TimeSpan.FromMilliseconds(500.0);
                    User.AttackMagic = magic.Info.Magic;
                    break;
                case MagicType.FlameSplash:
                    if (CEnvir.Now < ToggleTime)
                        break;
                    ToggleTime = CEnvir.Now.AddSeconds(1.0);
                    CEnvir.Enqueue(new MagicToggle()
                    {
                        Magic = magic.Info.Magic,
                        CanUse = !User.CanFlameSplash
                    });
                    break;
                default:
                    if (CEnvir.Now < User.NextMagicTime || User.Dead || (User.Buffs.Any((x =>
                    {
                        if (x.Type != BuffType.DragonRepulse)
                            return x.Type == BuffType.FrostBite;

                        return true;
                    })) || (User.Poison & PoisonType.Paralysis) == PoisonType.Paralysis) || (User.Poison & PoisonType.Silenced) == PoisonType.Silenced)
                        break;
                    if (CEnvir.Now < magic.NextCast)
                    {
                        if (!(CEnvir.Now >= OutputTime))
                            break;
                        OutputTime = CEnvir.Now.AddSeconds(1.0);
                        ReceiveChat("不能使用 " + magic.Info.Name + ", 该技能仍在冷却", MessageType.Hint);
                        break;
                    }
                    switch (magic.Info.Magic)
                    {
                        case MagicType.Heal:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 2 || User.Mingwen02 == 2 || User.Mingwen03 == 2)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.ExplosiveTalisman:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 15 || User.Mingwen02 == 15 || User.Mingwen03 == 15)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.EvilSlayer:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 16 || User.Mingwen02 == 16 || User.Mingwen03 == 16)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.LifeSteal:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 76 || User.Mingwen02 == 76 || User.Mingwen03 == 76)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.DemonicRecovery:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 79 || User.Mingwen02 == 79 || User.Mingwen03 == 79)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.Repulsion:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 99 || User.Mingwen02 == 99 || User.Mingwen03 == 99)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.Teleportation:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 104 || User.Mingwen02 == 104 || User.Mingwen03 == 104)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.GeoManipulation:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 125 || User.Mingwen02 == 125 || User.Mingwen03 == 125)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.JudgementOfHeaven:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 189 || User.Mingwen02 == 189 || User.Mingwen03 == 189)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.Interchange:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 171 || User.Mingwen02 == 171 || User.Mingwen03 == 171)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.Beckon:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 172 || User.Mingwen02 == 172 || User.Mingwen03 == 172)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.Might:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 174 || User.Mingwen02 == 174 || User.Mingwen03 == 174)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.MassBeckon:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 197 || User.Mingwen02 == 197 || User.Mingwen03 == 197)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.Fetter:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 200 || User.Mingwen02 == 200 || User.Mingwen03 == 200)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }
                            }
                            break;
                        case MagicType.HellFire:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 219 || User.Mingwen02 == 219 || User.Mingwen03 == 219)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.SummonPuppet:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 227 || User.Mingwen02 == 227 || User.Mingwen03 == 227)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.Abyss:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 248 || User.Mingwen02 == 248 || User.Mingwen03 == 248)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.Evasion:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 254 || User.Mingwen02 == 254 || User.Mingwen03 == 254)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.RagingWind:
                            if (magic.Cost > User.CurrentMP)
                            {
                                //if (User.Mingwen01 == 256 || User.Mingwen02 == 256 || User.Mingwen03 == 256)
                                //{
                                //    if (!(CEnvir.Now >= OutputTime))
                                //        return;
                                //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                //}
                                //else
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                    return;
                                }

                            }
                            break;
                        case MagicType.Cloak:
                            if (!User.VisibleBuffs.Contains(BuffType.Cloak))
                            {
                                if (CEnvir.Now < User.CombatTime.AddSeconds(10.0))
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("战斗中无法使用 " + magic.Info.Name + " ", MessageType.Hint);
                                    return;
                                }
                                if (User.Stats[Stat.Health] * magic.Cost / 1000 >= User.CurrentHP || User.CurrentHP < User.Stats[Stat.Health] / 10)
                                {
                                    if (!(CEnvir.Now >= OutputTime))
                                        return;
                                    OutputTime = CEnvir.Now.AddSeconds(1.0);
                                    ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的生命值", MessageType.Hint);
                                    return;
                                }
                                break;
                            }
                            break;
                        case MagicType.DarkConversion:
                            if (!User.VisibleBuffs.Contains(BuffType.DarkConversion) && magic.Cost > User.CurrentMP)
                            {
                                if (!(CEnvir.Now >= OutputTime))
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                return;
                            }
                            break;
                        case MagicType.DragonRepulse:
                            if (User.Stats[Stat.Health] * magic.Cost / 1000 >= User.CurrentHP || User.CurrentHP < User.Stats[Stat.Health] / 10)
                            {
                                if (!(CEnvir.Now >= OutputTime))
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的生命值", MessageType.Hint);
                                return;
                            }
                            if (User.Stats[Stat.Mana] * magic.Cost / 1000 >= User.CurrentMP || User.CurrentMP < User.Stats[Stat.Mana] / 10)
                            {
                                if (!(CEnvir.Now >= OutputTime))
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你没有足够的魔法值", MessageType.Hint);
                                return;
                            }
                            break;
                        //case MagicType.ElementalHurricane:
                        //    int cost = magic.Cost;
                        //    if (MapObject.User.VisibleBuffs.Contains(BuffType.ElementalHurricane))
                        //        cost = 0;

                        //    if (cost > User.CurrentMP)
                        //    {
                        //        if (CEnvir.Now >= OutputTime)
                        //        {
                        //            OutputTime = CEnvir.Now.AddSeconds(1);
                        //            ReceiveChat($"不能使用 {magic.Info.Name}, 你没有足够的魔法值.", MessageType.Hint);
                        //        }
                        //        return;
                        //    }
                        //    break;
                        //case MagicType.Concentration:
                        //    if (User.VisibleBuffs.Contains(BuffType.Concentration)) return;

                        //    if (magic.Cost > User.CurrentMP)
                        //    {
                        //        if (CEnvir.Now >= OutputTime)
                        //        {
                        //            OutputTime = CEnvir.Now.AddSeconds(1);
                        //            ReceiveChat($"不能使用 {magic.Info.Name}, 你没有足够的魔法值.", MessageType.Hint);
                        //        }
                        //        return;
                        //    }
                        //    break;
                        default:
                            if (magic.Cost > User.CurrentMP)
                            {
                                if (CEnvir.Now >= OutputTime)
                                {
                                    OutputTime = CEnvir.Now.AddSeconds(1);
                                    ReceiveChat($"不能使用 {magic.Info.Name}, 你没有足够的魔法值.", MessageType.Hint);
                                }
                                return;
                            }
                            break;
                    }
                    MirDirection direction = MapControl.MouseDirection();
                    switch (magic.Info.Magic)
                    {
                        case MagicType.ShoulderDash:
                            if (CEnvir.Now < User.ServerTime || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip)
                                return;
                            User.ServerTime = CEnvir.Now.AddSeconds(5.0);
                            User.NextMagicTime = CEnvir.Now + Globals.MagicDelay;
                            CEnvir.Enqueue(new Magic()
                            {
                                Direction = direction,
                                Action = MirAction.Spell,
                                Type = magic.Info.Magic
                            });
                            return;
                        case MagicType.FlamingSword:
                            return;
                        case MagicType.DragonRise:
                            return;
                        case MagicType.BladeStorm:
                            return;
                        case MagicType.DestructiveSurge:
                            return;
                        case MagicType.Interchange:
                        case MagicType.Beckon:
                            if (CanAttackTarget(MouseObject))
                            {
                                mapObject = MouseObject;
                                goto case MagicType.MassBeckon;
                            }
                            else
                                goto case MagicType.MassBeckon;
                        case MagicType.Defiance:
                        //case MagicType.Invincibility:
                        //case MagicType.Concentration:
                        //    direction = MirDirection.Down;
                        //    goto case MagicType.MassBeckon;
                        case MagicType.Might:
                            direction = MirDirection.Down;
                            goto case MagicType.MassBeckon;
                        case MagicType.SwiftBlade:
                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic);
                            if (mapObject != null && !Functions.InRange(mapObject.CurrentLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }
                            if (mapObject != null && mapObject != User)
                                direction = Functions.DirectionFromPoint(User.CurrentLocation, mapObject.CurrentLocation);

                            uint num2 = mapObject != null ? mapObject.ObjectID : 0U;
                            Point point = mapObject != null ? mapObject.CurrentLocation : MapControl.MapLocation;

                            if (MouseObject != null && MouseObject.Race == ObjectType.Monster)
                                FocusObject = MouseObject;

                            User.MagicAction = new ObjectAction(MirAction.Spell, direction, MapObject.User.CurrentLocation, new object[4]
                            {
                                 magic.Info.Magic,
                                 new List<uint>() { num2 },
                                 new List<Point>() { point },
                                 false
                            });
                            return;
                        case MagicType.FireWall:
                        case MagicType.GeoManipulation:
                            if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }
                            goto case MagicType.MassBeckon;
                        case MagicType.Assault:
                            return;
                        case MagicType.Endurance:
                            return;
                        case MagicType.ReflectDamage:
                            if (User.Buffs.Any<ClientBuffInfo>((Func<ClientBuffInfo, bool>)(x => x.Type == BuffType.ReflectDamage)))
                                return;
                            direction = MirDirection.Down;
                            goto case MagicType.MassBeckon;
                        case MagicType.Fetter:
                            direction = MirDirection.Down;
                            goto case MagicType.MassBeckon;
                        case MagicType.MassBeckon:
                        case MagicType.SeismicSlam:
                        case MagicType.Repulsion:
                        case MagicType.Teleportation:
                        case MagicType.ScortchedEarth:
                        case MagicType.LightningBeam:
                        case MagicType.FrozenEarth:
                        case MagicType.BlowEarth:
                        case MagicType.GreaterFrozenEarth:
                        case MagicType.Renounce:
                        case MagicType.JudgementOfHeaven:
                        case MagicType.ThunderStrike:
                        case MagicType.MirrorImage:
                        case MagicType.Invisibility:
                        case MagicType.StrengthOfFaith:
                        case MagicType.TheNewBeginning:
                        case MagicType.DarkConversion:
                        case MagicType.DragonRepulse:
                        case MagicType.Evasion:
                        case MagicType.RagingWind:
                            //if (mapObject != null && !Functions.InRange(mapObject.CurrentLocation, User.CurrentLocation, 10))
                            //{
                            //    if (CEnvir.Now < OutputTime)
                            //        return;
                            //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                            //    ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                            //    return;
                            //}
                            if (mapObject != null && mapObject != User)
                                direction = Functions.DirectionFromPoint(User.CurrentLocation, mapObject.CurrentLocation);

                            num2 = mapObject != null ? mapObject.ObjectID : 0U;
                            point = mapObject != null ? mapObject.CurrentLocation : MapControl.MapLocation;

                            if (MouseObject != null && MouseObject.Race == ObjectType.Monster)
                                FocusObject = MouseObject;

                            //Console.WriteLine($"触发 {magic.Info.Name}");

                            if (magic != null)
                            {
                                switch (magic.Info.Magic)
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
                                    case MagicType.ChainLightning:
                                    case MagicType.MeteorShower:
                                    case MagicType.Heal:
                                    case MagicType.BloodLust:
                                    case MagicType.MagicResistance:
                                    case MagicType.Resilience:
                                    case MagicType.ElementalSuperiority:
                                    case MagicType.ExplosiveTalisman:
                                    case MagicType.EvilSlayer:
                                    case MagicType.GreaterEvilSlayer:
                                    case MagicType.ImprovedExplosiveTalisman:
                                    case MagicType.PoisonDust:
                                    case MagicType.Asteroid:
                                        LastTarget = mapObject ?? MouseObject;
                                        break;
                                }
                            }

                            User.MagicAction = new ObjectAction(MirAction.Spell, direction, MapObject.User.CurrentLocation, new object[4]
                            {
                                 magic.Info.Magic,
                                 new List<uint>() { num2 },
                                 new List<Point>() { point },
                                 false
                            });
                            return;
                        case MagicType.FlashOfLight:
                            if (mapObject != null && mapObject != User)
                                direction = Functions.DirectionFromPoint(User.CurrentLocation, mapObject.CurrentLocation);

                            int dis = magic.Level >= 5 ? 3 : 2;
                            bool valid = false;

                            for(int i = 0; i < dis; i++)
                            {
                                var dst = Functions.Move(User.CurrentLocation, direction, i);
                                if (!MapControl.CanMove(direction, i)) continue;
                                if (MapControl.Cells[dst.X, dst.Y]?.Objects == null) continue;

                                foreach(var ob in MapControl.Cells[dst.X, dst.Y].Objects)
                                    if (ob is MonsterObject mon && CanAttackTarget(mon))
                                    {
                                        valid = true;
                                        break;
                                    }

                                if (valid) break;
                            }

                            if (!valid && TargetObject != null)
                            {
                                direction = Functions.DirectionFromPoint(User.CurrentLocation, TargetObject.CurrentLocation);
                                for (int i = 0; i < dis; i++)
                                {
                                    var dst = Functions.Move(User.CurrentLocation, direction, 1);
                                    if (!MapControl.CanMove(direction, 1)) continue;
                                    if (MapControl.Cells[dst.X, dst.Y]?.Objects == null) continue;

                                    foreach (var ob in MapControl.Cells[dst.X, dst.Y].Objects)
                                        if (ob is MonsterObject mon && CanAttackTarget(mon))
                                        {
                                            valid = true;
                                            mapObject = ob;
                                            break;
                                        }

                                    if (valid) break;
                                }
                            }

                            num2 = mapObject != null ? mapObject.ObjectID : 0U;
                            point = mapObject != null ? mapObject.CurrentLocation : MapControl.MapLocation;

                            if (MouseObject != null && MouseObject.Race == ObjectType.Monster)
                                FocusObject = MouseObject;

                            User.MagicAction = new ObjectAction(MirAction.Spell, direction, MapObject.User.CurrentLocation, new object[4]
                            {
                                 magic.Info.Magic,
                                 new List<uint>() { num2 },
                                 new List<Point>() { point },
                                 false
                            });
                            return;
                        case MagicType.FireBall:
                        case MagicType.LightningBall:
                        case MagicType.IceBolt:
                        case MagicType.GustBlast:
                        case MagicType.ElectricShock:
                        case MagicType.AdamantineFireBall:
                        case MagicType.ThunderBolt:
                        case MagicType.IceBlades:
                        case MagicType.Cyclone:
                        case MagicType.ExpelUndead:
                        case MagicType.FireStorm:
                        case MagicType.LightningWave:
                        case MagicType.IceStorm:
                        case MagicType.DragonTornado:
                        case MagicType.ChainLightning:
                        case MagicType.Purification:
                        case MagicType.Infection:
                            //case MagicType.Neutralize:

                            if (magic.Info.Magic == MagicType.Purification)
                                mapObject = MouseObject ?? (MapObject)User;
                            else
                                mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic);
                            
                            goto case MagicType.MassBeckon;
                        case MagicType.PoisonDust:
                        case MagicType.ImprovedExplosiveTalisman:
                        case MagicType.ExplosiveTalisman:
                        case MagicType.EvilSlayer:
                        case MagicType.GreaterEvilSlayer:
                            if (Config.自动换毒 && magic.Info.Magic == MagicType.PoisonDust)
                                AutoChangePoison(magic);

                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic);
                            goto case MagicType.MassBeckon;
                        case MagicType.MagicShield:
                            if (User.Buffs.Any(x => x.Type == BuffType.MagicShield))
                                return;
                            goto case MagicType.MassBeckon;
                        //case MagicType.SuperiorMagicShield:
                        //    if (User.Buffs.Any<ClientBuffInfo>((Func<ClientBuffInfo, bool>)(x => x.Type == BuffType.SuperiorMagicShield)))
                        //        return;
                        //    goto case MagicType.MassBeckon;
                        case MagicType.MeteorShower:
                        case MagicType.Tempest:
                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic);

                            if (mapObject != null && !Functions.InRange(mapObject.CurrentLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }

                            goto case MagicType.MassBeckon;
                        case MagicType.Asteroid:
                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic);
                            if (mapObject != null && !Functions.InRange(mapObject.CurrentLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }

                            goto case MagicType.MassBeckon;
                        case MagicType.RayOfLight:
                            return;
                        case MagicType.BurstOfEnergy:
                            return;
                        case MagicType.ShieldOfPreservation:
                            return;
                        case MagicType.RetrogressionOfEnergy:
                            return;
                        case MagicType.FuryBlast:
                            return;
                        case MagicType.TempestOfUnstableEnergy:
                            return;
                        case MagicType.AdvancedRenounce:
                            return;
                        case MagicType.FrostBite:
                            if (User.Buffs.Any<ClientBuffInfo>((Func<ClientBuffInfo, bool>)(x => x.Type == BuffType.FrostBite)))
                                return;
                            goto case MagicType.MassBeckon;
                        case MagicType.Heal:
                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic) ?? User;
                            goto case MagicType.MassBeckon;
                        case MagicType.SpiritSword:
                            return;
                        case MagicType.MagicResistance:
                        case MagicType.Resilience:
                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic) ?? User;
                            if (mapObject != null && !Functions.InRange(mapObject.CurrentLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }

                            goto case MagicType.MassBeckon;
                        case MagicType.MassInvisibility:
                            if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }
                            goto case MagicType.MassBeckon;
                        case MagicType.TrapOctagon:
                            if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }

                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic);
                            goto case MagicType.MassBeckon;
                        case MagicType.TaoistCombatKick:
                        case MagicType.ThunderKick:
                        //case MagicType.DarkSoulPrison:
                        //    if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, Globals.MagicRange))
                        //    {
                        //        if (CEnvir.Now < OutputTime) return;

                        //        OutputTime = CEnvir.Now.AddSeconds(1.0);
                        //        ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                        //        return;
                        //    }
                        //    goto case MagicType.MassBeckon;
                        case MagicType.ElementalSuperiority:
                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic) ?? User;
                            if (mapObject != null && !Functions.InRange(mapObject.CurrentLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }
                            goto case MagicType.MassBeckon;
                        case MagicType.MassHeal:
                            if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }

                            //mapObject = AutoRemoteTarget(mapObject, helpper) ?? User;

                            goto case MagicType.MassBeckon;
                        case MagicType.BloodLust:
                            mapObject = mapObject ?? AutoRemoteTarget(mapObject, helpper, magic.Info.Magic) ?? User;
                            if (mapObject != null && !Functions.InRange(mapObject.CurrentLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }

                            goto case MagicType.MassBeckon;
                        case MagicType.Resurrection:
                            if (MouseObject == null || !MouseObject.Dead || MouseObject.Race != ObjectType.Player)
                                return;
                            mapObject = MouseObject;
                            goto case MagicType.MassBeckon;
                        case MagicType.Transparency:
                            //if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, 10))
                            //{
                            //    if (CEnvir.Now < OutputTime)
                            //        return;
                            //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                            //    ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                            //    return;
                            //}
                            goto case MagicType.MassBeckon;
                        case MagicType.CelestialLight:
                            if (User.Buffs.All<ClientBuffInfo>((Func<ClientBuffInfo, bool>)(x => x.Type == BuffType.CelestialLight)))
                                return;
                            goto case MagicType.MassBeckon;
                        case MagicType.EmpoweredHealing:
                            return;
                        case MagicType.LifeSteal:
                            mapObject = null;

                            goto case MagicType.MassBeckon;
                        case MagicType.GreaterPoisonDust:
                            return;
                        case MagicType.Scarecrow:
                        case MagicType.PoisonousCloud:
                        case MagicType.Cloak:
                            //if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, 10))
                            //{
                            //    if (CEnvir.Now < OutputTime)
                            //        return;
                            //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                            //    ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                            //    return;
                            //}
                            goto case MagicType.MassBeckon;
                        case MagicType.DragonBreath:
                            return;
                        case MagicType.MassTransparency:
                            return;
                        case MagicType.GreaterHolyStrike:
                            return;
                        case MagicType.AugmentExplosiveTalisman:
                            return;
                        case MagicType.AugmentEvilSlayer:
                            return;
                        case MagicType.AugmentPurification:
                            return;
                        case MagicType.OathOfThePerished:
                            return;
                        case MagicType.SummonSkeleton:
                        case MagicType.SummonShinsu:
                        case MagicType.SummonJinSkeleton:
                        case MagicType.SummonDemonicCreature:
                        case MagicType.DemonExplosion:
                            //if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, 10))
                            //{
                            //    if (CEnvir.Now < OutputTime)
                            //        return;
                            //    OutputTime = CEnvir.Now.AddSeconds(1.0);
                            //    ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                            //    return;
                            //}
                            goto case MagicType.MassBeckon;
                        case MagicType.WraithGrip:
                        case MagicType.HellFire:
                        case MagicType.Abyss:
                            if (CanAttackTarget(MouseObject))
                            {
                                mapObject = MouseObject;
                                goto case MagicType.MassBeckon;
                            }
                            else
                                goto case MagicType.MassBeckon;
                        case MagicType.SummonPuppet:
                            if (!Functions.InRange(MapControl.MapLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }
                            goto case MagicType.MassBeckon;
                        case MagicType.DanceOfSwallow:
                            if (CEnvir.Now < User.ServerTime)
                                return;
                            if (CanAttackTarget(MouseObject))
                                mapObject = MouseObject;
                            if (mapObject == null)
                                return;
                            if (!Functions.InRange(mapObject.CurrentLocation, User.CurrentLocation, 10))
                            {
                                if (CEnvir.Now < OutputTime)
                                    return;
                                OutputTime = CEnvir.Now.AddSeconds(1.0);
                                ReceiveChat("不能使用 " + magic.Info.Name + ", 你的攻击目标太远了", MessageType.Hint);
                                return;
                            }
                            User.ServerTime = CEnvir.Now.AddSeconds(5.0);
                            User.NextMagicTime = CEnvir.Now + Globals.MagicDelay;
                            MapObject.TargetObject = mapObject;
                            MapObject.MagicObject = mapObject;
                            CEnvir.Enqueue(new Magic()
                            {
                                Action = MirAction.Spell,
                                Type = magic.Info.Magic,
                                Target = mapObject.ObjectID
                            });
                            return;
                        case MagicType.AdventOfDemon:
                            return;
                        case MagicType.AdventOfDevil:
                            return;
                        case MagicType.Stealth:
                            return;
                        default:
                            return;
                    }
            }
        }
        public void UseMagic(SpellKey key)
        {
            if (Game.Observer || User == null || User.Horse != HorseType.None || MagicBarBox == null) return;

            ClientUserMagic magic = null;

            foreach (KeyValuePair<MagicInfo, ClientUserMagic> pair in User.Magics)
            {

                switch (MagicBarBox.SpellSet)
                {
                    case 1:
                        if (pair.Value.Set1Key == key)
                            magic = pair.Value;
                        break;
                    case 2:
                        if (pair.Value.Set2Key == key)
                            magic = pair.Value;
                        break;
                    case 3:
                        if (pair.Value.Set3Key == key)
                            magic = pair.Value;
                        break;
                    case 4:
                        if (pair.Value.Set4Key == key)
                            magic = pair.Value;
                        break;
                }

                if (magic != null) break;
            }

            UseMagic(magic);

            if (magic != null)
            {
                switch(magic.Info.Magic)
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
                    case MagicType.ChainLightning:
                    case MagicType.MeteorShower:
                    case MagicType.Heal:
                    case MagicType.BloodLust:
                    case MagicType.MagicResistance:
                    case MagicType.Resilience:
                    case MagicType.ElementalSuperiority:
                    case MagicType.ExplosiveTalisman:
                    case MagicType.EvilSlayer:
                    case MagicType.GreaterEvilSlayer:

                    case MagicType.ImprovedExplosiveTalisman:
                    case MagicType.PoisonDust:
                    case MagicType.Asteroid:
                        LastMagic = magic.Info;
                        break;
                }
            }
        }
        public bool CanAttackTarget(MapObject ob)
        {
            if (ob == null || ob.Dead) return false;

            switch (ob.Race)
            {
                case ObjectType.Player:
                    switch (User.AttackMode)
                    {
                        case AttackMode.Peace: return false;
                        case AttackMode.Group:
                            if (ob.Name == User.Name) return false;
                            else
                            {
                                foreach (var mem in GroupBox.Members)
                                    if (mem.Name == ob.Name)
                                        return false;
                            }
                            break;
                        case AttackMode.Guild:
                            if (ob.Name == User.Name) return false;
                            else if (ob.Title == User.Title) return false;
                            break;
                        case AttackMode.WarRedBrown:
                            if (ob.Name == User.Name) return false;
                            else
                            {
                                if (ob.NameColour == Globals.RedNameColour
                                    || ob.NameColour == Globals.BrownNameColour
                                    || ob.NameColour != Color.Yellow)
                                    return true;

                                if (string.IsNullOrEmpty(ob.Title)) return false;

                                return GuildWars.Contains(ob.Title);       
                            }
                    }

                    return true;
                case ObjectType.Monster:
                    MonsterObject mob = (MonsterObject) ob;

                    if (mob.MonsterInfo.AI < 0) return false;

                    return true;
                default:
                    return false;
            }
        }
        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();

            int image = -1;
            Color color = Color.Empty;

            if (SelectedCell?.Item != null)
            {
                ItemInfo info = SelectedCell.Item.Info;

                if (info.Effect == ItemEffect.ItemPart)
                    info = Globals.ItemInfoList.Binding.First(x => x.Index == SelectedCell.Item.AddedStats[Stat.ItemIndex]);
                
                image = info.Image;
                color = SelectedCell.Item.Colour;
            }
            else if (GoldPickedUp)
                image = 124;

            MirLibrary library;

            if (image >= 0 && CEnvir.LibraryList.TryGetValue(LibraryFile.Inventory, out library))
            {


                Size imageSize = library.GetSize(image);
                Point p = new Point(CEnvir.MouseLocation.X - imageSize.Width/2, CEnvir.MouseLocation.Y - imageSize.Height/2);

                if (p.X + imageSize.Width >= Size.Width + Location.X)
                    p.X = Size.Width - imageSize.Width + Location.X;

                if (p.Y + imageSize.Height >= Size.Height + Location.Y)
                    p.Y = Size.Height - imageSize.Height + Location.Y;

                if (p.X < Location.X)
                    p.X = Location.X;

                if (p.Y <= Location.Y)
                    p.Y = Location.Y;


                library.Draw(image, p.X, p.Y, Color.White, false, 1f, ImageType.Image);

                if (color != Color.Empty)
                    library.Draw(image, p.X, p.Y, color, false, 1f, ImageType.Overlay);
            }

            if (ItemLabel != null && !ItemLabel.IsDisposed)
                ItemLabel.Draw();

            if (MagicLabel != null && !MagicLabel.IsDisposed)
                MagicLabel.Draw();

        }

        public void Displacement(MirDirection direction, Point location)
        {
            //if (MapObject.User.Direction == direction && MapObject.User.CurrentLocation == location) return;

            MapObject.User.ServerTime = DateTime.MinValue;
            MapObject.User.SetAction(new ObjectAction(MirAction.Standing, direction, location));
            MapObject.User.NextActionTime = CEnvir.Now.AddMilliseconds(300);
        }

        public void FillItems(List<ClientUserItem> items)
        {
            foreach (ClientUserItem item in items)
            {
                if (item.Slot >= Globals.EquipmentOffSet)
                {
                    CharacterBox.Grid[item.Slot - Globals.EquipmentOffSet].Item = item;
                    continue;
                }

                if (item.Slot >= 0 && item.Slot < InventoryBox.Grid.Grid.Length)
                    InventoryBox.Grid.Grid[item.Slot].Item = item;
            }
        }
        public void AddItems(List<ClientUserItem> items)
        {
            foreach (ClientUserItem item in items)
            {
                if (item.Info.Effect == ItemEffect.Experience) continue;
                if ((item.Flags & UserItemFlags.QuestItem) == UserItemFlags.QuestItem) continue;

                if (item.Info.Effect == ItemEffect.Gold)
                {
                    User.Gold += item.Count;
                    DXSoundManager.Play(SoundIndex.GoldGained);
                    continue;
                }

                bool handled = false;
                if (item.Info.StackSize > 1 && (item.Flags & UserItemFlags.Expirable) != UserItemFlags.Expirable)
                {
                    foreach (DXItemCell cell in InventoryBox.Grid.Grid)
                    {
                        if (cell.Item == null || cell.Item.Info != item.Info || cell.Item.Count >= cell.Item.Info.StackSize) continue;

                        if ((cell.Item.Flags & UserItemFlags.Expirable) == UserItemFlags.Expirable) continue;
                        if ((cell.Item.Flags & UserItemFlags.Bound) != (item.Flags & UserItemFlags.Bound)) continue;
                        if ((cell.Item.Flags & UserItemFlags.Worthless) != (item.Flags & UserItemFlags.Worthless)) continue;
                        if ((cell.Item.Flags & UserItemFlags.NonRefinable) != (item.Flags & UserItemFlags.NonRefinable)) continue;
                        if (!cell.Item.AddedStats.Compare(item.AddedStats)) continue;

                        if (cell.Item.Count + item.Count <= item.Info.StackSize)
                        {
                            cell.Item.Count += item.Count;
                            cell.RefreshItem();
                            handled = true;
                            break;
                        }

                        item.Count -= item.Info.StackSize - cell.Item.Count;
                        cell.Item.Count = item.Info.StackSize;
                        cell.RefreshItem();
                    }
                    if (handled) continue;
                }

                for (int i = 0; i < InventoryBox.Grid.Grid.Length; i++)
                {
                    if (InventoryBox.Grid.Grid[i].Item != null) continue;

                    InventoryBox.Grid.Grid[i].Item = item;
                    item.Slot = i;
                    break;
                }
            }
        }
        public void AddCompanionItems(List<ClientUserItem> items)
        {
            foreach (ClientUserItem item in items)
            {
                if (item.Info.Effect == ItemEffect.Experience) continue;
                if ((item.Flags & UserItemFlags.QuestItem) == UserItemFlags.QuestItem) continue;

                if (item.Info.Effect == ItemEffect.Gold)
                {
                    User.Gold += item.Count;
                    DXSoundManager.Play(SoundIndex.GoldGained);
                    continue;
                }

                bool handled = false;
                if (item.Info.StackSize > 1 && (item.Flags & UserItemFlags.Expirable) != UserItemFlags.Expirable)
                {
                    foreach (DXItemCell cell in CompanionBox.InventoryGrid.Grid)
                    {
                        if (cell.Item == null || cell.Item.Info != item.Info || cell.Item.Count >= cell.Item.Info.StackSize) continue;

                        if ((cell.Item.Flags & UserItemFlags.Expirable) == UserItemFlags.Expirable) continue;
                        if ((cell.Item.Flags & UserItemFlags.Bound) != (item.Flags & UserItemFlags.Bound)) continue;
                        if ((cell.Item.Flags & UserItemFlags.Worthless) != (item.Flags & UserItemFlags.Worthless)) continue;
                        if ((cell.Item.Flags & UserItemFlags.NonRefinable) != (item.Flags & UserItemFlags.NonRefinable)) continue;
                        if (!cell.Item.AddedStats.Compare(item.AddedStats)) continue;

                        if (cell.Item.Count + item.Count <= item.Info.StackSize)
                        {
                            cell.Item.Count += item.Count;
                            cell.RefreshItem();
                            handled = true;
                            break;
                        }

                        item.Count -= item.Info.StackSize - cell.Item.Count;
                        cell.Item.Count = item.Info.StackSize;
                        cell.RefreshItem();
                    }
                    if (handled) continue;
                }

                for (int i = 0; i < CompanionBox.InventoryGrid.Grid.Length; i++)
                {
                    if (CompanionBox.InventoryGrid.Grid[i].Item != null) continue;

                    CompanionBox.InventoryGrid.Grid[i].Item = item;
                    item.Slot = i;
                    break;
                }
            }
        }
        public bool CanUseItem(ClientUserItem item)
        {
            switch (User.Gender)
            {
                case MirGender.Male:
                    if (!item.Info.RequiredGender.HasFlag(RequiredGender.Male))
                        return false;
                    break;
                case MirGender.Female:
                    if (!item.Info.RequiredGender.HasFlag(RequiredGender.Female))
                        return false;
                    break;
            }

            switch (User.Class)
            {
                case MirClass.Warrior:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Warrior))
                        return false;
                    break;
                case MirClass.Wizard:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Wizard))
                        return false;
                    break;
                case MirClass.Taoist:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Taoist))
                        return false;
                    break;
                case MirClass.Assassin:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Assassin))
                        return false;
                    break;
            }
            switch (item.Info.RequiredType)
            {
                case RequiredType.Level:
                    if (User.Level < item.Info.RequiredAmount && User.Stats[Stat.Rebirth] == 0) return false;
                    break;
                case RequiredType.MaxLevel:
                    if (User.Level > item.Info.RequiredAmount || User.Stats[Stat.Rebirth] > 0) return false;
                    break;
                case RequiredType.AC:
                    if (User.Stats[Stat.MaxAC] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.MR:
                    if (User.Stats[Stat.MaxMR] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.DC:
                    if (User.Stats[Stat.MaxDC] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.MC:
                    if (User.Stats[Stat.MaxMC] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.SC:
                    if (User.Stats[Stat.MaxSC] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.Health:
                    if (User.Stats[Stat.Health] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.Mana:
                    if (User.Stats[Stat.Mana] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.Accuracy:
                    if (User.Stats[Stat.Accuracy] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.Agility:
                    if (User.Stats[Stat.Agility] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.CompanionLevel:
                    if (Companion == null || Companion.Level < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.MaxCompanionLevel:
                    if (Companion == null || Companion.Level > item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.RebirthLevel:
                    if (User.Stats[Stat.Rebirth] < item.Info.RequiredAmount) return false;
                    break;
                case RequiredType.MaxRebirthLevel:
                    if (User.Stats[Stat.Rebirth] > item.Info.RequiredAmount) return false;
                    break;
            }

            switch (item.Info.ItemType)
            {
                case ItemType.Book:
                    MagicInfo magic = Globals.MagicInfoList.Binding.FirstOrDefault(x => x.Index == item.Info.Shape);
                    if (magic == null) return false;
                    if (User.Magics.ContainsKey(magic) && (User.Magics[magic].Level < 3 || (item.Flags & UserItemFlags.NonRefinable) == UserItemFlags.NonRefinable)) return false;
                    if (User.Magics.ContainsKey(magic) && User.Magics[magic].Level >= CEnvir.SkillLevelLimit) return false;
                    break;
                case ItemType.Consumable:
                    switch (item.Info.Shape)
                    {
                        case 1: //Item Buffs

                            ClientBuffInfo buff = User.Buffs.FirstOrDefault(x => x.Type == BuffType.ItemBuff && x.ItemIndex == item.Info.Index);

                            if (buff != null && buff.RemainingTime == TimeSpan.MaxValue) return false;
                            break;
                    }
                    break;
            }

            return true;
        }

        public bool CanWearItem(ClientUserItem item, EquipmentSlot slot)
        {
            if (!CanUseItem(item)) return false;
            
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                case EquipmentSlot.Torch:
                case EquipmentSlot.Shield:
                    if (User.HandWeight - (Equipment[(int) slot]?.Info.Weight ?? 0) + item.Weight > User.Stats[Stat.HandWeight])
                    {
                        ReceiveChat($"无法装备 {item.Info.ItemName}, 太重了.", MessageType.System);
                        return false;
                    }
                    break;
                default:
                    if (User.WearWeight - (Equipment[(int) slot]?.Info.Weight ?? 0) + item.Weight > User.Stats[Stat.WearWeight])
                    {
                        ReceiveChat($"无法装备 {item.Info.ItemName}, 太重了.", MessageType.System);
                        return false;
                    }
                    break;
            }

            return true;
        }
        public bool CanCompanionWearItem(ClientUserItem item, CompanionSlot slot)
        {
            if (Companion == null) return false;
            if (!CanCompanionUseItem(item.Info)) return false;
            
            return true;
        }
        public bool CanCompanionUseItem(ItemInfo info)
        {
            switch (info.RequiredType)
            {
                case RequiredType.CompanionLevel:
                    if (Companion == null || Companion.Level < info.RequiredAmount) return false;
                    break;
                case RequiredType.MaxCompanionLevel:
                    if (Companion == null || Companion.Level > info.RequiredAmount) return false;
                    break;
            }


            return true;
        }

        public void UserChanged()
        {
            LevelChanged();
            ClassChanged();
            StatsChanged();
            ExperienceChanged();
            HealthChanged();
            ManaChanged();
            GoldChanged();
            SafeZoneChanged();
            AttackModeChanged();
            PetModeChanged();
            BigPatchBox.UserChanged();
            MagicBarBox.UpdateIcons();
            MarketPlaceBox.ConsignTab.TabButton.Visible = !Observer;
            TradeBox.CloseButton.Enabled = !Observer;
            TradeBox.ConfirmButton.Visible = !Observer;

            NPCBox.CloseButton.Enabled = !Observer;
            NPCGoodsBox.CloseButton.Enabled = !Observer;
            NPCRefineBox.CloseButton.Enabled = !Observer;
            NPCRepairBox.CloseButton.Enabled = !Observer;
            NPCSellBox.CloseButton.Enabled = !Observer;
            NPCRefineRetrieveBox.CloseButton.Enabled = !Observer;

            Game.AutoGuajiChanged();
        }
        public void LevelChanged()
        {
            if (User == null) return;

            User.MaxExperience = User.Level < Globals.ExperienceList.Count ? Globals.ExperienceList[User.Level] : 0;
            MainPanel.LevelLabel.Text = User.Level.ToString();

            foreach (NPCGoodsCell cell in NPCGoodsBox.Cells)
                cell.UpdateColours();

            foreach (KeyValuePair<MagicInfo, MagicCell> pair in MagicBox.Magics)
                pair.Value.Refresh();

            CheckNewQuests();
        }
        public void ClassChanged()
        {
            if (User == null) return;

            Type type = User.Class.GetType();
            MemberInfo[] infos = type.GetMember(User.Class.ToString());
            DescriptionAttribute description = infos[0].GetCustomAttribute<DescriptionAttribute>();

            MainPanel.ClassLabel.Text = description?.Description ?? "";

            foreach (NPCGoodsCell cell in NPCGoodsBox.Cells)
                cell.UpdateColours();

            MainPanel.MCLabel.Visible = User.Class != MirClass.Taoist;
            MainPanel.SCLabel.Visible = User.Class == MirClass.Taoist;
            
            MagicBox?.CreateTabs();
        }
        public void StatsChanged()
        {
            if (User.Stats == null) return;

            User.Light = Math.Max(3, User.Stats[Stat.Light]);
            
            MainPanel.ACLabel.Text = User.Stats.GetFormat(Stat.MaxAC);
            MainPanel.MRLabel.Text = User.Stats.GetFormat(Stat.MaxMR);
            MainPanel.DCLabel.Text = User.Stats.GetFormat(Stat.MaxDC);

            MainPanel.MCLabel.Text = User.Stats.GetFormat(Stat.MaxMC);
            MainPanel.SCLabel.Text = User.Stats.GetFormat(Stat.MaxSC);

            MainPanel.AccuracyLabel.Text = User.Stats[Stat.Accuracy].ToString();
            MainPanel.AgilityLabel.Text = User.Stats[Stat.Agility].ToString();

            HealthChanged();
            ManaChanged();

            foreach (NPCGoodsCell cell in NPCGoodsBox.Cells)
                cell.UpdateColours();

            CharacterBox.UpdateStats();
            
            // Update auto-potion thresholds ONLY when max HP/MP actually changes
            // This prevents excessive network traffic
            int currentMaxHP = User.Stats[Stat.Health];
            int currentMaxMP = User.Stats[Stat.Mana];
            
            if (currentMaxHP != _LastMaxHP || currentMaxMP != _LastMaxMP)
            {
                _LastMaxHP = currentMaxHP;
                _LastMaxMP = currentMaxMP;
                BigPatchBox?.Protect?.ResendAllAutoPotionLinks();
            }
        }
        public void ExperienceChanged()
        {
            if (User == null) return;

            MainPanel.ExperienceLabel.Text = User.MaxExperience > 0 ? $"{User.Experience/User.MaxExperience: #,##0.00%}" : $"{User.Experience: #,##0#}";

            MainPanel.ExperienceBar.Hint = $"{User.Experience:#,##0.#}/{User.MaxExperience: #,##0.#}";
        }
        public void HealthChanged()
        {
            if (User == null) return;

            MainPanel.HealthLabel.Text = $"{User.CurrentHP}/{User.Stats[Stat.Health]}";

        }
        public void ManaChanged()
        {
            if (User == null) return;

            MainPanel.ManaLabel.Text = $"{User.CurrentMP}/{User.Stats[Stat.Mana]}";
        }
        public void AttackModeChanged()
        {
            if (User == null) return;

            Type type = typeof(AttackMode);

            MemberInfo[] infos = type.GetMember(User.AttackMode.ToString());

            DescriptionAttribute description = infos[0].GetCustomAttribute<DescriptionAttribute>();

            MainPanel.AttackModeLabel.Text = description?.Description ?? User.AttackMode.ToString();
        }


        public void ShowKeyHints()
        {
            string tip_pot = GetKeyActionDesc(KeyBindAction.AutoPotionWindow);
            string tip_att = GetKeyActionDesc(KeyBindAction.ChangeAttackMode);
            string tip_pet = GetKeyActionDesc(KeyBindAction.ChangePetMode);

            if (!string.IsNullOrEmpty(tip_pot))
                ReceiveChat($"{tip_pot} 打开大补帖辅助工具；", MessageType.Hint);

            if (!string.IsNullOrEmpty(tip_att))
                ReceiveChat($"{tip_att} 切换攻击模式；", MessageType.Hint);

            if (!string.IsNullOrEmpty(tip_pet))
                ReceiveChat($"{tip_pet} 切换宠物攻击模式；", MessageType.Hint);
        }
        private string GetKeyActionDesc(KeyBindAction action)
        {
            KeyBindInfo key = CEnvir.GetKeyBind(action);
            return GetKeyDesc(key);
        }
        private string GetKeyDesc(KeyBindInfo key)
        {
            if (key == null || (key.Key1 == Keys.None && key.Key2 == Keys.None) ) return null;

            string desc = key.Control1 || key.Control2 ? "Ctrl + " : "";
            desc += (key.Alt1 || key.Alt2 ? " Alt + " : "");
            desc += (key.Shift1 || key.Shift2 ? " Shift + " : "");
            if (key.Key1 != Keys.None)
                desc += key.Key1.ToString();
            else
                desc += key.Key2.ToString();

            return desc;
        }
        public void UpdatePetModeTips()
        {
            KeyBindInfo key = CEnvir.GetKeyBind(KeyBindAction.ChangePetMode);

            if (key == LastPetModeKey) return;

            LastPetModeKey = key;

            if (key.Key1 == Keys.None && key.Key2 == Keys.None)
            {
                MainPanel.PetModeLabel.Hint = "\n请去设置窗口游戏页配置切换宠物攻击模式的快捷键\n";
            }
            else
            {
                string desc = key.Control1 || key.Control2 ? "Ctrl + " : "";
                desc += (key.Alt1 || key.Alt2 ? " Alt + " : "");
                desc += (key.Shift1 || key.Shift2 ? " Shift + " : "");
                if (key.Key1 != Keys.None)
                    desc += key.Key1.ToString();
                else
                    desc += key.Key2.ToString();

                MainPanel.PetModeLabel.Hint = $"\n{desc} 切换宠物攻击模式\n";
            }
        }
        public void UpdateAttackModeTips()
        {
            KeyBindInfo key = CEnvir.GetKeyBind(KeyBindAction.ChangeAttackMode);

            if (key == LastAttackModeKey) return;

            LastAttackModeKey = key;

            if (key.Key1 == Keys.None && key.Key2 == Keys.None)
            {
                MainPanel.AttackModeLabel.Hint = "\n请去设置窗口游戏页配置切换攻击模式的快捷键\n";
            }
            else
            {
                string desc = key.Control1 || key.Control2 ? "Ctrl + " : "";
                desc += (key.Alt1 || key.Alt2 ? " Alt + " : "");
                desc += (key.Shift1 || key.Shift2 ? " Shift + " : "");
                if (key.Key1 != Keys.None)
                    desc += key.Key1.ToString();
                else
                    desc += key.Key2.ToString();

                MainPanel.AttackModeLabel.Hint = $"\n{desc} 切换攻击模式\n";
            }
        }
        public void PetModeChanged()
        {
            if (User == null) return;

            Type type = typeof(PetMode);

            MemberInfo[] infos = type.GetMember(User.PetMode.ToString());

            DescriptionAttribute description = infos[0].GetCustomAttribute<DescriptionAttribute>();

            MainPanel.PetModeLabel.Text = description?.Description ?? User.PetMode.ToString();
        }
        public void GoldChanged()
        {
            if (User == null) return;

            InventoryBox.GoldLabel.Text = User.Gold.ToString("#,##0");
            MarketPlaceBox.GameGoldBox.Value = User.GameGold;
            MarketPlaceBox.HuntGoldBox.Value = User.HuntGold;
            NPCAdoptCompanionBox.RefreshUnlockButton();


            foreach (NPCGoodsCell cell in NPCGoodsBox.Cells)
                cell.UpdateColours();
        }
        public void SafeZoneChanged()
        {

        }
        public void WeightChanged()
        {
            if (User == null) return;

            InventoryBox.WeightLabel.Text = $"{User.BagWeight} of {User.Stats[Stat.BagWeight]}";

            InventoryBox.WeightLabel.ForeColour = User.BagWeight > User.Stats[Stat.BagWeight] ? Color.Red : Color.White;


            CharacterBox.WearWeightLabel.Text = $"{User.WearWeight}/{User.Stats[Stat.WearWeight]}";
            CharacterBox.HandWeightLabel.Text = $"{User.HandWeight}/{User.Stats[Stat.HandWeight]}";

            CharacterBox.WearWeightLabel.ForeColour = User.WearWeight > User.Stats[Stat.WearWeight] ? Color.Red : Color.White;
            CharacterBox.HandWeightLabel.ForeColour = User.HandWeight > User.Stats[Stat.HandWeight] ? Color.Red : Color.White;
        }
        public void CompanionChanged()
        {
            NPCCompanionStorageBox.UpdateScrollBar();

            CompanionBox.CompanionChanged();
        }
        public void MarriageChanged()
        {
            CharacterBox.MarriageIcon.Visible = !string.IsNullOrEmpty(Partner?.Name);
            CharacterBox.MarriageIcon.Hint = Partner?.Name;
        }

        public void ReceiveChat(string message, MessageType type)
        {
            if (Config.LogChat)
                CEnvir.ChatLog.Enqueue($"[{Time.Now:F}]: {message}");

            foreach (ChatTab tab in ChatTab.Tabs)
                tab.ReceiveChat(message, type);

            if (Config.自动回复)
                BigPatchBox?.ReceiveChat(message, type);
        }
        public void ReceiveChat(MessageAction action, params object[] args)
        {
            foreach (ChatTab tab in ChatTab.Tabs)
                tab.ReceiveChat(action, args);
        }

        public bool CanAccept(QuestInfo quest)
        {
            if (QuestLog.Any(x => x.Quest == quest)) return false;

            foreach (QuestRequirement requirement in quest.Requirements)
            {
                switch (requirement.Requirement)
                {
                    case QuestRequirementType.MinLevel:
                        if (User.Level < requirement.IntParameter1) return false;
                        break;
                    case QuestRequirementType.MaxLevel:
                        if (User.Level > requirement.IntParameter1) return false;
                        break;
                    case QuestRequirementType.NotAccepted:
                        if (QuestLog.Any(x => x.Quest == requirement.QuestParameter)) return false;

                        break;
                    case QuestRequirementType.HaveCompleted:
                        if (QuestLog.Any(x => x.Quest == requirement.QuestParameter && x.Completed)) break;

                        return false;
                    case QuestRequirementType.HaveNotCompleted:
                        if (QuestLog.Any(x => x.Quest == requirement.QuestParameter && x.Completed)) return false;

                        break;
                    case QuestRequirementType.Class:
                        switch (User.Class)
                        {
                            case MirClass.Warrior:
                                if ((requirement.Class & RequiredClass.Warrior) != RequiredClass.Warrior) return false;

                                break;
                            case MirClass.Wizard:
                                if ((requirement.Class & RequiredClass.Wizard) != RequiredClass.Wizard) return false;
                                break;
                            case MirClass.Taoist:
                                if ((requirement.Class & RequiredClass.Taoist) != RequiredClass.Taoist) return false;
                                break;
                            case MirClass.Assassin:
                                if ((requirement.Class & RequiredClass.Assassin) != RequiredClass.Assassin) return false;
                                break;
                        }
                        break;
                }

            }
            return true;
        }
        public void QuestChanged(ClientUserQuest quest)
        {
            CheckNewQuests();

            QuestBox.QuestChanged(quest);
        }
        public void CheckNewQuests()
        {
            QuestBox.PopulateQuests();

            QuestTrackerBox.PopulateQuests();
            
            NPCQuestBox.UpdateQuestDisplay();

            UpdateQuestIcons();
        }

        public bool HasQuest(MonsterInfo info, MapInfo map)
        {
            foreach (QuestTaskMonsterDetails detail in info.QuestDetails)
            {
                if (detail.Map != null && detail.Map != map) continue;

                QuestInfo quest = QuestBox.CurrentTab.Quests.FirstOrDefault(x => x == detail.Task.Quest);

                if (quest == null) continue;

                ClientUserQuest userQuest = QuestLog.First(x => x.Quest == quest);

                if (userQuest.IsComplete) continue;

                ClientUserQuestTask UserTask = userQuest.Tasks.FirstOrDefault(x => x.Task == detail.Task);

                if (UserTask != null && UserTask.Completed) continue;

                return true;
            }

            return false;
        }

        public string GetQuestText(QuestInfo questInfo, ClientUserQuest userQuest, bool isLog)
        {
            string text;

            if (userQuest == null)
                text = questInfo.AcceptText; //Available
            else if (userQuest.Completed)
                text = questInfo.ArchiveText; //Completed
            else if (userQuest.IsComplete && !isLog)
                text = questInfo.CompletedText; //Completed
            else
                text = questInfo.ProgressText; //Current


            text = text.Replace("[PLAYERNAME]", User.Name);
            text = text.Replace("[STARTNAME]", questInfo.StartNPC.NPCName);
            text = text.Replace("[FINISHNAME]", questInfo.FinishNPC.NPCName);

            return text;

        }

        public string GetTaskText(QuestInfo questInfo, ClientUserQuest userQuest)
        {
            StringBuilder builder = new StringBuilder();

            foreach (QuestTask task in questInfo.Tasks)
                builder.AppendLine(GetTaskText(task, userQuest));

            return builder.ToString(); //Available
        }
        public string GetTaskText(QuestTask task, ClientUserQuest userQuest)
        {
            StringBuilder builder = new StringBuilder();

            ClientUserQuestTask userTask = userQuest?.Tasks.FirstOrDefault(x => x.Task == task);

            switch (task.Task)
            {
                case QuestTaskType.KillMonster:
                    builder.AppendFormat("杀死 {0} ", task.Amount);
                    break;
                case QuestTaskType.GainItem:
                    builder.AppendFormat("收集 {0} {1} 从 ", task.Amount, task.ItemParameter?.ItemName);
                    
                    break;
            }

            if (string.IsNullOrEmpty(task.MobDescription))
            {
                bool needComma = false;
                for (int i = 0; i < task.MonsterDetails.Count; i++)
                {
                    QuestTaskMonsterDetails monster = task.MonsterDetails[i];
                    if (monster == null) continue;
                    if (i > 2)
                    {
                        builder.Append("...");
                        break;
                    }

                    if (needComma)
                        builder.Append(" 或 ");

                    needComma = true;

                    builder.Append(monster.Monster.MonsterName);

                    if (monster.Map != null)
                        builder.AppendFormat(" 在 {0}", monster.Map.Description);
                }
            }
            else
                builder.Append(task.MobDescription);

            if (userQuest != null)
            {
                if (userTask != null && userTask.Completed)
                    builder.Append(" (完成)");
                else
                    builder.Append($" ({userTask?.Amount ?? 0}/{task.Amount})");
            }

            return builder.ToString();
        }

        public void UpdateQuestIcons()
        {
            foreach (NPCInfo info in Globals.NPCInfoList.Binding)
                info.CurrentIcon = QuestIcon.None;

            foreach (QuestInfo quest in QuestBox.AvailableTab.Quests)
                quest.StartNPC.CurrentIcon |= QuestIcon.NewQuest;


            bool completed = false;

            foreach (QuestInfo quest in QuestBox.CurrentTab.Quests)
            {
                ClientUserQuest userQuest = QuestLog.First(x => x.Quest == quest);

                if (userQuest.IsComplete)
                {
                    quest.FinishNPC.CurrentIcon |= QuestIcon.QuestComplete;
                    completed = true;
                }
                else 
                    quest.FinishNPC.CurrentIcon |= QuestIcon.QuestIncomplete;
            }

            MainPanel.AvailableQuestIcon.Visible = QuestBox.AvailableTab.Quests.Count > 0;
            MainPanel.CompletedQuestIcon.Visible = completed;

            foreach (NPCInfo info in Globals.NPCInfoList.Binding)
            {
                BigMapBox.Update(info);
                MiniMapBox.Update(info);
            }

            foreach (MapObject ob in MapControl.Objects)
                ob.UpdateQuests();

            foreach (ClientObjectData data in DataDictionary.Values)
            {
                BigMapBox.Update(data);
                MiniMapBox.Update(data);
            }

        }
        public DXControl GetNPCControl(NPCInfo NPC)
        {
            int icon = 0;
            Color colour = Color.White;


            if ((NPC.CurrentIcon & QuestIcon.QuestComplete) == QuestIcon.QuestComplete)
            {
                icon = 98;
                colour = Color.Yellow;

            }
            else if ((NPC.CurrentIcon & QuestIcon.NewQuest) == QuestIcon.NewQuest)
            {
                icon = 97;
                colour = Color.Yellow;
            }
            else if ((NPC.CurrentIcon & QuestIcon.QuestIncomplete) == QuestIcon.QuestIncomplete)
            {
                icon = 98;
                colour = Color.White;
            }

            if (icon > 0)
            {
                DXImageControl image = new DXImageControl
                {
                    LibraryFile = LibraryFile.Interface,
                    Index = icon,
                    ForeColour = colour,
                    Hint = NPC.NPCName,
                    Tag = NPC.CurrentIcon,
                };
                image.OpacityChanged += (o, e) => image.ImageOpacity = image.Opacity;

                return image;
            }

            return new DXControl
            {
                Size = new Size(3, 3),
                DrawTexture = true,
                Hint = NPC.NPCName,
                BackColour = Color.Lime,
                Tag = NPC.CurrentIcon,
            };
        }

        public bool IsAlly(uint objectID)
        {
            if (User.ObjectID == objectID) return true;

            if (Partner != null && Partner.ObjectID == objectID) return true;

            foreach (ClientPlayerInfo member in GroupBox.Members)
                if (member.ObjectID == objectID) return true;

            if (GuildBox.GuildInfo != null)
            foreach (ClientGuildMemberInfo member in GuildBox.GuildInfo.Members)
                if (member.ObjectID == objectID) return true;

            return false;
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (Game == this) Game = null;

                _SelectedCell = null;

                _User = null;
                _MouseItem = null;
                _MouseMagic = null;

                GoldPickedUp = false;

                MagicObject = null;
                MouseObject = null;
                TargetObject = null;
                FocusObject = null;

                if (ItemLabel != null)
                {
                    if (!ItemLabel.IsDisposed)
                        ItemLabel.Dispose();

                    ItemLabel = null;
                }

                if (MagicLabel != null)
                {
                    if (!MagicLabel.IsDisposed)
                        MagicLabel.Dispose();

                    MagicLabel = null;
                }
                

                if (MapControl != null)
                {
                    if (!MapControl.IsDisposed)
                        MapControl.Dispose();

                    MapControl = null;
                }

                if (MainPanel != null)
                {
                    if (!MainPanel.IsDisposed)
                        MainPanel.Dispose();

                    MainPanel = null;
                }

                if (ConfigBox != null)
                {
                    if (!ConfigBox.IsDisposed)
                        ConfigBox.Dispose();

                    ConfigBox = null;
                }

                if (InventoryBox != null)
                {
                    if (!InventoryBox.IsDisposed)
                        InventoryBox.Dispose();

                    InventoryBox = null;
                }

                if (CharacterBox != null)
                {
                    if (!CharacterBox.IsDisposed)
                        CharacterBox.Dispose();

                    CharacterBox = null;
                }

                if (ExitBox != null)
                {
                    if (!ExitBox.IsDisposed)
                        ExitBox.Dispose();

                    ExitBox = null;
                }

                if (ChatTextBox != null)
                {
                    if (!ChatTextBox.IsDisposed)
                        ChatTextBox.Dispose();

                    ChatTextBox = null;
                }

                if (BeltBox != null)
                {
                    if (!BeltBox.IsDisposed)
                        BeltBox.Dispose();

                    BeltBox = null;
                }

                if (ChatOptionsBox != null)
                {
                    if (!ChatOptionsBox.IsDisposed)
                        ChatOptionsBox.Dispose();

                    ChatOptionsBox = null;
                }

                if (NPCBox != null)
                {
                    if (!NPCBox.IsDisposed)
                        NPCBox.Dispose();

                    NPCBox = null;
                }

                if (NPCGoodsBox != null)
                {
                    if (!NPCGoodsBox.IsDisposed)
                        NPCGoodsBox.Dispose();

                    NPCGoodsBox = null;
                }

                if (NPCSellBox != null)
                {
                    if (!NPCSellBox.IsDisposed)
                        NPCSellBox.Dispose();

                    NPCSellBox = null;
                }

                if (NPCRefinementStoneBox != null)
                {
                    if (!NPCRefinementStoneBox.IsDisposed)
                        NPCRefinementStoneBox.Dispose();

                    NPCRefinementStoneBox = null;
                }

                if (NPCRepairBox != null)
                {
                    if (!NPCRepairBox.IsDisposed)
                        NPCRepairBox.Dispose();

                    NPCRepairBox = null;
                }

                if (NPCRefineBox != null)
                {
                    if (!NPCRefineBox.IsDisposed)
                        NPCRefineBox.Dispose();

                    NPCRefineBox = null;
                }

                if (NPCRefineRetrieveBox != null)
                {
                    if (!NPCRefineRetrieveBox.IsDisposed)
                        NPCRefineRetrieveBox.Dispose();

                    NPCRefineRetrieveBox = null;
                }
                if (NPCMasterRefineBox != null)
                {
                    if (!NPCMasterRefineBox.IsDisposed)
                        NPCMasterRefineBox.Dispose();

                    NPCMasterRefineBox = null;
                }


                if (NPCQuestBox != null)
                {
                    if (!NPCQuestBox.IsDisposed)
                        NPCQuestBox.Dispose();

                    NPCQuestBox = null;
                }

                if (NPCAdoptCompanionBox != null)
                {
                    if (!NPCAdoptCompanionBox.IsDisposed)
                        NPCAdoptCompanionBox.Dispose();

                    NPCAdoptCompanionBox = null;
                }

                if (NPCCompanionStorageBox != null)
                {
                    if (!NPCCompanionStorageBox.IsDisposed)
                        NPCCompanionStorageBox.Dispose();

                    NPCCompanionStorageBox = null;
                }

                if (NPCWeddingRingBox != null)
                {
                    if (!NPCWeddingRingBox.IsDisposed)
                        NPCWeddingRingBox.Dispose();

                    NPCWeddingRingBox = null;
                }

                if (NPCItemFragmentBox != null)
                {
                    if (!NPCItemFragmentBox.IsDisposed)
                        NPCItemFragmentBox.Dispose();

                    NPCItemFragmentBox = null;
                }
                if (NPCAccessoryUpgradeBox != null)
                {
                    if (!NPCAccessoryUpgradeBox.IsDisposed)
                        NPCAccessoryUpgradeBox.Dispose();

                    NPCAccessoryUpgradeBox = null;
                }
                if (NPCAccessoryLevelBox != null)
                {
                    if (!NPCAccessoryLevelBox.IsDisposed)
                        NPCAccessoryLevelBox.Dispose();

                    NPCAccessoryLevelBox = null;
                }
                if (NPCAccessoryResetBox != null)
                {
                    if (!NPCAccessoryResetBox.IsDisposed)
                        NPCAccessoryResetBox.Dispose();

                    NPCAccessoryResetBox = null;
                }



                if (MiniMapBox != null)
                {
                    if (!MiniMapBox.IsDisposed)
                        MiniMapBox.Dispose();

                    MiniMapBox = null;
                }

                if (BigMapBox != null)
                {
                    if (!BigMapBox.IsDisposed)
                        BigMapBox.Dispose();

                    BigMapBox = null;
                }

                if (MagicBox != null)
                {
                    if (!MagicBox.IsDisposed)
                        MagicBox.Dispose();

                    MagicBox = null;
                }

                if (GroupBox != null)
                {
                    if (!GroupBox.IsDisposed)
                        GroupBox.Dispose();

                    GroupBox = null;
                }

                if (BuffBox != null)
                {
                    if (!BuffBox.IsDisposed)
                        BuffBox.Dispose();

                    BuffBox = null;
                }

                if (BigPatchBox != null)
                {
                    if (!BigPatchBox.IsDisposed)
                        BigPatchBox.Dispose();

                    BigPatchBox = null;
                }

                if (StorageBox != null)
                {
                    if (!StorageBox.IsDisposed)
                        StorageBox.Dispose();

                    StorageBox = null;
                }

                if (InspectBox != null)
                {
                    if (!InspectBox.IsDisposed)
                        InspectBox.Dispose();

                    InspectBox = null;
                }

                if (RankingBox != null)
                {
                    if (!RankingBox.IsDisposed)
                        RankingBox.Dispose();

                    RankingBox = null;
                }

                if (MarketPlaceBox != null)
                {
                    if (!MarketPlaceBox.IsDisposed)
                        MarketPlaceBox.Dispose();

                    MarketPlaceBox = null;
                }

                if (MailBox != null)
                {
                    if (!MailBox.IsDisposed)
                        MailBox.Dispose();

                    MailBox = null;
                }

                if (ReadMailBox != null)
                {
                    if (!ReadMailBox.IsDisposed)
                        ReadMailBox.Dispose();

                    ReadMailBox = null;
                }

                if (SendMailBox != null)
                {
                    if (!SendMailBox.IsDisposed)
                        SendMailBox.Dispose();

                    SendMailBox = null;
                }

                if (TradeBox != null)
                {
                    if (!TradeBox.IsDisposed)
                        TradeBox.Dispose();

                    TradeBox = null;
                }

                if (GuildBox != null)
                {
                    if (!GuildBox.IsDisposed)
                        GuildBox.Dispose();

                    GuildBox = null;
                }

                if (GuildMemberBox != null)
                {
                    if (!GuildMemberBox.IsDisposed)
                        GuildMemberBox.Dispose();

                    GuildMemberBox = null;
                }

                if (QuestBox != null)
                {
                    if (!QuestBox.IsDisposed)
                        QuestBox.Dispose();

                    QuestBox = null;
                }

                if (QuestTrackerBox != null)
                {
                    if (!QuestTrackerBox.IsDisposed)
                        QuestTrackerBox.Dispose();

                    QuestTrackerBox = null;
                }

                if (CompanionBox != null)
                {
                    if (!CompanionBox.IsDisposed)
                        CompanionBox.Dispose();

                    CompanionBox = null;
                }

                if (BlockBox != null)
                {
                    if (!BlockBox.IsDisposed)
                        BlockBox.Dispose();

                    BlockBox = null;
                }
                
                if (MonsterBox != null)
                {
                    if (!MonsterBox.IsDisposed)
                        MonsterBox.Dispose();

                    MonsterBox = null;
                }

                if (MagicBarBox != null)
                {
                    if (!MagicBarBox.IsDisposed)
                        MagicBarBox.Dispose();

                    MagicBarBox = null;
                }

                Inventory = null;
                Equipment = null;
                QuestLog = null;

                DataDictionary.Clear();
                DataDictionary = null;

                MoveFrame = false;
                MoveTime = DateTime.MinValue;
                OutputTime = DateTime.MinValue;
                ItemRefreshTime = DateTime.MinValue;

                CanRun = false;
                AutoRun = false;
                _NPCID = 0;
                _Companion = null;
                _Partner = null;

                PickUpTime = DateTime.MinValue;
                UseItemTime = DateTime.MinValue;
                NPCTime = DateTime.MinValue;
                ToggleTime = DateTime.MinValue;
                InspectTime = DateTime.MinValue;
                ItemTime = DateTime.MinValue;
                ItemReviveTime = DateTime.MinValue;
                
                _DayTime = 0f;
            }
        }

        #endregion

        public void AutoTimeChanged()
        {
            if (User == null) return;

            BigPatchBox.OnTimerChanged(User.AutoTime);
        }
        public void AutoZidongGuajiChanged()
        {
            if (User == null)
                return;

            //if (Game.User.Zdgjgongneng)
            //{
            //    if (BigPatchBox.Helper.AndroidPlayer.Checked)
            //        BigPatchBox.Helper.AndroidPlayer.Checked = false;
            //    if (BigPatchBox.Helper.AndroidPlayer.Visible)
            //        BigPatchBox.Helper.AndroidPlayer.Visible = false;
            //}
            //else
            {
                if (MapControl.MapInfo.AllowRT)
                    if (!BigPatchBox.Helper.AndroidPlayer.Visible)
                        BigPatchBox.Helper.AndroidPlayer.Visible = true;
            }
        }
        public void AutoGuajiChanged()
        {
            if (User == null)
                return;

            if (false)//if (!MapControl.MapInfo.AllowGuaji)
            {
                //if (BigPatchBox.Helper.AndroidPlayer.Checked)
                //    BigPatchBox.Helper.AndroidPlayer.Checked = false;
                //if (BigPatchBox.Helper.AndroidPlayer.Visible)
                //    BigPatchBox.Helper.AndroidPlayer.Visible = false;

            }
            else
            {
                if (!BigPatchBox.Helper.AndroidPlayer.Visible)
                    BigPatchBox.Helper.AndroidPlayer.Visible = true;
            }
        }
        private void ProcessSkills()
        {
            if (Config.是否开启自动技能1 && CEnvir.Now >= skillTime1)
            {
                if ((uint)Config.自动技能1 > 0U)
                    UseMagic(Config.自动技能1);
                skillTime1 = CEnvir.Now + TimeSpan.FromSeconds(Config.自动技能1多长时间使用一次 > 0L ? (double)Config.自动技能1多长时间使用一次 : 10.0);
            }
            if (!Config.是否开启自动技能2 || !(CEnvir.Now >= skillTime2))
                return;
            if ((uint)Config.自动技能2 > 0U)
                UseMagic(Config.自动技能2);
            skillTime2 = CEnvir.Now + TimeSpan.FromSeconds(Config.自动技能2多长时间使用一次 > 0L ? (double)Config.自动技能2多长时间使用一次 : 10.0);
        }
        public void SortFillItems(List<ClientUserItem> items)
        {
            for (int i = 0; i < Globals.InventorySize; i++)
            {
                InventoryBox.Grid.Grid[i].Item = null;
            }

            foreach (ClientUserItem item in items)
            {
                InventoryBox.Grid.Grid[item.Slot].Item = item;
            }
        }
         
        public void SortFillStorageItems(List<ClientUserItem> items)
        {
            for (int i = 0; i < Globals.StorageSize; i++)
            {
                StorageBox.Grid.Grid[i].Item = null;
            }

            foreach (ClientUserItem item in items)
            {
                StorageBox.Grid.Grid[item.Slot].Item = item;
            }
        }
        private MapObject AutoRemoteTarget(MapObject mp, MagicHelper helpper, MagicType magic)
        {
            MapObject result = mp;

            if (helpper == null) 
                helpper = GetMagicHelpper(magic);

            if (CanAttackTarget(MouseObject))
            {
                result = MouseObject;
                MapObject.MagicObject = helpper == null || !helpper.LockMonster || (MouseObject.Race != ObjectType.Monster || ((MonsterObject)MouseObject).MonsterInfo.AI < 0)
                    ? (helpper == null || !helpper.LockPlayer || MouseObject.Race != ObjectType.Player ? null : (MouseObject == User && magic == MagicType.Heal ? MagicObject : result))
                    : result;
            }
            else if (CanAttackTarget(MagicObject) 
                && ((helpper.LockMonster && MagicObject.Race == ObjectType.Monster)
                || (helpper.LockPlayer && MagicObject.Race == ObjectType.Player)))
            {
                result = MagicObject;
                MapObject.MagicObject = result;
            }
            else if (CanAttackTarget(MonsterBox.Monster)
                && (((helpper?.LockMonster ?? false) && MonsterBox.Monster.Race == ObjectType.Monster)
                || ((helpper?.LockPlayer ?? false) && MonsterBox.Monster.Race == ObjectType.Player)))
            {
                result = MonsterBox.Monster;
                MapObject.MagicObject = result;
            }

            return result;
        }
        public bool AutoPoison()
        {
            if (CEnvir.Now < AutoPoisonTime) return false;

            AutoPoisonTime = CEnvir.Now.AddMilliseconds(300);
            if (!Config.自动上毒 || User.Class != MirClass.Taoist || MapObject.TargetObject == null) return false;
                
            if (!Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 10)) return false;

            var helpper = GetMagicHelpper(MagicType.PoisonDust);
            if (helpper == null) return false;

            var item = Globals.ItemInfoList.Binding.FirstOrDefault(x => x.Index == helpper.Amulet);
            if (helpper.Amulet > 0 && item == null) return false;

            if ((helpper.Amulet == 0 || item.ItemName == "红毒") && (MapObject.TargetObject.Poison & PoisonType.Red) != PoisonType.Red)
            {
                UseMagic(MagicType.PoisonDust);
                BigPatchBox.AutoSkillsTime = CEnvir.Now.AddMilliseconds(500);
                return true;
            }

            if ((helpper.Amulet == 0 || item.ItemName == "绿毒") && (MapObject.TargetObject.Poison & PoisonType.Green) != PoisonType.Green)
            {
                UseMagic(MagicType.PoisonDust);
                BigPatchBox.AutoSkillsTime = CEnvir.Now.AddMilliseconds(500);
                return true;
            }

            return false;
        }
        private void AutoChangePoison(ClientUserMagic magic)
        {
            MagicHelper magicHelper = null;

            for (int index = 0; index < Config.magics.Count; ++index)
            {
                if (Config.magics[index].TypeID == magic.Info.Magic)
                {
                    magicHelper = Config.magics[index];
                    break;
                }
            }

            if (magicHelper == null) return;

            ClientUserItem clientUserItem1 = CharacterBox?.Grid[10]?.Item;

            if (magicHelper.Amulet > 0 && (clientUserItem1?.Info?.Index ?? -1) == magicHelper.Amulet)
                return;

            for (int index = 0; index < Inventory.Length; ++index)
            {
                ClientUserItem clientUserItem2 = Inventory[index];
                if (magicHelper.Amulet > 0 && (clientUserItem2?.Info?.Index ?? -1) == magicHelper.Amulet)
                {
                    CharacterBox.Grid[10].ToEquipment(InventoryBox.Grid.Grid[index]);
                    return;
                }
                else if(magicHelper.Amulet == 0 
                    && (clientUserItem2?.Info?.ItemType ?? ItemType.Nothing) == ItemType.Poison
                    && (clientUserItem2?.Info?.Index ?? magicHelper.Amulet) != magicHelper.Amulet)
                {
                    CharacterBox.Grid[10].ToEquipment(InventoryBox.Grid.Grid[index]);
                    return;
                }
            }

            ReceiveChat("你的毒用完了，释放失败", MessageType.Hint);
        }
    }
} 
