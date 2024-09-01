using Client.Envir;
using Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void OnLog(string msg, bool pop, string key)
        {
            txtLog.AppendText($"{msg}\r\n");

            if (pop && key != null && key != "创建账号") 
                MessageBox.Show(msg, key, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnMainStatusChanged()
        {
            if (CEnvir.MainStep == CEnvir.MainStepType.Ready)
                gpBase.Enabled = true;
            else gpBase.Enabled = false;

            if (CEnvir.MainStep == CEnvir.MainStepType.Connected)
                gpAccount.Enabled = true;
            else gpAccount.Enabled = false;

            if (CEnvir.MainStep == CEnvir.MainStepType.Upgrading)
                progress.Value = 0;

            if (CEnvir.MainStep == CEnvir.MainStepType.Upgraded)
            {
                progress.Value = 100;
                timer1.Stop();
                gpAccount.Enabled = true;
            }
            else gpAccount.Enabled = false;

            if (CEnvir.MainStep == CEnvir.MainStepType.Logon)
            {

                foreach(var character in CEnvir.SelectCharacters)
                {
                    string gender = Functions.GetEnumDesc(character.Gender);
                    string cls = Functions.GetEnumDesc(character.Class);

                    cbCharacter.Items.Add($"【{character.CharacterName}】 {character.Level}级{gender}{cls}  最后登录：{character.LastLogin.ToString()}");
                }

                cbCharacter.SelectedIndex = 0;
                pnCharacter.Enabled = true;
            }
            else
                pnCharacter.Enabled = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            txtHost.Text = Config.IPAddress;
            txtPort.Text = $"{Config.Port}";
            txtWidth.Text = $"{Config.GameSize.Width}";
            txtHeight.Text = $"{Config.GameSize.Height}";
            ckFullScreen.Checked = Config.FullScreen;
            ckRember.Checked = Config.Remember;

            txtAccount.Text = Config.Account;

            if (Config.Remember)
                txtPassword.Text = Config.Password;

            progress.Maximum = 100;

            CEnvir.LogEvent += OnLog;
            CEnvir.MainStepChanged += OnMainStatusChanged;
            CEnvir.Initialize();

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Enabled = false;
            CEnvir.MainStepChanged -= OnMainStatusChanged;

            CEnvir.Stop();

            while (CEnvir.MainStep != CEnvir.MainStepType.Stop) 
                Thread.Sleep(200);

            CEnvir.LogEvent -= OnLog;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (CEnvir.MainStep == CEnvir.MainStepType.Upgrading && CEnvir.UpgradeTotalSize != 0)
            {
                int val = (int)(CEnvir.UpgradedSize * 100 / CEnvir.UpgradeTotalSize);
                if (val != progress.Value)
                {
                    progress.Value = val;
                    progress.Refresh();
                }

            }
        }

        private void btnRecommand_Click(object sender, EventArgs e)
        {
            Screen s = Screen.PrimaryScreen;

            if (ckFullScreen.Checked)
            {
                Size p = s.Bounds.Size;
                txtWidth.Text = $"{p.Width}";
                txtHeight.Text = $"{p.Height}";
            }
            else
            {
                Size p = s.WorkingArea.Size;
                txtWidth.Text = $"{p.Width}";
                txtHeight.Text = $"{p.Height}";
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtHost.Text) || string.IsNullOrEmpty(txtPort.Text))
            {
                MessageBox.Show("主机和端口都不能为空！", "连接", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Config.Port = int.Parse(txtPort.Text);
            Config.IPAddress = txtHost.Text;
            Config.Remember = ckRember.Checked;

            ConfigReader.Save();

            CEnvir.Connect();
        }

        private void txtWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != 8) e.Handled = true;
        }

        private void txtHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != 8) e.Handled = true;

        }

        private void txtHost_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 'a' && e.KeyChar <= 'Z')
                || (e.KeyChar >= '0' && e.KeyChar <= '9')
                || e.KeyChar == '.' || e.KeyChar == '-' || e.KeyChar == 8)
                e.Handled = false;
            else 
                e.Handled = true;
        }

        private void txtPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != 8) e.Handled = true;

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Globals.EMailRegex.IsMatch(txtAccount.Text))
            {
                MessageBox.Show("账号格式错误，必须是邮箱地址", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Globals.PasswordRegex.IsMatch(txtPassword.Text))
            {
                MessageBox.Show($"密码格式错误，必须是 {Globals.MinPasswordLength} - {Globals.MaxPasswordLength} 位非空白字符", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Config.Account = txtAccount.Text;
            Config.Remember = ckRember.Checked;
            if (ckRember.Checked) Config.Password = txtPassword.Text;

            ConfigReader.Save();
            gpAccount.Enabled = false;
            CEnvir.Login(txtPassword.Text);
        }

        private void lkCreateAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            NewAccountForm diag = new NewAccountForm();
            diag.ShowDialog();
            
        }
    }
}
