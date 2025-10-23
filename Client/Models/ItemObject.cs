using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Controls;
using Client.Envir;
using Client.Scenes;
using Client.Scenes.Views;
using Library;
using Library.SystemModels;
using  S = Library.Network.ServerPackets;

namespace Client.Models
{
    public sealed class ItemObject : MapObject
    {
        public override ObjectType Race  => ObjectType.Item;

        public DXLabel FocusLabel;
        private BigPatchDialog.CItemFilterSet FilterSet = null;
        public override bool Blocking => false;

        public ClientUserItem Item;
        public MirLibrary BodyLibrary;
        public Color LabelBackColour = Color.FromArgb(30, 0, 24, 48);

        public ItemObject(S.ObjectItem info)
        {
            ObjectID = info.ObjectID;

            Item = info.Item;

            ItemInfo itemInfo = info.Item.Info;

            if (info.Item.Info.Effect == ItemEffect.ItemPart)
            {
                itemInfo = Globals.ItemInfoList.Binding.First(x => x.Index == Item.AddedStats[Stat.ItemIndex]);

                Title = "※";
            }

            Name = Item.Count > 1 ? $"{itemInfo.ItemName} ({Item.Count})" : itemInfo.ItemName;

            if ((Item.Flags & UserItemFlags.QuestItem) == UserItemFlags.QuestItem)
                Title = "(任务)";



            switch (itemInfo.Rarity)
            {
                case Rarity.Common:
                    if (Item.AddedStats.Values.Count > 0 && Item.Info.Effect != ItemEffect.ItemPart)
                    {
                        NameColour = Color.White;

                        Effects.Add(new MirEffect(110, 10, TimeSpan.FromMilliseconds(100), LibraryFile.ProgUse, 60, 60, Color.LightSkyBlue)
                        {
                            Target = this,
                            Loop = true,
                            Blend = true,
                            BlendRate = 0.5F,
                        });
                    }
                    else if (Item.Info.Effect == ItemEffect.ItemPart) // 碎片设置暗灰色
                        NameColour = Color.DarkGray;
                    else
                        NameColour = Color.LightGray;
                    break;
                case Rarity.Superior:
                    if (Item.AddedStats.Values.Count > 0 && Item.Info.Effect != ItemEffect.ItemPart)
                    {
                        NameColour = Color.Green; // 高级物品有额外属性时：正绿色

                        Effects.Add(new MirEffect(100, 10, TimeSpan.FromMilliseconds(100), LibraryFile.ProgUse, 60, 60, Color.Green)
                        {
                            Target = this,
                            Loop = true,
                            Blend = true,
                            BlendRate = 0.5F,
                        });
                    }
                    else if (Item.Info.Effect == ItemEffect.ItemPart) // 碎片设置灰绿色
                    {
                        NameColour = Color.DarkSeaGreen; // 更浅的灰绿色
                    }
                    else
                    {
                        NameColour = Color.LightGreen; // 高级物品：亮浅绿
                        Effects.Add(new MirEffect(100, 10, TimeSpan.FromMilliseconds(100), LibraryFile.ProgUse, 60, 60, Color.LightGreen)
                        {
                            Target = this,
                            Loop = true,
                            Blend = true,
                            BlendRate = 0.5F,
                        });
                    }
                    break;
                case Rarity.Elite:
                    if (Item.AddedStats.Values.Count > 0 && Item.Info.Effect != ItemEffect.ItemPart)
                    {
                        NameColour = Color.Magenta; // 稀有物品有额外属性时：正紫色

                        Effects.Add(new MirEffect(120, 10, TimeSpan.FromMilliseconds(100), LibraryFile.ProgUse, 60, 60, Color.BlueViolet)
                        {
                            Target = this,
                            Loop = true,
                            Blend = true,
                            BlendRate = 0.5F,
                        });
                    }
                    else if (Item.Info.Effect == ItemEffect.ItemPart) // 碎片设置灰色粉紫色
                    {
                        NameColour = Color.FromArgb(186, 130, 165); // 灰色粉紫色碎片
                    }
                    else
                    {
                        NameColour = Color.Violet; // 稀有物品：更饱和的亮浅紫
                        Effects.Add(new MirEffect(120, 10, TimeSpan.FromMilliseconds(100), LibraryFile.ProgUse, 60, 60, Color.Violet)
                        {
                            Target = this,
                            Loop = true,
                            Blend = true,
                            BlendRate = 0.5F,
                        });
                    }
                    break;
            }

            FilterSet = GameScene.Game?.BigPatchBox?.GetFilterItem(itemInfo.Index);
            if (FilterSet != null)
            {
                FilterSet.ShowChanged += OnNameShowChanged;

                if (FilterSet.hint)
                {
                    GameScene game = GameScene.Game;
                    string[] strArray = new string[8];
                    strArray[0] = ">>>>   极品   [ ";
                    strArray[1] = itemInfo.ItemName;
                    strArray[2] = " ";
                    int index1 = 3;
                    Point location = info.Location;
                    string str1 = location.X.ToString();
                    strArray[index1] = str1;
                    strArray[4] = ",";
                    int index2 = 5;
                    location = info.Location;
                    string str2 = location.Y.ToString();
                    strArray[index2] = str2;
                    strArray[6] = "]   在   ";
                    strArray[7] = CEnvir.GetDirName(MapObject.User.CurrentLocation, info.Location);
                    string message = string.Concat(strArray);
                    int num = 16;
                    game.ReceiveChat(message, (MessageType)num);
                }

                if (!FilterSet.show)
                {
                    if (NameLabel != null)
                        NameLabel.Visible = false;
                    if (TitleNameLabel != null)
                        TitleNameLabel.Visible = false;
                }
            }

            CurrentLocation = info.Location;


            UpdateLibraries();

            SetFrame(new ObjectAction(MirAction.Standing, Direction, CurrentLocation));

            GameScene.Game.MapControl.AddObject(this);
        }
        private void OnNameShowChanged(object sender, EventArgs arg)
        {
            if (FilterSet == null) return;

            if (NameLabel != null)
            {
                NameLabel.Visible = FilterSet.show;
                NameLabel.IsVisible = FilterSet.show;
            }
                
            if (TitleNameLabel != null)
            {
                TitleNameLabel.Visible = FilterSet.show;
                TitleNameLabel.IsVisible = FilterSet.show;
            }
        }
        public void UpdateLibraries()
        {
            Frames = FrameSet.DefaultItem;

            CEnvir.LibraryList.TryGetValue(LibraryFile.Ground, out BodyLibrary);
        }

        public override void SetAnimation(ObjectAction action)
        {

            CurrentAnimation = MirAnimation.Standing;
            if (!Frames.TryGetValue(CurrentAnimation, out CurrentFrame))
                CurrentFrame = Frame.EmptyFrame;
        }

        public override void Draw()
        {
            if (BodyLibrary == null) return;

            int drawIndex;

            if (Item.Info.Effect == ItemEffect.Gold)
            {
                if (Item.Count < 100)
                    drawIndex = 120;
                else if (Item.Count < 200)
                    drawIndex = 121;
                else if (Item.Count < 500)
                    drawIndex = 122;
                else if (Item.Count < 1000)
                    drawIndex = 123;
                else if (Item.Count < 1000000) //1 Million
                    drawIndex = 124;
                else if (Item.Count < 5000000) //5 Million
                    drawIndex = 125;
                else if (Item.Count < 10000000) //10 Million
                    drawIndex = 126;
                else
                    drawIndex = 127;
            }
            else
            {
                ItemInfo info = Item.Info;

                if (info.Effect == ItemEffect.ItemPart)
                    info = Globals.ItemInfoList.Binding.First(x => x.Index == Item.AddedStats[Stat.ItemIndex]);

                drawIndex = info.Image;
            }

            Size size = BodyLibrary.GetSize(drawIndex);

            BodyLibrary.Draw(drawIndex, DrawX + (CellWidth - size.Width)/2, DrawY + (CellHeight - size.Height)/2, DrawColour, false, 1F, ImageType.Image);

        }

        public override bool MouseOver(Point p)
        {
            return false;
        }

        public override void NameChanged()
        {
            base.NameChanged();

            // 特殊处理：如果是碎片，让 TitleNameLabel 也使用和 NameLabel 相同的颜色
            if (!string.IsNullOrEmpty(Title) && Title == "*")
            {
                if (!NameLabels.TryGetValue(Title, out List<DXLabel> titles))
                    NameLabels[Title] = titles = new List<DXLabel>();

                TitleNameLabel = titles.FirstOrDefault(x => x.ForeColour == NameColour && x.BackColour == Color.Empty);

                if (TitleNameLabel == null)
                {
                    TitleNameLabel = new DXLabel
                    {
                        BackColour = Color.Empty,
                        ForeColour = NameColour, // 使用与Name相同的颜色
                        Outline = true,
                        OutlineColour = Color.Black,
                        OutlineWeight = 1,
                        Text = Title,
                        IsControl = false,
                        IsVisible = true,
                    };

                    TitleNameLabel.Disposing += (o, e) => titles.Remove(TitleNameLabel);
                    titles.Add(TitleNameLabel);
                }
            }

            if (string.IsNullOrEmpty(Name))
            {
                FocusLabel = null;
            }
            else
            {
                if (!NameLabels.TryGetValue(Name, out List<DXLabel> focused))
                    NameLabels[Name] = focused = new List<DXLabel>();

                FocusLabel = focused.FirstOrDefault(x => x.ForeColour == NameColour && x.BackColour == LabelBackColour);

                if (FocusLabel != null) return;

                FocusLabel = new DXLabel
                {
                    BackColour = LabelBackColour,
                    ForeColour = NameColour,
                    Outline = true,
                    OutlineColour = Color.Black,
                    Text = Name,
                    Border = true,
                    BorderColour = Color.Black,
                    IsVisible = true,
                };

                FocusLabel.Disposing += (o, e) => focused.Remove(FocusLabel);
                focused.Add(FocusLabel);
            }
        }

        public void DrawFocus(int layer)
        {
            FocusLabel.Location = new Point(DrawX + (48 - FocusLabel.Size.Width) / 2, DrawY - (32 - FocusLabel.Size.Height / 2) + 8 - layer * 16);
            FocusLabel.Draw();
        }

        public override void Remove()
        {
            if (FilterSet != null)
            {
                FilterSet.ShowChanged -= OnNameShowChanged;
                FilterSet = null;
            }
            
            base.Remove();
        }
    }

}
