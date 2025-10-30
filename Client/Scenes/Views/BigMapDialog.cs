using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.UserModels;
using Library;
using Library.SystemModels;
using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Scenes.Views
{
    public sealed  class BigMapDialog : DXWindow
    {
        #region Properties

        #region SelectedInfo

        public MapInfo SelectedInfo
        {
            get { return _SelectedInfo; }
            set
            {
                if (_SelectedInfo == value) return;

                MapInfo oldValue = _SelectedInfo;
                _SelectedInfo = value;

                OnSelectedInfoChanged(oldValue, value);
            }
        }
        private MapInfo _SelectedInfo;
        public event EventHandler<EventArgs> SelectedInfoChanged;
        public void OnSelectedInfoChanged(MapInfo oValue, MapInfo nValue)
        {
            SelectedInfoChanged?.Invoke(this, EventArgs.Empty);

            foreach (DXControl control in MapInfoObjects.Values)
                control.Dispose();

            MapInfoObjects.Clear();

            if (SelectedInfo == null) return;

            RefreshTitle();
            Image.Index = SelectedInfo.MiniMap;

            SetClientSize(Image.Size);
            Location = new Point((GameScene.Game.Size.Width - Size.Width) / 2, (GameScene.Game.Size.Height - Size.Height) / 2);

            Size size = GetMapSize(SelectedInfo.FileName);
            ScaleX = Image.Size.Width / (float)size.Width;
            ScaleY = Image.Size.Height / (float)size.Height;

            // 设置地图本身的hint为初始坐标
            Image.Hint = "(0, 0)";

            foreach (NPCInfo ob in Globals.NPCInfoList.Binding)
                Update(ob);

            foreach (MovementInfo ob in Globals.MovementInfoList.Binding)
                Update(ob);

            foreach (ClientObjectData ob in GameScene.Game.DataDictionary.Values)
                Update(ob);


        }
        private Size GetMapSize(string fileName)
        {
            if (!File.Exists(Config.MapPath + fileName + ".map")) return Size.Empty;

            using (FileStream stream = File.OpenRead(Config.MapPath + fileName + ".map"))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                stream.Seek(22, SeekOrigin.Begin);
                
                return new Size(reader.ReadInt16(), reader.ReadInt16());
            }
        }

        #endregion
        public DateTime ClickTick { get; set; }
        private int bSearching;


        public Rectangle Area;
        public DXImageControl Image;
        public DXControl Panel;
        
        public static float ScaleX, ScaleY;

        private Point _lastCoordinateHint = Point.Empty; // 记录上一次显示的坐标，避免频繁更新

        public Dictionary<object, DXControl> MapInfoObjects = new Dictionary<object, DXControl>();

        public override void OnClientAreaChanged(Rectangle oValue, Rectangle nValue)
        {
            base.OnClientAreaChanged(oValue, nValue);

            if (Panel == null) return;

            Panel.Location = ClientArea.Location;
            Panel.Size = ClientArea.Size;
        }
        public override void OnIsVisibleChanged(bool oValue, bool nValue)
        {
            base.OnIsVisibleChanged(oValue, nValue);

            SelectedInfo = IsVisible ? GameScene.Game.MapControl.MapInfo : null;

        }
        public override void OnOpacityChanged(float oValue, float nValue)
        {
            base.OnOpacityChanged(oValue, nValue);

            foreach (DXControl control in Controls)
                control.Opacity = Opacity;

            foreach (DXControl control in MapInfoObjects.Values)
                control.Opacity = Opacity;

            if (Image != null)
            {
                Image.Opacity = Opacity;
                Image.ImageOpacity = Opacity;
            }
        }


        public override WindowType Type => WindowType.None;
        public override bool CustomSize => false;
        public override bool AutomaticVisiblity => false;

        #endregion

        public BigMapDialog()
        {
            BackColour = Color.Black;
            HasFooter = false;

            AllowResize = true;

            Panel = new DXControl
            {
                Parent = this,
                Location = Area.Location,
                Size = Area.Size
            };

            Image = new DXImageControl
            {
                Parent = Panel,
                LibraryFile = LibraryFile.MiniMap,
            };
            Image.MouseClick += Image_MouseClick;
            Image.MouseMove += Image_MouseMove;
            Image.MouseLeave += Image_MouseLeave;
        }

        private void Image_MouseClick(object sender, MouseEventArgs e)
        {

            //if (SelectedInfo != GameScene.Game.MapControl.MapInfo) return;

            //if (MapObject.User.Buffs.All(z => z.Type != BuffType.Developer))
            //if (!SelectedInfo.AllowRT || !SelectedInfo.AllowTT || !GameScene.Game.MapControl.MapInfo.AllowRT || !GameScene.Game.MapControl.MapInfo.AllowTT) return;
            GameScene.Game.MapControl.AutoPath = false;


            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                //TODO Teleport Ring
                int x = (int)((e.Location.X - Image.DisplayArea.X) / ScaleX);
                int y = (int)((e.Location.Y - Image.DisplayArea.Y) / ScaleY);
               
                CEnvir.Enqueue(new C.TeleportRing { Location = new Point(x, y), Index = SelectedInfo.Index });
            }
            else
            {
                int x = (int)((double)(e.Location.X - Image.DisplayArea.X) / (double)ScaleX);
                int y = (int)((double)(e.Location.Y - Image.DisplayArea.Y) / (double)ScaleY);

                if ((e.Button & MouseButtons.Left) != MouseButtons.Left)
                    return;

                DateTime clickTick = ClickTick;
                ClickTick = CEnvir.Now;
                if (clickTick.AddSeconds(1.0) > ClickTick)
                    GameScene.Game.ReceiveChat("你点击的太快了，请稍侯再试。。。", MessageType.System);
                else if (Interlocked.Exchange(ref bSearching, 1) == 0)
                {
                    try
                    {
                        GameScene.Game.MapControl.AutoPath = false;
                        PathFinder pathFinder = new PathFinder(GameScene.Game.MapControl);
                        List<Node> path = pathFinder.FindPath(MapObject.User.CurrentLocation, new Point(x, y));
                        if (path == null || path.Count == 0)
                        {
                            GameScene.Game.ReceiveChat("无法找到合适的路径", MessageType.System);
                        }
                        else
                        {
                            GameScene.Game.MapControl.PathFinder = pathFinder;
                            GameScene.Game.MapControl.CurrentPath = path;
                            GameScene.Game.MapControl.AutoPath = true;
                        }
                    }
                    finally
                    {
                        Interlocked.Exchange(ref bSearching, 0);
                    }
                }
                else
                    GameScene.Game.ReceiveChat("正在为你查找合适的线路，请稍等。。。", MessageType.System);
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            // 计算鼠标在地图上的坐标并更新地图hint
            int x = (int)((double)(e.Location.X - Image.DisplayArea.X) / (double)ScaleX);
            int y = (int)((double)(e.Location.Y - Image.DisplayArea.Y) / (double)ScaleY);
            
            // 限制坐标在地图范围内
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            Size mapSize = GetMapSize(SelectedInfo?.FileName ?? "");
            if (x >= mapSize.Width) x = mapSize.Width - 1;
            if (y >= mapSize.Height) y = mapSize.Height - 1;
            
            Point currentCoord = new Point(x, y);
            
            // 只有当坐标真正改变时才更新hint，避免频繁重绘
            if (_lastCoordinateHint != currentCoord)
            {
                _lastCoordinateHint = currentCoord;
                Image.Hint = $"({x}, {y})";
            }
        }

        private void Image_MouseLeave(object sender, EventArgs e)
        {
            // 鼠标离开时保持hint不变，这是地图的常驻hint
            // 不清除hint，因为这是地图本身的属性
        }
        private void RefreshTitle()
        {
            TitleLabel.Text = $"{GameScene.Game.MapControl?.MapInfo?.Description ?? "???"} ({X}，{Y})";
        }
        #region Methods
        public override void Draw()
        {
            if (!IsVisible || Size.Width == 0 || Size.Height == 0) return;

            OnBeforeDraw();
            DrawControl();
            OnBeforeChildrenDraw();
            DrawChildControls();
            DrawWindow();
            TitleLabel.Draw();
            DrawBorder();
            OnAfterDraw();
        }
        public void Update(NPCInfo ob)
        {
            if (SelectedInfo == null) return;

            DXControl control;

            if (!MapInfoObjects.TryGetValue(ob, out control))
            {
                if (ob.Region?.Map != SelectedInfo) return;

                control = GameScene.Game.GetNPCControl(ob);
                control.Parent = Image;
                control.Opacity = Opacity;
                MapInfoObjects[ob] = control;
            }
            else if ((QuestIcon)control.Tag == ob.CurrentIcon) return;

                control.Dispose();
                MapInfoObjects.Remove(ob);
            if (ob.Region?.Map != SelectedInfo)  return;

            control = GameScene.Game.GetNPCControl(ob);
            control.Parent = Image;
            control.Opacity = Opacity;
            MapInfoObjects[ob] = control;

            Size size = GetMapSize(SelectedInfo.FileName);

            if (ob.Region.PointList == null)
                ob.Region.CreatePoints(size.Width);

            int minX = size.Width, maxX = 0, minY = size.Height, maxY = 0;

            foreach (Point point in ob.Region.PointList)
            {
                if (point.X < minX)
                    minX = point.X;
                if (point.X > maxX)
                    maxX = point.X;

                if (point.Y < minY)
                    minY = point.Y;
                if (point.Y > maxY)
                    maxY = point.Y;
            }

            int x = (minX + maxX) / 2;
            int y = (minY + maxY) / 2;


            control.Location = new Point((int)(ScaleX * x) - control.Size.Width / 2, (int)(ScaleY * y) - control.Size.Height / 2);
        }
        public void Update(MovementInfo ob)
        {
            if (ob.SourceRegion == null || ob.SourceRegion.Map != SelectedInfo) return;
            if (ob.DestinationRegion?.Map == null || ob.Icon == MapIcon.None) return;

            Size size = GetMapSize(SelectedInfo.FileName);

            if (ob.SourceRegion.PointList == null)
                ob.SourceRegion.CreatePoints(size.Width);

            int minX = size.Width, maxX = 0, minY = size.Height, maxY = 0;

            foreach (Point point in ob.SourceRegion.PointList)
            {
                if (point.X < minX)
                    minX = point.X;
                if (point.X > maxX)
                    maxX = point.X;

                if (point.Y < minY)
                    minY = point.Y;
                if (point.Y > maxY)
                    maxY = point.Y;
            }

            int x = (minX + maxX)/2;
            int y = (minY + maxY)/2;


            DXImageControl control;
            MapInfoObjects[ob] = control = new DXImageControl
            {
                LibraryFile = LibraryFile.Interface,
                Parent = Image,
                Opacity =  Opacity,
                ImageOpacity =  Opacity,
                Hint = ob.DestinationRegion.Map.Description,
            };
            control.OpacityChanged += (o, e) => control.ImageOpacity = control.Opacity;

            switch (ob.Icon)
            {
                case MapIcon.Cave:
                    control.Index = 70;
                    control.ForeColour = Color.Red;
                    break;
                case MapIcon.Exit:
                    control.Index = 70;
                    control.ForeColour = Color.Green;
                    break;
                case MapIcon.Down:
                    control.Index = 70;
                    control.ForeColour = Color.MediumVioletRed;
                    break;
                case MapIcon.Up:
                    control.Index = 70;
                    control.ForeColour = Color.DeepSkyBlue;
                    break;
                case MapIcon.Province:
                    control.Index = 6125;
                    control.LibraryFile = LibraryFile.GameInter;
                    break;
                case MapIcon.Building:
                    control.Index = 6124;
                    control.LibraryFile = LibraryFile.GameInter;
                    break;
            }
            control.MouseClick += (o, e) => SelectedInfo = ob.DestinationRegion.Map;
            control.Location = new Point((int) (ScaleX*x) - control.Size.Width/2, (int) (ScaleY*y) - control.Size.Height/2);
        }
        public void Update(ClientObjectData ob)
        {
            if (SelectedInfo == null) return;


            DXControl control;

            if (!MapInfoObjects.TryGetValue(ob, out control))
            {
                if (ob.MapIndex != SelectedInfo.Index) return;
                if (ob.ItemInfo != null && ob.ItemInfo.Rarity == Rarity.Common) return;
                if (ob.MonsterInfo != null && ob.Dead) return;


                MapInfoObjects[ob] = control = new DXControl
                {
                    DrawTexture = true,
                    Parent = Image,
                    Opacity =  Opacity,
                };


            }
            else if (ob.MapIndex != SelectedInfo.Index || (ob.MonsterInfo != null && ob.Dead) || (ob.ItemInfo != null && ob.ItemInfo.Rarity == Rarity.Common))
            {
                control.Dispose();
                MapInfoObjects.Remove(ob);
                return;
            }
            
            Size size = new Size(3, 3);
            Color colour = Color.White;
            string name = ob.Name;

            if (ob.MonsterInfo != null)
            {

                name = $"{ob.MonsterInfo.MonsterName}";
                if (ob.MonsterInfo.AI < 0)
                {
                    colour = Color.LightBlue;
                }
                else
                {
                    colour = Color.Red;

                    if (GameScene.Game.HasQuest(ob.MonsterInfo, GameScene.Game.MapControl.MapInfo))
                        colour = Color.Orange;
                }

                if (ob.MonsterInfo.IsBoss)
                {
                    size = new Size(5, 5);

                    if (control.Controls.Count == 0) // This is disgusting but its cheap
                    {
                        new DXControl
                        {
                            Parent = control,
                            Location = new Point(1, 1),
                            BackColour = colour,
                            DrawTexture = true,
                            Size = new Size(3, 3)
                        };
                    }
                    else
                        control.Controls[0].BackColour = colour;

                    colour = Color.White;

                }

                if (!string.IsNullOrEmpty(ob.PetOwner))
                {
                    name += $" ({ob.PetOwner})";
                    control.DrawTexture = false;
                }
            }
            else if (ob.ItemInfo != null)
            {
                colour = Color.DarkBlue;
            }
            else
            {
                if (MapObject.User.ObjectID == ob.ObjectID)
                {
                    // 当前人物：6x6 magenta外框，内部2x2 cyan点，外框宽度2
                    size = new Size(6, 6);
                    
                    new DXControl
                    {
                        Parent = control,
                        Location = new Point(2, 2),
                        BackColour = Color.Cyan,
                        DrawTexture = true,
                        Size = new Size(2, 2)
                    };

                    colour = Color.Magenta;
                    X = ob.Location.X;
                    Y = ob.Location.Y;
                    RefreshTitle();
                }
                else if (GameScene.Game.Observer)
                {
                    control.Visible = false;
                }
                else if (GameScene.Game.GroupBox.Members.Any(x => x.ObjectID == ob.ObjectID))
                {
                    colour = Color.Blue;
                }
                else if (GameScene.Game.Partner != null && GameScene.Game.Partner.ObjectID == ob.ObjectID) 
                {
                    colour = Color.DeepPink;
                }
                else if (GameScene.Game.GuildBox.GuildInfo != null && GameScene.Game.GuildBox.GuildInfo.Members.Any(x => x.ObjectID == ob.ObjectID))
                {
                    colour = Color.DeepSkyBlue;
                }
            }

            control.Hint = name;
            control.BackColour = colour;
            control.Size = size;
            control.Location = new Point((int) (ScaleX*ob.Location.X) - size.Width/2, (int) (ScaleY*ob.Location.Y) - size.Height/2);
        }

        public void Remove(object ob)
        {
            DXControl control;

            if (!MapInfoObjects.TryGetValue(ob, out control)) return;

            control.Dispose();
            MapInfoObjects.Remove(ob);
        }

        private int X = 0;
        private int Y = 0;
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _SelectedInfo = null;
                SelectedInfoChanged = null;

                Area = Rectangle.Empty;
                ScaleX = 0;
                ScaleY = 0;

                foreach (KeyValuePair<object, DXControl> pair in MapInfoObjects)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }

                MapInfoObjects.Clear();
                MapInfoObjects = null;


                if (Image != null)
                {
                    if (!Image.IsDisposed)
                        Image.Dispose();

                    Image = null;
                }
                
                if (Panel != null)
                {
                    if (!Panel.IsDisposed)
                        Panel.Dispose();

                    Panel = null;
                }
            }
        }

        #endregion
    }
}
