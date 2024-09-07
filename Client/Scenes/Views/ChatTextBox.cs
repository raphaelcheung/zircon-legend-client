﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.UserModels;
using Library;
using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Scenes.Views
{

    public sealed class ChatTextBox : DXWindow
    {
        #region Properties

        #region Mode

        public ChatMode Mode
        {
            get { return _Mode; }
            set
            {
                if (_Mode == value) return;

                ChatMode oldValue = _Mode;
                _Mode = value;

                OnModeChanged(oldValue, value);
            }
        }
        private ChatMode _Mode;
        public event EventHandler<EventArgs> ModeChanged;
        public void OnModeChanged(ChatMode oValue, ChatMode nValue)
        {
            ModeChanged?.Invoke(this, EventArgs.Empty);

            if (ChatModeButton != null)
                ChatModeButton.Label.Text = Mode.ToString();

            OpenChat();
        }

        #endregion

        public string LastPM;
        

        public DXTextBox TextBox;
        public DXButton OptionsButton;
        public DXButton ChatModeButton;

        public override void OnParentChanged(DXControl oValue, DXControl nValue)
        {
            base.OnParentChanged(oValue, nValue);

            if (GameScene.Game.MainPanel == null) return;

            Location = new Point(GameScene.Game.MainPanel.Location.X, (GameScene.Game.MainPanel.DisplayArea.Top - Size.Height));
        }
        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);

            if (TextBox == null || ChatModeButton == null || OptionsButton == null) return;

            ChatModeButton.Location = new Point(ClientArea.Location.X, ClientArea.Y - 1);
            TextBox.Size = new Size(ClientArea.Width - ChatModeButton.Size.Width - 20 - OptionsButton.Size.Width, 25);
            TextBox.Location = new Point(ClientArea.Location.X + ChatModeButton.Size.Width + 15, ClientArea.Y);
            OptionsButton.Location = new Point(ClientArea.Location.X + TextBox.Size.Width + ChatModeButton.Size.Width + 10, ClientArea.Y - 1);
        }

        public override WindowType Type => WindowType.ChatTextBox;
        public override bool CustomSize => true;
        public override bool AutomaticVisiblity => true;

        #endregion

        public ChatTextBox()
        {

            Size = new Size(400, 30);

            Opacity = 0.6F;

            HasTitle = false;
            HasFooter = false;
            HasTopBorder = false;
            CloseButton.Visible = false;

            AllowResize = true;
            CanResizeHeight = false;

            ChatModeButton = new DXButton
            {
                ButtonType = ButtonType.SmallButton,
                Size = new Size(60, SmallButtonHeight),
                Label = { Text = Mode.ToString() },
                Parent = this,
            };
            ChatModeButton.MouseClick += (o, e) => Mode = (ChatMode) (((int) (Mode) + 1)%5);

            OptionsButton = new DXButton
            {
                ButtonType = ButtonType.SmallButton,
                Size = new Size(50, SmallButtonHeight),
                Label = { Text = "选项" },
                Parent = this,
            };
            OptionsButton.MouseClick += (o, e) =>
            {
                GameScene.Game.ChatOptionsBox.Visible = !GameScene.Game.ChatOptionsBox.Visible;
            };



            TextBox = new DXTextBox
            {
                Size = new Size(350, 20),
                Parent = this,
                MaxLength = Globals.MaxChatLength,
                Opacity = 0.3f,
                BackColour = Color.White,
                ForeColour = Color.Black,
            };
            TextBox.TextBox.KeyPress += TextBox_KeyPress;
            TextBox.FocusEvent += (sender, focus) =>
            {
                TextBox.Opacity = focus ? 1f : 0.3f;
            };
          //  TextBox.TextBox.KeyDown += TextBox_KeyDown;
          //   TextBox.TextBox.KeyUp += TextBox_KeyUp;

            SetClientSize(new Size(TextBox.Size.Width + ChatModeButton.Size.Width + 15 + OptionsButton.Size.Width, TextBox.Size.Height));

            ChatModeButton.Location = new Point(ClientArea.Location.X, ClientArea.Y - 1);
            TextBox.Location = new Point(ClientArea.Location.X + ChatModeButton.Size.Width + 5, ClientArea.Y);
            OptionsButton.Location = new Point(ClientArea.Location.X + TextBox.Size.Width + ChatModeButton.Size.Width + 10, ClientArea.Y - 1);
        }

        #region Methods
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    e.Handled = true;
                    if (!string.IsNullOrEmpty(TextBox.TextBox.Text))
                    {
                        CEnvir.Enqueue(new C.Chat
                        {
                            Text = TextBox.TextBox.Text,
                        });

                        if (TextBox.TextBox.Text[0] == '/')
                        {
                            string[] parts = TextBox.TextBox.Text.Split(' ');

                            if (parts.Length > 0) LastPM = parts[0];
                        }
                    }

                    DXTextBox.ActiveTextBox = null;
                    TextBox.TextBox.Text = string.Empty;
                    return;
                case (char)Keys.Escape:
                    e.Handled = true;
                    DXTextBox.ActiveTextBox = null;
                    TextBox.TextBox.Text = string.Empty;
                    return;
                case '！':
                    e.Handled = true;
                    //e.KeyChar = '!';
                    TextBox.TextBox.Text += '!';
                    break;
                case '＠':
                    e.Handled = true;
                    //e.KeyChar = '@';
                    TextBox.TextBox.Text += '@';

                    break;
                case '～':
                    e.Handled = true;
                    //e.KeyChar = '~';
                    TextBox.TextBox.Text += '~';

                    break;
                case '／':
                    e.Handled = true;
                    //e.KeyChar = '/';
                    TextBox.TextBox.Text += '/';

                    break;
                case '＃':
                    e.Handled = true;
                    //e.KeyChar = '#';
                    TextBox.TextBox.Text += '#';

                    break;
            }

            if (e.Handled)
            {
                TextBox.TextBox.SelectionLength = 0;
                TextBox.TextBox.SelectionStart = TextBox.TextBox.Text.Length;
            }
        }

        public override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            switch (e.KeyChar)
            {
                case '@':
                    TextBox.SetFocus();
                    TextBox.TextBox.Text = @"@";
                    TextBox.Visible = true;
                    TextBox.TextBox.SelectionLength = 0;
                    TextBox.TextBox.SelectionStart = TextBox.TextBox.Text.Length;
                    e.Handled = true;
                    break;
                case '!':
                    if (!Config.ShiftOpenChat) return;
                    TextBox.SetFocus();
                    TextBox.TextBox.Text = @"!";
                    TextBox.Visible = true;
                    TextBox.TextBox.SelectionLength = 0;
                    TextBox.TextBox.SelectionStart = TextBox.TextBox.Text.Length;
                    e.Handled = true;
                    break;
                case ' ':
                case (char)Keys.Enter:
                    OpenChat();
                    e.Handled = true;
                    break;
                case '/':
                    TextBox.SetFocus();
                    if (string.IsNullOrEmpty(LastPM))
                        TextBox.TextBox.Text = "/";
                    else
                        TextBox.TextBox.Text = LastPM + " ";
                    TextBox.Visible = true;
                    TextBox.TextBox.SelectionLength = 0;
                    TextBox.TextBox.SelectionStart = TextBox.TextBox.Text.Length;
                    e.Handled = true;
                    break;
            }
        }

        private string[] SplitCommand(string command)
        {
            string tmp;
            if (command.StartsWith("!!"))
            {
                tmp = command.Remove(0, 2).Trim();
                if (string.IsNullOrEmpty(tmp)) return null;

                return new string[] { "!!", tmp };
            }
            else if (command.StartsWith("!@"))
            {
                tmp = command.Remove(0, 2).Trim();
                if (string.IsNullOrEmpty(tmp)) return null;

                return new string[] { "!@", tmp };
            }
            else if (command.StartsWith("@!"))
            {
                tmp = command.Remove(0, 2).Trim();
                if (string.IsNullOrEmpty(tmp)) return null;

                return new string[] { "@!", tmp };
            }
            else if (command.StartsWith("!~"))
            {
                tmp = command.Remove(0, 2).Trim();
                if (string.IsNullOrEmpty(tmp)) return null;

                return new string[] { "!~", tmp };
            }
            else if (command.StartsWith("!"))
            {
                tmp = command.Remove(0, 1).Trim();
                if (string.IsNullOrEmpty(tmp)) return null;

                return new string[] { "!", tmp };
            }
            else if (command.StartsWith("/"))
            {
                var split = command.Remove(0, 1).Split(' ');
                if (split == null || split.Length < 2 || string.IsNullOrEmpty(split[1].Trim())) return null;

                return new string[] { $"/{split[0]}", split[1] };
            }
            else if (command.StartsWith("#"))
            {
                tmp = command.Remove(0, 1).Trim();
                if (string.IsNullOrEmpty(tmp)) return null;

                return new string[] { "#", tmp };
            }
            else
                return null;
        }

        public void OpenChat()
        {
            var cmd = SplitCommand(TextBox.TextBox.Text);

            string header = "";
            switch (Mode)
            {
                case ChatMode.喊话:
                    header = @"! ";
                    break;
                //case ChatMode.低语:
                //    if (!string.IsNullOrWhiteSpace(LastPM))
                //        TextBox.TextBox.Text = LastPM + " ";
                //    break;
                case ChatMode.队伍:
                    header = @"!! ";
                    break;
                case ChatMode.行会:
                    header= @"!~ ";
                    break;
                case ChatMode.全服:
                    header= @"!@ ";
                    break;
                //case ChatMode.观察:
                //    TextBox.TextBox.Text = @"# ";
                //    break;
            }

            if (string.IsNullOrEmpty(TextBox.TextBox.Text))
                TextBox.TextBox.Text = $"{header}";
            else if (cmd == null)
                TextBox.TextBox.Text = $"{header}{TextBox.TextBox.Text}";
            else
                TextBox.TextBox.Text = $"{header}{cmd[1]}";

            TextBox.SetFocus();
            TextBox.TextBox.SelectionLength = 0;
            TextBox.TextBox.SelectionStart = TextBox.TextBox.Text.Length;
        }
        public void StartPM(string name)
        {
            TextBox.TextBox.Text = $"/{name} ";
            TextBox.SetFocus();
            TextBox.TextBox.SelectionLength = 0;
            TextBox.TextBox.SelectionStart = TextBox.TextBox.Text.Length;
        }
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _Mode = 0;
                ModeChanged = null;

                LastPM = null;

                if (TextBox != null)
                {
                    if (!TextBox.IsDisposed)
                        TextBox.Dispose();

                    TextBox = null;
                }
                
                if (OptionsButton != null)
                {
                    if (!OptionsButton.IsDisposed)
                        OptionsButton.Dispose();

                    OptionsButton = null;
                }
                
                if (ChatModeButton != null)
                {
                    if (!ChatModeButton.IsDisposed)
                        ChatModeButton.Dispose();

                    ChatModeButton = null;
                }
            }

        }

        #endregion

    }

    public enum ChatMode
    {
        本地,
        //低语,
        队伍,
        行会,
        喊话,
        全服,
        //观察, //7
    }

    public class Message
    {
        public string Text { get; set; }
        public DateTime ReceivedTime { get; set; }
        public MessageType Type { get; set; }
    }
    
}
