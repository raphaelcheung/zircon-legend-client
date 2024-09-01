namespace Launcher
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.gpBase = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnRecommand = new System.Windows.Forms.Button();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.ckFullScreen = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.gpAccount = new System.Windows.Forms.GroupBox();
            this.lkCreateAccount = new System.Windows.Forms.LinkLabel();
            this.lkChangePassword = new System.Windows.Forms.LinkLabel();
            this.ckRember = new System.Windows.Forms.CheckBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtAccount = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pnCharacter = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCreateCharacter = new System.Windows.Forms.Button();
            this.btnDelCharacter = new System.Windows.Forms.Button();
            this.cbCharacter = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.gpBase.SuspendLayout();
            this.gpAccount.SuspendLayout();
            this.pnCharacter.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "主机：";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(62, 23);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(198, 21);
            this.txtHost.TabIndex = 1;
            this.txtHost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtHost_KeyPress);
            // 
            // gpBase
            // 
            this.gpBase.Controls.Add(this.btnConnect);
            this.gpBase.Controls.Add(this.btnRecommand);
            this.gpBase.Controls.Add(this.txtHeight);
            this.gpBase.Controls.Add(this.label5);
            this.gpBase.Controls.Add(this.txtWidth);
            this.gpBase.Controls.Add(this.ckFullScreen);
            this.gpBase.Controls.Add(this.label3);
            this.gpBase.Controls.Add(this.txtPort);
            this.gpBase.Controls.Add(this.label2);
            this.gpBase.Controls.Add(this.txtHost);
            this.gpBase.Controls.Add(this.label1);
            this.gpBase.Enabled = false;
            this.gpBase.Location = new System.Drawing.Point(12, 12);
            this.gpBase.Name = "gpBase";
            this.gpBase.Size = new System.Drawing.Size(420, 124);
            this.gpBase.TabIndex = 2;
            this.gpBase.TabStop = false;
            this.gpBase.Text = "基本设置";
            // 
            // btnConnect
            // 
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.Font = new System.Drawing.Font("黑体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnConnect.Location = new System.Drawing.Point(324, 64);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 48);
            this.btnConnect.TabIndex = 13;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnRecommand
            // 
            this.btnRecommand.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRecommand.Location = new System.Drawing.Point(246, 59);
            this.btnRecommand.Name = "btnRecommand";
            this.btnRecommand.Size = new System.Drawing.Size(51, 23);
            this.btnRecommand.TabIndex = 12;
            this.btnRecommand.Text = "推荐";
            this.btnRecommand.UseVisualStyleBackColor = true;
            this.btnRecommand.Click += new System.EventHandler(this.btnRecommand_Click);
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(167, 60);
            this.txtHeight.MaxLength = 5;
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(68, 21);
            this.txtHeight.TabIndex = 11;
            this.txtHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtHeight_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(148, 64);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(11, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "X";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(73, 61);
            this.txtWidth.MaxLength = 5;
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(68, 21);
            this.txtWidth.TabIndex = 9;
            this.txtWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWidth_KeyPress);
            // 
            // ckFullScreen
            // 
            this.ckFullScreen.AutoSize = true;
            this.ckFullScreen.Location = new System.Drawing.Point(17, 96);
            this.ckFullScreen.Name = "ckFullScreen";
            this.ckFullScreen.Size = new System.Drawing.Size(48, 16);
            this.ckFullScreen.TabIndex = 8;
            this.ckFullScreen.Text = "全屏";
            this.ckFullScreen.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "分辨率：";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(337, 23);
            this.txtPort.MaxLength = 5;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(59, 21);
            this.txtPort.TabIndex = 3;
            this.txtPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPort_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(289, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "端口：";
            // 
            // progress
            // 
            this.progress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progress.Location = new System.Drawing.Point(0, 436);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(869, 23);
            this.progress.TabIndex = 6;
            // 
            // btnSave
            // 
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.Location = new System.Drawing.Point(13, 394);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(72, 36);
            this.btnSave.TabIndex = 17;
            this.btnSave.Text = "保存设置";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Right;
            this.txtLog.Location = new System.Drawing.Point(438, 0);
            this.txtLog.MaxLength = 3276700;
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(431, 436);
            this.txtLog.TabIndex = 23;
            this.txtLog.WordWrap = false;
            // 
            // gpAccount
            // 
            this.gpAccount.Controls.Add(this.lkCreateAccount);
            this.gpAccount.Controls.Add(this.lkChangePassword);
            this.gpAccount.Controls.Add(this.ckRember);
            this.gpAccount.Controls.Add(this.btnLogin);
            this.gpAccount.Controls.Add(this.txtPassword);
            this.gpAccount.Controls.Add(this.label6);
            this.gpAccount.Controls.Add(this.txtAccount);
            this.gpAccount.Controls.Add(this.label4);
            this.gpAccount.Enabled = false;
            this.gpAccount.Location = new System.Drawing.Point(13, 143);
            this.gpAccount.Name = "gpAccount";
            this.gpAccount.Size = new System.Drawing.Size(419, 100);
            this.gpAccount.TabIndex = 24;
            this.gpAccount.TabStop = false;
            this.gpAccount.Text = "账号";
            // 
            // lkCreateAccount
            // 
            this.lkCreateAccount.AutoSize = true;
            this.lkCreateAccount.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lkCreateAccount.Location = new System.Drawing.Point(181, 68);
            this.lkCreateAccount.Name = "lkCreateAccount";
            this.lkCreateAccount.Size = new System.Drawing.Size(53, 12);
            this.lkCreateAccount.TabIndex = 34;
            this.lkCreateAccount.TabStop = true;
            this.lkCreateAccount.Text = "创建账号";
            this.lkCreateAccount.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkCreateAccount_LinkClicked);
            // 
            // lkChangePassword
            // 
            this.lkChangePassword.AutoSize = true;
            this.lkChangePassword.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lkChangePassword.Location = new System.Drawing.Point(238, 68);
            this.lkChangePassword.Name = "lkChangePassword";
            this.lkChangePassword.Size = new System.Drawing.Size(53, 12);
            this.lkChangePassword.TabIndex = 33;
            this.lkChangePassword.TabStop = true;
            this.lkChangePassword.Text = "修改密码";
            // 
            // ckRember
            // 
            this.ckRember.AutoSize = true;
            this.ckRember.Location = new System.Drawing.Point(9, 66);
            this.ckRember.Name = "ckRember";
            this.ckRember.Size = new System.Drawing.Size(72, 16);
            this.ckRember.TabIndex = 32;
            this.ckRember.Text = "记住密码";
            this.ckRember.UseVisualStyleBackColor = true;
            // 
            // btnLogin
            // 
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.Font = new System.Drawing.Font("黑体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnLogin.Location = new System.Drawing.Point(302, 59);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(104, 30);
            this.btnLogin.TabIndex = 30;
            this.btnLogin.Text = "登录";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(275, 26);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(131, 21);
            this.txtPassword.TabIndex = 29;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(238, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 28;
            this.label6.Text = "密码：";
            // 
            // txtAccount
            // 
            this.txtAccount.Location = new System.Drawing.Point(48, 26);
            this.txtAccount.Name = "txtAccount";
            this.txtAccount.Size = new System.Drawing.Size(176, 21);
            this.txtAccount.TabIndex = 27;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 26;
            this.label4.Text = "账号：";
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // pnCharacter
            // 
            this.pnCharacter.Controls.Add(this.btnStart);
            this.pnCharacter.Controls.Add(this.btnCreateCharacter);
            this.pnCharacter.Controls.Add(this.btnDelCharacter);
            this.pnCharacter.Controls.Add(this.cbCharacter);
            this.pnCharacter.Controls.Add(this.label7);
            this.pnCharacter.Location = new System.Drawing.Point(13, 249);
            this.pnCharacter.Name = "pnCharacter";
            this.pnCharacter.Size = new System.Drawing.Size(419, 133);
            this.pnCharacter.TabIndex = 25;
            // 
            // btnStart
            // 
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Font = new System.Drawing.Font("黑体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.Location = new System.Drawing.Point(303, 56);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(104, 61);
            this.btnStart.TabIndex = 27;
            this.btnStart.Text = "进入游戏";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // btnCreateCharacter
            // 
            this.btnCreateCharacter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreateCharacter.Location = new System.Drawing.Point(14, 56);
            this.btnCreateCharacter.Name = "btnCreateCharacter";
            this.btnCreateCharacter.Size = new System.Drawing.Size(68, 23);
            this.btnCreateCharacter.TabIndex = 26;
            this.btnCreateCharacter.Text = "创建角色";
            this.btnCreateCharacter.UseVisualStyleBackColor = true;
            // 
            // btnDelCharacter
            // 
            this.btnDelCharacter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelCharacter.Location = new System.Drawing.Point(96, 56);
            this.btnDelCharacter.Name = "btnDelCharacter";
            this.btnDelCharacter.Size = new System.Drawing.Size(64, 23);
            this.btnDelCharacter.TabIndex = 25;
            this.btnDelCharacter.Text = "删除角色";
            this.btnDelCharacter.UseVisualStyleBackColor = true;
            // 
            // cbCharacter
            // 
            this.cbCharacter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCharacter.FormattingEnabled = true;
            this.cbCharacter.Location = new System.Drawing.Point(55, 21);
            this.cbCharacter.Name = "cbCharacter";
            this.cbCharacter.Size = new System.Drawing.Size(352, 20);
            this.cbCharacter.TabIndex = 24;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 23;
            this.label7.Text = "角色：";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 459);
            this.Controls.Add(this.pnCharacter);
            this.Controls.Add(this.gpAccount);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.gpBase);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "皓石启动器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.gpBase.ResumeLayout(false);
            this.gpBase.PerformLayout();
            this.gpAccount.ResumeLayout(false);
            this.gpAccount.PerformLayout();
            this.pnCharacter.ResumeLayout(false);
            this.pnCharacter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.GroupBox gpBase;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.Button btnRecommand;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.CheckBox ckFullScreen;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.GroupBox gpAccount;
        private System.Windows.Forms.CheckBox ckRember;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtAccount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.LinkLabel lkCreateAccount;
        private System.Windows.Forms.LinkLabel lkChangePassword;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel pnCharacter;
        private System.Windows.Forms.Button btnCreateCharacter;
        private System.Windows.Forms.Button btnDelCharacter;
        private System.Windows.Forms.ComboBox cbCharacter;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnStart;
    }
}

