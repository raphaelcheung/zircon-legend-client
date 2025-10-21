using System;
using System.Collections.Generic;
using System.Drawing;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Library;
using System.Windows.Forms;

namespace Client.Scenes.Views
{
    public sealed class CompanionHealthPanel : DXControl
    {
        private static readonly Font NameFont = new Font("宋体", 9F);
        public CompanionHealthPanel()
        {
            Parent = GameScene.Game;
            Location = new Point(4, 4);
            Size = new Size(200, 200);
            DrawTexture = false;
            IsControl = false;
        }

        public override void Process()
        {
            base.Process();
            // nothing here, child-drawn from Draw Control
        }

        protected override void DrawControl()
        {
            base.DrawControl();

            if (GameScene.Game == null) return;

            MirLibrary lib;
            if (!CEnvir.LibraryList.TryGetValue(LibraryFile.Interface, out lib)) return;

            int x = DisplayArea.Left + 4;
            MirImage bgImage = lib.CreateImage(80, ImageType.Image);
            MirImage fgImage = lib.CreateImage(79, ImageType.Image);
            int barWidth = Math.Min(bgImage?.Width ?? 150, DisplayArea.Width - 40);
            if (barWidth < 10) barWidth = 10;
            int barHeight = bgImage?.Height ?? 8;
            int spacing = 1;

            int y = DisplayArea.Bottom - 4 - barHeight; // start from bottom, stack upwards

            foreach (var data in GameScene.Game.DataDictionary.Values)
            {
                if (string.IsNullOrEmpty(data.PetOwner)) continue;
                if (data.PetOwner != GameScene.Game.User.Name) continue;
                if (data.MaxHealth <= 0) continue;
                if (data.Dead) continue; // 死亡的不显示
                if (data.Health <= 0) continue; // 血量为0的不显示

                float percent = Math.Max(0, Math.Min(1F, data.Health / (float)data.MaxHealth));

                // draw background bar
                if (bgImage != null)
                    lib.Draw(80, x, y, Color.White, false, 1F, ImageType.Image);
                // draw health portion
                if (fgImage != null)
                    lib.Draw(79, x + 1, y + 1, Color.Red, new Rectangle(0, 0, (int)(fgImage.Width * percent), fgImage.Height), 1F, ImageType.Image);

                // draw name next to bar (bounded by panel width)
                DXManager.SetOpacity(1F);
                var g = DXManager.Graphics;
                int textWidth = DisplayArea.Width - (barWidth + 10);
                if (textWidth < 20) textWidth = 20;
                TextRenderer.DrawText(g, data.Name ?? string.Empty, NameFont, new Rectangle(x + barWidth + 6, y, textWidth, barHeight + 4), Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                y -= barHeight + spacing; // stack upward
                if (y < DisplayArea.Top + 4) break;
            }
        }
    }
}
