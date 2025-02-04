﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.Scenes;
using Library;
using SlimDX.Windows;

namespace Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            foreach (KeyValuePair<LibraryFile, string> pair in Libraries.LibraryList)
            {
                if (!File.Exists(@".\" + pair.Value)) continue;

                CEnvir.LibraryList[pair.Key] = new MirLibrary(@".\" + pair.Value);
            }


            ConfigReader.Load();

            CEnvir.SaveError($"命令行：{string.Join(" ", args)}");

            //解析命令行参数
            if (args.Length > 1)
            {
                string[] parts;

                foreach(var arg in args)
                {
                    if (string.IsNullOrEmpty(arg) || arg[0] != '-') continue;

                    parts = arg.Split(':');

                    switch(parts[0])
                    {
                        case "-QuickGame":
                            CEnvir.IsQuickGame = true;
                            break;
                        case "-IPAddress":
                            if (parts.Length > 1)
                            {
                                if (parts.Length > 2)
                                {
                                    string[] tmp = parts.Skip(1).ToArray();
                                    Config.IPAddress = string.Join(":", tmp);
                                }
                                else
                                    Config.IPAddress = parts[1];

                            }
                            break;
                        case "-Port":
                            if (parts.Length > 1 && int.TryParse(parts[1], out int port))
                                Config.Port = port;
                            break;
                        case "-FullScreen":
                            if (parts.Length > 1 && bool.TryParse(parts[1], out bool fs))
                                Config.FullScreen = fs;
                            break;
                        case "-GameSize":
                            if (parts.Length > 1)
                            {
                                string[] tmps = parts[1].Split('x');
                                if (tmps.Length != 2 || !int.TryParse(tmps[0], out int width) || !int.TryParse(tmps[1], out int height))
                                    continue;

                                Config.GameSize = new Size(width, height);
                            }
                            break;
                        case "-Account":
                            if (parts.Length < 2) continue;
                            Config.RememberedEMail = parts[1];

                            break;
                        case "-Remember":
                            if (parts.Length < 2 || !bool.TryParse(parts[1], out bool r)) continue;
                            Config.RememberDetails = r;
                            break;
                        case "-Password":
                            if (parts.Length < 2) continue;

                            Config.RememberedPassword = parts[1];
                            break;
                        case "-SelectChar":
                            if (parts.Length < 2 || !int.TryParse(parts[1], out int index)) continue;
                            CEnvir.QuickSelectCharacter = index;
                            
                            break;
                        case "-LauncherHash":
                            if (parts.Length < 2) continue;
                            CEnvir.LauncherHash = parts[1];
                            break;
                    }
                }
            }

            CEnvir.Target = new TargetForm();
            CEnvir.Target.ClientSize = Config.GameSize;
            CEnvir.Target.Location = new Point(0, 0);
            DXManager.Create();
            DXSoundManager.Create();
            
            DXControl.ActiveScene = new LoginScene(Config.IntroSceneSize);

            try { MessagePump.Run(CEnvir.Target, CEnvir.GameLoop); }
            catch(Exception ex)
            {
                CEnvir.SaveError(ex.Message);
                CEnvir.SaveError(ex.StackTrace);
            }

            DXControl.ActiveScene?.Dispose();

            if (!Config.RememberDetails) Config.RememberedPassword = "";
            ConfigReader.Save();

            CEnvir.Session?.Save(true);
            CEnvir.Unload();
            DXManager.Unload();
            DXSoundManager.Unload();
        }
    }
}
