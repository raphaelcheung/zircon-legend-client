using Client.Envir;
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
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void OnLog(string msg)
        {

        }

        private void OnMainStatusChanged()
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            txtHost.Text = Config.IPAddress;
            txtPort.Text = $"{Config.Port}";
            txtWidth.Text = $"{Config.GameSize.Width}";
            txtHeight.Text = $"{Config.GameSize.Height}";
            ckFullScreen.Checked = Config.FullScreen;

            txtAccount.Text = Config.RememberedEMail;
            txtPassword.Text = Config.RememberedPassword;
            cbCharacter.Enabled = false;

            CEnvir.LogEvent += OnLog;
            CEnvir.MainStepChanged += OnMainStatusChanged;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Enabled = false;
            CEnvir.LogEvent -= OnLog;
            CEnvir.MainStepChanged -= OnMainStatusChanged;

            CEnvir.Stop();

            while (CEnvir.MainStep != CEnvir.MainStepType.Stop) 
                Thread.Sleep(200);
        }
    }
}
