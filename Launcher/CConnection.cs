using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;
using Library.Network;
using Library;
using Library.SystemModels;
using G = Library.Network.GeneralPackets;
using S = Library.Network.ServerPackets;
using C = Library.Network.ClientPackets;

namespace Client.Envir
{
    public sealed class CConnection : BaseConnection
    {
        protected override TimeSpan TimeOutDelay => Config.TimeOutDuration;

        public bool ServerConnected { get; set; }

        public int Ping;

        private readonly List<byte> DbUpgrade = new List<byte>();

        public CConnection(TcpClient client)
            : base(client)
        {
            //OnException += (o, e) => CEnvir.SaveError(e.ToString());

            UpdateTimeOut();

            AdditionalLogging = true;

            BeginReceive();
        }

        public override void TryDisconnect()
        {
            Disconnect();
        }
        public override void Disconnect()
        {
            base.Disconnect();

            if (CEnvir.Connection == this)
            {
                CEnvir.Disconnect();
            }
        }
        public override void TrySendDisconnect(Packet p)
        {
            SendDisconnect(p);
        }

        public void Process(G.Disconnect p)
        {
            Disconnecting = true;


            switch (p.Reason)
            {
                case DisconnectReason.Unknown:
                    CEnvir.Log("服务器断开连接，原因: 未知");
                    break;
                case DisconnectReason.TimedOut:
                    CEnvir.Log("服务器断开连接，原因: 连接超时.");
                    break;
                case DisconnectReason.ServerClosing:
                    CEnvir.Log("服务器断开连接，原因: 服务器关闭.");
                    break;
                case DisconnectReason.AnotherUser:
                    CEnvir.Log("服务器断开连接，原因: 其他用户使用你的账号登录.");
                    break;
                case DisconnectReason.AnotherUserAdmin:
                    CEnvir.Log("服务器断开连接，原因: 管理员接管了你的账号.");
                    break;
                case DisconnectReason.Banned:
                    CEnvir.Log("服务器断开连接，原因: 你的账号被禁用.");
                    break;
                case DisconnectReason.Crashed:
                    CEnvir.Log("服务器断开连接，原因: 服务器崩溃.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (this == CEnvir.Connection)
                CEnvir.Disconnect();

        }
        public void Process(G.Connected p)
        {
            Enqueue(new G.Connected());
            ServerConnected = true;
            CEnvir.Connected();
        }
        public void Process(S.CheckClientHash p)
        {
            CEnvir.CheckUpgrade(p.ClientFileHash);
        }
        public void Process(G.Ping p)
        {
            Enqueue(new G.Ping());
        }
        public void Process(G.PingResponse p)
        {
            Ping = p.Ping;
        }

        //public void Process(S.NewAccount p)
        //{
        //    LoginScene login = DXControl.ActiveScene as LoginScene;
        //    if (login == null) return;

        //    login.AccountBox.CreateAttempted = false;

        //    switch (p.Result)
        //    {
        //        case NewAccountResult.Disabled:
        //            login.AccountBox.Clear();
        //            DXMessageBox.Show("创建账号的功能被禁用.", "创建账号");
        //            break;
        //        case NewAccountResult.BadEMail:
        //            login.AccountBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("E-Mail 地址不符合规范.", "创建账号");
        //            break;
        //        case NewAccountResult.BadPassword:
        //            login.AccountBox.Password1TextBox.SetFocus();
        //            DXMessageBox.Show("密码不符合规范.", "创建账号");
        //            break;
        //        case NewAccountResult.BadRealName:
        //            login.AccountBox.RealNameTextBox.SetFocus();
        //            DXMessageBox.Show("真实名称不符合规范.", "创建账号");
        //            break;
        //        case NewAccountResult.AlreadyExists:
        //            login.AccountBox.EMailTextBox.TextBox.Text = string.Empty;
        //            login.AccountBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("E-Mail 地址已被使用.", "创建账号");
        //            break;
        //        case NewAccountResult.BadReferral:
        //            login.AccountBox.ReferralTextBox.SetFocus();
        //            DXMessageBox.Show("推荐人的 E-Mail 地址不符合规范.", "创建账号");
        //            break;
        //        case NewAccountResult.ReferralNotFound:
        //            login.AccountBox.ReferralTextBox.SetFocus();
        //            DXMessageBox.Show("找不到推荐人的 E-Mail 地址.", "创建账号");
        //            break;
        //        case NewAccountResult.ReferralNotActivated:
        //            login.AccountBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("推荐人的 E-Mail 地址没有激活.", "创建账号");
        //            break;
        //        case NewAccountResult.Success:
        //            login.LoginBox.EMailTextBox.TextBox.Text = login.AccountBox.EMailTextBox.TextBox.Text;
        //            login.LoginBox.PasswordTextBox.TextBox.Text = login.AccountBox.Password1TextBox.TextBox.Text;
        //            login.AccountBox.Clear();
        //            DXMessageBox.Show("你的账号创建成功.\n" +
        //                              "祝你游戏愉快.", "创建账号");
        //            break;
        //    }

        //}
        //public void Process(S.ChangePassword p)
        //{
        //    LoginScene login = DXControl.ActiveScene as LoginScene;
        //    if (login == null) return;

        //    login.ChangeBox.ChangeAttempted = false;

        //    switch (p.Result)
        //    {
        //        case ChangePasswordResult.Disabled:
        //            login.ChangeBox.Clear();
        //            DXMessageBox.Show("修改密码被禁用.", "修改密码");
        //            break;
        //        case ChangePasswordResult.BadEMail:
        //            login.ChangeBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("E-Mail 不符合规范.", "修改密码");
        //            break;
        //        case ChangePasswordResult.BadCurrentPassword:
        //            login.ChangeBox.CurrentPasswordTextBox.SetFocus();
        //            DXMessageBox.Show("当前密码不符合规范.", "修改密码");
        //            break;
        //        case ChangePasswordResult.BadNewPassword:
        //            login.ChangeBox.NewPassword1TextBox.SetFocus();
        //            DXMessageBox.Show("新密码不符合规范.", "修改密码");
        //            break;
        //        case ChangePasswordResult.AccountNotFound:
        //            login.ChangeBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("账号不存在.", "修改密码");
        //            break;
        //        case ChangePasswordResult.AccountNotActivated:
        //            login.ShowActivationBox(login.ChangeBox);
        //            break;
        //        case ChangePasswordResult.WrongPassword:
        //            login.ChangeBox.CurrentPasswordTextBox.SetFocus();
        //            DXMessageBox.Show("密码错误.", "修改密码");
        //            break;
        //        case ChangePasswordResult.Banned:
        //            DateTime expiry = CEnvir.Now.Add(p.Duration);
        //            DXMessageBox box = DXMessageBox.Show($"该账号已被禁用.\n\n原因: {p.Message}\n" +
        //                                                 $"解禁时间: {expiry}\n" +
        //                                                 $"距离解封还有: {Math.Floor(p.Duration.TotalHours):#,##0} 小时, {p.Duration.Minutes} 分钟, {p.Duration.Seconds} 秒", "修改密码");

        //            box.ProcessAction = () =>
        //            {
        //                if (CEnvir.Now > expiry)
        //                {
        //                    if (login.ChangeBox.CanChange)
        //                        login.ChangeBox.Change();
        //                    box.ProcessAction = null;
        //                    return;
        //                }

        //                TimeSpan remaining = expiry - CEnvir.Now;

        //                box.Label.Text = $"该账号已被禁用.\n\n" +
        //                                 $"原因: {p.Message}\n" +
        //                                 $"解禁时间: {expiry}\n" +
        //                                 $"距离解禁还有: {Math.Floor(remaining.TotalHours):#,##0} 小时, {remaining.Minutes} 分钟, {remaining.Seconds} 秒";
        //            };
        //            break;
        //        case ChangePasswordResult.Success:
        //            login.ChangeBox.Clear();
        //            DXMessageBox.Show("密码修改成功.", "修改密码");
        //            break;
        //    }

        //}

        //public void Process(S.Login p)
        //{
        //    LoginScene login = DXControl.ActiveScene as LoginScene;
        //    if (login == null) return;

        //    login.LoginBox.LoginAttempted = false;

        //    SelectScene scene;
        //    switch (p.Result)
        //    {
        //        case LoginResult.Disabled:
        //            DXMessageBox.Show("当前禁止登录.", "登录");
        //            break;
        //        case LoginResult.BadEMail:
        //            login.LoginBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("账号不符合规范.", "登录");
        //            break;
        //        case LoginResult.BadPassword:
        //            login.LoginBox.PasswordTextBox.SetFocus();
        //            DXMessageBox.Show("密码不符合规范.", "登录");
        //            break;
        //        case LoginResult.AccountNotExists:
        //            login.LoginBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("账号不存在.", "登录");
        //            break;
        //        case LoginResult.AccountNotActivated:
        //            login.ShowActivationBox(login.LoginBox);
        //            break;
        //        case LoginResult.WrongPassword:
        //            login.LoginBox.PasswordTextBox.SetFocus();
        //            DXMessageBox.Show("密码错误.", "登录");
        //            break;
        //        case LoginResult.Banned:
        //            DateTime expiry = CEnvir.Now.Add(p.Duration);

        //            DXMessageBox box = DXMessageBox.Show($"该账号已被禁用.\n\n" +
        //                                                 $"原因: {p.Message}\n" +
        //                                                 $"解禁时间: {expiry}\n" +
        //                                                 $"距离解禁还有: {Math.Floor(p.Duration.TotalHours):#,##0} 小时, {p.Duration.Minutes} 分, {p.Duration.Seconds} 秒", "登录");

        //            box.ProcessAction = () =>
        //            {
        //                if (CEnvir.Now > expiry)
        //                {
        //                    if (login.LoginBox.CanLogin)
        //                        login.LoginBox.Login();
        //                    box.ProcessAction = null;
        //                    return;
        //                }

        //                TimeSpan remaining = expiry - CEnvir.Now;

        //                box.Label.Text = $"该账号已被禁用.\n\n" +
        //                                 $"原因: {p.Message}\n" +
        //                                 $"解禁时间: {expiry}\n" +
        //                                 $"距离解禁还有: {Math.Floor(remaining.TotalHours):#,##0} Hours, {remaining.Minutes} Minutes, {remaining.Seconds} Seconds";
        //            };
        //            break;
        //        case LoginResult.AlreadyLoggedIn:
        //            login.LoginBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("该账号正在使用中，稍候再试.", "登录");
        //            break;
        //        case LoginResult.AlreadyLoggedInPassword:
        //            login.LoginBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("该账号正在使用中\n" +
        //                              "新密码已发到 E-Mail 邮箱..", "登录");
        //            break;
        //        case LoginResult.AlreadyLoggedInAdmin:
        //            login.LoginBox.EMailTextBox.SetFocus();
        //            DXMessageBox.Show("账号正在被管理员接管", "登录");
        //            break;
        //        case LoginResult.Success:
        //            login.LoginBox.Visible = false;
        //            login.AccountBox.Visible = false;
        //            login.ChangeBox.Visible = false;
        //            login.RequestPassswordBox.Visible = false;
        //            login.ResetBox.Visible = false;
        //            login.ActivationBox.Visible = false;
        //            login.RequestActivationBox.Visible = false;

        //            CEnvir.TestServer = p.TestServer;
                    
        //            if (Config.RememberDetails)
        //            {
        //                Config.RememberedEMail = login.LoginBox.EMailTextBox.TextBox.Text;
        //                Config.RememberedPassword = login.LoginBox.PasswordTextBox.TextBox.Text;
        //            }

        //            login.Dispose();
        //            DXSoundManager.Stop(SoundIndex.LoginScene);
        //            DXSoundManager.Play(SoundIndex.SelectScene);

        //            p.Characters.Sort((x1, x2) => x2.LastLogin.CompareTo(x1.LastLogin));

        //            DXControl.ActiveScene = scene = new SelectScene(Config.IntroSceneSize)
        //            {
        //                SelectBox = { CharacterList = p.Characters },
        //            };

        //            scene.SelectBox.UpdateCharacters();

        //            CEnvir.BuyAddress = p.Address;
        //            CEnvir.FillStorage(p.Items, false);

        //            CEnvir.BlockList = p.BlockList;

        //            if (!string.IsNullOrEmpty(p.Message)) DXMessageBox.Show(p.Message, "登录消息");
                    
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}
        //public void Process(S.SelectLogout p)
        //{
        //    CEnvir.ReturnToLogin();
        //    ((LoginScene)DXControl.ActiveScene).LoginBox.Visible = true;
        //}
        //public void Process(S.GameLogout p)
        //{
        //    DXSoundManager.StopAllSounds();

        //    GameScene.Game.Dispose();

        //    DXSoundManager.Play(SoundIndex.SelectScene);

        //    SelectScene scene;

        //    p.Characters.Sort((x1, x2) => x2.LastLogin.CompareTo(x1.LastLogin));

        //    DXControl.ActiveScene = scene = new SelectScene(Config.IntroSceneSize)
        //    {
        //        SelectBox = { CharacterList = p.Characters },
        //    };

        //    CEnvir.Storage = CEnvir.MainStorage;

        //    scene.SelectBox.UpdateCharacters();
        //}
        //public void Process(S.UpgradeClient p)
        //{
        //    CEnvir.Upgrade(p.FileKey, p.TotalSize, p.StartIndex, p.Datas);
        //}

        //public void Process(S.NewCharacter p)
        //{
        //    SelectScene select = DXControl.ActiveScene as SelectScene;
        //    if (select == null) return;

        //    select.CharacterBox.CreateAttempted = false;
            
        //    switch (p.Result)
        //    {
        //        case NewCharacterResult.Disabled:
        //            select.CharacterBox.Clear();
        //            DXMessageBox.Show("创建角色功能被禁用.", "创建角色");
        //            break;
        //        case NewCharacterResult.BadCharacterName:
        //            select.CharacterBox.CharacterNameTextBox.SetFocus();
        //            DXMessageBox.Show("角色名称不符合规范.", "创建角色");
        //            break;
        //        case NewCharacterResult.BadHairType:
        //            select.CharacterBox.HairNumberBox.Value = 1;
        //            DXMessageBox.Show("错误: 无效的发型.", "创建角色");
        //            break;
        //        case NewCharacterResult.BadHairColour:
        //            DXMessageBox.Show("错误: 无效的头发颜色.", "创建角色");
        //            break;
        //        case NewCharacterResult.BadArmourColour:
        //            DXMessageBox.Show("错误: 无效的盔甲颜色.", "创建角色");
        //            break;
        //        case NewCharacterResult.BadGender:
        //            select.CharacterBox.SelectedGender = MirGender.Male;
        //            DXMessageBox.Show("错误: 无效的性别.", "创建角色");
        //            break;
        //        case NewCharacterResult.BadClass:
        //            select.CharacterBox.SelectedClass = MirClass.Warrior;
        //            DXMessageBox.Show("错误: 无效的职业.", "创建角色");
        //            break;
        //        case NewCharacterResult.ClassDisabled:
        //            DXMessageBox.Show("选中的职业当前不可用.", "创建角色");
        //            break;
        //        case NewCharacterResult.MaxCharacters:
        //            select.CharacterBox.Clear();
        //            DXMessageBox.Show("可创建的角色数量已达上限.", "创建角色");
        //            break;
        //        case NewCharacterResult.AlreadyExists:
        //            select.CharacterBox.CharacterNameTextBox.SetFocus();
        //            DXMessageBox.Show("角色已存在.", "创建角色");
        //            break;
        //        case NewCharacterResult.Success:
        //            select.CharacterBox.Clear();

        //            select.SelectBox.CharacterList.Add(p.Character);
        //            select.SelectBox.UpdateCharacters();
        //            select.SelectBox.SelectedButton = select.SelectBox.SelectButtons[select.SelectBox.CharacterList.Count -1];

        //            DXMessageBox.Show("角色创建成功.", "创建角色");
        //            break;
        //    }
        //}
        //public void Process(S.DeleteCharacter p)
        //{
        //    SelectScene select = DXControl.ActiveScene as SelectScene;
        //    if (select == null) return;

        //    switch (p.Result)
        //    {
        //        case DeleteCharacterResult.Disabled:
        //            DXMessageBox.Show("删除角色被禁用.", "删除角色");
        //            break;
        //        case DeleteCharacterResult.AlreadyDeleted:
        //            DXMessageBox.Show("该角色已经被删除了.", "删除角色");
        //            break;
        //        case DeleteCharacterResult.NotFound:
        //            DXMessageBox.Show("角色没找到.", "删除角色");
        //            break;
        //        case DeleteCharacterResult.Success:
        //            for (int i = select.SelectBox.CharacterList.Count - 1; i >= 0; i--)
        //            {
        //                if (select.SelectBox.CharacterList[i].CharacterIndex != p.DeletedIndex) continue;

        //                select.SelectBox.CharacterList.RemoveAt(i);
        //                break;
        //            }
        //            select.SelectBox.UpdateCharacters();
        //            DXMessageBox.Show("角色删除成功.", "删除角色");
        //            break;
        //    }
        //}
        //public void Process(S.StartGame p)
        //{
        //    try
        //    {
                
        //    SelectScene select = DXControl.ActiveScene as SelectScene;
        //    if (select == null) return;

        //    select.SelectBox.StartGameAttempted = false;


        //    DXMessageBox box;
        //    DateTime expiry;
        //    switch (p.Result)
        //    {
        //        case StartGameResult.Disabled:
        //            DXMessageBox.Show("开始游戏功能被禁用.", "开始游戏");
        //            break;
        //        case StartGameResult.Deleted:
        //            DXMessageBox.Show("你不能用已删除的角色进入游戏.", "开始游戏");
        //            break;
        //        case StartGameResult.Delayed:
        //            expiry = CEnvir.Now.Add(p.Duration);

        //            box = DXMessageBox.Show($"该角色刚刚下线，稍候才能上线.\n" +
        //                                    $"等待: {Math.Floor(p.Duration.TotalHours):#,##0} 小时, {p.Duration.Minutes} 分, {p.Duration.Seconds} 秒", "开始游戏");

        //            box.ProcessAction = () =>
        //            {
        //                if (CEnvir.Now > expiry)
        //                {
        //                    if (select.SelectBox.CanStartGame)
        //                        select.SelectBox.StartGame();
        //                    box.ProcessAction = null;
        //                    return;
        //                }

        //                TimeSpan remaining = expiry - CEnvir.Now;

        //                box.Label.Text = $"该角色刚刚下线，稍候才能上线.\n" +
        //                                 $"等待: {Math.Floor(remaining.TotalHours):#,##0} 小时, {remaining.Minutes:#,##0} 分, {remaining.Seconds} 秒";
        //            };
        //            break;
        //        case StartGameResult.UnableToSpawn:
        //            DXMessageBox.Show("无法进入游戏，生成角色失败.", "开始游戏");
        //            break;
        //        case StartGameResult.NotFound:
        //            DXMessageBox.Show("无法进入游戏，该角色没找到.", "开始游戏");
        //            break;
        //        case StartGameResult.Success:
        //            select.Dispose();
        //            DXSoundManager.StopAllSounds();

        //            GameScene scene = new GameScene(Config.GameSize);
        //            DXControl.ActiveScene = scene;

        //            scene.MapControl.MapInfo = Globals.MapInfoList.Binding.FirstOrDefault(x => x.Index == p.StartInformation.MapIndex);
        //            GameScene.Game.QuestLog = p.StartInformation.Quests;

        //            GameScene.Game.NPCAdoptCompanionBox.AvailableCompanions = p.StartInformation.AvailableCompanions;
        //            GameScene.Game.NPCAdoptCompanionBox.RefreshUnlockButton();

        //            GameScene.Game.NPCCompanionStorageBox.Companions = p.StartInformation.Companions;
        //            GameScene.Game.NPCCompanionStorageBox.UpdateScrollBar();
                    
        //            GameScene.Game.Companion = GameScene.Game.NPCCompanionStorageBox.Companions.FirstOrDefault(x => x.Index == p.StartInformation.Companion);

        //            scene.User = new UserObject(p.StartInformation);

        //            GameScene.Game.BuffBox.BuffsChanged();
        //            GameScene.Game.RankingBox.Observable = p.StartInformation.Observable;

        //            GameScene.Game.StorageSize = p.StartInformation.StorageSize;

        //            if (!string.IsNullOrEmpty(p.Message)) DXMessageBox.Show(p.Message, "开始游戏");


        //            break;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}

        //public void Process(S.CheckClientDb p)
        //{
        //    if (!CEnvir.DbVersionChecking) return;

        //    if (p.IsUpgrading)
        //    {
        //        if (p.CurrentIndex == 0)
        //            DbUpgrade.Clear();

        //        DbUpgrade.AddRange(p.Datas);

        //        if (p.CurrentIndex >= (p.TotalCount - 1))
        //        {

        //            byte[] bytes = DbUpgrade.ToArray();
        //            DbUpgrade.Clear();
                    
        //            File.WriteAllBytes(@"./Data/System.db", bytes);
        //            CEnvir.DbVersionChecking = false;
        //            CEnvir.DbVersionChecked = true;
        //        }
        //    }
        //    else
        //    {
        //        DbUpgrade.Clear();
        //        CEnvir.DbVersionChecking = false;
        //        CEnvir.DbVersionChecked = true;
        //    }
        //}

    }
}

