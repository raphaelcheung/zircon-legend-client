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
        public DXControl Headers;
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
                return this._HeightLight;
            }
            set
            {
                if (this._HeightLight == value)
                    return;
                DXControl heightLight = this._HeightLight;
                this._HeightLight = value;
                if (heightLight != null)
                {
                    foreach (DXControl control in heightLight.Controls)
                    {
                        control.BackColour = this.ItemBackColour;
                        control.BorderColour = this.ItemBorderColour;
                    }
                    heightLight.BorderColour = this.ItemBorderColour;
                    heightLight.Border = false;
                }
                foreach (DXControl control in this._HeightLight.Controls)
                {
                    control.BackColour = this.ItemSelectedBackColour;
                    control.BorderColour = this.ItemSelectedBorderColour;
                }
                this._HeightLight.BorderColour = this.ItemSelectedBorderColour;
                this._HeightLight.Border = true;
            }
        }

        public uint ItemCount
        {
            get
            {
                return (uint)this.Items.Controls.Count;
            }
        }

        public uint ColumnCount
        {
            get
            {
                return (uint)this.Headers.Controls.Count;
            }
        }

        public DXListView()
        {
            this.ItemHeight = 18;
            this.HasHeader = true;
            this.Hspac = 1;
            this.Vspac = 0;
            this.HeightLight = (DXControl)null;
            this._First = 0;
            this._Last = 0;
            this.ItemBorderColour = Color.FromArgb(69, 56, 32);
            this.ItemBackColour = Color.Empty;
            this.ItemSelectedBorderColour = Color.FromArgb(160, 125, 22);
            this.ItemSelectedBackColour = Color.FromArgb(31, 25, 12);
            this.ItemBorder = true;
            this.HeaderBorderColour = Color.FromArgb(160, 125, 22);
            this.HeaderBackColour = Color.FromArgb(31, 25, 12);
            this.HeaderSelectedBorderColour = Color.Yellow;
            this.HeaderSelectedBackColour = Color.FromArgb(89, 68, 12);
            this.HeaderBorder = true;
            this.SelectedBorder = false;
            this.Headers = new DXControl()
            {
                Parent = (DXControl)this,
                Size = new Size(this.ItemHeight, this.ItemHeight)
            };
            this.Items = new DXControl()
            {
                Parent = (DXControl)this,
                Size = new Size(this.ItemHeight, this.ItemHeight)
            };
            this.VScrollValue = 0;
            DXVScrollBar dxvScrollBar = new DXVScrollBar();
            dxvScrollBar.Parent = (DXControl)this;
            dxvScrollBar.Size = new Size(14, this.ItemHeight);
            dxvScrollBar.Visible = true;
            dxvScrollBar.Value = 0;
            dxvScrollBar.MaxValue = 1;
            dxvScrollBar.MinValue = 0;
            dxvScrollBar.BorderColour = this.HeaderBorderColour;
            this.VScrollBar = dxvScrollBar;
            this.VScrollBar.ValueChanged += (EventHandler<EventArgs>)((o, e) => this.UpdateItems());
            this.MouseWheel += new EventHandler<MouseEventArgs>(this.VScrollBar.DoMouseWheel);
            this.Headers.MouseWheel += new EventHandler<MouseEventArgs>(this.VScrollBar.DoMouseWheel);
            this.Headers.MouseUp += new EventHandler<MouseEventArgs>(this.OnItemMouseUp);
            this.Items.MouseWheel += new EventHandler<MouseEventArgs>(this.VScrollBar.DoMouseWheel);
            this.Items.MouseUp += new EventHandler<MouseEventArgs>(this.OnItemMouseUp);
        }

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);
            this.UpdateViewRect();
        }

        public void UpdateViewRect()
        {
            this.Headers.Location = new Point(5, 5);
            DXControl headers = this.Headers;
            Size size1 = this.Size;
            int width1 = size1.Width;
            size1 = this.VScrollBar.Size;
            int width2 = size1.Width;
            int width3 = width1 - width2 - 10;
            size1 = this.Headers.Size;
            int height1 = size1.Height;
            Size size2 = new Size(width3, height1);
            headers.Size = size2;
            this.Items.Location = new Point(5, this.Headers.Location.Y + this.Headers.Size.Height + 3);
            DXControl items = this.Items;
            Size size3 = this.Headers.Size;
            int width4 = size3.Width;
            size3 = this.Size;
            int height2 = size3.Height;
            size3 = this.Headers.Size;
            int height3 = size3.Height;
            int height4 = height2 - height3 - 10 - 3;
            Size size4 = new Size(width4, height4);
            items.Size = size4;
            this.UpdateScrollBar();
            this.UpdateItems();
        }

        protected void DrawGrid(int x, int y, int cx, int cy)
        {
            Vector2[] vertexList = new Vector2[5] { new Vector2((float)x, (float)y), new Vector2((float)cx, (float)y), new Vector2((float)cx, (float)cy), new Vector2((float)x, (float)cy), new Vector2((float)x, (float)y) };
            if ((double)DXManager.Line.Width != 1.0)
                DXManager.Line.Width = 1f;
            Surface currentSurface = DXManager.CurrentSurface;
            DXManager.SetSurface(DXManager.ScratchSurface);
            DXManager.Device.Clear(ClearFlags.Target, 0, 0.0f, 0);
            DXManager.Line.Draw(vertexList, (Color4)this.ItemBorderColour);
            DXManager.SetSurface(currentSurface);
            PresentTexture(DXManager.ScratchTexture, this.Items.Parent, Rectangle.Inflate(this.Items.DisplayArea, 1, 1), Color.White, this.Items, 0, 0);
        }

        protected override void DrawChildControls()
        {
            foreach (DXControl control in this.Items.Controls)
                control.Draw();
            foreach (DXControl control in this.Headers.Controls)
                control.Draw();
            if (this.HeightLight != null)
                this.HeightLight.Draw();
            this.VScrollBar.Draw();
        }

        public void UpdateScrollBar()
        {
            if (this.ItemCount == 0U || this.ColumnCount == 0U)
            {
                this.VScrollBar.Visible = false;
            }
            else
            {
                DXVScrollBar vscrollBar1 = this.VScrollBar;
                int x = this.Headers.Location.X;
                Size size1 = this.Headers.Size;
                int width1 = size1.Width;
                Point point = new Point(x + width1, this.Headers.Location.Y + 1);
                vscrollBar1.Location = point;
                DXVScrollBar vscrollBar2 = this.VScrollBar;
                size1 = this.Items.Size;
                int height1 = size1.Height;
                vscrollBar2.VisibleSize = height1;
                DXVScrollBar vscrollBar3 = this.VScrollBar;
                size1 = this.VScrollBar.Size;
                int width2 = size1.Width;
                size1 = this.Headers.Size;
                int height2 = size1.Height;
                size1 = this.Items.Size;
                int height3 = size1.Height;
                int height4 = height2 + height3 + 3;
                Size size2 = new Size(width2, height4);
                vscrollBar3.Size = size2;
                this.VScrollBar.Visible = true;
                DXVScrollBar vscrollBar4 = this.VScrollBar;
                size1 = this.Items.Controls[0].Size;
                int num1 = size1.Height + this.Vspac;
                vscrollBar4.Change = num1;
                int num2 = this.VScrollBar.VisibleSize % this.VScrollBar.Change;
                if (num2 <= 0)
                    return;
                this.VScrollBar.VisibleSize -= num2;
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
                    DXControl control = this.Items.Controls[first1];
                    control.Location = new Point(0, y);
                    DXControl dxControl = control;
                    size1 = this.Items.Size;
                    int width = size1.Width;
                    size1 = control.Size;
                    int height2 = size1.Height;
                    Size size2 = new Size(width, height2);
                    dxControl.Size = size2;
                    int num4 = y;
                    size1 = control.Size;
                    int num5 = size1.Height + this.Vspac;
                    y = num4 + num5;
                    control.Visible = true;
                    this._Last = first1;
                    ++first1;
                }
                else
                    break;
            }
            for (int index = this._Last + 1; (long)index < (long)this.ItemCount; ++index)
                this.Items.Controls[index].Visible = false;
            this.VScrollBar.MaxValue = (int)((long)this.ItemCount * (long)this.VScrollBar.Change);
            for (uint index = 0; index < this.ColumnCount; ++index)
            {
                DXControl control1 = this.Headers.Controls[(int)index];
                control1.Location = new Point(x, 1);
                DXControl dxControl1 = control1;
                size1 = control1.Size;
                int width1 = size1.Width;
                size1 = this.Headers.Size;
                int height1 = size1.Height - 2;
                Size size2 = new Size(width1, height1);
                dxControl1.Size = size2;
                for (int first2 = this._First; first2 <= this._Last && (long)first2 < (long)this.ItemCount; ++first2)
                {
                    DXControl control2 = this.Items.Controls[first2];
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
                int num4 = size1.Width + this.Hspac;
                x = num3 + num4;
            }
        }

        public void OnItemMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            EventHandler<MouseEventArgs> itemMouseRbutton = this.ItemMouseRButton;
            if (itemMouseRbutton == null)
                return;
            itemMouseRbutton(sender, e);
        }

        public void OnItemMouseEnter(object sender, EventArgs e)
        {
            DXControl dxControl = sender as DXControl;
            if (dxControl == null)
                return;
            if (this.HeightLight == dxControl.Parent)
            {
                if (this.SelectedBorder)
                    dxControl.BorderColour = this.ItemSelectedBorderColour;
                else
                    dxControl.BackColour = this.ItemSelectedBackColour;
            }
            else if (this.SelectedBorder)
                dxControl.BorderColour = this.ItemSelectedBorderColour;
            else
                dxControl.BackColour = this.ItemSelectedBackColour;

            EventHandler<EventArgs> itemMouseEnter = this.ItemMouseEnter;
            if (itemMouseEnter == null)
                return;
            itemMouseEnter(sender, e);
        }

        public void OnItemMouseLeave(object sender, EventArgs e)
        {
            DXControl dxControl = sender as DXControl;
            if (dxControl == null)
                return;
            if (this.HeightLight == dxControl.Parent)
            {
                if (this.SelectedBorder)
                    dxControl.BorderColour = this.ItemSelectedBorderColour;
                else
                    dxControl.BackColour = this.ItemSelectedBackColour;
            }
            else if (this.SelectedBorder)
                dxControl.BorderColour = this.ItemBorderColour;
            else
                dxControl.BackColour = this.ItemBackColour;

            EventHandler<EventArgs> itemMouseLeave = this.ItemMouseLeave;
            if (itemMouseLeave == null)
                return;
            itemMouseLeave(sender, e);
        }

        public void OnItemMouseClick(object sender, MouseEventArgs e)
        {
            DXControl dxControl = sender as DXControl;
            if (dxControl == null)
                return;
            this.HeightLight = dxControl.Parent;

            EventHandler<MouseEventArgs> itemMouseClick = this.ItemMouseClick;
            if (itemMouseClick == null)
                return;
            itemMouseClick(sender, e);
        }

        public void OnItemMouseDbClick(object sender, MouseEventArgs e)
        {
            if (!(sender is DXControl))
                return;

            EventHandler<MouseEventArgs> itemMouseDbClick = this.ItemMouseDbClick;
            if (itemMouseDbClick == null)
                return;
            itemMouseDbClick(sender, e);
        }

        public uint InsertColumn(uint col, DXControl control)
        {
            control.Border = this.HeaderBorder;
            control.BorderColour = this.HeaderBorderColour;
            control.BackColour = this.HeaderBackColour;
            DXControl headers = this.Headers;
            Size size1 = this.Headers.Size;
            int width = size1.Width;
            size1 = control.Size;
            int height = size1.Height;
            Size size2 = new Size(width, height);
            headers.Size = size2;
            this.UpdateViewRect();
            if (col >= this.ColumnCount)
            {
                col = this.ColumnCount;
                control.Parent = this.Headers;
            }
            else
            {
                control.Parent = this.Headers;
                this.Headers.Controls.Remove(control);
                this.Headers.Controls.Insert((int)col, control);
            }
            control.MouseClick += (EventHandler<MouseEventArgs>)((o, e) => { });
            control.MouseEnter += (EventHandler<EventArgs>)((o, e) => control.BackColour = this.HeaderSelectedBackColour);
            control.MouseLeave += (EventHandler<EventArgs>)((o, e) => control.BackColour = this.HeaderBackColour);
            control.MouseUp += new EventHandler<MouseEventArgs>(this.OnItemMouseUp);
            this.UpdateScrollBar();
            this.UpdateItems();
            return col;
        }

        public void DeleteColumn(uint col)
        {
            if (col >= this.ColumnCount)
                return;
            this.Headers.Controls.RemoveAt((int)col);
            foreach (DXControl control in this.Items.Controls)
                control.Controls.RemoveAt((int)col);
            this.UpdateScrollBar();
            this.UpdateItems();
        }

        public uint InsertItem(uint nItem, DXControl control)
        {
            if (this.ColumnCount == 0U)
            {
                int num = (int)this.InsertColumn(0U, new DXControl() { Text = "unnamed" });
            }
            DXControl dxControl = new DXControl() { Size = control.Size };
            dxControl.MouseClick += (EventHandler<MouseEventArgs>)((o, e) => this.HeightLight = o as DXControl);
            dxControl.MouseWheel += new EventHandler<MouseEventArgs>(this.VScrollBar.DoMouseWheel);
            if (nItem >= this.ItemCount)
            {
                nItem = this.ItemCount;
                dxControl.Parent = this.Items;
            }
            else
            {
                dxControl.Parent = this.Items;
                this.Items.Controls.Remove(dxControl);
                this.Items.Controls.Insert((int)nItem, dxControl);
            }
            control.Parent = dxControl;
            control.Border = this.ItemBorder;
            control.BorderColour = this.ItemBorderColour;
            control.BackColour = this.ItemBackColour;
            control.MouseUp += new EventHandler<MouseEventArgs>(this.OnItemMouseUp);
            control.MouseEnter += new EventHandler<EventArgs>(this.OnItemMouseEnter);
            control.MouseLeave += new EventHandler<EventArgs>(this.OnItemMouseLeave);
            control.MouseClick += new EventHandler<MouseEventArgs>(this.OnItemMouseClick);
            control.MouseDoubleClick += new EventHandler<MouseEventArgs>(this.OnItemMouseDbClick);
            for (uint index = 1; index < this.ColumnCount; ++index)
            {
                DXLabel dxLabel = new DXLabel();
                dxLabel.Parent = control.Parent;
                dxLabel.Text = index.ToString();
                dxLabel.Border = control.Border;
                dxLabel.BorderColour = control.BorderColour;
                dxLabel.AutoSize = false;
                dxLabel.MouseWheel += new EventHandler<MouseEventArgs>(this.VScrollBar.DoMouseWheel);
            }
            control.MouseWheel += new EventHandler<MouseEventArgs>(this.VScrollBar.DoMouseWheel);
            this.UpdateScrollBar();
            return nItem;
        }

        public DXControl SetItem(uint nItem, uint subItem, DXControl control)
        {
            if (nItem < this.ItemCount && subItem < this.ColumnCount)
            {
                DXControl control1 = this.Items.Controls[(int)nItem];
                DXControl control2 = control1.Controls[(int)subItem];
                control2.Parent = (DXControl)null;
                control.Parent = control1;
                control1.Controls.Remove(control);
                control1.Controls.Insert((int)subItem, control);
                control.Border = this.ItemBorder;
                control.BorderColour = this.ItemBorderColour;
                control.BackColour = this.ItemBackColour;
                control.MouseUp += new EventHandler<MouseEventArgs>(this.OnItemMouseUp);
                control.MouseWheel += new EventHandler<MouseEventArgs>(this.VScrollBar.DoMouseWheel);
                control.MouseEnter += new EventHandler<EventArgs>(this.OnItemMouseEnter);
                control.MouseLeave += new EventHandler<EventArgs>(this.OnItemMouseLeave);
                control.MouseClick += new EventHandler<MouseEventArgs>(this.OnItemMouseClick);
                control.MouseDoubleClick += new EventHandler<MouseEventArgs>(this.OnItemMouseDbClick);
                control2.Dispose();
            }
            return control;
        }

        public DXControl GetItem(uint nItem, uint nSubItem)
        {
            if (nItem < this.ItemCount && nSubItem < this.ColumnCount)
                return this.Items.Controls[(int)nItem].Controls[(int)nSubItem];
            return (DXControl)null;
        }

        public uint InsertItem(uint nItem, string text)
        {
            int num = (int)nItem;
            DXLabel dxLabel = new DXLabel();
            dxLabel.Text = text;
            dxLabel.AutoSize = false;
            dxLabel.Size = new Size(0, this.ItemHeight);
            dxLabel.DrawFormat = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
            return this.InsertItem((uint)num, (DXControl)dxLabel);
        }

        public DXControl SetItem(uint nItem, uint nSubItem, string text)
        {
            int num1 = (int)nItem;
            int num2 = (int)nSubItem;
            DXLabel dxLabel = new DXLabel();
            dxLabel.Text = text;
            dxLabel.AutoSize = false;
            dxLabel.DrawFormat = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
            return this.SetItem((uint)num1, (uint)num2, (DXControl)dxLabel);
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
            return this.InsertColumn((uint)num, (DXControl)dxLabel);
        }

        public void SortByName(string name)
        {
            for (int index = 0; (long)index < (long)this.ItemCount; ++index)
            {
                DXControl control = this.Items.Controls[index];
                if (control.Controls.Count != 0 && control.Controls[0].Text.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    this.Items.Controls.RemoveAt(index);
                    this.Items.Controls.Insert(0, control);
                }
            }
            this.VScrollBar.Value = 0;
            this.UpdateItems();
            this.UpdateScrollBar();
        }

        public void DeleteItem(uint nItem)
        {
            if (nItem >= this.ItemCount)
                return;
            DXControl control = this.Items.Controls[(int)nItem];
            this.Items.Controls.RemoveAt((int)nItem);
            this.UpdateScrollBar();
            this.UpdateItems();
            control.Dispose();
        }

        public void RemoveAll()
        {
            for (uint index = 0; index < this.ItemCount; ++index)
            {
                DXControl control = this.Items.Controls[(int)index];
                this.Items.Controls.RemoveAt((int)index);
                control.Dispose();
            }
            this.UpdateScrollBar();
            this.UpdateItems();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.Items == null || this.Items.IsDisposed)
                return;
            this.Items.Dispose();
        }
    }
}
