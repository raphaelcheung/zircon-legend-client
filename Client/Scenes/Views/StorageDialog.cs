using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using Client.Controls;
using Client.Envir;
using Client.UserModels;
using Library;
using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Scenes.Views
{
    public sealed class StorageDialog : DXWindow
    {
        #region Properties
        public DXItemGrid Grid { get; set; }
        
        public override void OnIsVisibleChanged(bool oValue, bool nValue)
        {
            base.OnIsVisibleChanged(oValue, nValue);

            if (GameScene.Game.InventoryBox == null) return;

            if (IsVisible)
                GameScene.Game.InventoryBox.Visible = true;

            if (!IsVisible)
                Grid.ClearLinks();
        }

        public override WindowType Type => WindowType.StorageBox;
        public override bool CustomSize => false;
        public override bool AutomaticVisiblity => true;

        #endregion
        
        public DXTextBox ItemNameTextBox;
        public DXComboBox ItemTypeComboBox;
        public DXButton ClearButton;
        private DXButton RefreshButton;
        public DXVScrollBar StorageScrollBar;
        public ClientUserItem[] GuildStorage = new ClientUserItem[1000];
        private DateTime RefreshTime = DateTime.MinValue;

        public StorageDialog()
        {
            TitleLabel.Text = "仓库";

            SetClientSize(new Size(473, 380));

            DXControl filterPanel = new DXControl
            {
                Parent = this,
                Size = new Size(ClientArea.Width, 26),
                Location = new Point(ClientArea.Location.X, ClientArea.Location.Y),
                Border = true,
                BorderColour = Color.FromArgb(198, 166, 99)
            };

            DXLabel label = new DXLabel
            {
                Parent = filterPanel,
                Location = new Point(5, 5),
                Text = "名称:",
            };

            ItemNameTextBox = new DXTextBox
            {
                Parent = filterPanel,
                Size = new Size(120, 20),
                Location = new Point(label.Location.X + label.Size.Width + 5, label.Location.Y),
            };
            ItemNameTextBox.TextBox.TextChanged += (o, e) => ApplyStorageFilter();

            label = new DXLabel
            {
                Parent = filterPanel,
                Location = new Point(ItemNameTextBox.Location.X + ItemNameTextBox.Size.Width + 10, 5),
                Text = "物品:",
            };



            ItemTypeComboBox = new DXComboBox
            {
                Parent = filterPanel,
                Location = new Point(label.Location.X + label.Size.Width + 5, label.Location.Y),
                Size = new Size(95, DXComboBox.DefaultNormalHeight),
                DropDownHeight = 198
            };
            ItemTypeComboBox.SelectedItemChanged += (o, e) => ApplyStorageFilter();

            new DXListBoxItem
            {
                Parent = ItemTypeComboBox.ListBox,
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
                    Parent = ItemTypeComboBox.ListBox,
                    Label = { Text = description?.Description ?? i.ToString() },
                    Item = i
                };
            }

            ItemTypeComboBox.ListBox.SelectItem(null);


            ClearButton = new DXButton
            {
                Size = new Size(60, SmallButtonHeight),
                Location = new Point(ItemTypeComboBox.Location.X + ItemTypeComboBox.Size.Width + 17, label.Location.Y - 1),
                Parent = filterPanel,
                ButtonType = ButtonType.SmallButton,
                Label = { Text = "清除" }
            };
            ClearButton.MouseClick += (o, e) =>
            {
                ItemTypeComboBox.ListBox.SelectItem(null);
                ItemNameTextBox.TextBox.Text = string.Empty;
            };


            RefreshButton = new DXButton
            {
                Size = new Size(60, SmallButtonHeight),
                Location = new Point(ClearButton.Location.X + ClearButton.Size.Width + 17, label.Location.Y - 1),
                Parent = filterPanel,
                ButtonType = ButtonType.SmallButton,
                Label = { Text = "整理" }
            };
            RefreshButton.MouseClick += (o, e) =>
            {
                RefreshButton.Enabled = false;
                RefreshTime = CEnvir.Now.AddSeconds(5);
                CEnvir.Connection.Enqueue(new C.SortStorageItem() { });
            };


            Grid = new DXItemGrid
            {
                Parent = this,
                GridSize = new Size(1, 1),
                Location = new Point(ClientArea.Location.X, ClientArea.Location.Y + 30),
                GridType = GridType.Storage,
                ItemGrid = CEnvir.Storage,
                VisibleHeight = 10,
            };

            Grid.GridSizeChanged += StorageGrid_GridSizeChanged;
            


            StorageScrollBar = new DXVScrollBar
            {
                Parent = this,
                Location = new Point(ClientArea.Right - 14, ClientArea.Location.Y + 31),
                Size = new Size(14, 349),
                VisibleSize = 10,
                Change = 1,
            };
            StorageScrollBar.ValueChanged += StorageScrollBar_ValueChanged;


            foreach (DXItemCell cell in Grid.Grid)
                cell.MouseWheel += StorageScrollBar.DoMouseWheel;


        }

        public void Proccess()
        {
            if (!RefreshButton.Enabled && CEnvir.Now > RefreshTime)
            {
                RefreshButton.Enabled = true;
            }
            
        }

        public void RefreshStorage()
        {
            Grid.GridSize = new Size(13, Math.Max(10, (int)Math.Ceiling(GameScene.Game.StorageSize / (float)13)));

            StorageScrollBar.MaxValue = Grid.GridSize.Height;

            ApplyStorageFilter();
            
        }
        private void StorageGrid_GridSizeChanged(object sender, EventArgs e)
        {
            foreach (DXItemCell cell in Grid.Grid)
                cell.ItemChanged += (o, e1) => FilterCell(cell);


            foreach (DXItemCell cell in Grid.Grid)
                cell.MouseWheel += StorageScrollBar.DoMouseWheel;
        }

        public void ApplyStorageFilter()
        {
            foreach (DXItemCell cell in Grid.Grid)
                FilterCell(cell);
        }
        public void FilterCell(DXItemCell cell)
        {
            if (cell.Slot >= GameScene.Game.StorageSize)
            {
                cell.Enabled = false;
                return;
            }

            if (cell.Item == null && (ItemTypeComboBox.SelectedItem != null || !string.IsNullOrEmpty(ItemNameTextBox.TextBox.Text)))
            {
                cell.Enabled = false;
                return;
            }


            if (ItemTypeComboBox.SelectedItem != null && cell.Item != null && cell.Item.Info.ItemType != (ItemType)ItemTypeComboBox.SelectedItem)
            {
                cell.Enabled = false;
                return;
            }


            if (!string.IsNullOrEmpty(ItemNameTextBox.TextBox.Text) && cell.Item != null && cell.Item.Info.ItemName.IndexOf(ItemNameTextBox.TextBox.Text, StringComparison.OrdinalIgnoreCase) < 0)
            {
                cell.Enabled = false;
                return;
            }


            cell.Enabled = true;
        }


        private void StorageScrollBar_ValueChanged(object sender, EventArgs e)
        {
            Grid.ScrollValue = StorageScrollBar.Value;
        }

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
            }

        }

        #endregion
    }
}
