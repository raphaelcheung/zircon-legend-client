using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Library;
using Library.Network.ClientPackets;
using Library.SystemModels;
using SlimDX;
using SlimDX.Direct3D9;
using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Scenes.Views
{
    public sealed class MapControl : DXControl
    {
        #region Properties
        private DateTime PathFinderTime;

        public static UserObject User => GameScene.Game.User;
        public PathFinder PathFinder { get; set; } = null;
        public List<Node> CurrentPath { get; set; } = null;
        public bool AutoPath
        {
            get
            {
                return _autoPath;
            }
            set
            {
                if (_autoPath == value)
                    return;
                _autoPath = value;
                if (!_autoPath)
                    CurrentPath = null;
                if (GameScene.Game == null || Config.开始挂机)
                    return;
                GameScene.Game.ReceiveChat(value ? "[寻路:开 (停止:鼠标左键或右键)]" : "[寻路:关]", MessageType.System);
            }
        }
        private bool _autoPath;

        public bool IsLongDistanceMode { get; set; } = false;
        
        // ！ 改进4：模式状态机
        private enum ShortDistanceMode
        {
            Mode1_ShortDistance,  // 模式1：短距离移动（寻找怪物）
            Mode2_Pathfind,       // 模式2：寻路模式（卡住时的寻路方案）
            Mode3_RandomJump      // 模式3：随机跳跃（模式2失败时的恢复）
        }
        
        private ShortDistanceMode _currentShortDistanceMode = ShortDistanceMode.Mode1_ShortDistance;
        
        // ！ 追踪上一次的挂机状态，只在状态改变时清除路径
        private bool _lastAutoAndroidState = false;
        
        // ！ 新增：用于跟踪是否应该清除AutoPath的标志
        // 用来解决pathfinding中检测到怪物需要立即停止的问题
        private bool _shouldClearAutoPath = false;
        
        // ！ 新增：后台常驻位置追踪和状态判断机制
        private enum CharacterStuckState
        {
            Standing = 0,  // 静止不动
            Moving = 1,    // 正常移动
            Stuck = 2      // 卡住（在小范围内反复）
        }
        
        private Point[] _positionHistory = new Point[3];  // 记录最近3个位置
        private DateTime _lastPositionRecordTime = DateTime.MinValue;
        private CharacterStuckState _currentCharacterState = CharacterStuckState.Moving;
        private const double POSITION_RECORD_INTERVAL = 1.0; // 每1秒记录一次位置
        
        private DateTime _lastAutoStateChangeTime = DateTime.MinValue;
        private const double STATE_CHANGE_DELAY = 1.0; // 1秒延迟
        
        // ！ 参数化短距离怪物检测距离
        private const int SHORT_DISTANCE_DETECTION_RANGE = 9; // 战斗模式的怪物检测范围（格）

        public DateTime ProtectTime
        {
            get
            {
                return GameScene.Game.BigPatchBox._ProtectTime;
            }
            set
            {
                GameScene.Game.BigPatchBox._ProtectTime = value;
            }
        }

        #region MapInformation

        public MapInfo MapInfo
        {
            get => _MapInfo;
            set
            {
                if (_MapInfo == value) return;

                MapInfo oldValue = _MapInfo;
                _MapInfo = value;

                OnMapInfoChanged(oldValue, value);
            }
        }
        private MapInfo _MapInfo;
        public event EventHandler<EventArgs> MapInfoChanged;
        public void OnMapInfoChanged(MapInfo oValue, MapInfo nValue)
        {
            TextureValid = false;
            LoadMap();

            if (oValue != null)
            {
                if (nValue == null || nValue.Music != oValue.Music)
                    DXSoundManager.Stop(oValue.Music);
            }

            if (nValue != null)
                DXSoundManager.Play(nValue.Music);

            LLayer.UpdateLights();

            PathFinder = new PathFinder(this);

            if (GameScene.Game != null && GameScene.Game.BigPatchBox != null)
                GameScene.Game.AutoGuajiChanged();

            MapInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Animation

        public int Animation
        {
            get => _Animation;
            set
            {
                if (_Animation == value) return;

                int oldValue = _Animation;
                _Animation = value;

                OnAnimationChanged(oldValue, value);
            }
        }
        private int _Animation;
        public event EventHandler<EventArgs> AnimationChanged;
        public void OnAnimationChanged(int oValue, int nValue)
        {
            AnimationChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }



        #endregion
        

        #region MouseLocation

        public Point MouseLocation
        {
            get => _MouseLocation;
            set
            {
                if (_MouseLocation == value) return;

                Point oldValue = _MouseLocation;
                _MouseLocation = value;

                OnMouseLocationChanged(oldValue, value);
            }
        }
        private Point _MouseLocation;
        public event EventHandler<EventArgs> MouseLocationChanged;
        public void OnMouseLocationChanged(Point oValue, Point nValue)
        {
            MouseLocationChanged?.Invoke(this, EventArgs.Empty);
            UpdateMapLocation();
        }

        #endregion

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);

            if (FLayer != null)
                FLayer.Size = Size;

            if (LLayer != null)
                LLayer.Size = Size;


            OffSetX = Size.Width/2/CellWidth;
            OffSetY = Size.Height/2/CellHeight;
        }

        public MouseButtons MapButtons;
        public Point MapLocation { get; set; }
        public bool Mining;
        public Point MiningPoint;
        public MirDirection MiningDirection;
        
        public Floor FLayer;
        private Light LLayer;

        public Cell[,] Cells;
        public int Width, Height;

        public List<DXControl> MapInfoObjects = new List<DXControl>();
        public List<MapObject> Objects = new List<MapObject>();
        public List<MirEffect> Effects { get; private set; } = new List<MirEffect>();

        public const int CellWidth = 48, CellHeight = 32;

        public int ViewRangeX = 12, ViewRangeY = 24;

        public static int OffSetX;
        public static int OffSetY;

        private Point TargetLocation;
        private DateTime UpdateTarget;
        #endregion

        public MapControl()
        {
            DrawTexture = true;

            BackColour = Color.Empty;
            
            FLayer = new Floor { Parent = this, Size = Size };
            LLayer = new Light { Parent = this, Location = new Point(-GameScene.Game.Location.X, -GameScene.Game.Location.Y), Size = Size };
            
            // ！ 进入游戏时关闭自动挂机
            Config.开始挂机 = false;
        }

        #region Methods

        protected override void OnClearTexture()
        {
            base.OnClearTexture();

            if (FLayer.TextureValid)
                DXManager.Sprite.Draw(FLayer.ControlTexture, Color.White);

            if (Config.DrawEffects)
            {
                foreach (MirEffect ob in Effects)
                {
                    if (ob.DrawType != DrawType.Floor) continue;

                    ob.Draw();
                }
            }

            DrawObjects();

            if (MapObject.MouseObject != null) // && MapObject.MouseObject != MapObject.TargetObject)
                MapObject.MouseObject.DrawBlend();

            foreach (MapObject ob in Objects)
            {
                if (ob.Dead) continue;

                switch (ob.Race)
                {
                    case ObjectType.Player:
                        if (!Config.ShowPlayerNames) continue;

                        break;
                    case ObjectType.Item:
                        if (!Config.ShowItemNames || ob.CurrentLocation == MapLocation) continue;
                        break;
                    case ObjectType.NPC:
                        break;
                    case ObjectType.Spell:
                        break;
                    case ObjectType.Monster:
                        if (!Config.ShowMonsterNames) continue;
                        break;
                }

                ob.DrawName();
            }

            if (MapObject.MouseObject != null && MapObject.MouseObject.Race != ObjectType.Item)
                MapObject.MouseObject.DrawName();



            foreach (MapObject ob in Objects)
            {
                ob.DrawChat();
                ob.DrawPoison();
                ob.DrawHealth();
            }

            if (Config.DrawEffects)
            {
                foreach (MirEffect ob in Effects)
                {
                    if (ob.DrawType != DrawType.Object || (ob.MapTarget.IsEmpty && ob.Target == User)) continue;

                    ob.Draw();
                }

                foreach (MirEffect ob in Effects)
                {
                    if (ob.DrawType != DrawType.Object || (ob.MapTarget.IsEmpty && ob.Target != User)) continue;

                    ob.Draw();
                }

                foreach (MirEffect ob in Effects)
                {
                    if (ob.DrawType != DrawType.Final) continue;

                    ob.Draw();
                }
            }

            if (Config.ShowDamageNumbers)
                foreach (MapObject ob in Objects)
                    ob.DrawDamage();

            if (MapLocation.X >= 0 && MapLocation.X < Width && MapLocation.Y >= 0 && MapLocation.Y < Height)
            {
                Cell cell = Cells[MapLocation.X, MapLocation.Y];
                int layer = 0;
                if (cell.Objects != null)
                    for (int i = cell.Objects.Count - 1; i >= 0; i--)
                    {
                        ItemObject ob = cell.Objects[i] as ItemObject;

                        ob?.DrawFocus(layer++);
                    }
            }

            DXManager.Sprite.Flush();
            DXManager.Device.SetRenderState(RenderState.SourceBlend, Blend.DestinationColor);
            DXManager.Device.SetRenderState(RenderState.DestinationBlend, Blend.BothInverseSourceAlpha);

            DXManager.Sprite.Draw(LLayer.ControlTexture, Color.White);

            DXManager.Sprite.End();
            DXManager.Sprite.Begin(SpriteFlags.AlphaBlend);
        }
        public override void Draw()
        {
            if (!IsVisible || Size.Width == 0 || Size.Height == 0) return;

            FLayer.CheckTexture();
            LLayer.CheckTexture();
            
            //CreateTexture();
            OnBeforeDraw();

            DrawControl();
            
            DrawBorder();
            OnAfterDraw();
        }

        private void DrawObjects()
        {
            int minX = Math.Max(0, User.CurrentLocation.X - OffSetX - 4), maxX = Math.Min(Width - 1, User.CurrentLocation.X + OffSetX + 4);
            int minY = Math.Max(0, User.CurrentLocation.Y - OffSetY - 4), maxY = Math.Min(Height - 1, User.CurrentLocation.Y + OffSetY + 25);


            for (int y = minY; y <= maxY; y++)
            {
                foreach (MapObject ob in Objects)
                {
                    if (ob.RenderY == y && ob.Dead)
                        ob.Draw();
                }
            }

            for (int y = minY; y <= maxY; y++)
            {
                int drawY = (y - User.CurrentLocation.Y + OffSetY + 1)*CellHeight - User.MovingOffSet.Y;

                for (int x = minX; x <= maxX; x++)
                {
                    int drawX = (x - User.CurrentLocation.X + OffSetX)*CellWidth - User.MovingOffSet.X;

                    Cell cell = Cells[x, y];

                    MirLibrary library;
                    LibraryFile file;

                    if (Libraries.KROrder.TryGetValue(cell.MiddleFile, out file) && file != LibraryFile.Tilesc && CEnvir.LibraryList.TryGetValue(file, out library))
                    {
                        int index = cell.MiddleImage - 1;

                        bool blend = false;
                        if (cell.MiddleAnimationFrame > 1 && cell.MiddleAnimationFrame < 255)
                        {
                            index += Animation%(cell.MiddleAnimationFrame & 0x4F);
                            blend = (cell.MiddleAnimationFrame & 0x50) > 0;
                        }

                        Size s = library.GetSize(index);

                        if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                        {
                            if (!blend)
                                library.Draw(index, drawX, drawY - s.Height, Color.White, false, 1F, ImageType.Image);
                            else
                                library.DrawBlend(index, drawX, drawY - s.Height, Color.White, false, 0.5F, ImageType.Image);
                        }
                    }



                    if (Libraries.KROrder.TryGetValue(cell.FrontFile, out file) && file != LibraryFile.Tilesc && CEnvir.LibraryList.TryGetValue(file, out library))
                    {
                        int index = cell.FrontImage - 1;

                        bool blend = false;
                        if (cell.FrontAnimationFrame > 1 && cell.FrontAnimationFrame < 255)
                        {
                            index += Animation % (cell.FrontAnimationFrame & 0x4F);
                            blend = (cell.MiddleAnimationFrame & 0x50) > 0;
                        }
                    
                        Size s = library.GetSize(index);


                        if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                        {
                            if (!blend)
                                library.Draw(index, drawX, drawY - s.Height, Color.White, false, 1F, ImageType.Image);
                            else
                                library.DrawBlend(index, drawX, drawY - s.Height, Color.White, false, 0.5F, ImageType.Image);
                        }
                    }
                }

                foreach (MapObject ob in Objects)
                {
                    if (ob.RenderY == y && !ob.Dead)
                        ob.Draw();
                }

                if (Config.DrawEffects)
                {
                    foreach (MirEffect ob in Effects)
                    {
                        if (ob.DrawType != DrawType.Object) continue;

                        if (ob.Target != null && ob.Target.RenderY == y && ob.Target != User)
                            ob.Draw();
                    }
                }
            }

            if (User.Opacity != 1f) return;
            float oldOpacity = MapObject.User.Opacity;
            MapObject.User.Opacity = 0.65F;

            MapObject.User.DrawBody(false);

            MapObject.User.Opacity = oldOpacity;
        }
        public void UpdateLights()
        { LLayer?.UpdateLights(); }

        private void LoadMap()
        {
            try
            {
                if (!File.Exists(Config.MapPath + MapInfo.FileName + ".map")) return;

                using (MemoryStream mStream = new MemoryStream(File.ReadAllBytes(Config.MapPath + MapInfo.FileName + ".map")))
                using (BinaryReader reader = new BinaryReader(mStream))
                {
                    mStream.Seek(22, SeekOrigin.Begin);
                    Width = reader.ReadInt16();
                    Height = reader.ReadInt16();

                    mStream.Seek(28, SeekOrigin.Begin);

                    Cells = new Cell[Width, Height];
                    for (int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                            Cells[x, y] = new Cell();

                    for (int x = 0; x < Width/2; x++)
                        for (int y = 0; y < Height/2; y++)
                        {
                            Cells[(x*2), (y*2)].BackFile = reader.ReadByte();
                            Cells[(x*2), (y*2)].BackImage = reader.ReadUInt16();
                        }

                    for (int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                        {
                            byte flag = reader.ReadByte();
                            Cells[x, y].MiddleAnimationFrame = reader.ReadByte();

                            byte value = reader.ReadByte();
                            Cells[x, y].FrontAnimationFrame = value == 255 ? 0 : value;
                            Cells[x, y].FrontAnimationFrame &= 0x8F; //Probably a Blend Flag

                            Cells[x, y].FrontFile = reader.ReadByte();
                            Cells[x, y].MiddleFile = reader.ReadByte();

                            Cells[x, y].MiddleImage = reader.ReadUInt16() + 1;
                            Cells[x, y].FrontImage = reader.ReadUInt16() + 1;

                            mStream.Seek(3, SeekOrigin.Current);

                            Cells[x, y].Light = (byte) (reader.ReadByte() & 0x0F)*2;

                            mStream.Seek(1, SeekOrigin.Current);

                            Cells[x, y].Flag = ((flag & 0x01) != 1) || ((flag & 0x02) != 2);
                        }
                }
            }
            catch (Exception ex)
            {
                CEnvir.SaveError(ex.ToString());
            }

            foreach (MapObject ob in Objects)
                if (ob.CurrentLocation.X < Width && ob.CurrentLocation.Y < Height)
                    Cells[ob.CurrentLocation.X, ob.CurrentLocation.Y].AddObject(ob);
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            MouseLocation = e.Location;
        }
        public override void OnMouseDown(MouseEventArgs e)
        {

            base.OnMouseDown(e);

            if (GameScene.Game.Observer) return;

            MapButtons |= e.Button;

            if (e.Button == MouseButtons.Right)
            {
                if (Config.RightClickDeTarget && MapObject.TargetObject?.Race == ObjectType.Monster)
                    MapObject.TargetObject = null;
            }

            if (e.Button == MouseButtons.Middle)
            {
                if (Config.是否开启鼠标中间按钮自动使用坐骑)
                {
                    if (!GameScene.Game.Observer)
                    {
                        if (CEnvir.Now < User.NextActionTime || User.ActionQueue.Count > 0) return;
                        if (CEnvir.Now < User.ServerTime) return;

                        User.ServerTime = CEnvir.Now.AddSeconds(5);
                        CEnvir.Enqueue(new C.Mount());
                    }
                }
                else if (Config.是否开启鼠标中间按钮自动使用技能)
                {
                    if (!GameScene.Game.Observer)
                    {
                        switch (Config.鼠标中间按钮使用F几的技能)
                        {
                            case 1:
                                GameScene.Game.UseMagic(SpellKey.Spell01);
                                break;
                            case 2:
                                GameScene.Game.UseMagic(SpellKey.Spell02);
                                break;
                            case 3:
                                GameScene.Game.UseMagic(SpellKey.Spell03);
                                break;
                            case 4:
                                GameScene.Game.UseMagic(SpellKey.Spell04);
                                break;
                            case 5:
                                GameScene.Game.UseMagic(SpellKey.Spell05);
                                break;
                            case 6:
                                GameScene.Game.UseMagic(SpellKey.Spell06);
                                break;
                            case 7:
                                GameScene.Game.UseMagic(SpellKey.Spell07);
                                break;
                            case 8:
                                GameScene.Game.UseMagic(SpellKey.Spell08);
                                break;
                            case 9:
                                GameScene.Game.UseMagic(SpellKey.Spell09);
                                break;
                            case 10:
                                GameScene.Game.UseMagic(SpellKey.Spell10);
                                break;
                            case 11:
                                GameScene.Game.UseMagic(SpellKey.Spell11);
                                break;
                            case 12:
                                GameScene.Game.UseMagic(SpellKey.Spell12);
                                break;
                        }
                    }
                }
            }

            if (e.Button != MouseButtons.Left) return;


            DXItemCell cell = DXItemCell.SelectedCell;
            if (cell != null)
            {
                MapButtons &= ~e.Button;

                if (cell.GridType == GridType.Belt)
                {
                    cell.QuickInfo = null;
                    cell.QuickItem = null;
                    DXItemCell.SelectedCell = null;

                    ClientBeltLink link = GameScene.Game.BeltBox.Links[cell.Slot];
                    CEnvir.Enqueue(new C.BeltLinkChanged { Slot = link.Slot, LinkIndex = link.LinkInfoIndex, LinkItemIndex = link.LinkItemIndex }); //Update serve
                    return;
                }

                //if (cell.GridType == GridType.AutoPotion)
                //{
                //    cell.QuickInfo = null;
                //    cell.QuickItem = null;
                //    DXItemCell.SelectedCell = null;

                //    GameScene.Game.AutoPotionBox.Rows[cell.Slot].SendUpdate();
                //    return;
                //}


                if ((cell.Item.Flags & UserItemFlags.Locked) == UserItemFlags.Locked || (cell.GridType != GridType.Inventory && cell.GridType != GridType.CompanionInventory))
                {
                    DXItemCell.SelectedCell = null;
                    return;
                }

                if (cell.Item.Count <= 1)
                {
                    // 只有1个，直接丢弃
                    CEnvir.Enqueue(new C.ItemDrop
                    {
                        Link = new CellLinkInfo { GridType = cell.GridType, Slot = cell.Slot, Count = 1 }
                    });
                    cell.Locked = true;
                    DXItemCell.SelectedCell = null;
                    return;
                }

                // 多于1个，弹窗，默认最大值
                DXItemAmountWindow window = new DXItemAmountWindow("掉落物品", cell.Item);
                window.ConfirmButton.MouseClick += (o, a) =>
                {
                    if (window.Amount <= 0) return;

                    CEnvir.Enqueue(new C.ItemDrop
                    {
                        Link = new CellLinkInfo { GridType = cell.GridType, Slot = cell.Slot, Count = window.Amount }
                    });

                    cell.Locked = true;
                };
                DXItemCell.SelectedCell = null;
                return;
            }

            if (GameScene.Game.GoldPickedUp)
            {
                MapButtons &= ~e.Button;
                DXItemAmountWindow window = new DXItemAmountWindow("掉落物品", new ClientUserItem(Globals.GoldInfo, User.Gold));

                window.ConfirmButton.MouseClick += (o, a) =>
                {
                    if (window.Amount <= 0) return;

                    CEnvir.Enqueue(new C.GoldDrop
                    {
                        Amount = window.Amount
                    });

                };

                GameScene.Game.GoldPickedUp = false;
                return;
            }
            
            if (CanAttack(MapObject.MouseObject))
            {
                MapObject.TargetObject = MapObject.MouseObject;

                if (MapObject.MouseObject.Race == ObjectType.Monster && ((MonsterObject) MapObject.MouseObject).MonsterInfo.AI >= 0) //Check if AI is guard
                {
                    MapObject.MagicObject = MapObject.TargetObject;
                    GameScene.Game.FocusObject = MapObject.MouseObject;
                }
                return;
            }

            MapObject.TargetObject = null;
            GameScene.Game.FocusObject = null;
            //GameScene.Game.OldTargetObjectID = 0;
        }
        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (GameScene.Game.Observer) return;

                    GameScene.Game.AutoRun = false;
                    if (MapObject.MouseObject == null) return;
                    NPCObject npc = MapObject.MouseObject as NPCObject;
                    if (npc != null)
                    {
                        if (CEnvir.Now <= GameScene.Game.NPCTime) return;

                        GameScene.Game.NPCTime = CEnvir.Now.AddSeconds(1);

                        CEnvir.Enqueue(new C.NPCCall { ObjectID = npc.ObjectID });
                    }
                    break;
                case MouseButtons.Right:
                    GameScene.Game.AutoRun = false;

                    if (User.CurrentAction == MirAction.Standing)
                        GameScene.Game.CanRun = false;

                    if (!CEnvir.Ctrl) return;

                    PlayerObject player = MapObject.MouseObject as PlayerObject;

                    if (player == null || player == MapObject.User) return;
                    if (CEnvir.Now <= GameScene.Game.InspectTime && player.ObjectID == GameScene.Game.InspectID) return;

                    GameScene.Game.InspectTime = CEnvir.Now.AddMilliseconds(2500);
                    GameScene.Game.InspectID = player.ObjectID;
                    CEnvir.Enqueue(new C.Inspect { Index = player.CharacterIndex });
                    break;
            }
        }

        public void CheckCursor()
        {
            MapObject deadObject = null, itemObject = null;
            
            for (int d = 0; d < 4; d++)
            {
                for (int y = MapLocation.Y - d; y <= MapLocation.Y + d; y++)
                {
                    if (y >= Height) continue;
                    if (y < 0) break;
                    for (int x = MapLocation.X - d; x <= MapLocation.X + d; x++)
                    {
                        if (x >= Width) continue;
                        if (x < 0) break;

                        List<MapObject> list = Cells[x, y].Objects;
                        if (list == null) continue;

                        MapObject cellSelect = null;
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            MapObject ob = list[i];

                            if (ob == MapObject.User || ob.Race == ObjectType.Spell || ((x != MapLocation.X || y != MapLocation.Y) && !ob.MouseOver(MouseLocation))) continue;

                            if (ob.Dead || (ob.Race == ObjectType.Monster && ((MonsterObject)ob).CompanionObject != null))
                            {
                                if (deadObject == null)
                                    deadObject = ob;
                                continue;
                            }
                            if (ob.Race == ObjectType.Item)
                            {
                                if (itemObject == null)
                                    itemObject = ob;
                                continue;
                            }
                            if (x == MapLocation.X && y == MapLocation.Y && !ob.MouseOver(MouseLocation))
                            {
                                if (cellSelect == null)
                                    cellSelect = ob;
                            }
                            else
                            {
                                MapObject.MouseObject = ob;
                                return;
                            }
                        }

                        if (cellSelect != null)
                        {
                            MapObject.MouseObject = cellSelect;
                            return;
                        }
                    }
                }
            }

            MapObject mouseOb = deadObject ?? itemObject;

            if (mouseOb == null)
            {
                if ((User.CurrentLocation.X == MapLocation.X && User.CurrentLocation.Y == MapLocation.Y) || User.MouseOver(MouseLocation))
                    mouseOb = User;
            }


            MapObject.MouseObject = mouseOb;
        }

        public void ProcessInput()
        {
            if (GameScene.Game.Observer) return;

            // ！ 新增：统一的AutoPath清除机制
            // 在每帧开始检查是否需要清除pathfinding
            ClearAutoPathIfNeeded();

            // ！ 修复：仅在挂机状态改变时清除路径，而不是每帧都清除
            if (!Config.开始挂机 && _lastAutoAndroidState)
            {
                // 挂机从打开切换到关闭，此时才清除挂机路径
                AutoPath = false;
                if (CurrentPath != null)
                {
                    CurrentPath.Clear();
                    CurrentPath = null;
                }
                // ！ 停止当前脚步
                if (User.CurrentAction == MirAction.Moving)
                {
                    User.AttemptAction(new ObjectAction(MirAction.Standing, User.Direction, User.CurrentLocation));
                }
            }
            _lastAutoAndroidState = Config.开始挂机; // 记录本帧状态

            if (User == null || (User.Dead || (User.Poison & PoisonType.Paralysis) == PoisonType.Paralysis || User.Buffs.Any(x => x.Type == BuffType.DragonRepulse || x.Type == BuffType.FrostBite))) return; //Para or Frozen??


            if (User.MagicAction != null)
            {
                if (CEnvir.Now < MapObject.User.NextActionTime || MapObject.User.ActionQueue.Count != 0) return;

                //  if (QueuedMagic.Action == MirAction.Magic && (Spell)QueuedMagic.Extra[1] == Magic.ShoulderDash && !GameScene.Game.MoveFrame) return;

                MapObject.User.AttemptAction(User.MagicAction);
                User.MagicAction = null;
                Mining = false;
            }

            // 新增：自动选怪和寻路逻辑（目测来自国服大补帖 ProcessInput2）
            if (Config.开始挂机)
            {
                // ！ 新增：检查是否有近身怪物，优先攻击
                // 这样即使在长距离pathfinding中也能快速响应近身怪物
                MapObject closestMonster = null;
                int closestDistance = int.MaxValue;
                
                foreach (MapObject obj in Objects)
                {
                    if (obj == null || obj.Dead || obj == User) continue;
                    if (obj.Race != ObjectType.Monster || !string.IsNullOrEmpty(obj.PetOwner)) continue;
                    
                    int distance = Functions.Distance(User.CurrentLocation, obj.CurrentLocation);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestMonster = obj;
                    }
                }
                
                // 如果发现近身怪物（3格以内），立即停止pathfinding并切换到战斗
                if (closestMonster != null && closestDistance < 4)
                {
                    GameScene.Game.TargetObject = closestMonster;
                    _shouldClearAutoPath = true; // 标记需要清除pathfinding
                }
                // 否则按原逻辑选怪
                else if (GameScene.Game.TargetObject == null || GameScene.Game.TargetObject.Dead || !Functions.InRange(GameScene.Game.TargetObject.CurrentLocation, MapControl.User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE))
                {
                    MapObject mapObject = null;

                    mapObject = SelectMonster();

                    int num;
                    if (mapObject != null)
                    {
                        int objectId1 = (int)mapObject.ObjectID;
                        uint? objectId2 = GameScene.Game.TargetObject?.ObjectID;
                        int valueOrDefault = (int)objectId2.GetValueOrDefault();
                        num = !(objectId1 == valueOrDefault & objectId2.HasValue) ? 1 : 0;
                    }
                    else
                        num = 0;
                    if (num != 0)
                        GameScene.Game.TargetObject = mapObject;
                    else
                    {
                        // ！ 改进：仅在1秒延迟后再执行下一次寻路计算，减少消耗
                        if (CEnvir.Now >= _lastAutoStateChangeTime.AddSeconds(STATE_CHANGE_DELAY))
                        {
                            ChangeAutoFightLocation();
                            _lastAutoStateChangeTime = CEnvir.Now;
                        }
                    }
                }
                else
                    AndroidProcess();
            }
            
            if (MapObject.TargetObject != null && !MapObject.TargetObject.Dead && ((MapObject.TargetObject.Race == ObjectType.Monster && string.IsNullOrEmpty(MapObject.TargetObject.PetOwner)) || (CEnvir.Shift || Config.免SHIFT)))
            {
                //if (Functions.Distance(MapObject.TargetObject.CurrentLocation, MapObject.User.CurrentLocation) ==  1 && CEnvir.Now > User.AttackTime && User.Horse == HorseType.None)
                //{
                //    MapObject.User.AttemptAction(new ObjectAction(
                //        MirAction.Attack,
                //        Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation),
                //        MapObject.User.CurrentLocation,
                //        0, //Ranged Attack Target ID
                //        MagicType.None,
                //        Element.None));
                //    return;
                //}
                bool flag = true;
                
                // 修改：自动四花只在战斗状态下执行
                if (Config.自动四花 && CEnvir.Now < User.CombatTime.AddSeconds(10))
                {
                    if (!User.Buffs.Any(x =>
                    {
                        if (x.Type != BuffType.FullBloom && x.Type != BuffType.WhiteLotus)
                            return x.Type == BuffType.RedLotus;

                        return true;
                    }))
                    {
                        GameScene.Game.UseMagic(MagicType.FullBloom);
                        flag = false;
                    }


                    if (User.Buffs.Any(x => x.Type == BuffType.FullBloom))
                    {
                        GameScene.Game.UseMagic(MagicType.WhiteLotus);
                        flag = false;
                    }

                    if (User.Buffs.Any<ClientBuffInfo>((Func<ClientBuffInfo, bool>)(x => x.Type == BuffType.WhiteLotus)))
                    {
                        GameScene.Game.UseMagic(MagicType.RedLotus);
                        flag = false;
                    }

                    if (User.Buffs.Any<ClientBuffInfo>((Func<ClientBuffInfo, bool>)(x => x.Type == BuffType.RedLotus)))
                    {
                        GameScene.Game.UseMagic(MagicType.SweetBrier);
                        flag = false;
                    }
                }

                if (Config.开始挂机)
                {
                    PathFinderTime = CEnvir.Now.AddSeconds(2.0);
                    // ！ 改进：使用"远程技能挂机"配置项替代职业判断
                    // if (Config.自动躲避 && ((User.Class == MirClass.Wizard || User.Class == MirClass.Taoist) && (double)Functions.Distance(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation) < 18.0))
                    if (Config.自动躲避 && (Config.是否远战挂机 && (double)Functions.Distance(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation) < 18.0))
                    {
                        MirDirection mirDirection1 = Functions.ShiftDirection(Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation), 4);
                        if (!CanMove(mirDirection1, 1))
                        {
                            MirDirection mirDirection2 = DirectionBest(mirDirection1, 1, MapObject.TargetObject.CurrentLocation);

                            if (mirDirection2 == mirDirection1)
                            {
                                if (mirDirection1 != User.Direction)
                                    MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, mirDirection1, MapObject.User.CurrentLocation, Array.Empty<object>()));

                                if (!Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE))
                                    return;

                                if (Config.远战挂机是否使用技能)
                                    GameScene.Game.UseMagic(Config.挂机自动技能);
                                return;
                            }
                            mirDirection1 = mirDirection2;
                        }

                        if (GameScene.Game.MoveFrame && (User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip)
                        {
                            MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, mirDirection1, Functions.Move(MapObject.User.CurrentLocation, mirDirection1, 1), new object[2]
                            {
                                1,
                                MagicType.None
                            }));
                            return;
                        }
                    }

                    //if (GameScene.Game.AutoPoison()) return;

                    // ！ 改进：使用"远程技能挂机"配置项替代职业判断
                    //  if ((User.Class == MirClass.Taoist || User.Class == MirClass.Wizard) && Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE))
                    if (Config.是否远战挂机 && Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE))
                    {
                        if (Config.远战挂机是否使用技能)
                            GameScene.Game.UseMagic(Config.挂机自动技能);
                        return;
                    }
                }

                int targetDistance = Functions.Distance(MapObject.TargetObject.CurrentLocation, MapObject.User.CurrentLocation);
                
                // ！ 新增：如果距离为0（重叠），尝试向任意可移动方向走一步
                if (targetDistance == 0 && GameScene.Game.MoveFrame && (User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip)
                {
                    // 尝试所有8个方向，找到第一个可以移动的方向
                    for (int i = 0; i < 8; i++)
                    {
                        MirDirection tryDirection = (MirDirection)i;
                        if (CanMove(tryDirection, 1))
                        {
                            // 找到可移动的方向，执行移动
                            MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, tryDirection, Functions.Move(MapObject.User.CurrentLocation, tryDirection, 1), new object[2]
                            {
                                1,
                                MagicType.None
                            }));
                            return;
                        }
                    }
                    // 如果所有方向都不能移动，至少改变朝向
                    MirDirection faceDirection = Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation);
                    if (faceDirection != User.Direction)
                    {
                        MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, faceDirection, MapObject.User.CurrentLocation));
                    }
                    return;
                }
                // 距离为1则执行操作
                if (targetDistance == 1 && CEnvir.Now > User.AttackTime && User.Horse == HorseType.None)
                {
                    if (Config.开始挂机 && (flag && User.Class == MirClass.Assassin && Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE)))
                        GameScene.Game.UseMagic(Config.挂机自动技能);

                    MapObject.User.AttemptAction(new ObjectAction(MirAction.Attack, Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation), MapObject.User.CurrentLocation, new object[3]
                    {
                        0,
                        MagicType.None,
                        Element.None
                    }));

                    return;
                }
            }
            else if(Config.开始挂机 && (PathFinderTime < CEnvir.Now && !GameScene.Game.MapControl.AutoPath))
            {
                int x;
                int y;

                if (Config.范围挂机)
                {
                    x = CEnvir.Random.Next(Math.Max((int)(Config.范围挂机坐标.X - Config.范围距离), 0), Math.Min((int)(Config.范围挂机坐标.X + Config.范围距离), GameScene.Game.MapControl.Width - 1));
                    y = CEnvir.Random.Next(Math.Max((int)(Config.范围挂机坐标.Y - Config.范围距离), 0), Math.Min((int)(Config.范围挂机坐标.Y + Config.范围距离), GameScene.Game.MapControl.Height - 1));
                }
                else
                {
                    x = CEnvir.Random.Next(0, GameScene.Game.MapControl.Width - 1);
                    y = CEnvir.Random.Next(0, GameScene.Game.MapControl.Height - 1);
                }

                List<Node> path = GameScene.Game.MapControl.PathFinder.FindPath(MapObject.User.CurrentLocation, new Point(x, y));
                if (path != null && path.Count != 0)
                {
                    GameScene.Game.MapControl.CurrentPath = path;
                    GameScene.Game.MapControl.AutoPath = true;
                }
            }

            //If  auto run

            MirDirection direction = MouseDirection(), best;

            if (GameScene.Game.AutoRun)
            {
                if (!GameScene.Game.MoveFrame || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip) 
                    return;

                Run(direction);
                return;
            }

            if (MouseControl == this)
            {
                switch (MapButtons)
                {
                    case MouseButtons.Left:
                        Mining = false;

                        if ((CEnvir.Shift || Config.免SHIFT) && MapObject.TargetObject == null)
                        {

                            if (CEnvir.Now > User.AttackTime && User.Horse == HorseType.None)
                                MapObject.User.AttemptAction(new ObjectAction(
                                    MirAction.Attack, //RANDOMIZE
                                    direction,
                                    MapObject.User.CurrentLocation,
                                    0, //Ranged Attack Target ID
                                    MagicType.None,
                                    Element.None));
                            return;
                        }

                        if (CEnvir.Alt)
                        {
                            if (User.Horse == HorseType.None)
                                MapObject.User.AttemptAction(new ObjectAction(
                                MirAction.Harvest,
                                direction,
                                MapObject.User.CurrentLocation));
                            return;
                        }

                        if (MapLocation == MapObject.User.CurrentLocation)
                        {
                            if (CEnvir.Now <= GameScene.Game.PickUpTime) return;


                            CEnvir.Enqueue(new C.PickUp() { PickType = (byte)PickType.Sequence });
                            GameScene.Game.PickUpTime = CEnvir.Now.AddMilliseconds(250);

                            return;
                        }

                        if (MapObject.MouseObject != null && MapObject.MouseObject.Race != ObjectType.Item && !MapObject.MouseObject.Dead) break;

                        // 新增：停止自动寻路（来自 ProcessInput2）
                        if (AutoPath)
                            AutoPath = false;

                        ClientUserItem weap = GameScene.Game.Equipment[(int) EquipmentSlot.Weapon];
                        
                        if (MapInfo.CanMine && weap != null && weap.Info.Effect == ItemEffect.PickAxe)
                        {
                            MiningPoint = Functions.Move(User.CurrentLocation, direction);

                            if (MiningPoint.X >= 0 && MiningPoint.Y >= 0 && MiningPoint.X < Width && MiningPoint.Y < Height && Cells[MiningPoint.X, MiningPoint.Y].Flag)
                            {
                                Mining = true;
                                break;
                            }
                        }

                        if (!CanMove(direction, 1))
                        {
                            best = MouseDirectionBest(direction, 1);

                            if (best == direction)
                            {
                                if (direction != User.Direction)
                                    MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                                return;
                            }

                            direction = best;
                        }

                        if (GameScene.Game.MoveFrame && (User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip)
                            MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, direction, Functions.Move(MapObject.User.CurrentLocation, direction), 1, MagicType.None));
                        
                        return;
                    case MouseButtons.Right:

                        Mining = false;

                        // 新增：停止自动寻路（来自 ProcessInput2）
                        if (AutoPath)
                            AutoPath = false;

                        if (MapObject.MouseObject is PlayerObject && MapObject.MouseObject != MapObject.User && CEnvir.Ctrl) 
                            break;

                        if (!GameScene.Game.MoveFrame || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip) 
                            break;
                        
                        int distanceToTarget = Functions.Distance(MapLocation, MapObject.User.CurrentLocation);
                        
                        // Shift+右键：只改变方向，不移动
                        if (CEnvir.Shift)
                        {
                            if (direction != User.Direction)
                                MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                            return;
                        }
                        
                        // 距离为1格时：走一格
                        if (distanceToTarget == 1)
                        {
                            if (!CanMove(direction, 1))
                            {
                                if (direction != User.Direction)
                                    MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                                return;
                            }
                            MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, direction, Functions.Move(MapObject.User.CurrentLocation, direction, 1), 1, MagicType.None));
                            return;
                        }
                        
                        // 其他情况：执行跑步
                        Run(direction);

                        return;
                }
            }

            if (Mining)
            {
                ClientUserItem weap = GameScene.Game.Equipment[(int)EquipmentSlot.Weapon];

                if (MapInfo.CanMine && weap != null && (weap.CurrentDurability > 0 || weap.Info.Durability == 0) && weap.Info.Effect == ItemEffect.PickAxe &&
                    MiningPoint.X >= 0 && MiningPoint.Y >= 0 && MiningPoint.X < Width && MiningPoint.Y < Height && Cells[MiningPoint.X, MiningPoint.Y].Flag &&
                    Functions.Distance(MiningPoint, MapObject.User.CurrentLocation) == 1  && User.Horse == HorseType.None)
                {
                    if (CEnvir.Now > User.AttackTime)
                        MapObject.User.AttemptAction(new ObjectAction(
                            MirAction.Mining,
                            Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MiningPoint),
                            MapObject.User.CurrentLocation,
                            false));
                }
                else
                {
                    Mining = false;
                }
            }

            // ！ 重要修复：先检查是否有 AutoPath，如果有则执行（不依赖 TargetObject）
            if (AutoPath)
            {
                AutoWalkPath();
                return;  // 执行完自动寻路后返回，不继续处理 TargetObject
            }

            if (MapObject.TargetObject == null || MapObject.TargetObject.Dead) return;

            if ((MapObject.TargetObject.Race == ObjectType.Player || !string.IsNullOrEmpty(MapObject.TargetObject.PetOwner)) 
                && (!CEnvir.Shift && !Config.免SHIFT)) 
                return;

            if (Functions.InRange(MapObject.TargetObject.CurrentLocation, MapObject.User.CurrentLocation, 1)) 
                return;

            direction = Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation);

            if (!CanMove(direction, 1))
            {
                best = DirectionBest(direction, 1, MapObject.TargetObject.CurrentLocation);

                if (best != direction)
                {
                    direction = best;
                }
                // 如果 DirectionBest 也失败了，不要 return，让 Run() 方法用 FindWallBypass() 继续绕路
            }

            // 修改：向目标移动时使用跑步而不是走路
            if (GameScene.Game.MoveFrame && (User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip)
                Run(direction, true);  // 启用绕墙逻辑，这样能左右晃绕过短墙

            // 新增：ForceAttack、DigEarth 和 AutoWalkPath（来自未知的 ProcessInput2）
            if (MapObject.TargetObject != null)
            {
                if (UpdateTarget < CEnvir.Now)
                {
                    UpdateTarget = CEnvir.Now.AddMilliseconds(200.0);
                    TargetLocation = MapObject.TargetObject.CurrentLocation;
                }
                if (ForceAttack(TargetLocation))
                    return;
            }

            DigEarth();
        }

        public void Run(MirDirection direction, bool bDetour = true)
        {
            int steps = 1;

            if (CEnvir.Now >= User.NextRunTime && User.BagWeight <= User.Stats[Stat.BagWeight] && User.WearWeight <= User.Stats[Stat.WearWeight])
            {
                steps++;
                if (User.Horse != HorseType.None)
                    steps++;
            }

            for (int i = 1; i <= steps; i++)
            {
                if (CanMove(direction, i)) continue;

                MirDirection best = direction;

                if (bDetour)
                {
                    // 骑马时的特殊处理：只有3步和1步，没有2步 
                    if (User.Horse != HorseType.None)
                    {
                        // 骑马时：如果3步不能走，直接走1步
                        if (i >= 3 && CanMove(direction, 3))
                        {
                            steps = 3;  // 保持3步
                            break;
                        }
                        else if (i >= 1 && CanMove(direction, 1))
                        {
                            steps = 1;  // 回退到1步
                            break;
                        }
                    }
                    else
                    {
                        // 非骑马时的原有逻辑：优先多步，再回退到少步
                        if (i >= 2 && CanMove(direction, 2))
                        {
                            steps = 4;
                            break;
                        }
                        else if (i >= 1 && CanMove(direction, 1))
                        {
                            steps = 1;
                            break;
                        }
                    }

                    // 跑不了才尝试45度逃脱
                    best = MouseDirectionBest(direction, 1);

                    // 如果45度还是挡住，尝试左右晃绕过短墙
                    if (best == direction)
                    {
                        best = FindWallBypass(direction);
                    }
                }

                if (best == direction)
                {
                    if (i == 1)
                    {
                        if (direction != User.Direction)
                            MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                        return;
                    }

                    steps = i - 1;
                }
                else
                    steps = 1;

                direction = best;
                break;
            }

            MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, direction, Functions.Move(MapObject.User.CurrentLocation, direction, steps), steps, MagicType.None));
        }

        /// <summary>
        /// 绕过短墙：优先选择能移动的左右方向
        /// </summary>
        private MirDirection FindWallBypass(MirDirection frontDir)
        {
            MirDirection leftDir = Functions.ShiftDirection(frontDir, -1);    // 左转45度
            MirDirection rightDir = Functions.ShiftDirection(frontDir, 1);   // 右转45度

            // 交替尝试左右，如果能移动就返回（让下一帧继续判断前方）
            if (CanMove(leftDir, 1))
                return leftDir;

            if (CanMove(rightDir, 1))
                return rightDir;

            // 如果左右都挡住了，尝试更深的左右方向
            MirDirection farLeftDir = Functions.ShiftDirection(frontDir, -2);
            MirDirection farRightDir = Functions.ShiftDirection(frontDir, 2);

            if (CanMove(farLeftDir, 1))
                return farLeftDir;

            if (CanMove(farRightDir, 1))
                return farRightDir;

            // 都不行，返回原方向
            return frontDir;
        }

        public MirDirection MouseDirectionBest(MirDirection dir, int distance) //22.5 = 16
        {

            Point loc = Functions.Move(MapObject.User.CurrentLocation, dir, distance);

            if (loc.X >= 0 && loc.Y >= 0 && loc.X < Width && loc.Y < Height && !Cells[loc.X, loc.Y].Blocking()) return dir;
            

            PointF c = new PointF(OffSetX * CellWidth + CellWidth / 2F, OffSetY * CellHeight + CellHeight / 2F);
            PointF a = new PointF(c.X, 0);
            PointF b = MouseLocation;
            float bc = (float)Functions.Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            double ab = (float)Functions.Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (MouseLocation.X < c.X) angle = 360 - angle;

            MirDirection best = (MirDirection)(angle / 45F);

            if (best == dir)
                best = Functions.ShiftDirection(dir, 1);

            MirDirection next = Functions.ShiftDirection(dir, -((int)best - (int)dir));

            if (CanMove(best, distance))
                return best;

            if (CanMove(next, distance))
                return next;

            return dir;
        }
        public MirDirection DirectionBest(MirDirection dir, int distance, Point targetLocation) //22.5 = 16
        {
            Point loc = Functions.Move(MapObject.User.CurrentLocation, dir, distance);

            if (loc.X >= 0 && loc.Y >= 0 && loc.X < Width && loc.Y < Height && !Cells[loc.X, loc.Y].Blocking()) return dir;
            

            PointF c = new PointF(MapObject.OffSetX * MapObject.CellWidth + MapObject.CellWidth / 2F, MapObject.OffSetY * MapObject.CellHeight + MapObject.CellHeight / 2F);
            PointF a = new PointF(c.X, 0);
            PointF b = new PointF((targetLocation.X - MapObject.User.CurrentLocation.X + MapObject.OffSetX) * MapObject.CellWidth + MapObject.CellWidth / 2F,
                (targetLocation.Y - MapObject.User.CurrentLocation.Y + MapObject.OffSetY) * MapObject.CellHeight + MapObject.CellHeight / 2F);
            float bc = (float)Functions.Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            double ab = (float)Functions.Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (b.X < c.X) angle = 360 - angle;

            MirDirection best = (MirDirection)(angle / 45F);

            if (best == dir)
                best = Functions.ShiftDirection(dir, 1);

            MirDirection next = Functions.ShiftDirection(dir, -((int)best - (int)dir));

            if (CanMove(best, distance))
                return best;

            return CanMove(next, distance) ? next : dir;
        }

        public bool CanMove(MirDirection dir, int distance)
        {
            for (int i = 1; i <= distance; i++)
            {
                Point loc = Functions.Move(User.CurrentLocation, dir, i);

                if (loc.X < 0 || loc.Y < 0 || loc.X >= Width || loc.Y > Height) return false;

                if (Cells[loc.X, loc.Y].Blocking())
                    return false;
            }
            return true;
        }

        public MirDirection MouseDirection() //22.5 = 16
        {
            PointF p = new PointF(MouseLocation.X  / CellWidth, MouseLocation.Y / CellHeight);

            //If close proximity then co by co ords 
            if (Functions.InRange(new Point(OffSetX, OffSetY), Point.Truncate(p), 2))
                return Functions.DirectionFromPoint(new Point(OffSetX, OffSetY), Point.Truncate(p));

            PointF c = new PointF(OffSetX * CellWidth + CellWidth / 2F, OffSetY * CellHeight + CellHeight / 2F);
            PointF a = new PointF(c.X, 0);
            PointF b = new PointF(MouseLocation.X, MouseLocation.Y);
            float bc = (float)Functions.Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            float ab = (float)Functions.Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (MouseLocation.X < c.X) angle = 360 - angle;
            angle += 22.5F;
            if (angle > 360) angle -= 360;


            return (MirDirection)(angle / 45F);
        }

        public void AddObject(MapObject ob)
        {
            Objects.Add(ob);


            if (ob.CurrentLocation.X < Width && ob.CurrentLocation.Y < Height)
                Cells[ob.CurrentLocation.X, ob.CurrentLocation.Y].AddObject(ob);

        }

        public void RemoveObject(MapObject ob)
        {
            Objects.Remove(ob);

            if (ob.CurrentLocation.X < Width && ob.CurrentLocation.Y < Height)
                Cells[ob.CurrentLocation.X, ob.CurrentLocation.Y].RemoveObject(ob);
        }

        public bool CanAttack(MapObject ob)
        {
            if (ob == null || ob == User || (ob is MonsterObject mon && mon.Dead)) return false;

            switch (ob.Race)
            {
                case ObjectType.Player:
                    break;
                case ObjectType.Monster:
                    MonsterObject mob = (MonsterObject) ob;

                    if (mob.MonsterInfo.AI < 0) return false;

                    break;
                default:
                    return false;
            }

            return !ob.Dead;
        }

        public void UpdateMapLocation()
        {
            if (User == null) return;


            GameScene.Game.MapControl.MapLocation = new Point((GameScene.Game.MapControl.MouseLocation.X - GameScene.Game.Location.X) / CellWidth - OffSetX + User.CurrentLocation.X,
                                                              (GameScene.Game.MapControl.MouseLocation.Y - GameScene.Game.Location.Y) / CellHeight - OffSetY + User.CurrentLocation.Y);
        }

        public bool HasTarget(Point loc)
        {
            if (loc.X < 0 || loc.Y < 0 || loc.X >= Width || loc.Y > Height) return false;

            Cell cell = Cells[loc.X, loc.Y];

            if (cell.Objects == null) return false;

            foreach (MapObject ob in cell.Objects)
                if (ob.Blocking) return true;

            return false;
        }
        public bool CanEnergyBlast(MirDirection direction)
        {
            return HasTarget(Functions.Move(MapObject.User.CurrentLocation, direction, 1));
        }

        public bool CanHalfMoon(MirDirection direction)
        {
            if (HasTarget(Functions.Move(MapObject.User.CurrentLocation, Functions.ShiftDirection(direction, -1)))) return true;
            if (HasTarget(Functions.Move(MapObject.User.CurrentLocation, Functions.ShiftDirection(direction, 1)))) return true;
            if (HasTarget(Functions.Move(MapObject.User.CurrentLocation, Functions.ShiftDirection(direction, 2)))) return true;

            return false;
        }

        public bool CanDestructiveBlow(MirDirection direction)
        {
            for (int i = 1; i < 8; i++)
                if (HasTarget(Functions.Move(MapObject.User.CurrentLocation, Functions.ShiftDirection(direction, i)))) return true;

            return false;
        }


        public bool ValidCell(Point location)
        {
            if (location.X < 0 || location.Y < 0 || location.X >= Width || location.Y >= Height) return false;

            return !Cells[location.X, location.Y].Flag;
        }
        public bool EmptyCell(Point loc)
        {
            return loc.X >= 0 && loc.Y >= 0 && loc.X < Width && loc.Y <= Height && !Cells[loc.X, loc.Y].Blocking();
        }
        public ItemObject FindNearstItem()
        {
            int dis = 1000;
            ClientObjectData result = null;

            foreach (ClientObjectData ob in GameScene.Game.DataDictionary.Values)
            {
                if (GameScene.Game.MapControl.MapInfo == null) continue;
                if (ob.MapIndex != GameScene.Game.MapControl.MapInfo.Index) continue;
                if (ob.MonsterInfo != null) continue;
                if (ob.ItemInfo == null || !GameScene.Game.BigPatchBox.NeedPick(ob.ItemInfo)) continue;

                var dis0 = Functions.Distance(GameScene.Game.User.CurrentLocation, ob.Location);
                if (result != null && dis0 >= dis)
                    continue;

                result = ob;
                dis = dis0;
            }

            if (result == null) return null;

            return GameScene.Game.MapControl.Objects.FirstOrDefault(x => (int)x.ObjectID == result.ObjectID) as ItemObject;
        }
        public MapObject SelectMonsterTarget(MapObject currentTarget = null)
        {
            MapObject newTarget = null;
            int bestDistance = 100;

            foreach(var ob in Objects)
            {
                if (MapInfo == null || !(ob is MonsterObject mon)) continue;
                if (!string.IsNullOrEmpty(ob.PetOwner) || !GameScene.Game.CanAttackTarget(ob)) continue;
                if (currentTarget != null && ob == currentTarget) continue;

                if (Config.范围挂机 
                    && (mon.CurrentLocation.X < (int)(Config.范围挂机坐标.X - Config.范围距离) 
                    || mon.CurrentLocation.X > (int)(Config.范围挂机坐标.X + Config.范围距离) 
                    || mon.CurrentLocation.Y < (int)(Config.范围挂机坐标.Y - Config.范围距离) 
                    || mon.CurrentLocation.Y > (int)(Config.范围挂机坐标.Y + Config.范围距离)))
                    continue;

                int distance = Functions.Distance(User.CurrentLocation, mon.CurrentLocation);
                if (distance > 0 && distance < bestDistance)
                {
                    bestDistance = distance;
                    newTarget = ob;
                }
            }

            // ！ 修复：扩大挂机视野范围从9格增加到25格，避免频繁寻路
            if (bestDistance > 25)
                return null;

            // if (User.Class == MirClass.Assassin || User.Class == MirClass.Warrior)
            if (!Config.是否远战挂机)
            {
                MirDirection direction = Functions.DirectionFromPoint(newTarget.CurrentLocation, User.CurrentLocation);
                var distance = Functions.Distance(newTarget.CurrentLocation, User.CurrentLocation);
                Point target = Functions.Move(newTarget.CurrentLocation, direction, distance >= 2 ? 2 : 1);

                List<Node> path = GameScene.Game.MapControl.PathFinder.FindPath(MapObject.User.CurrentLocation, target);
                
                if (path == null || path.Count == 0 || (double)path.Count > bestDistance)
                    return null;

                path.Clear();
            }

            return newTarget;
        }

        public MapObject SelectMonster()
        {
            int num1 = 100;
            ClientObjectData minob = null;
            List<Node> nodeList = null;
            foreach (ClientObjectData clientObjectData in GameScene.Game.DataDictionary.Values)
            {
                int mapIndex = clientObjectData.MapIndex;
                int? index = GameScene.Game.MapControl?.MapInfo?.Index;
                int valueOrDefault = index.GetValueOrDefault();
                if (mapIndex == valueOrDefault & index.HasValue
                    && clientObjectData.ItemInfo == null
                    && (clientObjectData.MonsterInfo != null && clientObjectData.MonsterInfo.Index != 16)
                    && !clientObjectData.Dead
                    && ((clientObjectData.MonsterInfo == null || !clientObjectData.Dead)
                    && string.IsNullOrEmpty(clientObjectData.PetOwner)
                    && (clientObjectData.MonsterInfo.AI >= 0 && clientObjectData.MapIndex == GameScene.Game.MapControl.MapInfo.Index)))
                {
                    if (Config.范围挂机)
                    {
                        int x1 = clientObjectData.Location.X;
                        Point androidCoord = Config.范围挂机坐标;
                        int num2 = (int)(androidCoord.X - Config.范围距离);
                        int num3;

                        if (x1 >= num2)
                        {
                            int x2 = clientObjectData.Location.X;
                            androidCoord = Config.范围挂机坐标;
                            int num4 = (int)(androidCoord.X + Config.范围距离);
                            num3 = x2 > num4 ? 1 : 0;
                        }
                        else
                            num3 = 1;

                        if (num3 == 0)
                        {
                            int y1 = clientObjectData.Location.Y;
                            androidCoord = Config.范围挂机坐标;
                            int num4 = (int)(androidCoord.Y - Config.范围距离);
                            int num5;

                            if (y1 >= num4)
                            {
                                int y2 = clientObjectData.Location.Y;
                                androidCoord = Config.范围挂机坐标;
                                int num6 = (int)(androidCoord.Y + Config.范围距离);
                                num5 = y2 > num6 ? 1 : 0;
                            }
                            else
                                num5 = 1;

                            if (num5 != 0)
                                continue;
                        }
                        else
                            continue;
                    }
                    int num7 = Functions.Distance(GameScene.Game.User.CurrentLocation, clientObjectData.Location);

                    if (minob == null)
                    {
                        minob = clientObjectData;
                        num1 = num7;
                    }
                    else
                    {
                        if (num7 < num1)
                        {
                            num1 = num7;
                            minob = clientObjectData;
                        }
                        if ((User.Class == MirClass.Assassin || (uint)User.Class <= 0U) && Functions.InRange(clientObjectData.Location, User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE))
                        {
                            List<Node> path = PathFinder.FindPath(User.CurrentLocation, Functions.PointNearTarget(User.CurrentLocation, clientObjectData.Location, 1));

                            if (path != null && num7 + 25 >= path.Count)
                                nodeList = path;
                        }
                    }
                }
            }

            if (nodeList != null && nodeList.Count > 0)
            {
                CurrentPath = nodeList;
                AutoPath = true;
            }

            return Objects.FirstOrDefault<MapObject>((Func<MapObject, bool>)(x =>
            {
                int objectId1 = (int)x.ObjectID;
                uint? objectId2 = minob?.ObjectID;
                int valueOrDefault = (int)objectId2.GetValueOrDefault();
                return objectId1 == valueOrDefault & objectId2.HasValue;
            }));
        }
        public static bool CanAttackAction(MapObject target)
        {
            return target != null && !target.Dead && (target.Race == ObjectType.Monster && string.IsNullOrEmpty(target.PetOwner) || (CEnvir.Shift || Config.免SHIFT));
        }

        // ！ 新增：统一的AutoPath清除入口
        // 用于在以下情况清除pathfinding：
        // 1. 接近目标怪物1格时
        // 2. 进入战斗时
        // 3. 长距离寻路中检测到怪物进入范围时
        private void ClearAutoPathIfNeeded()
        {
            if (!_shouldClearAutoPath)
                return;

            _shouldClearAutoPath = false;
            AutoPath = false;
            if (CurrentPath != null)
            {
                CurrentPath.Clear();
                CurrentPath = null;
            }
        }

        // ！ 新增：后台常驻位置追踪和卡住状态判断
        private void UpdateCharacterStuckState()
        {
            double timeSinceLastRecord = CEnvir.Now.Subtract(_lastPositionRecordTime).TotalSeconds;
            
            if (timeSinceLastRecord >= POSITION_RECORD_INTERVAL)
            {
                // 数组右移（丢弃最老的位置）
                _positionHistory[2] = _positionHistory[1];
                _positionHistory[1] = _positionHistory[0];
                _positionHistory[0] = User.CurrentLocation;
                _lastPositionRecordTime = CEnvir.Now;
                
                // 判断当前状态
                // 如果当前位置和前两个位置都相同，则为 Standing
                if (_positionHistory[0] == _positionHistory[1] && _positionHistory[1] == _positionHistory[2])
                {
                    _currentCharacterState = CharacterStuckState.Standing;
                }
                // 如果三个位置都在1格以内（距离 <= 1），则为 Stuck
                else if (Functions.Distance(_positionHistory[0], _positionHistory[1]) <= 1 &&
                         Functions.Distance(_positionHistory[1], _positionHistory[2]) <= 1)
                {
                    _currentCharacterState = CharacterStuckState.Stuck;
                }
                // 否则正常移动
                else
                {
                    _currentCharacterState = CharacterStuckState.Moving;
                }
                
                // ！ 每秒同时检查：是否有近身怪物需要优先攻击
                CheckAndPrioritizeNearbyMonster();
            }
        }
        
        /// <summary>
        /// ！ 新增：检查是否有更近的怪物需要优先攻击
        /// 逻辑：总是选择距离最近的怪物，每秒判断一次
        /// 如果当前目标距离为0（重叠），自动选择下一个最近的目标
        /// </summary>
        private void CheckAndPrioritizeNearbyMonster()
        {
            if (!Config.开始挂机)
                return;
            
            // 检查当前目标是否与玩家重叠（距离为0）
            if (GameScene.Game.TargetObject != null && !GameScene.Game.TargetObject.Dead)
            {
                int currentDistance = Functions.Distance(User.CurrentLocation, GameScene.Game.TargetObject.CurrentLocation);
                
                // 如果当前目标距离为0，清除目标以便选择新目标
                if (currentDistance == 0)
                {
                    GameScene.Game.TargetObject = null;
                }
            }
            
            // 查找最近的怪物（排除距离为0的怪物）
            MapObject nearestMonster = null;
            int nearestDistance = int.MaxValue;
            
            foreach (MapObject obj in Objects)
            {
                if (obj == null || obj.Dead || obj == User) continue;
                if (obj.Race != ObjectType.Monster || !string.IsNullOrEmpty(obj.PetOwner)) continue;
                
                int distance = Functions.Distance(User.CurrentLocation, obj.CurrentLocation);
                
                // 只选择距离大于0的怪物（避免重叠的怪物）
                if (distance >= 1 && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestMonster = obj;
                }
            }
            
            // 如果找到怪物，检查是否需要切换目标
            if (nearestMonster != null && nearestDistance <= SHORT_DISTANCE_DETECTION_RANGE)
            {
                // 如果当前没有目标，或者新的目标更近，则切换
                if (GameScene.Game.TargetObject == null ||
                    GameScene.Game.TargetObject.Dead ||
                    nearestDistance < Functions.Distance(User.CurrentLocation, GameScene.Game.TargetObject.CurrentLocation))
                {
                    GameScene.Game.TargetObject = nearestMonster;
                }
            }
        }

        // ！ 新增：计算怪物密度最高的区域，用于智能寻路
        /// <summary>
        /// 尝试生成一个有效的长距离寻路目标点
        /// 采用逐步扩大范围搜索的策略，确保目标点是可以到达的
        /// </summary>
        private Point GenerateValidLongDistanceTarget(int minDistance = 20, int maxDistance = 40, int maxAttempts = 5)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int distance = CEnvir.Random.Next(minDistance, maxDistance);
                MirDirection dir = (MirDirection)CEnvir.Random.Next(8);
                
                Point targetPoint = Functions.Move(User.CurrentLocation, dir, distance);
                
                // 检查目标点是否在地图范围内
                if (targetPoint.X < 0 || targetPoint.Y < 0 || targetPoint.X >= Width || targetPoint.Y >= Height)
                    continue;
                
                // 检查该点是否可以通过寻路到达
                List<Node> testPath = PathFinder.FindPath(User.CurrentLocation, targetPoint);
                if (testPath != null && testPath.Count > 0)
                {
                    return targetPoint; // 找到有效目标
                }
            }
            
            // 如果多次尝试都失败，返回当前位置附近的点（会直接返回不设置 AutoPath）
            return User.CurrentLocation;
        }

        public Point FindMonsterDenseArea()
        {
            Dictionary<Point, int> densityMap = new Dictionary<Point, int>();
            Point userLoc = User.CurrentLocation;

            // ！ 后台常驻：每秒更新一次位置历史和卡住状态
            UpdateCharacterStuckState();

            // 统计每个区域的怪物数量
            foreach (ClientObjectData clientObjectData in GameScene.Game.DataDictionary.Values)
            {
                if (GameScene.Game.MapControl.MapInfo == null) continue;
                if (clientObjectData.MapIndex != GameScene.Game.MapControl.MapInfo.Index) continue;
                if (clientObjectData.ItemInfo != null) continue;
                if (clientObjectData.MonsterInfo == null || clientObjectData.Dead) continue;
                if (!string.IsNullOrEmpty(clientObjectData.PetOwner) || clientObjectData.MonsterInfo.AI < 0) continue;

                float distance = (float)Functions.Distance(userLoc, clientObjectData.Location);
                if (distance > 20.0f) continue; // 只考虑30格内的怪物

                // 将怪物位置按5x5格区域分组统计密度
                int gridX = clientObjectData.Location.X / 5;
                int gridY = clientObjectData.Location.Y / 5;
                Point gridKey = new Point(gridX, gridY);

                if (!densityMap.ContainsKey(gridKey))
                    densityMap[gridKey] = 0;
                densityMap[gridKey]++;
            }

            // 找到密度最高的区域
            Point bestGrid = Point.Empty;
            int maxDensity = 0;

            foreach (var kvp in densityMap)
            {
                if (kvp.Value > maxDensity)
                {
                    maxDensity = kvp.Value;
                    bestGrid = kvp.Key;
                }
            }

            if (maxDensity == 0) return Point.Empty; // 没有找到怪物

            // 转换回实际坐标（取区域中心）
            Point targetPoint = new Point(bestGrid.X * 5 + 2, bestGrid.Y * 5 + 2);

            // 确保目标点在合理范围内（5-15格之间）
            float distanceToTarget = (float)Functions.Distance(userLoc, targetPoint);
            if (distanceToTarget < 5.0f)
            {
                // 太近了，往外推一点
                MirDirection dir = Functions.DirectionFromPoint(userLoc, targetPoint);
                targetPoint = Functions.Move(userLoc, dir, 8);
            }
            else if (distanceToTarget > 15.0f)
            {
                // 太远了，拉近一点
                MirDirection dir = Functions.DirectionFromPoint(userLoc, targetPoint);
                targetPoint = Functions.Move(userLoc, dir, 12);
            }

            return targetPoint;
        }

        public void ChangeAutoFightLocation()
        {
            // 仅在挂机模式下运行
            if (!Config.开始挂机)
                return;
            
            // 检查是否还在寻路中或没有到达下一次计算时间
            if (PathFinderTime >= CEnvir.Now || AutoPath)
                return;

            int x = User.CurrentLocation.X;
            int y = User.CurrentLocation.Y;
            Point monsterDenseArea = FindMonsterDenseArea();
            int distanceToMonsters = monsterDenseArea.IsEmpty ? int.MaxValue : Functions.Distance(User.CurrentLocation, monsterDenseArea);
            
            // 首先判断远距离模式还是短距离模式
            if (monsterDenseArea.IsEmpty)
            {
                // 没有怪物：长距离随机移动模式
                IsLongDistanceMode = true;
                _currentShortDistanceMode = ShortDistanceMode.Mode1_ShortDistance; // 重置短距离模式
                
                // ！ 改进：长距离寻路时检测怪物进入范围
                // 如果怪物进入SHORT_DISTANCE_DETECTION_RANGE范围，标记清除pathfinding并返回
                foreach (MapObject obj in Objects)
                {
                    if (obj == null || obj.Dead || obj == User) continue;
                    if (obj.Race != ObjectType.Monster || !string.IsNullOrEmpty(obj.PetOwner)) continue;
                    
                    if (Functions.Distance(User.CurrentLocation, obj.CurrentLocation) <= SHORT_DISTANCE_DETECTION_RANGE)
                    {
                        // 发现怪物进入范围：立即清除pathfinding并返回
                        if (AutoPath)
                        {
                            AutoPath = false;
                            if (CurrentPath != null)
                            {
                                CurrentPath.Clear();
                                CurrentPath = null;
                            }
                        }
                        GameScene.Game.TargetObject = obj;
                        return; // 立即返回，ProcessInput会处理这个怪物
                    }
                }
                
                if (Config.范围挂机)
                {
                    // ！ 改进：范围挂机使用有效的寻路目标
                    Random random1 = CEnvir.Random;
                    int minValue1 = (int)((long)Config.范围挂机坐标.X - Config.范围距离);
                    Point androidCoord = Config.范围挂机坐标;
                    int maxValue1 = (int)((long)androidCoord.X + Config.范围距离);
                    x = random1.Next(minValue1, maxValue1);
                    Random random2 = CEnvir.Random;
                    androidCoord = Config.范围挂机坐标;
                    int minValue2 = (int)((long)androidCoord.Y - Config.范围距离);
                    androidCoord = Config.范围挂机坐标;
                    int maxValue2 = (int)((long)androidCoord.Y + Config.范围距离);
                    y = random2.Next(minValue2, maxValue2);
                    PathFinderTime = CEnvir.Now.AddSeconds(8.0);
                }
                else if (Config.是否开启随机保护)
                {
                    DXItemCell dxItemCell = (GameScene.Game.InventoryBox.Grid.Grid).FirstOrDefault(X => X?.Item?.Info.ItemName == "随机传送卷");
                    if (dxItemCell != null && dxItemCell.UseItem())
                        ProtectTime = CEnvir.Now.AddSeconds(5.0);
                    return;
                }
                else
                {
                    // ！ 改进：使用有效的长距离寻路目标
                    Point validTarget = GenerateValidLongDistanceTarget(20, 40, 5);
                    if (validTarget == User.CurrentLocation)
                    {
                        // 无法找到有效的寻路目标，推迟下一次尝试
                        PathFinderTime = CEnvir.Now.AddSeconds(5.0);
                        return;
                    }
                    x = validTarget.X;
                    y = validTarget.Y;
                    PathFinderTime = CEnvir.Now.AddSeconds(10.0);
                }
            }
            else
            {
                // 有怪物：短距离模式
                IsLongDistanceMode = false;
                
                // ！ 改进：进入短距离模式时，清除之前的长距离pathfinding点
                // 这样可以安全地从长距离切换到短距离
                if (AutoPath)
                {
                    AutoPath = false;
                    if (CurrentPath != null)
                    {
                        CurrentPath.Clear();
                        CurrentPath = null;
                    }
                }
                
                // ========== 根据怪物距离决定起始模式 ==========
                // 5格以内：从模式1开始
                // 5格以上：从模式2开始
                if (_currentShortDistanceMode == ShortDistanceMode.Mode1_ShortDistance || 
                    _currentShortDistanceMode == ShortDistanceMode.Mode2_Pathfind ||
                    _currentShortDistanceMode == ShortDistanceMode.Mode3_RandomJump)
                {
                    // 已经在模式中，根据当前卡住状态判断是否切换
                    if (_currentCharacterState == CharacterStuckState.Stuck || _currentCharacterState == CharacterStuckState.Standing)
                    {
                        // 卡住或站着了：进入恢复循环
                        if (_currentShortDistanceMode == ShortDistanceMode.Mode1_ShortDistance)
                        {
                            // 模式1卡住，切换到模式2
                            _currentShortDistanceMode = ShortDistanceMode.Mode2_Pathfind;
                        }
                        else if (_currentShortDistanceMode == ShortDistanceMode.Mode2_Pathfind)
                        {
                            // 模式2还是卡住，切换到模式3
                            _currentShortDistanceMode = ShortDistanceMode.Mode3_RandomJump;
                        }
                        else if (_currentShortDistanceMode == ShortDistanceMode.Mode3_RandomJump)
                        {
                            // 模式3执行后，回到模式2继续寻路
                            _currentShortDistanceMode = ShortDistanceMode.Mode2_Pathfind;
                        }
                    }
                    else if (_currentCharacterState == CharacterStuckState.Moving)
                    {
                        // 正常移动，保持当前模式继续
                        // 不做改变
                    }
                }
                else
                {
                    // 首次进入短距离模式，根据距离决定起始模式
                    if (distanceToMonsters <= 5)
                    {
                        _currentShortDistanceMode = ShortDistanceMode.Mode1_ShortDistance;
                    }
                    else
                    {
                        _currentShortDistanceMode = ShortDistanceMode.Mode2_Pathfind;
                    }
                }
                
                // ========== 执行当前模式的移动 ==========
                switch (_currentShortDistanceMode)
                {
                    case ShortDistanceMode.Mode1_ShortDistance:
                        // 模式1：短距离移动到怪物密集区
                        x = monsterDenseArea.X;
                        y = monsterDenseArea.Y;
                        PathFinderTime = CEnvir.Now.AddSeconds(2.0);
                        break;
                        
                    case ShortDistanceMode.Mode2_Pathfind:
                        // 模式2：长距离寻路到目标密度区
                        x = monsterDenseArea.X;
                        y = monsterDenseArea.Y;
                        PathFinderTime = CEnvir.Now.AddSeconds(2.0);
                        break;
                        
                    case ShortDistanceMode.Mode3_RandomJump:
                        // 模式3：随机跳跃8格范围内的点
                        Point fallbackTarget = new Point(
                            User.CurrentLocation.X + CEnvir.Random.Next(-8, 9),
                            User.CurrentLocation.Y + CEnvir.Random.Next(-8, 9)
                        );
                        x = fallbackTarget.X;
                        y = fallbackTarget.Y;
                        PathFinderTime = CEnvir.Now.AddSeconds(2.0);
                        break;
                }
            }

            // ！ 改进：验证 pathfinding 结果，只有有效路径才设置 AutoPath
            List<Node> path = PathFinder.FindPath(User.CurrentLocation, new Point(x, y));

            if (path == null || path.Count == 0)
            {
                // 无法到达目标，推迟下一次尝试
                PathFinderTime = CEnvir.Now.AddSeconds(2.0);
                return;
            }

            CurrentPath = path;
            AutoPath = true;
        }

        public void AndroidProcess()
        {
            // ！ 修改：长距离寻路时仍允许检查怪物，但不执行战斗逻辑
            if (AutoPath && IsLongDistanceMode)
                return;

            if (GameScene.Game.TargetObject == null || GameScene.Game.TargetObject.Dead)
            {
                CurrentPath?.Clear();
            }
            else
            {
                if (!CanAttackAction(GameScene.Game.TargetObject))
                    return;

                // ！ 修复：所有职业都可以设置锁定施法目标，不再限制为法师道士
                GameScene.Game.MagicObject = GameScene.Game.TargetObject;

                // ！ 改进：使用"远程技能挂机"配置项替代职业判断
                if (Config.是否远战挂机)
                {

                    if (Config.自动躲避 && Functions.Distance(User.CurrentLocation, GameScene.Game.TargetObject.CurrentLocation) < 3)
                    {
                        MirDirection mirDirection1 = Functions.ShiftDirection(Functions.DirectionFromPoint(User.CurrentLocation, GameScene.Game.TargetObject.CurrentLocation), 4);
                        if (!CanMove(mirDirection1, 1))
                        {
                            MirDirection mirDirection2 = DirectionBest(mirDirection1, 1, GameScene.Game.TargetObject.CurrentLocation);
                            if (mirDirection2 == mirDirection1)
                            {
                                if (mirDirection1 != User.Direction)
                                    User.AttemptAction(new ObjectAction(MirAction.Standing, mirDirection1, User.CurrentLocation, Array.Empty<object>()));

                                if (!Config.远战挂机是否使用技能 || !Functions.InRange(GameScene.Game.TargetObject.CurrentLocation, User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE))
                                    return;

                                GameScene.Game.UseMagic(Config.挂机自动技能);
                                return;
                            }
                            mirDirection1 = mirDirection2;
                        }

                        if (GameScene.Game.MoveFrame && (User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip)
                        {
                            User.AttemptAction(new ObjectAction(MirAction.Moving, mirDirection1, Functions.Move(User.CurrentLocation, mirDirection1, 1), new object[2]
                            {
                                1,
                                MagicType.None
                            }));
                            return;
                        }
                    }
                }

                // 本段逻辑实际由GameScene相似代码执行，注释掉以供参考👇
                // 注：原本想要添加挂机自动施毒包括传染，结果发现这玩意已经写到了gamescene里去了。
                // if (Config.自动上毒 && User.Class == MirClass.Taoist && Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE))
                // {
                //     ClientUserMagic infectionMagic =   GameScene.Game.GetMagic(MagicType.Infection);
                //     // if (infectionMagic != null && (GameScene.Game.TargetObject.Poison & PoisonType.Infection) != PoisonType.Infection)
                //     // {
                //     //     string debugMsg = $"[AutoPoison] Try Infection: TargetID={GameScene.Game.TargetObject?.ObjectID}, Poison={GameScene.Game.TargetObject?.Poison}, MagicType={infectionMagic.Info.Magic}, Name={infectionMagic.Info.Name}, Level={infectionMagic.Level}";
                //     //     GameScene.Game.UseMagic(MagicType.Infection, GameScene.Game.TargetObject);
                //     //     return;
                //     // }
                //     // 检查PoisonDust（红毒和绿毒）
                //     // else 
                //     if ((MapObject.TargetObject.Poison & PoisonType.Red) != PoisonType.Red || (MapObject.TargetObject.Poison & PoisonType.Green) != PoisonType.Green)
                //     {
                //         GameScene.Game.UseMagic(MagicType.PoisonDust);
                //         return;
                //     }
                // }

                // 自动施毒
                // if (Config.自动上毒) GameScene.Game.AutoPoison();

                if (Config.远战挂机是否使用技能 && Config.是否远战挂机)
                {

                    if (Functions.InRange(GameScene.Game.TargetObject.CurrentLocation, User.CurrentLocation, SHORT_DISTANCE_DETECTION_RANGE))
                    {
                        var autoMagic = GameScene.Game.GetMagic(Config.挂机自动技能);
                        // string debugMsg = $"[AutoSkill] Config.挂机自动技能={Config.挂机自动技能}, MagicObj={(autoMagic == null ? "null" : ($"Type={autoMagic.Info.Magic}, Name={autoMagic.Info.Name}, Level={autoMagic.Level}"))}";
                        // GameScene.Game.ReceiveChat(debugMsg, MessageType.Hint);
                        GameScene.Game.UseMagic(Config.挂机自动技能);
                        return;
                    }

                    GameScene.Game.TargetObject = null;
                }

                if (Functions.Distance(User.CurrentLocation, GameScene.Game.TargetObject.CurrentLocation) == 1 && CEnvir.Now > User.AttackTime && User.Horse == HorseType.None)
                    User.AttemptAction(new ObjectAction(MirAction.Attack, Functions.DirectionFromPoint(User.CurrentLocation, GameScene.Game.TargetObject.CurrentLocation), User.CurrentLocation, new object[3]
                    {
                        0,
                        MagicType.None,
                        Element.None
                    }));
            }
        }
        public static void Walk(MirDirection direction)
        {
            MapObject.User.Moving(direction, 1);
        }

        public bool ForceAttack(Point target)
        {
            if (AutoPath || Config.开始挂机 && Config.是否远战挂机)
                return false;

            bool flag = false;

            if (CanAttackAction(MapObject.TargetObject))
            {
                if (Functions.Distance(target, MapObject.User.CurrentLocation) == 1)
                {
                    if (CEnvir.Now > User.AttackTime && User.Horse == HorseType.None)
                        MapObject.User.AttemptAction(new ObjectAction(MirAction.Attack, Functions.DirectionFromPoint(MapObject.User.CurrentLocation, target), MapObject.User.CurrentLocation, new object[3]
                        {
                            0,
                            MagicType.None,
                            Element.None
                        }));

                    flag = true;
                }
                else
                {
                    bool bDetour = true;
                    MirDirection direction = Functions.DirectionFromPoint(MapObject.User.CurrentLocation, target);
                    
                    if (bDetour)
                        direction = Detour(direction, target, 1);

                    if (GameScene.Game.MoveFrame && (User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip)
                    {
                        Run(direction, bDetour);
                    }
                }
            }

            return flag;
        }

        public static MirDirection Detour(MirDirection direction, Point targ, int step)
        {
            MirDirection mirDirection = direction;
            if (!GameScene.Game.MapControl.CanMove(direction, step))
                mirDirection = GameScene.Game.MapControl.DirectionBest(direction, step, targ);
            return mirDirection;
        }

        public void DigEarth()
        {
            if (!Mining)
                return;
            ClientUserItem clientUserItem = GameScene.Game.Equipment[0];
            if (MapInfo.CanMine && clientUserItem != null && (clientUserItem.CurrentDurability > 0 || clientUserItem.Info.Durability == 0) && (clientUserItem.Info.Effect == ItemEffect.PickAxe && MiningPoint.X >= 0 && (MiningPoint.Y >= 0 && MiningPoint.X < Width)) && (MiningPoint.Y < Height && Cells[MiningPoint.X, MiningPoint.Y].Flag && Functions.Distance(MiningPoint, MapObject.User.CurrentLocation) == 1) && User.Horse == HorseType.None)
            {
                if (CEnvir.Now > User.AttackTime)
                    MapObject.User.AttemptAction(new ObjectAction(MirAction.Mining, Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MiningPoint), MapObject.User.CurrentLocation, new object[1]
                    {
                        false
                    }));
            }
            else
                Mining = false;
        }


        public void AutoWalkPath()
        {
            if (CurrentPath == null || CurrentPath.Count == 0)
            {
                AutoPath = false;
                IsLongDistanceMode = false;
            }
            else
            {
                // ！ 新增：长距离移动时检查怪物，如果发现怪物则中断寻路切换到战斗模式
                if (IsLongDistanceMode && Config.开始挂机)
                {
                    Point monsterDenseArea = FindMonsterDenseArea();
                    if (!monsterDenseArea.IsEmpty)
                    {
                        // 发现怪物，中断当前长距离寻路，切换到短距离怪物追踪
                        AutoPath = false;
                        IsLongDistanceMode = false;
                        CurrentPath?.Clear();
                        
                        // 立即触发怪物追踪
                        ChangeAutoFightLocation();
                        return;
                    }
                }

                if (!GameScene.Game.MoveFrame || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip)
                    return;
                Node node1 = CurrentPath.SingleOrDefault<Node>((Func<Node, bool>)(x => User.CurrentLocation == x.Location));
                if (node1 != null)
                {
                    Node node2;
                    do
                    {
                        node2 = CurrentPath.First<Node>();
                        CurrentPath.Remove(node2);
                    }
                    while (node2 != node1);
                }

                if (CurrentPath.Count <= 0)
                {
                    AutoPath = false;
                    IsLongDistanceMode = false;
                    return;
                }

                MirDirection dir = Functions.DirectionFromPoint(User.CurrentLocation, CurrentPath.First<Node>().Location);
                
                if (!CanMove(dir, 1))
                {
                    CurrentPath = PathFinder.FindPath(MapObject.User.CurrentLocation, CurrentPath.Last<Node>().Location);
                }
                else
                {
                    int distance = 1;
                    if (GameScene.Game.CanRun && CEnvir.Now >= User.NextRunTime && User.BagWeight <= User.Stats[Stat.BagWeight] && User.WearWeight <= User.Stats[Stat.WearWeight])
                    {
                        ++distance;
                        if ((uint)User.Horse > 0U)
                            ++distance;
                    }

                    Node node2 = (Node)null;

                    for (int i = distance; i > 0; i--)
                    {
                        if (CanMove(dir, i))
                        {
                            node2 = CurrentPath.SingleOrDefault<Node>((Func<Node, bool>)(x => Functions.Move(User.CurrentLocation, dir, i) == x.Location));
                            if (node2 != null)
                            {
                                distance = i;
                                break;
                            }
                        }
                    }

                    if (node2 != null)
                        MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, dir, Functions.Move(MapObject.User.CurrentLocation, dir, distance), new object[2]
                        {
                            distance,
                            MagicType.None
                        }));
                }
            }
        }
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _MapInfo = null;
                MapInfoChanged = null;

                _Animation = 0;
                AnimationChanged = null;

                MapButtons = 0;
                MapLocation = Point.Empty;
                Mining = false;
                MiningPoint = Point.Empty;
                MiningDirection = 0;


                if (FLayer != null)
                {
                    if (!FLayer.IsDisposed)
                        FLayer.Dispose();

                    FLayer = null;
                }

                if (LLayer != null)
                {
                    if (!LLayer.IsDisposed)
                        LLayer.Dispose();

                    LLayer = null;
                }

                Cells = null;

                Width = 0;
                Height = 0;

                MapInfoObjects.Clear();
                MapInfoObjects = null;

                Objects.Clear();
                Objects = null;

                Effects.Clear();
                Effects = null;
                ViewRangeX = 0;
                ViewRangeY = 0;
                OffSetX = 0;
                OffSetY = 0;
            }

        }

        #endregion

        public sealed class Floor : DXControl
        {
            public Floor()
            {
                IsControl = false;
            }

            #region Methods
            public void CheckTexture()
            {
                if (!TextureValid)
                    CreateTexture();
            }
            
            protected override void OnClearTexture()
            {
                base.OnClearTexture();

                int minX = Math.Max(0, User.CurrentLocation.X - OffSetX - 4), maxX = Math.Min(GameScene.Game.MapControl.Width - 1, User.CurrentLocation.X + OffSetX + 4);
                int minY = Math.Max(0, User.CurrentLocation.Y - OffSetY - 4), maxY = Math.Min(GameScene.Game.MapControl.Height - 1, User.CurrentLocation.Y + OffSetY + 4);

                for (int y = minY; y <= maxY; y++)
                {
                    if (y < 0) continue;
                    if (y >= GameScene.Game.MapControl.Height) break;

                    int drawY = (y - User.CurrentLocation.Y + OffSetY) * CellHeight - User.MovingOffSet.Y;

                    for (int x = minX; x <= maxX; x++)
                    {
                        if (x < 0) continue;
                        if (x >= GameScene.Game.MapControl.Width) break;

                        int drawX = (x - User.CurrentLocation.X + OffSetX) * CellWidth - User.MovingOffSet.X;

                        Cell tile = GameScene.Game.MapControl.Cells[x, y];

                        if (y % 2 == 0 && x % 2 == 0)
                        {
                            MirLibrary library;
                            LibraryFile file;

                            if (!Libraries.KROrder.TryGetValue(tile.BackFile, out file)) continue;

                            if (!CEnvir.LibraryList.TryGetValue(file, out library)) continue;

                            library.Draw(tile.BackImage, drawX, drawY, Color.White, false, 1F, ImageType.Image);
                        }
                    }
                }

                for (int y = minY; y <= maxY; y++)
                {
                    int drawY = (y - User.CurrentLocation.Y + OffSetY + 1) * CellHeight - User.MovingOffSet.Y;

                    for (int x = minX; x <= maxX; x++)
                    {
                        int drawX = (x - User.CurrentLocation.X + OffSetX) * CellWidth - User.MovingOffSet.X;

                        Cell cell = GameScene.Game.MapControl.Cells[x, y];

                        MirLibrary library;
                        LibraryFile file;

                        if (Libraries.KROrder.TryGetValue(cell.MiddleFile, out file) && file != LibraryFile.Tilesc && CEnvir.LibraryList.TryGetValue(file, out library))
                        {
                            int index = cell.MiddleImage - 1;

                            if (cell.MiddleAnimationFrame > 1 && cell.MiddleAnimationFrame < 255)
                                continue;//   index += GameScene.Game.MapControl.Animation % cell.MiddleAnimationFrame;

                            Size s = library.GetSize(index);

                            if ((s.Width == CellWidth && s.Height == CellHeight) || (s.Width == CellWidth*2 && s.Height == CellHeight*2))
                                library.Draw(index, drawX, drawY - CellHeight, Color.White, false, 1F, ImageType.Image);
                        }

                        
                        if (Libraries.KROrder.TryGetValue(cell.FrontFile, out file) && file != LibraryFile.Tilesc && CEnvir.LibraryList.TryGetValue(file, out library))
                        {
                            int index = cell.FrontImage - 1;

                            if (cell.FrontAnimationFrame > 1 && cell.FrontAnimationFrame < 255)
                                continue;//  index += GameScene.Game.MapControl.Animation % cell.FrontAnimationFrame;

                                Size s = library.GetSize(index);

                            if ((s.Width == CellWidth && s.Height == CellHeight) || (s.Width == CellWidth*2 && s.Height == CellHeight*2))
                                library.Draw(index, drawX, drawY - CellHeight, Color.White, false, 1F, ImageType.Image);
                        }
                    }
                }
            }

            public override void Draw()
            {
            }
            protected override void DrawControl()
            {
            }

            #endregion
        }

        public sealed class Light : DXControl
        {
            public Light()
            {
                IsControl = false;
                BackColour = Color.FromArgb(15, 15, 15);
            }

            #region Methods

            public void CheckTexture()
            {
                CreateTexture();
            }

            protected override void OnClearTexture()
            {
                base.OnClearTexture();

                if (MapObject.User.Dead)
                {
                    DXManager.Device.Clear(ClearFlags.Target, Color.IndianRed, 0, 0);
                    return;
                }

                DXManager.SetBlend(true);
                DXManager.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);


                const float lightScale = 0.02F; //Players/Monsters
                const float baseSize = 0.1F;
                
                float fX;
                float fY;

                if ((MapObject.User.Poison & PoisonType.Abyss) == PoisonType.Abyss)
                {
                    DXManager.Device.Clear(ClearFlags.Target, Color.Black, 0, 0);

                    float scale = baseSize + 4  * lightScale;

                    fX = (OffSetX + MapObject.User.CurrentLocation.X - User.CurrentLocation.X) * CellWidth  + CellWidth / 2;
                    fY = (OffSetY + MapObject.User.CurrentLocation.Y - User.CurrentLocation.Y) * CellHeight;

                    fX -= (DXManager.LightWidth * scale) / 2;
                    fY -= (DXManager.LightHeight * scale) / 2;

                    fX /= scale;
                    fY /= scale;

                    DXManager.Sprite.Transform = Matrix.Scaling(scale, scale, 1);

                    DXManager.Sprite.Draw(DXManager.LightTexture, Vector3.Zero, new Vector3(fX, fY, 0), Color.White);

                    DXManager.Sprite.Transform = Matrix.Identity;

                    DXManager.SetBlend(false);
                    
                    MapObject.User.AbyssEffect.Draw();
                    return;
                }


                foreach (MapObject ob in GameScene.Game.MapControl.Objects)
                {
                    if (ob.Light > 0 && (!ob.Dead || ob == MapObject.User || ob.Race == ObjectType.Spell))
                    {
                        float scale = baseSize + ob.Light * 2 * lightScale;

                        fX = (OffSetX + ob.CurrentLocation.X - User.CurrentLocation.X) * CellWidth + ob.MovingOffSet.X - User.MovingOffSet.X + CellWidth / 2;
                        fY = (OffSetY + ob.CurrentLocation.Y - User.CurrentLocation.Y) * CellHeight + ob.MovingOffSet.Y - User.MovingOffSet.Y;

                        fX -= (DXManager.LightWidth * scale) / 2;
                        fY -= (DXManager.LightHeight * scale) / 2;

                        fX /= scale;
                        fY /= scale;

                        DXManager.Sprite.Transform = Matrix.Scaling(scale, scale, 1);

                        DXManager.Sprite.Draw(DXManager.LightTexture, Vector3.Zero, new Vector3(fX, fY, 0), ob.LightColour);

                        DXManager.Sprite.Transform = Matrix.Identity;
                    }
                }

                foreach (MirEffect ob in GameScene.Game.MapControl.Effects)
                {
                    float frameLight = ob.FrameLight;

                    if (frameLight > 0)
                    {
                        float scale = baseSize + frameLight * 2 * lightScale / 5;

                        fX = ob.DrawX + CellWidth / 2;
                        fY = ob.DrawY + CellHeight / 2;

                        fX -= (DXManager.LightWidth * scale) / 2;
                        fY -= (DXManager.LightHeight * scale) / 2;

                        fX /= scale;
                        fY /= scale;

                        DXManager.Sprite.Transform = Matrix.Scaling(scale, scale, 1);

                        DXManager.Sprite.Draw(DXManager.LightTexture, Vector3.Zero, new Vector3(fX, fY, 0), ob.FrameLightColour);

                        DXManager.Sprite.Transform = Matrix.Identity;
                    }
                }

                int minX = Math.Max(0, User.CurrentLocation.X - OffSetX - 15), maxX = Math.Min(GameScene.Game.MapControl.Width - 1, User.CurrentLocation.X + OffSetX + 15);
                int minY = Math.Max(0, User.CurrentLocation.Y - OffSetY - 15), maxY = Math.Min(GameScene.Game.MapControl.Height - 1, User.CurrentLocation.Y + OffSetY + 15);

                for (int y = minY; y <= maxY; y++)
                {
                    if (y < 0) continue;
                    if (y >= GameScene.Game.MapControl.Height) break;

                    int drawY = (y - User.CurrentLocation.Y + OffSetY)*CellHeight - User.MovingOffSet.Y;

                    for (int x = minX; x <= maxX; x++)
                    {
                        if (x < 0) continue;
                        if (x >= GameScene.Game.MapControl.Width) break;

                        int drawX = (x - User.CurrentLocation.X + OffSetX)*CellWidth - User.MovingOffSet.X;

                        Cell tile = GameScene.Game.MapControl.Cells[x, y];

                        if (tile.Light == 0) continue;

                        float scale = baseSize + tile.Light * 30 * lightScale;

                        fX = drawX + CellWidth / 2;
                        fY = drawY  + CellHeight / 2;

                        fX -= DXManager.LightWidth * scale / 2;
                        fY -= DXManager.LightHeight * scale / 2;

                        fX /= scale;
                        fY /= scale;

                        DXManager.Sprite.Transform = Matrix.Scaling(scale, scale, 1);

                        DXManager.Sprite.Draw(DXManager.LightTexture, Vector3.Zero, new Vector3(fX, fY, 0), Color.White);

                        DXManager.Sprite.Transform = Matrix.Identity;
                    }
                }


                DXManager.SetBlend(false);
            }

            public void UpdateLights()
            {
                if (Config.免蜡烛)
                {
                    BackColour = Color.White;
                    Visible = true;
                    return;
                }

                switch (GameScene.Game.MapControl.MapInfo.Light)
                {
                    case LightSetting.Default:
                        byte shading = (byte) (255*GameScene.Game.DayTime);
                        BackColour = Color.FromArgb(shading, shading, shading);
                        Visible = true;
                        break;
                    case LightSetting.Night:
                        BackColour = Color.FromArgb(15, 15, 15);
                        Visible = true;
                        break;
                    case LightSetting.Light:
                        Visible = MapObject.User != null && (MapObject.User.Poison & PoisonType.Abyss) != PoisonType.Abyss;
                        break;
                }

            }
            protected override void DrawControl()
            {
            }

            public override void Draw()
            {
            }
            #endregion
        }
    }

    public sealed class Cell
    {
        public int BackFile;
        public int BackImage;

        public int MiddleFile;
        public int MiddleImage;

        public int FrontFile;
        public int FrontImage;

        public int FrontAnimationFrame;
        public int FrontAnimationTick;

        public int MiddleAnimationFrame;
        public int MiddleAnimationTick;

        public int Light;

        public bool Flag;

        public List<MapObject> Objects;

        public bool Blocking()
        {
            if (Objects != null)
            {
                foreach (MapObject ob in Objects)
                    if (ob.Blocking) return true;
            }

            return Flag;
        }

        public void AddObject(MapObject ob)
        {
            if (Objects == null)
                Objects = new List<MapObject>();

            if (ob.Race == ObjectType.Spell)
                Objects.Insert(0, ob);
            else
                Objects.Add(ob);

            ob.CurrentCell = this;
        }

        public void RemoveObject(MapObject ob)
        {
            Objects.Remove(ob);

            if (Objects.Count == 0)
                Objects = null;

            ob.CurrentCell = null;
        }
    }

}
