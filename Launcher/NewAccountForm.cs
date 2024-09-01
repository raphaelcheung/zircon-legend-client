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
    public partial class NewAccountForm : Form
    {
        public NewAccountForm()
        {
            InitializeComponent();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            btnCreate.Enabled = !string.IsNullOrEmpty(txtAccount.Text) && !string.IsNullOrEmpty(txtPassword.Text) && !string.IsNullOrEmpty(txtConfirm.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {

            if (!Globals.EMailRegex.IsMatch(txtAccount.Text))
            {
                MessageBox.Show("账号格式不符合要求", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Globals.PasswordRegex.IsMatch(txtPassword.Text))
            {
                MessageBox.Show("密码格式不符合要求", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtPassword.Text != txtConfirm.Text)
            {
                MessageBox.Show("两次输入密码不一致", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            pnMain.Enabled = false;
            CEnvir.CreateAccount(txtAccount.Text, txtPassword.Text);
        }

        private void OnLog(string msg, bool pop, string key)
        {
            if (key != "创建账号") return;

            MessageBox.Show(this, msg, key, MessageBoxButtons.OK, MessageBoxIcon.Information);
            pnMain.Enabled = true;

            if (!pop)
            {
                Thread.Sleep(300);
                this.Close();
            }
        }

        private void OnMainStatusChanged()
        {

        }

        private void NewAccountForm_Load(object sender, EventArgs e)
        {
            CEnvir.MainStepChanged += OnMainStatusChanged;
            CEnvir.LogEvent += OnLog;
        }

        private void NewAccountForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            CEnvir.LogEvent -= OnLog;
            CEnvir.MainStepChanged -= OnMainStatusChanged;
        }
    }
}
