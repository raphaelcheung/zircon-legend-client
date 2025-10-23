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
            Parent = GameScene.Game.MainPanel;
            Location = new Point(0, 0);  // 从左上角开始调试
            Size = new Size(800, 60);       // 宽度足够容纳多个血条，高度足够显示一行
            DrawTexture = false;
            IsControl = false;
        }

        public override void Process()
        {
            base.Process();
        }

        protected override void DrawControl()
        {
            base.DrawControl();

            if (GameScene.Game == null) return;

            MirLibrary lib;
            if (!CEnvir.LibraryList.TryGetValue(LibraryFile.Interface, out lib)) return;

            // 相对于面板的位置
            int xStart = 0;
            int y = 0;
            
            // 绘制时的坐标调整
            int xOffset = 280;
            int yOffset = -33;

            // 获取血条图像（使用游戏中宠物血条的图像ID 79为血条，80为边框）
            MirImage barBackImage = lib.CreateImage(80, ImageType.Image);  // 边框
            MirImage barFillImage = lib.CreateImage(79, ImageType.Image);  // 填充
            
            if (barBackImage == null || barFillImage == null) return;

            int barWidth = barBackImage.Width;
            int barHeight = barBackImage.Height;
            int spacing = barWidth + 2;  // 水平间距

            // 收集并排序宠物数据，炎魔排第一个
            var companions = new List<ClientObjectData>();
            var infernalSoldiers = new List<ClientObjectData>();

            foreach (var data in GameScene.Game.DataDictionary.Values)
            {
                if (string.IsNullOrEmpty(data.PetOwner)) continue;
                if (data.PetOwner != GameScene.Game.User.Name) continue;
                if (data.MaxHealth <= 0) continue;
                if (data.Dead) continue;

                if (data.MonsterInfo?.Image == MonsterImage.InfernalSoldier)
                    infernalSoldiers.Add(data);
                else
                    companions.Add(data);
            }

            // 合并列表，炎魔在前
            var sortedCompanions = new List<ClientObjectData>();
            sortedCompanions.AddRange(infernalSoldiers);
            sortedCompanions.AddRange(companions);

            int x = xStart;

            foreach (var data in sortedCompanions)
            {
                float percent = Math.Max(0, Math.Min(1F, data.Health / (float)data.MaxHealth));

                // 根据宠物类型设置血条颜色
                Color barColor = (data.MonsterInfo?.Image == MonsterImage.InfernalSoldier) ? Color.Orange : Color.Yellow;

                // 使用相对于该面板的坐标，加上偏移量
                int screenX = DisplayArea.X + x + xOffset;
                int screenY = DisplayArea.Y + y + yOffset;

                // 先绘制边框（图像80）
                DXManager.SetOpacity(1F);
                lib.Draw(80, screenX, screenY, Color.White, false, 1F, ImageType.Image);
                
                // 再绘制填充的血条（图像79，按比例缩放宽度）
                lib.Draw(79, screenX + 1, screenY + 1, barColor, new Rectangle(0, 0, (int)(barWidth * percent), barHeight), 1F, ImageType.Image);

                // draw name above bar
                var g = DXManager.Graphics;
                TextRenderer.DrawText(g, data.Name ?? string.Empty, NameFont, new Rectangle(screenX, screenY - 16, barWidth, 14), Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

                x += spacing;  // 从左向右排列
                
                if (x > Size.Width - barWidth - 10) break; // 防止超出面板右边界
            }
        }
    }
}

