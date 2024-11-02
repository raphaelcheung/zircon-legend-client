using System;
using System.Drawing;
using System.Windows.Forms;
using Library;

//Cleaned
namespace Client.Controls
{
    public sealed class DXCheckBox : DXControl
    {
        #region Properites
        public bool bAlignRight { get; set; } = false;

        public bool AutoSize
        {
            get => _AutoSize;
            set
            {
                if (value == _AutoSize) return;
                
                var oldValue = _AutoSize;
                Label.AutoSize = value;
                _AutoSize = value;
            }
        }
        private bool _AutoSize = false;


        #region Checked

        public bool Checked
        {
            get => _Checked;
            set
            {
                if (_Checked == value) return;

                bool oldValue = _Checked;
                _Checked = value;

                OnCheckedChanged(oldValue, value);
            }
        }
        private bool _Checked;
        public event EventHandler<EventArgs> CheckedChanged;
        public void OnCheckedChanged(bool oValue, bool nValue)
        {
            CheckedChanged?.Invoke(this, EventArgs.Empty);

            Box.Index = Checked ? 162 : 161;
        }

        #endregion

        #region ReadOnly

        public bool ReadOnly
        {
            get => _ReadOnly;
            set
            {
                if (_ReadOnly == value) return;

                bool oldValue = _ReadOnly;
                _ReadOnly = value;

                OnReadOnlyChanged(oldValue, value);
            }
        }
        private bool _ReadOnly;
        public event EventHandler<EventArgs> ReadOnlyChanged;
        public void OnReadOnlyChanged(bool oValue, bool nValue)
        {
            ReadOnlyChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public DXLabel Label { get; private set; }
        public DXImageControl Box { get; private set; }
        
        public override void OnDisplayAreaChanged(Rectangle oValue, Rectangle nValue)
        {
            base.OnDisplayAreaChanged(oValue, nValue);

            UpdateControl();
        }

        #endregion
        
        public DXCheckBox()
        {
            BackColour = Color.Empty;
            DrawTexture = true;

            Label = new DXLabel
            {
                Parent = this,
                IsControl = false,
                Location = new Point(0, -1),
                VerticalCenter = true,
                AutoSize = false,
            };

            Label.DisplayAreaChanged += (o, e) =>
            {
                Rectangle displayArea;
                if (AutoSize)
                {
                    displayArea = Label.DisplayArea;
                    int width1 = displayArea.Width;
                    displayArea = Box.DisplayArea;
                    int width2 = displayArea.Width;
                    int width3 = width1 + width2;
                    displayArea = Box.DisplayArea;
                    int height = displayArea.Height;
                    Size = new Size(width3, height);
                }

                if (bAlignRight)
                {
                    Size size1;
                    if (AutoSize)
                    {
                        DXLabel label = Label;
                        size1 = Size;
                        int width1 = size1.Width;
                        displayArea = Box.DisplayArea;
                        int width2 = displayArea.Width;
                        int width3 = width1 - width2;
                        displayArea = Box.DisplayArea;
                        int height = displayArea.Height;
                        Size size2 = new Size(width3, height);
                        label.Size = size2;
                    }
                    else
                    {
                        DXLabel label = Label;
                        size1 = Size;
                        int width1 = size1.Width;
                        displayArea = Box.DisplayArea;
                        int width2 = displayArea.Width;
                        int width3 = width1 - width2;
                        size1 = Size;
                        int height = size1.Height;
                        Size size2 = new Size(width3, height);
                        label.Size = size2;
                    }
                    DXImageControl box = Box;
                    displayArea = Label.DisplayArea;
                    int width = displayArea.Width;
                    size1 = Size;
                    int height1 = size1.Height;
                    size1 = Label.Size;
                    int height2 = size1.Height;
                    int y = (height1 - height2) / 2;
                    Point point = new Point(width, y);
                    box.Location = point;
                }
                else
                {
                    Size size1;
                    if (AutoSize)
                    {
                        DXLabel label = Label;
                        size1 = Size;
                        int width1 = size1.Width;
                        displayArea = Box.DisplayArea;
                        int width2 = displayArea.Width;
                        int width3 = width1 - width2;
                        displayArea = Box.DisplayArea;
                        int height = displayArea.Height;
                        Size size2 = new Size(width3, height);
                        label.Size = size2;
                    }
                    else
                    {
                        DXLabel label = Label;
                        size1 = Size;
                        int width1 = size1.Width;
                        displayArea = Box.DisplayArea;
                        int width2 = displayArea.Width;
                        int width3 = width1 - width2;
                        size1 = Size;
                        int height = size1.Height;
                        Size size2 = new Size(width3, height);
                        label.Size = size2;
                    }
                    DXLabel label1 = Label;
                    displayArea = Box.DisplayArea;
                    int width = displayArea.Width;
                    size1 = Size;
                    int height1 = size1.Height;
                    size1 = Label.Size;
                    int height2 = size1.Height;
                    int y = (height1 - height2) / 2;
                    Point point = new Point(width, y);
                    label1.Location = point;
                }
                //Size = new Size(Label.DisplayArea.Width + Box.DisplayArea.Width, Box.DisplayArea.Height);
                //Box.Location = new Point(Label.DisplayArea.Width, 0);
            };


            Box = new DXImageControl
            {
                Location = new Point(Label.Size.Width + 2, 0),
                Index = 161,
                LibraryFile = LibraryFile.GameInter,
                Parent = this,
                IsControl = false,
            };
            Box.DisplayAreaChanged += (o, e) =>
            {
                Rectangle displayArea;
                if (AutoSize)
                {
                    displayArea = Label.DisplayArea;
                    int width1 = displayArea.Width;
                    displayArea = Box.DisplayArea;
                    int width2 = displayArea.Width;
                    int width3 = width1 + width2;
                    displayArea = Box.DisplayArea;
                    int height = displayArea.Height;
                    Size = new Size(width3, height);
                }
                if (bAlignRight)
                {
                    Size size1;
                    if (AutoSize)
                    {
                        DXLabel label = Label;
                        size1 = Size;
                        int width1 = size1.Width;
                        displayArea = Box.DisplayArea;
                        int width2 = displayArea.Width;
                        int width3 = width1 - width2;
                        displayArea = Box.DisplayArea;
                        int height = displayArea.Height;
                        Size size2 = new Size(width3, height);
                        label.Size = size2;
                    }
                    else
                    {
                        DXLabel label = Label;
                        size1 = Size;
                        int width1 = size1.Width;
                        displayArea = Box.DisplayArea;
                        int width2 = displayArea.Width;
                        int width3 = width1 - width2;
                        size1 = Size;
                        int height = size1.Height;
                        Size size2 = new Size(width3, height);
                        label.Size = size2;
                    }
                    DXImageControl box = Box;
                    displayArea = Label.DisplayArea;
                    int width = displayArea.Width;
                    size1 = Size;
                    int height1 = size1.Height;
                    size1 = Box.Size;
                    int height2 = size1.Height;
                    int y = (height1 - height2) / 2;
                    Point point = new Point(width, y);
                    box.Location = point;
                }
                else
                {
                    Size size1;
                    if (AutoSize)
                    {
                        DXLabel label = Label;
                        size1 = Size;
                        int width1 = size1.Width;
                        displayArea = Box.DisplayArea;
                        int width2 = displayArea.Width;
                        int width3 = width1 - width2;
                        displayArea = Box.DisplayArea;
                        int height = displayArea.Height;
                        Size size2 = new Size(width3, height);
                        label.Size = size2;
                    }
                    else
                    {
                        DXLabel label = Label;
                        size1 = Size;
                        int width1 = size1.Width;
                        displayArea = Box.DisplayArea;
                        int width2 = displayArea.Width;
                        int width3 = width1 - width2;
                        size1 = Size;
                        int height = size1.Height;
                        Size size2 = new Size(width3, height);
                        label.Size = size2;
                    }
                    DXLabel label1 = Label;
                    displayArea = Box.DisplayArea;
                    int width = displayArea.Width;
                    size1 = Size;
                    int height1 = size1.Height;
                    size1 = Label.Size;
                    int height2 = size1.Height;
                    int y = (height1 - height2) / 2;
                    Point point = new Point(width, y);
                    label1.Location = point;
                }
                //Size = new Size(Label.DisplayArea.Width + Box.DisplayArea.Width, Box.DisplayArea.Height);
                //Box.Location = new Point(Label.DisplayArea.Width, 0);
            };

            if (!AutoSize) return;

            Size = new Size(18, 18);
        }

        #region Methods

        public void UpdateControl()
        {
            if (Label == null) return;

            //Label.Location = new Point(0, 0);
            //Size = new Size(Label.DisplayArea.Width + Box.DisplayArea.Width, Box.DisplayArea.Height);
            //Box.Location = new Point(Label.DisplayArea.Width, 0);

            Rectangle displayArea;
            if (AutoSize)
            {
                int width = Label.DisplayArea.Width + Box.DisplayArea.Width;
                displayArea = Box.DisplayArea;
                int height = displayArea.Height;
                Size = new Size(width, height);
            }
            if (bAlignRight)
            {
                Size size1;
                if (AutoSize)
                {
                    DXLabel label = Label;
                    size1 = Label.Size;
                    int width = size1.Width;
                    displayArea = Box.DisplayArea;
                    int height = displayArea.Height;
                    Size size2 = new Size(width, height);
                    label.Size = size2;
                }
                else
                {
                    DXLabel label = Label;
                    int width1 = Size.Width;
                    displayArea = Box.DisplayArea;
                    int width2 = displayArea.Width;
                    Size size2 = new Size(width1 - width2, Size.Height);
                    label.Size = size2;
                }
                DXLabel label1 = Label;
                int x = 0;
                size1 = Size;
                int height1 = size1.Height;
                size1 = Label.Size;
                int height2 = size1.Height;
                int y1 = (height1 - height2) / 2;
                Point point1 = new Point(x, y1);
                label1.Location = point1;
                DXImageControl box = Box;
                displayArea = Label.DisplayArea;
                int width3 = displayArea.Width;
                size1 = Size;
                int height3 = size1.Height;
                size1 = Box.Size;
                int height4 = size1.Height;
                int y2 = (height3 - height4) / 2;
                Point point2 = new Point(width3, y2);
                box.Location = point2;
            }
            else
            {
                Size size1;
                if (AutoSize)
                {
                    DXLabel label = Label;
                    size1 = Label.Size;
                    int width = size1.Width;
                    displayArea = Box.DisplayArea;
                    int height = displayArea.Height;
                    Size size2 = new Size(width, height);
                    label.Size = size2;
                }
                else
                {
                    DXLabel label = Label;
                    int width1 = Size.Width;
                    displayArea = Box.DisplayArea;
                    int width2 = displayArea.Width;
                    Size size2 = new Size(width1 - width2, Size.Height);
                    label.Size = size2;
                }
                DXImageControl box = Box;
                int x = 0;
                size1 = Size;
                int height1 = size1.Height;
                size1 = Box.Size;
                int height2 = size1.Height;
                int y1 = (height1 - height2) / 2;
                Point point1 = new Point(x, y1);
                box.Location = point1;
                DXLabel label1 = Label;
                displayArea = Box.DisplayArea;
                int width3 = displayArea.Width;
                size1 = Size;
                int height3 = size1.Height;
                size1 = Label.Size;
                int height4 = size1.Height;
                int y2 = (height3 - height4) / 2;
                Point point2 = new Point(width3, y2);
                label1.Location = point2;
            }
        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            if (!IsEnabled) return;

            base.OnMouseClick(e);

            if (ReadOnly) return;

            Checked = !Checked;
        }

        public override void OnTextChanged(string oValue, string nValue)
        {
            Label.Text = nValue;

            base.OnTextChanged(oValue, nValue);
        }
        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (Label != null)
                {
                    if (!Label.IsDisposed)
                        Label.Dispose();
                    Label = null;
                }

                if (Box != null)
                {
                    if (!Box.IsDisposed)
                        Box.Dispose();
                    Box = null;
                }

                _Checked = false;
                CheckedChanged = null;

                _ReadOnly = false;
                ReadOnlyChanged = null;
            }
        }
        #endregion
    }
}
