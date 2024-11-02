using Client.Envir;
using SlimDX;
using SlimDX.Direct3D9;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Client.Controls
{
    public class DXListView : DXControl
    {
        public int ItemHeight;
        public int Vspac;
        public int Hspac;
        public Color ItemBorderColour;
        public Color ItemSelectedBorderColour;
        public Color ItemBackColour;
        public Color ItemSelectedBackColour;
        public Color HeaderBorderColour;
        public Color HeaderSelectedBorderColour;
        public Color HeaderBackColour;
        public Color HeaderSelectedBackColour;
        public bool ItemBorder;
        public bool HeaderBorder;
        public bool SelectedBorder;
        public bool HasHeader;
        public DXVScrollBar HScrollBar;
        public DXVScrollBar VScrollBar;
        public int VScrollValue;
        public DXControl Items;
        public DXControl Headers { get; set; }
        private int _First;
        private int _Last;
        private DXControl _HeightLight;

        public event EventHandler<EventArgs> ItemMouseEnter;

        public event EventHandler<EventArgs> ItemMouseLeave;

        public event EventHandler<MouseEventArgs> ItemMouseClick;

        public event EventHandler<MouseEventArgs> ItemMouseDbClick;

        public event EventHandler<MouseEventArgs> ItemMouseRButton;

        public DXControl HeightLight
        {
            get
            {
                return _HeightLight;
            }
            set
            {
                if (_HeightLight == value)
                    return;
                DXControl heightLight = _HeightLight;
                _HeightLight = value;
                if (heightLight != null)
                {
                    foreach (DXControl control in heightLight.Controls)
                    {
                        control.BackColour = ItemBackColour;
                        control.BorderColour = ItemBorderColour;
                    }
                    heightLight.BorderColour = ItemBorderColour;
                    heightLight.Border = false;
                }
                foreach (DXControl control in _HeightLight.Controls)
                {
                    control.BackColour = ItemSelectedBackColour;
                    control.BorderColour = ItemSelectedBorderColour;
                }
                _HeightLight.BorderColour = ItemSelectedBorderColour;
                _HeightLight.Border = true;
            }
        }

        public uint ItemCount
        {
            get
            {
                return (uint)Items.Controls.Count;
            }
        }

        public uint ColumnCount
        {
            get
            {
                return (uint)Headers.Controls.Count;
            }
        }

        public DXListView()
        {
            ItemHeight = 18;
            HasHeader = true;
            Hspac = 1;
            Vspac = 0;
            HeightLight = (DXControl)null;
            _First = 0;
            _Last = 0;
            ItemBorderColour = Color.FromArgb(69, 56, 32);
            ItemBackColour = Color.Empty;
            ItemSelectedBorderColour = Color.FromArgb(160, 125, 22);
            ItemSelectedBackColour = Color.FromArgb(31, 25, 12);
            ItemBorder = true;
            HeaderBorderColour = Color.FromArgb(160, 125, 22);
            HeaderBackColour = Color.FromArgb(31, 25, 12);
            HeaderSelectedBorderColour = Color.Yellow;
            HeaderSelectedBackColour = Color.FromArgb(89, 68, 12);
            HeaderBorder = true;
            SelectedBorder = false;
            Headers = new DXControl()
            {
                Parent = (DXControl)this,
                Size = new Size(ItemHeight, ItemHeight)
            };
            Items = new DXControl()
            {
                Parent = (DXControl)this,
                Size = new Size(ItemHeight, ItemHeight)
            };
            VScrollValue = 0;
            DXVScrollBar dxvScrollBar = new DXVScrollBar();
            dxvScrollBar.Parent = (DXControl)this;
            dxvScrollBar.Size = new Size(14, ItemHeight);
            dxvScrollBar.Visible = true;
            dxvScrollBar.Value = 0;
            dxvScrollBar.MaxValue = 1;
            dxvScrollBar.MinValue = 0;
            dxvScrollBar.BorderColour = HeaderBorderColour;
            VScrollBar = dxvScrollBar;
            VScrollBar.ValueChanged += (EventHandler<EventArgs>)((o, e) => UpdateItems());
            MouseWheel += new EventHandler<MouseEventArgs>(VScrollBar.DoMouseWheel);
            Headers.MouseWheel += new EventHandler<MouseEventArgs>(VScrollBar.DoMouseWheel);
            Headers.MouseUp += new EventHandler<MouseEventArgs>(OnItemMouseUp);
            Items.MouseWheel += new EventHandler<MouseEventArgs>(VScrollBar.DoMouseWheel);
            Items.MouseUp += new EventHandler<MouseEventArgs>(OnItemMouseUp);
        }

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);
            UpdateViewRect();
        }

        public void UpdateViewRect()
        {
            Headers.Location = new Point(5, 5);
            DXControl headers = Headers;
            Size size1 = Size;
            int width1 = size1.Width;
            size1 = VScrollBar.Size;
            int width2 = size1.Width;
            int width3 = width1 - width2 - 10;
            size1 = Headers.Size;
            int height1 = size1.Height;
            Size size2 = new Size(width3, height1);
            headers.Size = size2;
            Items.Location = new Point(5, Headers.Location.Y + Headers.Size.Height + 3);
            DXControl items = Items;
            Size size3 = Headers.Size;
            int width4 = size3.Width;
            size3 = Size;
            int height2 = size3.Height;
            size3 = Headers.Size;
            int height3 = size3.Height;
            int height4 = height2 - height3 - 10 - 3;
            Size size4 = new Size(width4, height4);
            items.Size = size4;
            UpdateScrollBar();
            UpdateItems();
        }

        protected void DrawGrid(int x, int y, int cx, int cy)
        {
            Vector2[] vertexList = new Vector2[5] { new Vector2((float)x, (float)y), new Vector2((float)cx, (float)y), new Vector2((float)cx, (float)cy), new Vector2((float)x, (float)cy), new Vector2((float)x, (float)y) };
            if ((double)DXManager.Line.Width != 1.0)
                DXManager.Line.Width = 1f;
            Surface currentSurface = DXManager.CurrentSurface;
            DXManager.SetSurface(DXManager.ScratchSurface);
            DXManager.Device.Clear(ClearFlags.Target, 0, 0.0f, 0);
            DXManager.Line.Draw(vertexList, (Color4)ItemBorderColour);
            DXManager.SetSurface(currentSurface);
            PresentTexture(DXManager.ScratchTexture, Items.Parent, Rectangle.Inflate(Items.DisplayArea, 1, 1), Color.White, Items, 0, 0);
        }

        protected override void DrawChildControls()
        {
            foreach (DXControl control in Items.Controls)
                control.Draw();
            foreach (DXControl control in Headers.Controls)
                control.Draw();
            if (HeightLight != null)
                HeightLight.Draw();
            VScrollBar.Draw();
        }

        public void UpdateScrollBar()
        {
            if (ItemCount == 0U || ColumnCount == 0U)
            {
                VScrollBar.Visible = false;
            }
            else
            {
                DXVScrollBar vscrollBar1 = VScrollBar;
                int x = Headers.Location.X;
                Size size1 = Headers.Size;
                int width1 = size1.Width;
                Point point = new Point(x + width1, Headers.Location.Y + 1);
                vscrollBar1.Location = point;
                DXVScrollBar vscrollBar2 = VScrollBar;
                size1 = Items.Size;
                int height1 = size1.Height;
                vscrollBar2.VisibleSize = height1;
                DXVScrollBar vscrollBar3 = VScrollBar;
                size1 = VScrollBar.Size;
                int width2 = size1.Width;
                size1 = Headers.Size;
                int height2 = size1.Height;
                size1 = Items.Size;
                int height3 = size1.Height;
                int height4 = height2 + height3 + 3;
                Size size2 = new Size(width2, height4);
                vscrollBar3.Size = size2;
                VScrollBar.Visible = true;
                DXVScrollBar vscrollBar4 = VScrollBar;
                size1 = Items.Controls[0].Size;
                int num1 = size1.Height + Vspac;
                vscrollBar4.Change = num1;
                int num2 = VScrollBar.VisibleSize % VScrollBar.Change;
                if (num2 <= 0)
                    return;
                VScrollBar.VisibleSize -= num2;
            }
        }

        public void UpdateItems()
        {
            int x = 1;
            int y = 0;
            int num1 = VScrollBar.Value;
            int num2 = -num1;
            if (ItemCount > 0U)
                _First = num1 / VScrollBar.Change;
            for (int index = 0; index < _First; ++index)
                Items.Controls[index].Visible = false;
            int first1 = _First;
            Size size1;
            while (true)
            {
                int num3 = y;
                size1 = Size;
                int height1 = size1.Height;
                if (num3 < height1 && (long)first1 < (long)ItemCount)
                {
                    DXControl control = Items.Controls[first1];
                    control.Location = new Point(0, y);
                    DXControl dxControl = control;
                    size1 = Items.Size;
                    int width = size1.Width;
                    size1 = control.Size;
                    int height2 = size1.Height;
                    Size size2 = new Size(width, height2);
                    dxControl.Size = size2;
                    int num4 = y;
                    size1 = control.Size;
                    int num5 = size1.Height + Vspac;
                    y = num4 + num5;
                    control.Visible = true;
                    _Last = first1;
                    ++first1;
                }
                else
                    break;
            }
            for (int index = _Last + 1; (long)index < (long)ItemCount; ++index)
                Items.Controls[index].Visible = false;
            VScrollBar.MaxValue = (int)((long)ItemCount * (long)VScrollBar.Change);
            for (uint index = 0; index < ColumnCount; ++index)
            {
                DXControl control1 = Headers.Controls[(int)index];
                control1.Location = new Point(x, 1);
                DXControl dxControl1 = control1;
                size1 = control1.Size;
                int width1 = size1.Width;
                size1 = Headers.Size;
                int height1 = size1.Height - 2;
                Size size2 = new Size(width1, height1);
                dxControl1.Size = size2;
                for (int first2 = _First; first2 <= _Last && (long)first2 < (long)ItemCount; ++first2)
                {
                    DXControl control2 = Items.Controls[first2];
                    if ((long)index < (long)control2.Controls.Count)
                    {
                        DXControl control3 = control2.Controls[(int)index];
                        control3.Location = new Point(control1.Location.X, 1);
                        DXControl dxControl2 = control3;
                        size1 = control1.Size;
                        int width2 = size1.Width;
                        size1 = control2.Size;
                        int height2 = size1.Height;
                        Size size3 = new Size(width2, height2);
                        dxControl2.Size = size3;
                    }
                }
                int num3 = x;
                size1 = control1.Size;
                int num4 = size1.Width + Hspac;
                x = num3 + num4;
            }
        }

        public void OnItemMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            EventHandler<MouseEventArgs> itemMouseRbutton = ItemMouseRButton;
            if (itemMouseRbutton == null)
                return;
            itemMouseRbutton(sender, e);
        }

        public void OnItemMouseEnter(object sender, EventArgs e)
        {
            DXControl dxControl = sender as DXControl;
            if (dxControl == null)
                return;
            if (HeightLight == dxControl.Parent)
            {
                if (SelectedBorder)
                    dxControl.BorderColour = ItemSelectedBorderColour;
                else
                    dxControl.BackColour = ItemSelectedBackColour;
            }
            else if (SelectedBorder)
                dxControl.BorderColour = ItemSelectedBorderColour;
            else
                dxControl.BackColour = ItemSelectedBackColour;

            EventHandler<EventArgs> itemMouseEnter = ItemMouseEnter;
            if (itemMouseEnter == null)
                return;
            itemMouseEnter(sender, e);
        }

        public void OnItemMouseLeave(object sender, EventArgs e)
        {
            DXControl dxControl = sender as DXControl;
            if (dxControl == null)
                return;
            if (HeightLight == dxControl.Parent)
            {
                if (SelectedBorder)
                    dxControl.BorderColour = ItemSelectedBorderColour;
                else
                    dxControl.BackColour = ItemSelectedBackColour;
            }
            else if (SelectedBorder)
                dxControl.BorderColour = ItemBorderColour;
            else
                dxControl.BackColour = ItemBackColour;

            EventHandler<EventArgs> itemMouseLeave = ItemMouseLeave;
            if (itemMouseLeave == null)
                return;
            itemMouseLeave(sender, e);
        }

        public void OnItemMouseClick(object sender, MouseEventArgs e)
        {
            DXControl dxControl = sender as DXControl;
            if (dxControl == null)
                return;
            HeightLight = dxControl.Parent;

            EventHandler<MouseEventArgs> itemMouseClick = ItemMouseClick;
            if (itemMouseClick == null)
                return;
            itemMouseClick(sender, e);
        }

        public void OnItemMouseDbClick(object sender, MouseEventArgs e)
        {
            if (!(sender is DXControl))
                return;

            EventHandler<MouseEventArgs> itemMouseDbClick = ItemMouseDbClick;
            if (itemMouseDbClick == null)
                return;
            itemMouseDbClick(sender, e);
        }

        public uint InsertColumn(uint col, DXControl control)
        {
            control.Border = HeaderBorder;
            control.BorderColour = HeaderBorderColour;
            control.BackColour = HeaderBackColour;
            DXControl headers = Headers;
            Size size1 = Headers.Size;
            int width = size1.Width;
            size1 = control.Size;
            int height = size1.Height;
            Size size2 = new Size(width, height);
            headers.Size = size2;
            UpdateViewRect();
            if (col >= ColumnCount)
            {
                col = ColumnCount;
                control.Parent = Headers;
            }
            else
            {
                control.Parent = Headers;
                Headers.Controls.Remove(control);
                Headers.Controls.Insert((int)col, control);
            }
            control.MouseClick += (EventHandler<MouseEventArgs>)((o, e) => { });
            control.MouseEnter += (EventHandler<EventArgs>)((o, e) => control.BackColour = HeaderSelectedBackColour);
            control.MouseLeave += (EventHandler<EventArgs>)((o, e) => control.BackColour = HeaderBackColour);
            control.MouseUp += new EventHandler<MouseEventArgs>(OnItemMouseUp);
            UpdateScrollBar();
            UpdateItems();
            return col;
        }

        public void DeleteColumn(uint col)
        {
            if (col >= ColumnCount)
                return;
            Headers.Controls.RemoveAt((int)col);
            foreach (DXControl control in Items.Controls)
                control.Controls.RemoveAt((int)col);
            UpdateScrollBar();
            UpdateItems();
        }

        public uint InsertItem(uint nItem, DXControl control)
        {
            if (ColumnCount == 0U)
            {
                int num = (int)InsertColumn(0U, new DXControl() { Text = "unnamed" });
            }
            DXControl dxControl = new DXControl() { Size = control.Size };
            dxControl.MouseClick += (EventHandler<MouseEventArgs>)((o, e) => HeightLight = o as DXControl);
            dxControl.MouseWheel += new EventHandler<MouseEventArgs>(VScrollBar.DoMouseWheel);
            if (nItem >= ItemCount)
            {
                nItem = ItemCount;
                dxControl.Parent = Items;
            }
            else
            {
                dxControl.Parent = Items;
                Items.Controls.Remove(dxControl);
                Items.Controls.Insert((int)nItem, dxControl);
            }
            control.Parent = dxControl;
            control.Border = ItemBorder;
            control.BorderColour = ItemBorderColour;
            control.BackColour = ItemBackColour;
            control.MouseUp += new EventHandler<MouseEventArgs>(OnItemMouseUp);
            control.MouseEnter += new EventHandler<EventArgs>(OnItemMouseEnter);
            control.MouseLeave += new EventHandler<EventArgs>(OnItemMouseLeave);
            control.MouseClick += new EventHandler<MouseEventArgs>(OnItemMouseClick);
            control.MouseDoubleClick += new EventHandler<MouseEventArgs>(OnItemMouseDbClick);
            for (uint index = 1; index < ColumnCount; ++index)
            {
                DXLabel dxLabel = new DXLabel();
                dxLabel.Parent = control.Parent;
                dxLabel.Text = index.ToString();
                dxLabel.Border = control.Border;
                dxLabel.BorderColour = control.BorderColour;
                dxLabel.AutoSize = false;
                dxLabel.MouseWheel += new EventHandler<MouseEventArgs>(VScrollBar.DoMouseWheel);
            }
            control.MouseWheel += new EventHandler<MouseEventArgs>(VScrollBar.DoMouseWheel);
            UpdateScrollBar();
            return nItem;
        }

        public DXControl SetItem(uint nItem, uint subItem, DXControl control)
        {
            if (nItem < ItemCount && subItem < ColumnCount)
            {
                DXControl control1 = Items.Controls[(int)nItem];
                DXControl control2 = control1.Controls[(int)subItem];
                control2.Parent = (DXControl)null;
                control.Parent = control1;
                control1.Controls.Remove(control);
                control1.Controls.Insert((int)subItem, control);
                control.Border = ItemBorder;
                control.BorderColour = ItemBorderColour;
                control.BackColour = ItemBackColour;
                control.MouseUp += new EventHandler<MouseEventArgs>(OnItemMouseUp);
                control.MouseWheel += new EventHandler<MouseEventArgs>(VScrollBar.DoMouseWheel);
                control.MouseEnter += new EventHandler<EventArgs>(OnItemMouseEnter);
                control.MouseLeave += new EventHandler<EventArgs>(OnItemMouseLeave);
                control.MouseClick += new EventHandler<MouseEventArgs>(OnItemMouseClick);
                control.MouseDoubleClick += new EventHandler<MouseEventArgs>(OnItemMouseDbClick);
                control2.Dispose();
            }
            return control;
        }

        public DXControl GetItem(uint nItem, uint nSubItem)
        {
            if (nItem < ItemCount && nSubItem < ColumnCount)
                return Items.Controls[(int)nItem].Controls[(int)nSubItem];
            return (DXControl)null;
        }

        public uint InsertItem(uint nItem, string text)
        {
            int num = (int)nItem;
            DXLabel dxLabel = new DXLabel();
            dxLabel.Text = text;
            dxLabel.AutoSize = false;
            dxLabel.Size = new Size(0, ItemHeight);
            dxLabel.DrawFormat = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
            return InsertItem((uint)num, (DXControl)dxLabel);
        }

        public DXControl SetItem(uint nItem, uint nSubItem, string text)
        {
            int num1 = (int)nItem;
            int num2 = (int)nSubItem;
            DXLabel dxLabel = new DXLabel();
            dxLabel.AutoSize = true;
            dxLabel.Text = text;

            //dxLabel.DrawFormat = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
            return SetItem((uint)num1, (uint)num2, (DXControl)dxLabel);
        }

        public uint InsertColumn(uint nColumn, string text, int Width, int Height, string hint = null)
        {
            int num = (int)nColumn;
            DXLabel dxLabel = new DXLabel();
            dxLabel.Text = text;
            dxLabel.AutoSize = false;
            dxLabel.Size = new Size(Width, Height);
            dxLabel.DrawFormat = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
            dxLabel.Hint = hint;
            return InsertColumn((uint)num, (DXControl)dxLabel);
        }

        public void SortByName(string name)
        {
            for (int index = 0; (long)index < (long)ItemCount; ++index)
            {
                DXControl control = Items.Controls[index];
                if (control.Controls.Count != 0 && control.Controls[0].Text.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Items.Controls.RemoveAt(index);
                    Items.Controls.Insert(0, control);
                }
            }
            VScrollBar.Value = 0;
            UpdateItems();
            UpdateScrollBar();
        }

        public void DeleteItem(uint nItem)
        {
            if (nItem >= ItemCount)
                return;
            DXControl control = Items.Controls[(int)nItem];
            Items.Controls.RemoveAt((int)nItem);
            UpdateScrollBar();
            UpdateItems();
            control.Dispose();
        }

        public void RemoveAll()
        {
            for (uint index = 0; index < ItemCount; ++index)
            {
                DXControl control = Items.Controls[(int)index];
                Items.Controls.RemoveAt((int)index);
                control.Dispose();
            }
            UpdateScrollBar();
            UpdateItems();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Items == null || Items.IsDisposed)
                return;
            Items.Dispose();
        }
    }
}
