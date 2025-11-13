using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.UserModels;
using Library;
using Library.SystemModels;
using C = Library.Network.ClientPackets;

namespace Client.Scenes.Views
{
    public sealed class FortuneCheckerDialog : DXWindow
    {
        public DXTextBox ItemNameBox;
        public DXComboBox ItemTypeBox;
        public DXVScrollBar SearchScrollBar;
        public DXButton SearchButton;
        public DXButton ItemBrowseButton; // 检索物品按钮

        public FortuneCheckerRow[] SearchRows;

        public List<ItemInfo> SearchResults;
        
        // 物品浏览相关
        private bool _isItemBrowseMode = false;
        private List<DXImageControl> ItemImageControls; // 直接使用图像控件，不与仓库混淆
        private DXVScrollBar ItemScrollBar;
        private DXImageControl SelectedImageControl; // 当前选中的图像控件
        
        // 模拟鼠标跟随渲染
        private bool _hasMouseFollowImage = false; // 是否有跟随鼠标的图像
        private int _mouseFollowImageIndex = -1; // 跟随鼠标的图像索引
        private const int TotalItems = 6666; // 总共6666个物品
        private const int ItemsPerRow = 20; // 每行20个物品
        private const int ItemRows = 14; // 可见14行（适应36x36格子，768宽度）
        private const int ItemIconSize = 36; // 图标大小36x36
        private const int ItemSpacing = 36; // 格子间距（一格接一格）

        public override WindowType Type => WindowType.None;
        public override bool CustomSize => false;
        public override bool AutomaticVisiblity => false;


        public FortuneCheckerDialog()
        {
            //HasFooter = true;
            TitleLabel.Text = "算命人";
            SetClientSize(new Size(768, 551));
            
            #region Search

            DXControl filterPanel = new DXControl
            {
                Parent = this,
                Size = new Size(ClientArea.Width, 26),
                Location = new Point(ClientArea.Left, ClientArea.Top),
                Border = true,
                BorderColour = Color.FromArgb(198, 166, 99)
            };

            DXLabel label = new DXLabel
            {
                Parent = filterPanel,
                Location = new Point(5, 5),
                Text = "名称:",
            };

            ItemNameBox = new DXTextBox
            {
                Parent = filterPanel,
                Size = new Size(180, 20),
                Location = new Point(label.Location.X + label.Size.Width + 5, label.Location.Y),
            };
            ItemNameBox.TextBox.KeyPress += TextBox_KeyPress;



            label = new DXLabel
            {
                Parent = filterPanel,
                Location = new Point(ItemNameBox.Location.X + ItemNameBox.Size.Width + 10, 5),
                Text = "物品:",
            };



            ItemTypeBox = new DXComboBox
            {
                Parent = filterPanel,
                Location = new Point(label.Location.X + label.Size.Width + 5, label.Location.Y),
                Size = new Size(95, DXComboBox.DefaultNormalHeight),
                DropDownHeight = 198
            };


            new DXListBoxItem
            {
                Parent = ItemTypeBox.ListBox,
                Label = { Text = $"全部" },
                Item = null
            };

            Type itemType = typeof(ItemType);

            for (ItemType i = ItemType.Nothing; i <= ItemType.ItemPart; i++)
            {
                MemberInfo[] infos = itemType.GetMember(i.ToString());

                DescriptionAttribute description = infos[0].GetCustomAttribute<DescriptionAttribute>();

                new DXListBoxItem
                {
                    Parent = ItemTypeBox.ListBox,
                    Label = { Text = description?.Description ?? i.ToString() },
                    Item = i
                };
            }

            ItemTypeBox.ListBox.SelectItem(null);
            
            SearchButton = new DXButton
            {
                Size = new Size(80, SmallButtonHeight),
                Location = new Point(ItemTypeBox.Location.X + ItemTypeBox.Size.Width + 15, label.Location.Y - 1),
                Parent = filterPanel,
                ButtonType = ButtonType.SmallButton,
                Label = { Text = "搜索" }
            };
            SearchButton.MouseClick += (o, e) => Search();

            // 检索物品按钮将在GM状态检查后动态添加
            // ItemBrowseButton 初始为null，稍后在Search()中检查GM状态后创建
        
            SearchRows = new FortuneCheckerRow[9];

            SearchScrollBar = new DXVScrollBar
            {
                Parent = this,
                Location = new Point(ClientArea.Size.Width - 14 + ClientArea.Left, ClientArea.Y + filterPanel.Size.Height + 5),
                Size = new Size(14, ClientArea.Height - 5 - filterPanel.Size.Height),
                VisibleSize = SearchRows.Length,
                Change = 3,
            };
            SearchScrollBar.ValueChanged += SearchScrollBar_ValueChanged;


            for (int i = 0; i < SearchRows.Length; i++)
            {
                int index = i;
                SearchRows[index] = new FortuneCheckerRow
                {
                    Parent = this,
                    Location = new Point(ClientArea.X, ClientArea.Y + filterPanel.Size.Height + 5 + i * 58),
                };
                SearchRows[index].SetupLayout(); // 调用自适应布局
             //   SearchRows[index].MouseClick += (o, e) => { SelectedRow = SearchRows[index]; };
                SearchRows[index].MouseWheel += SearchScrollBar.DoMouseWheel;
            }

            #endregion

            InitializeItemBrowse();
        }

        private void CreateItemBrowseButton()
        {
            if (ItemBrowseButton != null) return; // 防止重复创建

            // 找到父级面板（filterPanel）
            DXControl filterPanel = SearchButton.Parent;
            
            ItemBrowseButton = new DXButton
            {
                Size = new Size(80, SmallButtonHeight),
                Location = new Point(SearchButton.Location.X + SearchButton.Size.Width + 10, SearchButton.Location.Y),
                Parent = filterPanel,
                ButtonType = ButtonType.SmallButton,
                Label = { Text = "检索物品" },
                Visible = true
            };
            ItemBrowseButton.MouseClick += (o, e) => ToggleItemBrowseMode();
        }

        private bool ShouldExcludeItem(ItemInfo info)
        {
            if (info == null) return true;
            
            // 筛选掉无意义的物品
            if (string.IsNullOrEmpty(info.ItemName)) return true;
            
            // 筛选掉部分、金币、声望点、贡献点等
            string itemName = info.ItemName.Trim();
            if (itemName == "部分" || itemName == "[部分]" || 
                itemName == "Gold" || itemName == "gold" ||
                itemName == "Fame Point" || itemName == "声望点" ||
                itemName == "Contribution Point" || itemName == "贡献点" ||
                string.IsNullOrWhiteSpace(itemName))
            {
                return true;
            }
            
            return false;
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter) return;

            e.Handled = true;

            if (SearchButton.Enabled)
                Search();
        }
        public void Search()
        {
            SearchResults = new List<ItemInfo>();
            SearchScrollBar.MaxValue = 0;

            // 如果正在物品检索模式，先切换回搜索模式
            if (_isItemBrowseMode)
            {
                ShowSearchMode();
            }

            // 显示搜索相关控件
            foreach (var row in SearchRows)
                row.Visible = true;

            ItemType filter = (ItemType?) ItemTypeBox.SelectedItem ?? 0;
            bool useFilter = ItemTypeBox.SelectedItem != null;

            bool isDeveloper = GameScene.Game?.User?.Buffs != null && GameScene.Game.User.Buffs.Exists(x => x.Type == BuffType.Developer);

            // GM状态下创建检索物品按钮
            if (isDeveloper && ItemBrowseButton == null)
            {
                CreateItemBrowseButton();
            }

            foreach (ItemInfo info in Globals.ItemInfoList.Binding)
            {
                // 筛选掉无意义的物品类型
                if (ShouldExcludeItem(info)) continue;

                // If not a developer, skip items with no drops.
                if (!isDeveloper && (info.Drops == null || info.Drops.Count == 0)) continue;

                if (useFilter && info.ItemType != filter) continue;

                if (!string.IsNullOrEmpty(ItemNameBox.TextBox.Text) && info.ItemName.IndexOf(ItemNameBox.TextBox.Text, StringComparison.OrdinalIgnoreCase) < 0) continue;

                SearchResults.Add(info);
            }
            
            RefreshList();
        }
        public void RefreshList()
        {
            if (SearchResults == null) return;

            SearchScrollBar.MaxValue = SearchResults.Count;

            for (int i = 0; i < SearchRows.Length; i++)
            {
                if (i + SearchScrollBar.Value >= SearchResults.Count)
                {
                    SearchRows[i].ItemInfo = null;
                    SearchRows[i].Visible = false;
                    continue;
                }

                SearchRows[i].ItemInfo = SearchResults[i + SearchScrollBar.Value];
            }

        }
        private void SearchScrollBar_ValueChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void InitializeItemBrowse()
        {
            // 创建图像控件列表
            ItemImageControls = new List<DXImageControl>();
            
            // 创建滚动条
            ItemScrollBar = new DXVScrollBar
            {
                Parent = this,
                Location = new Point(ClientArea.Right - 14, ClientArea.Y + 31),
                Size = new Size(14, ClientArea.Height - 31 ),
                VisibleSize = ItemRows,
                Change = 3,
                MaxValue = 0,
                Visible = false
            };
            ItemScrollBar.ValueChanged += ItemScrollBar_ValueChanged;
            
            // 创建图像控件网格
            for (int i = 0; i < ItemsPerRow * ItemRows; i++)
            {
                int row = i / ItemsPerRow;
                int col = i % ItemsPerRow;
                
                var imageControl = new DXImageControl
                {
                    Parent = this,
                    Size = new Size(ItemIconSize, ItemIconSize),
                    Location = new Point(ClientArea.X + col * ItemSpacing, ClientArea.Y + 31 + row * ItemSpacing),
                    LibraryFile = LibraryFile.StoreItems,
                    Index = 0,
                    Border = true,
                    BorderColour = Color.FromArgb(99, 83, 50),
                    Visible = false
                };
                
                // 添加鼠标事件
                imageControl.MouseWheel += ItemScrollBar.DoMouseWheel;
                imageControl.MouseEnter += ImageControl_MouseEnter;
                imageControl.MouseClick += ImageControl_MouseClick;
                
                ItemImageControls.Add(imageControl);
            }
                
            RefreshItemGrid();
        }

        private void ImageControl_MouseEnter(object sender, EventArgs e)
        {
            var control = (DXImageControl)sender;
            if (!_isItemBrowseMode) return;
            
            // 显示图标信息和使用该图标的物品列表
            var itemsUsingThisIcon = GetItemsUsingIcon(control.Index);
            
            if (itemsUsingThisIcon.Count > 0)
            {
                var displayItems = itemsUsingThisIcon.Count > 10 ? itemsUsingThisIcon.GetRange(0, 10) : itemsUsingThisIcon;
                var itemNames = new string[displayItems.Count];
                for (int i = 0; i < displayItems.Count; i++)
                {
                    itemNames[i] = $"• {displayItems[i].ItemName}";
                }
                control.Hint = $"图标ID: {control.Index}\n" +
                              $"使用此图标的物品 ({itemsUsingThisIcon.Count}个):\n" + 
                              string.Join("\n", itemNames) +
                              (itemsUsingThisIcon.Count > 10 ? $"\n... 以及{itemsUsingThisIcon.Count - 10}个物品" : "");
            }
            else
            {
                control.Hint = $"图标ID: {control.Index}\n暂无物品使用此图标";
            }
        }

        private void ImageControl_MouseClick(object sender, MouseEventArgs e)
        {
            var control = (DXImageControl)sender;
            if (!_isItemBrowseMode) return;
            
            // 如果已经有跟随鼠标的图像，清除
            if (_hasMouseFollowImage)
            {
                _hasMouseFollowImage = false;
                _mouseFollowImageIndex = -1;
                
                // 重置之前选中的控件外观
                if (SelectedImageControl != null)
                {
                    SelectedImageControl.BorderColour = Color.FromArgb(99, 83, 50);
                    SelectedImageControl = null;
                }
                return;
            }
            
            // 开始跟随鼠标显示图像
            _hasMouseFollowImage = true;
            _mouseFollowImageIndex = control.Index;
            SelectedImageControl = control;
            
            // 高亮选中的控件
            control.BorderColour = Color.Yellow;
        }

        private List<ItemInfo> GetItemsUsingIcon(int iconIndex)
        {
            var result = new List<ItemInfo>();
            
            if (Globals.ItemInfoList?.Binding != null)
            {
                foreach (var item in Globals.ItemInfoList.Binding)
                {
                    if (item != null && item.Image == iconIndex)
                    {
                        result.Add(item);
                    }
                }
            }
            
            return result;
        }

        private void ShowItemBrowseMode()
        {
            if (ItemBrowseButton == null) return; // GM权限检查
            
            _isItemBrowseMode = true;
            
            // 隐藏搜索相关控件
            foreach (var row in SearchRows)
                row.Visible = false;
            SearchScrollBar.Visible = false;
            
            // 显示物品图像控件
            foreach (var control in ItemImageControls)
                control.Visible = true;
            ItemScrollBar.Visible = true;
            ItemScrollBar.Value = 0;
            
            // 刷新物品网格
            RefreshItemGrid();
        }

        private void ShowSearchMode()
        {
            _isItemBrowseMode = false;
            
            // 重置选中状态
            if (SelectedImageControl != null)
            {
                SelectedImageControl.BorderColour = Color.FromArgb(99, 83, 50);
                SelectedImageControl = null;
            }
            
            // 清除跟随鼠标的图像
            _hasMouseFollowImage = false;
            _mouseFollowImageIndex = -1;
            
            // 隐藏物品图像控件
            foreach (var control in ItemImageControls)
                control.Visible = false;
            ItemScrollBar.Visible = false;
            
            // 显示搜索相关控件
            foreach (var row in SearchRows)
                row.Visible = true;
            SearchScrollBar.Visible = true;
        }

        private void ToggleItemBrowseMode()
        {
            if (ItemBrowseButton == null) return; // GM权限检查
            
            if (_isItemBrowseMode)
            {
                ShowSearchMode();
            }
            else
            {
                ShowItemBrowseMode();
            }
        }

        private void ItemScrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (_isItemBrowseMode)
            {
                RefreshItemGrid();
            }
        }

        private void RefreshItemGrid()
        {
            if (!_isItemBrowseMode) return;

            // 计算起始索引
            int startIndex = ItemScrollBar.Value * ItemsPerRow;

            // 计算总行数
            int totalRows = (TotalItems + ItemsPerRow - 1) / ItemsPerRow;
            ItemScrollBar.MaxValue = Math.Max(0, totalRows - ItemRows);

            // 更新每个图像控件
            for (int i = 0; i < ItemImageControls.Count; i++)
            {
                int itemIndex = startIndex + i;
                var control = ItemImageControls[i];

                if (itemIndex >= TotalItems)
                {
                    // 超出范围，隐藏控件
                    control.Visible = false;
                }
                else
                {
                    // 设置图标索引并显示（从0开始）
                    control.Index = itemIndex;
                    control.Visible = _isItemBrowseMode;

                    // 重置大小和边框颜色（除非是选中状态）
                    if (control != SelectedImageControl)
                    {
                        control.Size = new Size(ItemIconSize, ItemIconSize);
                        control.BorderColour = Color.FromArgb(99, 83, 50);
                    }
                }
            }
        }
        
        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);
            
            // 窗口大小改变时，重新调整所有搜索结果行的布局
            if (SearchRows != null)
            {
                foreach (var row in SearchRows)
                {
                    if (row != null && !row.IsDisposed)
                    {
                        row.SetupLayout();
                    }
                }
            }
        }

        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();

            // 模拟GameScene的鼠标跟随图像渲染
            if (_hasMouseFollowImage && _mouseFollowImageIndex >= 0)
            {
                // 获取两个库的引用
                MirLibrary inventoryLibrary;
                MirLibrary storeLibrary;
                bool hasInventoryImage = CEnvir.LibraryList.TryGetValue(LibraryFile.Inventory, out inventoryLibrary);
                bool hasStoreImage = CEnvir.LibraryList.TryGetValue(LibraryFile.StoreItems, out storeLibrary);
                
                // 比较两个库中图像的尺寸，判断是否真正有大图
                MirLibrary library = null;
                bool isActualLargeImage = false;
                
                if (hasInventoryImage && hasStoreImage)
                {
                    Size inventorySize = inventoryLibrary.GetSize(_mouseFollowImageIndex);
                    Size storeSize = storeLibrary.GetSize(_mouseFollowImageIndex);
                    
                    // 如果Inventory库有图像且尺寸比StoreItems更大，则认为是大图
                    if (inventorySize.Width > 0 && (inventorySize.Width > storeSize.Width || inventorySize.Height > storeSize.Height))
                    {
                        library = inventoryLibrary;
                        isActualLargeImage = true;
                    }
                    else if (storeSize.Width > 0)
                    {
                        library = storeLibrary;
                        isActualLargeImage = false;
                    }
                }
                else if (hasInventoryImage && inventoryLibrary.GetSize(_mouseFollowImageIndex).Width > 0)
                {
                    library = inventoryLibrary;
                    isActualLargeImage = false; // 无法比较，保守处理
                }
                else if (hasStoreImage)
                {
                    library = storeLibrary;
                    isActualLargeImage = false;
                }
                
                if (library != null)
                {
                    Size imageSize = library.GetSize(_mouseFollowImageIndex);
                    Point p = new Point(CEnvir.MouseLocation.X - imageSize.Width / 2, CEnvir.MouseLocation.Y - imageSize.Height / 2);

                    // 边界检查，防止图像超出屏幕
                    if (p.X + imageSize.Width >= Size.Width + Location.X)
                        p.X = Size.Width - imageSize.Width + Location.X;

                    if (p.Y + imageSize.Height >= Size.Height + Location.Y)
                        p.Y = Size.Height - imageSize.Height + Location.Y;

                    if (p.X < Location.X)
                        p.X = Location.X;

                    if (p.Y <= Location.Y)
                        p.Y = Location.Y;

                    // 只有真正的大图才使用特殊效果
                    float opacity = isActualLargeImage ? 0.9f : 1.0f;
                    Color drawColor = isActualLargeImage ? Color.FromArgb(230, 255, 255, 255) : Color.White;

                    // 绘制跟随鼠标的图像
                    library.Draw(_mouseFollowImageIndex, p.X, p.Y, drawColor, false, opacity, ImageType.Image);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // ItemBrowseButton可能为null（非GM玩家）
                if (ItemBrowseButton != null)
                {
                    if (!ItemBrowseButton.IsDisposed)
                        ItemBrowseButton.Dispose();
                    ItemBrowseButton = null;
                }

                if (ItemScrollBar != null)
                {
                    if (!ItemScrollBar.IsDisposed)
                        ItemScrollBar.Dispose();
                    ItemScrollBar = null;
                }

                if (ItemImageControls != null)
                {
                    foreach (var control in ItemImageControls)
                    {
                        if (control != null && !control.IsDisposed)
                            control.Dispose();
                    }
                    ItemImageControls.Clear();
                    ItemImageControls = null;
                }

                SelectedImageControl = null;
            }
        }
    }

    public sealed class FortuneCheckerRow : DXControl
    {

        #region Properties

        #region Selected

        public bool Selected
        {
            get => _Selected;
            set
            {
                if (_Selected == value) return;

                bool oldValue = _Selected;
                _Selected = value;

                OnSelectedChanged(oldValue, value);
            }
        }
        private bool _Selected;
        public event EventHandler<EventArgs> SelectedChanged;
        public void OnSelectedChanged(bool oValue, bool nValue)
        {
            BackColour = Selected ? Color.FromArgb(80, 80, 125) : Color.FromArgb(25, 20, 0);
            ItemCell.BorderColour = Selected ? Color.FromArgb(198, 166, 99) : Color.FromArgb(99, 83, 50);

            SelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region ItemInfo

        public ItemInfo ItemInfo
        {
            get { return _ItemInfo; }
            set
            {

                ItemInfo oldValue = _ItemInfo;
                _ItemInfo = value;

                OnItemInfoChanged(oldValue, value);
            }
        }
        private ItemInfo _ItemInfo;
        public event EventHandler<EventArgs> ItemInfoChanged;
        public void OnItemInfoChanged(ItemInfo oValue, ItemInfo nValue)
        {
            ItemInfoChanged?.Invoke(this, EventArgs.Empty);
            Visible = ItemInfo != null;
            Fortune = null;

            if (ItemInfo == null)
            {
                return;
            }

            ItemCell.Item = new ClientUserItem(ItemInfo, 1);
            ItemCell.RefreshItem();

            NameLabel.Text = ItemInfo.ItemName;

            NameLabel.ForeColour = Color.FromArgb(198, 166, 99);

            GameScene.Game.FortuneDictionary.TryGetValue(ItemInfo, out Fortune);
            
            UpdateInfo();

            ItemInfoChanged?.Invoke(this, EventArgs.Empty);
        }
        private void UpdateInfo()
        {
            if (Fortune == null)
            {
                CountLabel.Text = "未算命";
                ProgressLabel.Text = "未算命";
                DateLabel.Text = "未算命";
                return;
            }
            
            CountLabel.Text = Fortune.DropCount.ToString("#,##0");

            string format = "#,##0";

            if (Fortune.Progress < 10000)
                format += ".#####%";
            else
                format += ".##%";
            
            ProgressLabel.Text = (1 + Fortune.DropCount - Fortune.Progress ).ToString(format);
            DateLabel.Text = Functions.ToString(CEnvir.Now - Fortune.CheckDate, true, true);
        }

        #endregion

        private ClientFortuneInfo Fortune;
        
        public DXItemCell ItemCell;
        public DXLabel NameLabel, MonsterLabel, CountLabelLabel, CountLabel, ProgressLabelLabel, ProgressLabel, DateLabel, TogoLabel, DateLabelLabel;
        public DXButton CheckButton;
    // No DropGrid - use wrapped MonsterLabel for drop info
        #endregion



        public FortuneCheckerRow()
        {
            Size = new Size(750, 55); // 初始化为合理默认值，会在SetupLayout()时调整

            DrawTexture = true;
            BackColour = Selected ? Color.FromArgb(80, 80, 125) : Color.FromArgb(25, 20, 0);

            Visible = false;

            ItemCell = new DXItemCell
            {
                Parent = this,
                Location = new Point((Size.Height - DXItemCell.CellHeight) / 2, (Size.Height - DXItemCell.CellHeight) / 2),
                FixedBorder = true,
                Border = true,
                ReadOnly = true,
                ItemGrid = new ClientUserItem[1],
                Slot = 0,
                FixedBorderColour = true,
            };

            NameLabel = new DXLabel
            {
                Parent = this,
                Location = new Point(ItemCell.Location.X + ItemCell.Size.Width, 22),
                IsControl = false,
                DrawFormat = System.Windows.Forms.TextFormatFlags.SingleLine | System.Windows.Forms.TextFormatFlags.EndEllipsis,
            };

            MonsterLabel = new DXLabel
            {
                Parent = this,
                Location = new Point(ItemCell.Location.X + ItemCell.Size.Width, 4),
                IsControl = false,
                ForeColour = Color.Wheat,
                Visible = false,
            };

            ItemCell.MouseEnter += (o, e) =>
            {
                try
                {
                    if (ItemInfo != null)
                    {
                        // Hide item name to avoid layout conflicts and show monster list
                        NameLabel.Visible = false;
                        PopulateDropMonsters();
                        MonsterLabel.Visible = true;
                        // Hide right-side info while showing the monster list
                        CountLabelLabel.Visible = false;
                        CountLabel.Visible = false;
                        ProgressLabelLabel.Visible = false;
                        ProgressLabel.Visible = false;
                        DateLabelLabel.Visible = false;
                        DateLabel.Visible = false;
                        // Hide the fortune button so MonsterLabel can expand
                        if (CheckButton != null) CheckButton.Visible = false;
                        MonsterLabel.Size = new Size(MonsterLabel.Size.Width + 50, MonsterLabel.Size.Height);
                    }
                }
                catch (Exception)
                {
                    // Swallow any errors to avoid crashing the client from hover.
                    MonsterLabel.Visible = false;
                }
            };

            ItemCell.MouseLeave += (o, e) =>
            {
                MonsterLabel.Visible = false;
                NameLabel.Visible = true;
                // Restore right-side info when not hovering
                CountLabelLabel.Visible = true;
                CountLabel.Visible = true;
                ProgressLabelLabel.Visible = true;
                ProgressLabel.Visible = true;
                DateLabelLabel.Visible = true;
                DateLabel.Visible = true;
                // Restore the fortune button
                if (CheckButton != null) CheckButton.Visible = true;
                MonsterLabel.Size = new Size(MonsterLabel.Size.Width - 50, MonsterLabel.Size.Height);
            };

            CountLabelLabel = new DXLabel
            {
                Parent = this,
                Text = "掉落数量:",
                ForeColour = Color.White,
                IsControl = false,

            };
            
            CountLabel = new DXLabel
            {
                Parent = this,
                IsControl = false,
            };

            // Limit monster label width so it will wrap inside bounds instead of overflowing
            MonsterLabel.AutoSize = false;
            MonsterLabel.Size = new Size(550, 40);
            MonsterLabel.DrawFormat = System.Windows.Forms.TextFormatFlags.WordBreak;

            ProgressLabelLabel = new DXLabel
            {
                Parent = this,
                Text = "幸运降临:",
                ForeColour = Color.White,
                IsControl = false,

            };

            ProgressLabel = new DXLabel
            {
                    Parent = this,
                IsControl = false,
            };

            DateLabelLabel = new DXLabel
            {
                Parent = this,
                Text = "最后算命:",
                ForeColour = Color.White,
                IsControl = false,

            };

            DateLabel = new DXLabel
            {
                    Parent = this,
                IsControl = false,
            };

            CheckButton = new DXButton
            {
                Parent = this,
                Label = { Text = "算命" },
                ButtonType = ButtonType.SmallButton,
                Size = new Size(50, SmallButtonHeight),
            };

            CheckButton.MouseClick += CheckButton_MouseClick;
            
            // Layout will be finalized in SetupLayout() after Parent is set
        }
        
        /// <summary>
        /// 自适应布局 - 在Parent设置后调用以调整所有位置和大小
        /// </summary>
        public void SetupLayout()
        {
            // 获取行宽度（Parent的ClientArea宽度减去滚动条宽度）
            DXWindow parent = Parent as DXWindow;
            if (parent == null) return;
            
            int rowWidth = parent.ClientArea.Width - 16; // 减去滚动条宽度
            Size = new Size(rowWidth, 55);
            
            // 计算右侧信息区域的baseX位置
            // 右侧布局（从右向左）：
            // - CheckButton: rowWidth - 55 (50px宽 + 5px边距)
            // - 信息标签们: 从 rowWidth - 60 往左排列
            
            // 右侧三行信息（掉落数量、幸运降临、最后算命）应该从右向左排列
            // 每行约 100px 宽（标签名+标签值）
            int baseX = rowWidth - 120; // 留空间给右侧标签组
            
            CountLabelLabel.Location = new Point(baseX - 100 - CountLabelLabel.Size.Width, 5);
            CountLabel.Location = new Point(baseX - 100, 5);

            ProgressLabelLabel.Location = new Point(baseX - 100 - ProgressLabelLabel.Size.Width, 20);
            ProgressLabel.Location = new Point(baseX - 100, 20);

            DateLabelLabel.Location = new Point(baseX - 100 - DateLabelLabel.Size.Width, 35);
            DateLabel.Location = new Point(baseX - 100, 35);

            CheckButton.Location = new Point(rowWidth - 55, 34);
        }

        private void CheckButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (GameScene.Game.Observer) return;

            DXMessageBox box = new DXMessageBox("确定要推演你的命运吗?", "算命", DXMessageBoxButtons.YesNo);

            box.YesButton.MouseClick += (o1, e1) =>
            {
                CEnvir.Enqueue(new C.FortuneCheck { ItemIndex = ItemInfo.Index });
            };
        }

        public override void Process()
        {
            base.Process();

            if (Fortune == null)
            {
                DateLabel.Text = "未算命";
                return;
            }

            DateLabel.Text = Functions.ToString(CEnvir.Now - Fortune.CheckDate, true, true);
        }

        private void PopulateDropMonsters()
        {
            try
            {
                if (ItemInfo == null)
                {
                    MonsterLabel.Text = string.Empty;
                    return;
                }

                if (ItemInfo.Drops == null || ItemInfo.Drops.Count == 0)
                {
                    MonsterLabel.Text = "掉落: 无";
                    return;
                }

                var names = new HashSet<string>();
                foreach (var drop in ItemInfo.Drops)
                {
                    if (drop?.Monster?.MonsterName == null) continue;
                    names.Add(drop.Monster.MonsterName);
                }

                MonsterLabel.Text = names.Count > 0 ? "掉落: " + string.Join(" ", names) : "掉落: 无";
            }
            catch (Exception)
            {
                MonsterLabel.Text = "掉落: 错误";
            }
        }

        // Removed DropGrid and PopulateDropGrid to keep hover handling simple

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _Selected = false;
                SelectedChanged = null;

                _ItemInfo = null;
                ItemInfoChanged = null;

                if (ItemCell != null)
                {
                    if (!ItemCell.IsDisposed)
                        ItemCell.Dispose();

                    ItemCell = null;
                }

                if (NameLabel != null)
                {
                    if (!NameLabel.IsDisposed)
                        NameLabel.Dispose();

                    NameLabel = null;
                }

                if (MonsterLabel != null)
                {
                    if (!MonsterLabel.IsDisposed)
                        MonsterLabel.Dispose();

                    MonsterLabel = null;
                }

                if (CountLabelLabel != null)
                {
                    if (!CountLabelLabel.IsDisposed)
                        CountLabelLabel.Dispose();

                    CountLabelLabel = null;
                }

                if (CountLabel != null)
                {
                    if (!CountLabel.IsDisposed)
                        CountLabel.Dispose();

                    CountLabel = null;
                }

                if (ProgressLabelLabel != null)
                {
                    if (!ProgressLabelLabel.IsDisposed)
                        ProgressLabelLabel.Dispose();

                    ProgressLabelLabel = null;
                }

                if (ProgressLabel != null)
                {
                    if (!ProgressLabel.IsDisposed)
                        ProgressLabel.Dispose();

                    ProgressLabel = null;
                }

                if (DateLabelLabel != null)
                {
                    if (!DateLabelLabel.IsDisposed)
                        DateLabelLabel.Dispose();

                    DateLabelLabel = null;
                }

                if (DateLabel != null)
                {
                    if (!DateLabel.IsDisposed)
                        DateLabel.Dispose();

                    DateLabel = null;
                }

                if (CheckButton != null)
                {
                    if (!CheckButton.IsDisposed)
                        CheckButton.Dispose();

                    CheckButton = null;
                }
            }

        }

        #endregion
    }
}
