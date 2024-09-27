using System;
using System.Drawing;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.UserModels;
using Library;
using C = Library.Network.ClientPackets;


namespace Client.Scenes.Views
{
    public sealed class InventoryDialog : DXWindow
    {
        #region Properties

        public DXItemGrid Grid;

        public DXLabel GoldLabel, WeightLabel, GameGoldLabel;
        public override void OnIsVisibleChanged(bool oValue, bool nValue)
        {
            if (!IsVisible)
                Grid.ClearLinks();

            base.OnIsVisibleChanged(oValue, nValue);
        }

        public override WindowType Type => WindowType.InventoryBox;
        public override bool CustomSize => false;
        public override bool AutomaticVisiblity => true;
        public DXButton CanSortItem { get; set; }
        public DXButton StorageBox { get; set; }
        public DXButton Jyhuishou { get; set; }
        public DXImageControl Image;
        public DXImageControl BagWeightBar;
        private DXTabControl TabControl;
        private DXTab GridTab;
        //private DXTab PatchTab, BaoshiTab;
        //public DXItemGrid PatchGrid, BaoshiGrid;
        //public DXVScrollBar PatchGridScrollBar, BaoshiScrollBar;
        public DXVScrollBar GridScrollBar;
        public DXImageControl RefreshPatch;

        #endregion

        public InventoryDialog()
        {
            TitleLabel.Text = "背包";
            SetClientSize(new Size(275, 352));
            DXTabControl dxTabControl = new DXTabControl();
            dxTabControl.Parent = this;
            dxTabControl.Location = ClientArea.Location;
            dxTabControl.Size = ClientArea.Size;
            TabControl = dxTabControl;
            DXTab dxTab1 = new DXTab();
            dxTab1.Parent = TabControl;
            dxTab1.Border = true;
            dxTab1.TabButton.Label.Text = "一般";
            GridTab = dxTab1;

            DXImageControl dxImageControl5 = new DXImageControl()
            {
                LibraryFile = LibraryFile.Interface,
                Index = 122,
                Location = new Point(1, 1),
                Parent = dxTab1,
            };

            DXVScrollBar GridScrollBar = new()
            {
                Parent = GridTab,
                Size = new Size(16, 248),
                Location = new Point(ClientArea.Right - 30, 5),
                VisibleSize = 7,
                Change = 1,
            };

            DXTab dxTab2 = new DXTab();
            dxTab2.Parent = TabControl;
            dxTab2.Border = true;
            dxTab2.TabButton.Label.Text = "碎片";
            //PatchTab = dxTab2;
            DXImageControl dxImageControl6 = new DXImageControl()
            {
                LibraryFile = LibraryFile.Interface,
                Index = 122,
                Location = new Point(1, 1),
                Parent = dxTab2,
            };
            //DXTab dxTab3 = new DXTab();
            //dxTab3.Parent = TabControl;
            //dxTab3.Border = true;
            //dxTab3.TabButton.Label.Text = "宝石";
            //BaoshiTab = dxTab3;
            //DXImageControl dxImageControl7 = new DXImageControl()
            //{
            //    LibraryFile = LibraryFile.Interface,
            //    Index = 122,
            //    Location = new Point(1, 1),
            //    Parent = dxTab3,
            //};

            DXItemGrid dxItemGrid1 = new DXItemGrid();
            dxItemGrid1.GridSize = new Size(7, 7);
            dxItemGrid1.Parent = GridTab;
            dxItemGrid1.Location = new Point(5, 5);
            dxItemGrid1.ItemGrid = GameScene.Game.Inventory;
            dxItemGrid1.GridType = GridType.Inventory;
            Grid = dxItemGrid1;

            //DXItemGrid dxItemGrid2 = new DXItemGrid();
            //dxItemGrid2.GridSize = new Size(7, 7);
            //dxItemGrid2.Parent = PatchTab;
            //dxItemGrid2.Location = new Point(5, 5);
            //dxItemGrid2.GridType = GridType.PatchGrid;
            //dxItemGrid2.ItemGrid = GameScene.Game.PatchGrid;
            //dxItemGrid2.VisibleHeight = 7;
            //PatchGrid = dxItemGrid2;
            //PatchGrid.GridSizeChanged += new EventHandler<EventArgs>(PatchGrid_GridSizeChanged);
            //DXVScrollBar dxvScrollBar2 = new();
            //dxvScrollBar2.Parent = PatchTab;
            //dxvScrollBar2.Size = new Size(16, 248);
            //dxvScrollBar2.Location = new Point(ClientArea.Right - 30, 5);
            //dxvScrollBar2.VisibleSize = 7;
            //dxvScrollBar2.Change = 1;
            //PatchGridScrollBar = dxvScrollBar2;
            //PatchGridScrollBar.ValueChanged += new EventHandler<EventArgs>(PatchGridScrollBar_ValueChanged);
            //foreach (DXControl dxControl in PatchGrid.Grid)
            //    dxControl.MouseWheel += new EventHandler<MouseEventArgs>(PatchGridScrollBar.DoMouseWheel);

            //DXItemGrid dxItemBaoshi3 = new DXItemGrid();
            //dxItemBaoshi3.GridSize = new Size(7, 7);
            //dxItemBaoshi3.Parent = BaoshiTab;
            //dxItemBaoshi3.Location = new Point(5, 5);
            //dxItemBaoshi3.GridType = GridType.BaoshiItems;
            //dxItemBaoshi3.ItemGrid = GameScene.Game.BaoshiGrid;
            //dxItemBaoshi3.VisibleHeight = 7;
            //BaoshiGrid = dxItemBaoshi3;
            //BaoshiGrid.GridSizeChanged += new EventHandler<EventArgs>(BaoshiGrid_GridSizeChanged);

            //DXVScrollBar dxvScrollBar3 = new();
            //dxvScrollBar3.Parent = BaoshiTab;
            //dxvScrollBar3.Size = new Size(16, 248);
            //dxvScrollBar3.Location = new Point(ClientArea.Right - 30, 5);
            //dxvScrollBar3.VisibleSize = 7;
            //dxvScrollBar3.Change = 1;
            //BaoshiScrollBar = dxvScrollBar3;
            //BaoshiScrollBar.ValueChanged += new EventHandler<EventArgs>(BaoshiGridScrollBar_ValueChanged);
            //foreach (DXControl dxControl in BaoshiGrid.Grid)
            //    dxControl.MouseWheel += new EventHandler<MouseEventArgs>(BaoshiScrollBar.DoMouseWheel);



            DXImageControl dxImageControl1 = new DXImageControl();
            dxImageControl1.LibraryFile = LibraryFile.Interface;
            dxImageControl1.Index = 108;
            dxImageControl1.Location = new Point(ClientArea.Left + 15, ClientArea.Bottom - 76);
            dxImageControl1.Parent = this;
            Image = dxImageControl1;
            DXImageControl dxImageControl2 = new DXImageControl();
            dxImageControl2.Parent = this;
            dxImageControl2.LibraryFile = LibraryFile.Interface;
            dxImageControl2.Index = 109;
            BagWeightBar = dxImageControl2;
            DXImageControl bagWeightBar = BagWeightBar;
            int x1 = ClientArea.Left + 60;
            Rectangle clientArea = ClientArea;
            int y1 = clientArea.Bottom - 71;
            Point point = new Point(x1, y1);
            bagWeightBar.Location = point;
            BagWeightBar.AfterDraw += (o, e) =>
            {
                if (BagWeightBar.Library == null || MapObject.User.Stats[Stat.BagWeight] == 0)
                    return;
                float num = Math.Min(1f, Math.Max(0.0f, MapObject.User.BagWeight / (float)MapObject.User.Stats[Stat.BagWeight]));
                if (num == 0.0)
                    return;
                MirImage image = BagWeightBar.Library.CreateImage(110, ImageType.Image);
                if (image == null)
                    return;
                PresentTexture(image.Image, this, new Rectangle(BagWeightBar.DisplayArea.X + 5, BagWeightBar.DisplayArea.Y + 4, (int)(image.Width * (double)num), image.Height), Color.White, BagWeightBar, 0, 0);
            };
            DXLabel dxLabel1 = new DXLabel();
            dxLabel1.Parent = this;
            clientArea = ClientArea;
            int x2 = clientArea.Left + 120;
            clientArea = ClientArea;
            int y2 = clientArea.Bottom - 70;
            dxLabel1.Location = new Point(x2, y2);
            dxLabel1.Text = "0";
            clientArea = ClientArea;
            dxLabel1.Size = new Size(clientArea.Width - 91, 20);
            dxLabel1.Sound = SoundIndex.GoldPickUp;
            WeightLabel = dxLabel1;

            GoldLabel = new DXLabel
            {
                AutoSize = false,
                Border = true,
                BorderColour = Color.FromArgb(99, 83, 50),
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter,
                Parent = this,
                Location = new Point(ClientArea.Left + 55, ClientArea.Bottom - 44),
                Text = "0",
                Size = new Size(ClientArea.Width - 180, 18),
                Sound = SoundIndex.GoldPickUp
            };
            GoldLabel.MouseClick += GoldLabel_MouseClick;

            new DXLabel
            {
                AutoSize = false,
                Border = true,
                BorderColour = Color.FromArgb(99, 83, 50),
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Parent = this,
                Location = new Point(ClientArea.Left + 6, ClientArea.Bottom - 44),
                Text = "金币",
                Size = new Size(48, 18),
                IsControl = false,
            };

            DXLabel dxLabel4 = new DXLabel();
            dxLabel4.AutoSize = false;
            dxLabel4.Border = true;
            dxLabel4.BorderColour = Color.FromArgb(99, 83, 50);
            dxLabel4.ForeColour = Color.White;
            dxLabel4.DrawFormat = TextFormatFlags.VerticalCenter;
            dxLabel4.Parent = this;
            clientArea = ClientArea;
            int x5 = clientArea.Left + 55;
            clientArea = ClientArea;
            int y5 = clientArea.Bottom - 25;
            dxLabel4.Location = new Point(x5, y5);
            dxLabel4.Text = "0";
            clientArea = ClientArea;
            dxLabel4.Size = new Size(clientArea.Width - 180, 18);
            dxLabel4.Sound = SoundIndex.GoldPickUp;
            GameGoldLabel = dxLabel4;

            DXLabel dxLabel5 = new DXLabel();
            dxLabel5.AutoSize = false;
            dxLabel5.Border = true;
            dxLabel5.BorderColour = Color.FromArgb(99, 83, 50);
            dxLabel5.ForeColour = Color.White;
            dxLabel5.DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            dxLabel5.Parent = this;
            clientArea = ClientArea;
            int x6 = clientArea.Left + 6;
            clientArea = ClientArea;
            int y6 = clientArea.Bottom - 25;
            dxLabel5.Location = new Point(x6, y6);
            dxLabel5.Text = "声望";
            dxLabel5.Size = new Size(48, 18);
            dxLabel5.IsControl = false;

            StorageBox = new DXButton
            {
                LibraryFile = LibraryFile.GameInter,
                Index = 358,
                Parent = this,
                Hint = "仓库[S]",
                Location = new Point(ClientArea.Right - 81, ClientArea.Bottom - 43)
            };
            StorageBox.MouseClick += (o, e) =>
            {
                OnStorageBoxChanged();
                //if (GameScene.Game.User.Stats[Stat.Rebirth] < GameScene.Game.User.ZaixianFenjie)
                {
                    GameScene.Game.StorageBox.Visible = !GameScene.Game.StorageBox.Visible;
                }
                //else
                //    GameScene.Game.NPCItemZaixianFragmentBox.Visible = !GameScene.Game.NPCItemZaixianFragmentBox.Visible;
            };

            /*
            StorageBox.MouseClick += (o, e) =>
            {
                if (!GameScene.Game.StorageBox.Visible)
                {
                    StorageBox.Index = 359;
                }
                else
                {
                    StorageBox.Index = 358;
                }
                GameScene.Game.StorageBox.Visible = !GameScene.Game.StorageBox.Visible;
            };
            */

            //Jyhuishou = new DXButton
            //{
            //    LibraryFile = LibraryFile.GameInter,
            //    Index = 125,
            //    Parent = this,
            //    Hint = "经验回收",
            //    Location = new Point(ClientArea.Right - 120, ClientArea.Bottom - 43)
            //};
            //Jyhuishou.MouseClick += (o, e) => GameScene.Game.NPCJyhuishouBox.Visible = !GameScene.Game.NPCJyhuishouBox.Visible;
            CanSortItem = new DXButton
            {
                LibraryFile = LibraryFile.GameInter,
                Index = 364,
                Parent = this,
                Hint = "整理",
                Location = new Point(ClientArea.Right - 42, ClientArea.Bottom - 43)
            };
            CanSortItem.MouseClick += (o, e) =>
            {
                if (GameScene.Game.Observer) return;
                if (SortItemTime < CEnvir.Now)
                {
                    SortItemTime = CEnvir.Now.AddMinutes(1);
                    nowcansortItem = false;
                    OnSortItemChanged();
                    CEnvir.Enqueue(new C.SortBagItem { });
                }
            };

        }
        private DateTime SortItemTime { get; set; }  = DateTime.MinValue;
        public bool nowcansortItem = true;
        public void OnSortItemChanged()
        {
            if (SortItemTime > CEnvir.Now)
            {
                CanSortItem.Index = 365;
                CanSortItem.Hint = $"无法使用";
                RefreshPatch = CanSortItem;
            }
            else
            {
                CanSortItem.Index = 364;
                CanSortItem.Hint = "整理";
            }
        }

        public void OnStorageBoxChanged()
        {
            //if (GameScene.Game.User.Stats[Stat.Rebirth] < GameScene.Game.User.ZaixianFenjie)
            {
                StorageBox.Index = 358;
                StorageBox.Hint = "仓库[S]";
            }
            //else
            //{
            //    StorageBox.Index = 127;
            //    StorageBox.Hint = "装备分解";
            //}

        }

        public void InventoryDialogProcess()
        {
            OnStorageBoxChanged();

            if (!(SortItemTime <= CEnvir.Now))
                return;

            OnSortItemChanged();
        }

        #region Methods
        private void GoldLabel_MouseClick(object sender, MouseEventArgs e)
        {
            if (GameScene.Game.SelectedCell == null)
                GameScene.Game.GoldPickedUp = !GameScene.Game.GoldPickedUp && MapObject.User.Gold > 0;
        }

        //public void RefreshPatchGrid()
        //{
        //    PatchGrid.GridSize = new Size(7, Math.Max(7, (int)Math.Ceiling((double)GameScene.Game.PatchGridSize / 7.0)));
        //    PatchGridScrollBar.MaxValue = PatchGrid.GridSize.Height;
        //    ApplyPatchGridFilter();
        //}

        //public void RefreshBaoshiGrid()
        //{
        //    BaoshiGrid.GridSize = new Size(7, Math.Max(7, (int)Math.Ceiling((double)GameScene.Game.BaoshiGridSize / 7.0)));
        //    BaoshiScrollBar.MaxValue = BaoshiGrid.GridSize.Height;
        //    ApplyBaoshiGridFilter();
        //}

        //public void ApplyPatchGridFilter()
        //{
        //    foreach (DXItemCell cell in PatchGrid.Grid)
        //        FilterCell(cell);
        //}

        //public void ApplyBaoshiGridFilter()
        //{
        //    foreach (DXItemCell cell in BaoshiGrid.Grid)
        //        FilterBaoshiCell(cell);
        //}

        public void FilterCell(DXItemCell cell)
        {
            //if (cell.Slot >= GameScene.Game.PatchGridSize)
            //    cell.Enabled = false;
            //else
            //    cell.Enabled = true;
        }

        //public void FilterBaoshiCell(DXItemCell cell)
        //{
        //    if (cell.Slot >= GameScene.Game.BaoshiGridSize)
        //        cell.Enabled = false;
        //    else
        //        cell.Enabled = true;
        //}

        //private void PatchGrid_GridSizeChanged(object sender, EventArgs e)
        //{
        //    foreach (DXItemCell dxItemCell in PatchGrid.Grid)
        //    {
        //        DXItemCell cell = dxItemCell;
        //        cell.ItemChanged += (EventHandler<EventArgs>)((o, e1) => FilterCell(cell));
        //    }
        //    foreach (DXControl dxControl in PatchGrid.Grid)
        //        dxControl.MouseWheel += new EventHandler<MouseEventArgs>(PatchGridScrollBar.DoMouseWheel);
        //}

        //private void BaoshiGrid_GridSizeChanged(object sender, EventArgs e)
        //{
        //    foreach (DXItemCell dxItemCell in BaoshiGrid.Grid)
        //    {
        //        DXItemCell cell = dxItemCell;
        //        cell.ItemChanged += (EventHandler<EventArgs>)((o, e1) => FilterBaoshiCell(cell));
        //    }
        //    foreach (DXControl dxControl in BaoshiGrid.Grid)
        //        dxControl.MouseWheel += new EventHandler<MouseEventArgs>(BaoshiScrollBar.DoMouseWheel);
        //}

        //private void PatchGridScrollBar_ValueChanged(object sender, EventArgs e)
        //{
        //    PatchGrid.ScrollValue = PatchGridScrollBar.Value;
        //}

        //private void BaoshiGridScrollBar_ValueChanged(object sender, EventArgs e)
        //{
        //    BaoshiGrid.ScrollValue = BaoshiScrollBar.Value;
        //}

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (Grid != null)
                {
                    if (!Grid.IsDisposed)
                        Grid.Dispose();

                    Grid = null;
                }

                if (GoldLabel != null)
                {
                    if (!GoldLabel.IsDisposed)
                        GoldLabel.Dispose();

                    GoldLabel = null;
                }
                //if (PatchGrid != null)
                //{
                //    if (!PatchGrid.IsDisposed)
                //        PatchGrid.Dispose();
                //    PatchGrid = null;
                //}

                if (GameGoldLabel != null)
                {
                    if (!GameGoldLabel.IsDisposed)
                        GameGoldLabel.Dispose();
                    GameGoldLabel = null;
                }

                if (WeightLabel != null)
                {
                    if (!WeightLabel.IsDisposed)
                        WeightLabel.Dispose();

                    WeightLabel = null;
                }

                if (BagWeightBar != null)
                {
                    if (!BagWeightBar.IsDisposed)
                        BagWeightBar.Dispose();
                    BagWeightBar = null;
                }
                //if (PatchGridScrollBar != null)
                //{
                //    if (!PatchGridScrollBar.IsDisposed)
                //        PatchGridScrollBar.Dispose();
                //    PatchGridScrollBar = null;
                //}

                //if (BaoshiGrid != null)
                //{
                //    if (!BaoshiGrid.IsDisposed)
                //        BaoshiGrid.Dispose();
                //    BaoshiGrid = null;
                //}

                //if (BaoshiScrollBar != null)
                //{
                //    if (!BaoshiScrollBar.IsDisposed)
                //        BaoshiScrollBar.Dispose();
                //    BaoshiScrollBar = null;
                //}
            }

        }

        #endregion
    }

}