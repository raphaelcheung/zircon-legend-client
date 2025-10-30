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

            if (User.Dead || (User.Poison & PoisonType.Paralysis) == PoisonType.Paralysis || User.Buffs.Any(x => x.Type == BuffType.DragonRepulse || x.Type == BuffType.FrostBite)) return; //Para or Frozen??


            if (User.MagicAction != null)
            {
                if (CEnvir.Now < MapObject.User.NextActionTime || MapObject.User.ActionQueue.Count != 0) return;

                //  if (QueuedMagic.Action == MirAction.Magic && (Spell)QueuedMagic.Extra[1] == Magic.ShoulderDash && !GameScene.Game.MoveFrame) return;

                MapObject.User.AttemptAction(User.MagicAction);
                User.MagicAction = null;
                Mining = false;
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
                if (Config.自动四花)
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
                    if (Config.自动躲避 && ((User.Class == MirClass.Wizard || User.Class == MirClass.Taoist) && (double)Functions.Distance(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation) < 18.0))
                    {
                        MirDirection mirDirection1 = Functions.ShiftDirection(Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation), 4);
                        if (!CanMove(mirDirection1, 1))
                        {
                            MirDirection mirDirection2 = DirectionBest(mirDirection1, 1, MapObject.TargetObject.CurrentLocation);
                            
                            if (mirDirection2 == mirDirection1)
                            {
                                if (mirDirection1 != User.Direction)
                                    MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, mirDirection1, MapObject.User.CurrentLocation, Array.Empty<object>()));

                                if (!Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 10))
                                    return;

                                if (Config.是否开启挂机自动技能)
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

                    if ((User.Class == MirClass.Taoist || User.Class == MirClass.Wizard) && Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 10))
                    {
                        if (Config.是否开启挂机自动技能)
                            GameScene.Game.UseMagic(Config.挂机自动技能);
                        return;
                    }
                }

                if (Functions.Distance(MapObject.TargetObject.CurrentLocation, MapObject.User.CurrentLocation) == 1 && CEnvir.Now > User.AttackTime && User.Horse == HorseType.None)
                {
                    if (Config.开始挂机 && (flag && User.Class == MirClass.Assassin && Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 10)))
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

                        if (MapObject.MouseObject is PlayerObject && MapObject.MouseObject != MapObject.User && CEnvir.Ctrl) 
                            break;

                        if (!GameScene.Game.MoveFrame || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip) 
                            break;
                        
                        if (Functions.InRange(MapLocation, MapObject.User.CurrentLocation, 2))
                        {
                            if (direction != User.Direction)
                                MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                            
                            return;
                        }

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
                    best = MouseDirectionBest(direction, 1);

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

            if (bestDistance > 10.0 || newTarget == null)
                return null;

            if (User.Class == MirClass.Assassin || User.Class == MirClass.Warrior)
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

        //public void ProcessInput2()
        //{
        //    bool bDetour = true;
        //    if (GameScene.Game.Observer || User == null || (User.Dead || (User.Poison & PoisonType.Paralysis) == PoisonType.Paralysis || User.Buffs.Any<ClientBuffInfo>((Func<ClientBuffInfo, bool>)(x =>
        //    {
        //        if (x.Type != BuffType.DragonRepulse)
        //            return x.Type == BuffType.FrostBite;
        //        return true;
        //    }))))
        //        return;
        //    if (User.MagicAction != null)
        //    {
        //        if (CEnvir.Now < MapObject.User.NextActionTime || (uint)MapObject.User.ActionQueue.Count > 0U)
        //            return;
        //        MapObject.User.AttemptAction(User.MagicAction);
        //        User.MagicAction = (ObjectAction)null;
        //        Mining = false;
        //    }
        //    //bool haselementalhurricane = MapObject.User.VisibleBuffs.Contains(BuffType.ElementalHurricane);
        //    if (Config.开始挂机)
        //    {
        //        //if (!haselementalhurricane)
        //        {
        //            if (GameScene.Game.TargetObject == null || GameScene.Game.TargetObject.Dead || !Functions.InRange(GameScene.Game.TargetObject.CurrentLocation, MapControl.User.CurrentLocation, 10))
        //            {
        //                MapObject mapObject = null;

        //                //if (GameScene.Game.User.XunzhaoGuaiwuMoshi01)
        //                //    mapObject = LaoSelectMonster();
        //                //else if (GameScene.Game.User.XunzhaoGuaiwuMoshi02)
        //                //    mapObject = SelectMonster();
        //                //else if (!GameScene.Game.User.XunzhaoGuaiwuMoshi01 && !GameScene.Game.User.XunzhaoGuaiwuMoshi02)
        //                    mapObject = SelectMonsterTarget();

        //                int num;
        //                if (mapObject != null)
        //                {
        //                    int objectId1 = (int)mapObject.ObjectID;
        //                    uint? objectId2 = GameScene.Game.TargetObject?.ObjectID;
        //                    int valueOrDefault = (int)objectId2.GetValueOrDefault();
        //                    num = !(objectId1 == valueOrDefault & objectId2.HasValue) ? 1 : 0;
        //                }
        //                else
        //                    num = 0;
        //                if (num != 0)
        //                    GameScene.Game.TargetObject = mapObject;
        //                else
        //                    ChangeAutoFightLocation();
        //            }
        //            else
        //                AndroidProcess();
        //        }
        //    }
        //    MirDirection mirDirection1 = MouseDirection();
        //    if (GameScene.Game.AutoRun)//if(GameScene.Game.AutoRun && !haselementalhurricane)
        //    {
        //        if (!GameScene.Game.MoveFrame || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip)
        //            return;
        //        Run(mirDirection1, true);
        //    }
        //    else
        //    {
        //        if (MouseControl == this)
        //        {
        //            switch (MapButtons)
        //            {
        //                case MouseButtons.Left:
        //                    Mining = false;
        //                    if (MapLocation == MapObject.User.CurrentLocation)
        //                    {

        //                        if (CEnvir.Now <= GameScene.Game.PickUpTime) return;

        //                        CEnvir.Enqueue(new C.PickUp());
        //                        GameScene.Game.PickUpTime = CEnvir.Now.AddMilliseconds(250);

        //                        return;
        //                    }
        //                    if (MapObject.TargetObject == null && (Config.免SHIFT || CEnvir.Shift))
        //                    {
        //                        if (!(CEnvir.Now > User.AttackTime) || User.Horse != HorseType.None)
        //                            return;

        //                        MapObject.User.AttemptAction(new ObjectAction(MirAction.Attack, mirDirection1, MapObject.User.CurrentLocation, new object[3]
        //                        {
        //                            0,
        //                            MagicType.None,
        //                            Element.None
        //                        }));

        //                        return;
        //                    }
        //                    if (CEnvir.Alt)
        //                    {
        //                        if (User.Horse != HorseType.None)
        //                            return;

        //                        MapObject.User.AttemptAction(new ObjectAction(MirAction.Harvest, mirDirection1, MapObject.User.CurrentLocation, Array.Empty<object>()));
        //                        return;
        //                    }


        //                    if (AutoPath)
        //                        AutoPath = false;

        //                    if (MapObject.MouseObject == null || MapObject.MouseObject.Race == ObjectType.Item || MapObject.MouseObject.Dead)
        //                    {
        //                        ClientUserItem clientUserItem = GameScene.Game.Equipment[0];
        //                        if (MapInfo.CanMine && clientUserItem != null && clientUserItem.Info.Effect == ItemEffect.PickAxe) //if (!haselementalhurricane && MapInfo.CanMine && clientUserItem != null && clientUserItem.Info.Effect == ItemEffect.PickAxe)
        //                        {
        //                            MiningPoint = Functions.Move(User.CurrentLocation, mirDirection1, 1);
        //                            if (MiningPoint.X >= 0 && MiningPoint.Y >= 0 && (MiningPoint.X < Width && MiningPoint.Y < Height) && Cells[MiningPoint.X, MiningPoint.Y].Flag)
        //                            {
        //                                Mining = true;
        //                                break;
        //                            }
        //                        }
        //                        if (!CanMove(mirDirection1, 1))
        //                        {
        //                            MirDirection mirDirection2 = mirDirection1;
        //                            if (bDetour)
        //                                mirDirection2 = MouseDirectionBest(mirDirection1, 1);
        //                            if (mirDirection2 == mirDirection1)
        //                            {
        //                                if (mirDirection1 == User.Direction)
        //                                    return;
        //                                Run(mirDirection1, bDetour);
        //                                return;
        //                            }
        //                            mirDirection1 = mirDirection2;
        //                        }
        //                        if (!GameScene.Game.MoveFrame || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip)
        //                            return;

        //                        Walk(mirDirection1);
        //                        return;
        //                    }
        //                    break;
        //                case MouseButtons.Right:
        //                    Mining = false;


        //                    if (AutoPath)
        //                        AutoPath = false;


        //                    if ((!(MapObject.MouseObject is PlayerObject) || MapObject.MouseObject == MapObject.User || !CEnvir.Ctrl) && (GameScene.Game.MoveFrame && (MapControl.User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip))
        //                    {
        //                        if (Functions.InRange(MapLocation, MapObject.User.CurrentLocation, 1))
        //                        {
        //                            if (mirDirection1 == User.Direction)
        //                                return;
        //                            MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, mirDirection1, MapObject.User.CurrentLocation, Array.Empty<object>()));
        //                            return;
        //                        }
        //                        Run(mirDirection1, bDetour);
        //                        return;
        //                    }
        //                    break;
        //            }
        //        }

        //        if (MapObject.TargetObject != null)
        //        {
        //            if (UpdateTarget < CEnvir.Now)
        //            {
        //                UpdateTarget = CEnvir.Now.AddMilliseconds(200.0);
        //                TargetLocation = MapObject.TargetObject.CurrentLocation;
        //            }
        //            if (ForceAttack(TargetLocation))
        //                return;
        //        }

        //        DigEarth();

        //        if (!AutoPath) return;

        //        AutoWalkPath();
        //    }
        //}
        public static bool CanAttackAction(MapObject target)
        {
            return target != null && !target.Dead && (target.Race == ObjectType.Monster && string.IsNullOrEmpty(target.PetOwner) || (CEnvir.Shift || Config.免SHIFT));
        }
        public void ChangeAutoFightLocation()
        {
            if (PathFinderTime >= CEnvir.Now || AutoPath)
                return;

            int x = User.CurrentLocation.X;
            int y = User.CurrentLocation.Y;

            if (Config.范围挂机)
            {
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
            }
            else if (Config.是否开启随机保护)
            {
                DXItemCell dxItemCell = (GameScene.Game.InventoryBox.Grid.Grid).FirstOrDefault(X => X?.Item?.Info.ItemName == "随机传送卷");
                if (dxItemCell != null && dxItemCell.UseItem())
                    ProtectTime = CEnvir.Now.AddSeconds(5.0);
            }
            else
            {
                Point currentLocation = User.CurrentLocation;
                x = CEnvir.Random.Next(currentLocation.X - 5, currentLocation.X + 5);
                y = CEnvir.Random.Next(currentLocation.Y - 5, currentLocation.Y + 5);
            }

            List<Node> path = PathFinder.FindPath(User.CurrentLocation, new Point(x, y));

            if (path == null || path.Count == 0)
                return;

            CurrentPath = path;
            AutoPath = true;
        }

        public void AndroidProcess()
        {
            if (AutoPath)
                return;

            if (GameScene.Game.TargetObject == null || GameScene.Game.TargetObject.Dead)
            {
                CurrentPath?.Clear();
            }
            else
            {
                if (!CanAttackAction(GameScene.Game.TargetObject))
                    return;

                if (User.Class == MirClass.Wizard || User.Class == MirClass.Taoist)
                {
                    GameScene.Game.MagicObject = GameScene.Game.TargetObject;

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

                                if (!Config.是否开启挂机自动技能 || !Functions.InRange(GameScene.Game.TargetObject.CurrentLocation, User.CurrentLocation, 10))
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
                if (Config.自动上毒 && User.Class == MirClass.Taoist && Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 10) && ((MapObject.TargetObject.Poison & PoisonType.Red) != PoisonType.Red || (MapObject.TargetObject.Poison & PoisonType.Green) != PoisonType.Green))
                    GameScene.Game.UseMagic(MagicType.PoisonDust);

                if (Config.是否开启挂机自动技能 && (User.Class == MirClass.Taoist || User.Class == MirClass.Wizard))
                {
                    if (Functions.InRange(GameScene.Game.TargetObject.CurrentLocation, User.CurrentLocation, 10))
                    {
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
            if (AutoPath || Config.开始挂机 && (User.Class == MirClass.Taoist || User.Class == MirClass.Wizard))
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
                        int num = Functions.Distance(target, MapObject.User.CurrentLocation);
                        if (num == 2)
                            Walk(direction);
                        else if (num > 2)
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
            }
            else
            {
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
                    return;

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
