﻿using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using Client.Controls;
using Client.Envir;
using Client.UserModels;
using Library;
using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Scenes.Views
{
    public sealed class BlockDialog : DXWindow
    {
        #region Propetries
        private DXListBox ListBox;
        public List<DXListBoxItem> ListBoxItems = new List<DXListBoxItem>();

        public override WindowType Type => WindowType.BlockBox;
        public override bool CustomSize => false;
        public override bool AutomaticVisiblity => true;

        #endregion

        public BlockDialog()
        {
            TitleLabel.Text = "黑名单";

            HasFooter = false;

            SetClientSize(new Size(200, 200));

            ListBox = new DXListBox
            {
                Parent = this,
                Location = ClientArea.Location,
                Size = new Size(ClientArea.Width, ClientArea.Height - 25)
            };

            DXButton addButton = new DXButton
            {
                Label = { Text = "增加" },
                Parent = this,
                Location = new Point(ClientArea.X, ClientArea.Bottom - 20),
                Size = new Size(80, SmallButtonHeight),
                ButtonType = ButtonType.SmallButton,
            };
            addButton.MouseClick += (o, e) =>
            {
                DXInputWindow window = new DXInputWindow("请输入你想加入黑名单者的名称.", "黑名单")
                {
                    ConfirmButton = { Enabled = false },
                    Modal = true
                };
                window.ValueTextBox.TextBox.TextChanged += (o1, e1) =>
                {
                    window.ConfirmButton.Enabled = Regex.IsMatch(window.ValueTextBox.TextBox.Text, Globals.CharacterReg, RegexOptions.IgnoreCase);
                };
                window.ConfirmButton.MouseClick += (o1, e1) =>
                {
                    CEnvir.Enqueue(new C.BlockAdd { Name = window.Value });
                };
            };

            DXButton removeButton = new DXButton
            {
                Label = { Text = "移除" },
                Parent = this,
                Location = new Point(ClientArea.Right - 80, ClientArea.Bottom - 20),
                Size = new Size(80, SmallButtonHeight),
                ButtonType = ButtonType.SmallButton,
                Enabled =  false,
            };
            removeButton.MouseClick += (o, e) =>
            {
                if (ListBox.SelectedItem == null) return;

                DXMessageBox box = new DXMessageBox($"确定要将 {ListBox.SelectedItem.Label.Text} 从黑名单移除?", "黑名单", DXMessageBoxButtons.YesNo);

                box.YesButton.MouseClick += (o1, e1) =>
                {
                    CEnvir.Enqueue(new C.BlockRemove { Index = (int)ListBox.SelectedItem.Item });
                };

            };

            ListBox.selectedItemChanged += (o, e) =>
            {
                removeButton.Enabled = ListBox.SelectedItem != null;
            };

            RefreshList();
        }
        
        #region Methods
        public void RefreshList()
        {
            ListBox.SelectedItem = null;

            foreach (DXListBoxItem item in ListBoxItems)
                item.Dispose();

            ListBoxItems.Clear();

            foreach (ClientBlockInfo info in CEnvir.BlockList)
            {
                ListBoxItems.Add(new DXListBoxItem
                {
                    Parent = ListBox,
                    Label = { Text = info.Name },
                    Item = info.Index
                });
            }
        }
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (ListBoxItems != null)
                {
                    for (int i = 0; i < ListBoxItems.Count; i++)
                    {
                        if (ListBoxItems[i] != null)
                        {
                            if (!ListBoxItems[i].IsDisposed)
                                ListBoxItems[i].Dispose();

                            ListBoxItems[i] = null;
                        }
                    }

                    ListBoxItems.Clear();
                    ListBoxItems = null;
                }

                if (ListBox != null)
                {
                    if (!ListBox.IsDisposed)
                        ListBox.Dispose();

                    ListBox = null;
                }

            }

        }

        #endregion
    }
}
