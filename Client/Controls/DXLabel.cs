using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security;
using System.Windows.Forms;
using Client.Envir;
using SlimDX;
using SlimDX.Direct3D9;
using static Client.Scenes.Views.MapControl;
using Font = System.Drawing.Font;

//Cleaned
namespace Client.Controls
{
    public class DXLabel : DXControl
    {
        #region Static
        public static Size GetSize(string text, Font font, byte outlineWeight = 0)
        {
            if (string.IsNullOrEmpty(text))
                return Size.Empty;
            
            Size tempSize = TextRenderer.MeasureText(DXManager.Graphics, text, font);

            if (tempSize.Width > 0 && tempSize.Height > 0)
            {
                tempSize.Width += outlineWeight * 2;
                tempSize.Height += outlineWeight * 2;
            }

            return tempSize;
        }

        public Size MeasureSize()
        {
            if (string.IsNullOrEmpty(Text))
                return Size.Empty;

            Size tempSize = TextRenderer.MeasureText(DXManager.Graphics, Text, Font, DisplayArea.Size, DrawFormat);

            if (Outline && tempSize.Width > 0 && tempSize.Height > 0)
            {
                tempSize.Width += OutlineWeight * 2;
                tempSize.Height += OutlineWeight * 2;
            }

            return tempSize;
        }
        public static Size GetHeight(DXLabel label, int width)
        {
            Size tempSize = TextRenderer.MeasureText(DXManager.Graphics, label.Text, label.Font, new Size(width, 2000), label.DrawFormat);

            if (label.Outline && tempSize.Width > 0 && tempSize.Height > 0)
            {
                tempSize.Width += label.OutlineWeight * 2;
                tempSize.Height += label.OutlineWeight * 2;
            }

            return tempSize;
        }
        #endregion

        #region Properties

        #region AutoSize

        public bool AutoSize
        {
            get => _AutoSize;
            set
            {
                if (_AutoSize == value) return;

                bool oldValue = _AutoSize;
                _AutoSize = value;

                OnAutoSizeChanged(oldValue, value);
            }
        }
        private bool _AutoSize;
        public event EventHandler<EventArgs> AutoSizeChanged;
        public virtual void OnAutoSizeChanged(bool oValue, bool nValue)
        {
            TextureValid = false;
            CreateSize();

            AutoSizeChanged?.Invoke(this, EventArgs.Empty);
        }



        #endregion
        
        #region DrawFormat

        public TextFormatFlags DrawFormat
        {
            get => _DrawFormat;
            set
            {
                if (_DrawFormat == value) return;

                TextFormatFlags oldValue = _DrawFormat;
                _DrawFormat = value;

                OnDrawFormatChanged(oldValue, value);
            }
        }
        private TextFormatFlags _DrawFormat;
        public event EventHandler<EventArgs> DrawFormatChanged;
        public virtual void OnDrawFormatChanged(TextFormatFlags oValue, TextFormatFlags nValue)
        {
            TextureValid = false;

            DrawFormatChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
        
        #region Font

        public Font Font
        {
            get => _Font;
            set
            {
                if (_Font == value) return;

                Font oldValue = _Font;
                _Font = value;

                OnFontChanged(oldValue, value);
            }
        }
        private Font _Font;
        public event EventHandler<EventArgs> FontChanged;
        public virtual void OnFontChanged(Font oValue, Font nValue)
        {
            FontChanged?.Invoke(this, EventArgs.Empty);

            TextureValid = false;
            CreateSize();
        }

        #endregion

        #region Outline
        public byte OutlineWeight
        {
            get => _OutlineWeight;
            set
            {
                if (_OutlineWeight == value) return;

                byte old = _OutlineWeight;
                _OutlineWeight = value;
                OnOutlineWeightChanged(old, value);
            }
        }
        private byte _OutlineWeight = 1;
        public virtual void OnOutlineWeightChanged(byte oValue, byte nValue)
        {
            TextureValid = false;
            CreateSize();
        }

        public bool Outline
        {
            get => _Outline;
            set
            {
                if (_Outline == value) return;

                bool oldValue = _Outline;
                _Outline = value;

                OnOutlineChanged(oldValue, value);
            }
        }
        private bool _Outline;
        public event EventHandler<EventArgs> OutlineChanged;
        public virtual void OnOutlineChanged(bool oValue, bool nValue)
        {
            TextureValid = false;
            CreateSize();

            OutlineChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
        
        #region OutlineColour

        public Color OutlineColour
        {
            get => _OutlineColour;
            set
            {
                if (_OutlineColour == value) return;

                Color oldValue = _OutlineColour;
                _OutlineColour = value;

                OnOutlineColourChanged(oldValue, value);
            }
        }
        private Color _OutlineColour;
        public event EventHandler<EventArgs> OutlineColourChanged;
        public event EventHandler<EventArgs> VerticalCenterChanged;
        public virtual void OnOutlineColourChanged(Color oValue, Color nValue)
        {
            TextureValid = false;

            OutlineColourChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public bool VerticalCenter
        {
            get => _VerticalCenter;
            set
            {
                if (_VerticalCenter == value) return;

                _VerticalCenter = value;
                OnVerticalCenterChanged();
                VerticalCenterChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        private bool _VerticalCenter = false;

        public override void OnTextChanged(string oValue, string nValue)
        {
            base.OnTextChanged(oValue, nValue);

            TextureValid = false;
            CreateSize();
        }
        protected virtual void OnVerticalCenterChanged()
        {
            TextureValid = false;
        }
        public override void OnForeColourChanged(Color oValue, Color nValue)
        {
            base.OnForeColourChanged(oValue, nValue);

            TextureValid = false;
        }
        #endregion

        public DXLabel()
        {
            BackColour = Color.Empty;
            DrawTexture = true;
            AutoSize = true;
            Font = new Font("宋体", CEnvir.FontSize(9F));
            DrawFormat = TextFormatFlags.WordBreak;
            
            Outline = true;
            ForeColour = Color.FromArgb(198, 166, 99);
            OutlineColour = Color.FromArgb(255, 10, 10, 10);
        }

        #region Methods
        private void CreateSize()
        {
            if (!AutoSize) return;

            Size = GetSize(Text, Font, Outline ? OutlineWeight : (byte)0);
        }

        protected override void CreateTexture()
        {
            int width = DisplayArea.Width;
            int height = DisplayArea.Height;

            if (ControlTexture == null || DisplayArea.Size != TextureSize)
            {
                DisposeTexture();
                TextureSize = DisplayArea.Size;
                ControlTexture = new Texture(DXManager.Device, TextureSize.Width, TextureSize.Height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                DXManager.ControlList.Add(this);
            }
            
            DataRectangle rect = ControlTexture.LockRectangle(0, LockFlags.Discard);
            Point start = new Point(0, 0);

            if (!AutoSize && VerticalCenter)
            {
                var text_size = MeasureSize();

                start = new Point(0, (height - text_size.Height) / 2);
            }

            using (Bitmap image = new Bitmap(width, height, width*4, PixelFormat.Format32bppArgb, rect.Data.DataPointer))
            using (Graphics graphics = Graphics.FromImage(image))
            {
                DXManager.ConfigureGraphics(graphics);
                graphics.Clear(BackColour);

                if (Outline)
                    DrawOutline(graphics, start.X, start.Y, width, height, OutlineWeight);
                else
                    TextRenderer.DrawText(graphics, Text, Font, new Rectangle(start.X + 1, start.Y + 0, width, height), ForeColour, DrawFormat);
            }

            ControlTexture.UnlockRectangle(0);
            rect.Data.Dispose();
            
            TextureValid = true;
            ExpireTime = CEnvir.Now + Config.CacheDuration;
        }
        private void DrawOutline(Graphics graphics, int x, int y, int w, int h, byte weight)
        {
            if (weight <= 0) return;


            for(int i = 1; i <= weight; i++)
            {
                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x - i + weight, y + weight, w, h), OutlineColour, DrawFormat);
                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x + i + weight, y + weight, w, h), OutlineColour, DrawFormat);
                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x + weight, y - i + weight, w, h), OutlineColour, DrawFormat);
                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x + weight, y + i + weight, w, h), OutlineColour, DrawFormat);
                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x + i + weight, y + i + weight, w, h), OutlineColour, DrawFormat);
                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x + i + weight, y - i + weight, w, h), OutlineColour, DrawFormat);

                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x - i + weight, y - i + weight, w, h), OutlineColour, DrawFormat);
                TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x - i + weight, y + i + weight, w, h), OutlineColour, DrawFormat);


            }

            TextRenderer.DrawText(graphics, Text, Font, new Rectangle(x + weight, y + weight, w, h), ForeColour, DrawFormat);

        }
        protected override void DrawControl()
        {
            if (!DrawTexture) return;

            if (!TextureValid) CreateTexture();

            DXManager.SetOpacity(Opacity);
            
            PresentTexture(ControlTexture, Parent, DisplayArea, IsEnabled ? Color.White : Color.FromArgb(75, 75, 75), this);

            ExpireTime = CEnvir.Now + Config.CacheDuration;
        }
        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _AutoSize = false;
                _DrawFormat = TextFormatFlags.Default;
                _Font?.Dispose();
                _Font = null;
                _Outline = false;
                _OutlineColour = Color.Empty;

                AutoSizeChanged = null;
                DrawFormatChanged = null;
                FontChanged = null;
                OutlineChanged = null;
                OutlineColourChanged = null;
            }
        }
        #endregion
    }
}
