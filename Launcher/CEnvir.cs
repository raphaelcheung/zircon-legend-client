using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Library;
using Library.Network;
using Library.SystemModels;
using MirDB;
using System.IO.IsolatedStorage;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;
using C = Library.Network.ClientPackets;
using S = Library.Network.ServerPackets;


namespace Client.Envir
{
    public static class CEnvir
    {
        public delegate void LogEventType(string msg);
        public delegate void StatusChangedType();
        public enum MainStepType
        {
            Ready = 0,
            Connecting,
            Connected,
            Upgrading,
            Upgraded,
            Logining,
            Logon,
            Stopping,
            Stop,
        }

        public static event LogEventType LogEvent;
        public static event StatusChangedType MainStepChanged;
        public static MainStepType MainStep
        {
            get => _MainStep;
            set
            {
                if (value != _MainStep)
                {
                    _MainStep = value;
                    MainStepChanged?.Invoke();
                }
            }
        }
        private static MainStepType _MainStep = MainStepType.Ready;
        public static DateTime Now { get; private set; } = DateTime.Now;

        private static TcpClient ConnectingClient { get; set; }
        public static CConnection Connection { get; private set; }

        private static ConcurrentQueue<string> LogQueue { get; } = new ConcurrentQueue<string>();

        private static bool DnsRefreshed { get; set; } = false;
        private static IPAddress IpServer { get; set; } = null;
        private static string CurrentUpgradeFileName { get; set; } = null;
        private static byte[] CurrentUpgradeDatas { get; set; } = null;
        private static Dictionary<string, string> ClientFileHash { get; } =  new Dictionary<string, string>();


        public static List<ClientBlockInfo> BlockList = new List<ClientBlockInfo>();

        private static bool LoadingDb = false;
        public static string RootPath { get; private set; }

        public static bool DbVersionChecked { get; set; } = false;
        public static bool DbVersionChecking { get; set; } = false;
        private static Queue<string> UpgradeQueue { get; } = new Queue<string>();

        [DllImport("dnsapi", EntryPoint = "DnsFlushResolverCache")]
        private static extern int DnsFlushResolverCache();

        static CEnvir()
        {
            //try
            //{ A(); }
            //catch { }

            LoadClientHash();

            Task.Run(() =>
            {
                while (MainStep < MainStepType.Stopping)
                {
                    Process();
                    Thread.Sleep(10);
                }

                OnStopping();
                MainStep = MainStepType.Stop;
            });
        }
        private static void LoadDirHash(DirectoryInfo di, string keyroot)
        {
            FileInfo[] files = di.GetFiles();
            byte[] datas;
            string key;
            foreach (var file in files)
            {
                key = Path.Combine(keyroot, file.Name).ToLower();
                datas = File.ReadAllBytes(file.FullName);
                ClientFileHash[key] = Functions.CalcMD5(datas);
            }

            DirectoryInfo[] directories = di.GetDirectories();
            foreach (var dir in directories)
            {
                LoadDirHash(dir, Path.Combine(keyroot, $"{dir.Name}/"));
            }
        }
        private static void LoadClientHash()
        {
            RootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string hash_file = Path.Combine(RootPath, @"./clientupgrade.hash");
            if (File.Exists(hash_file))
            {
                using (StreamReader sr = new StreamReader(hash_file))
                {
                    string line;
                    string[] parts;
                    while ((line = sr.ReadLine()) != null)
                    {
                        parts = line.Split('=');
                        if (parts.Length < 2) continue;

                        ClientFileHash[parts[0]] = parts[1];
                    }
                }

                //Log($"已读取读取客户端更新列表，共 {ClientFileHash.Count} 个文件");
            }
            else
            {
                //Log($"生成客户端更新列表 ...");
                DirectoryInfo di = new DirectoryInfo(RootPath);
                LoadDirHash(di, @"./");
                SaveHashFile(hash_file);
                //Log($"客户端更新列表已成功生成 {hash_file}，共 {ClientFileHash.Count} 个文件");
            }
        }
        private static void SaveHashFile(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename, false))
            {
                foreach (var item in ClientFileHash)
                {
                    sw.WriteLine($"{item.Key}={item.Value}");
                }
            }
        }
        private static void AttemptConnect(IPAddress ip)
        {
            ConnectingClient?.Close();
            ConnectingClient = new TcpClient(ip.AddressFamily);
            ConnectingClient.BeginConnect(ip, Config.Port, Connecting, ConnectingClient);
        }
        private static void ProcDnsConnect()
        {
            if (!DnsRefreshed && Config.DynamicServerIp)
            {
                DnsFlushResolverCache();
                DnsRefreshed = true;
            }

            var result = Dns.GetHostEntry(Config.IPAddress);


            foreach (var ip in result.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork
                    || ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    IpServer = ip;
                    break;
                }
            }

            AttemptConnect(IpServer);
        }
        public static void CheckUpgrade(Dictionary<string, string> server_list)
        {
            if (MainStep != MainStepType.Connected) return;
            foreach(var item in server_list)
            {
                if (ClientFileHash.TryGetValue(item.Key, out string hash) && hash == item.Value)
                    continue;

                UpgradeQueue.Enqueue(item.Key);
            }

            if (UpgradeQueue.Count > 0) MainStep = MainStepType.Upgrading;
            else MainStep = MainStepType.Upgraded;
        }
        public static void Connected()
        {
            if (MainStep != MainStepType.Connecting) return;

            MainStep = MainStepType.Connected;
        }

        private static void Process()
        {
            Now = DateTime.Now;

            while(LogQueue.TryDequeue(out string msg))
            {
                LogEvent?.Invoke(msg);
            }

            if (MainStep == MainStepType.Connecting && !(Connection?.ServerConnected ?? false))
            {
                if (Connection == null)
                    ProcDnsConnect();

                return;
            }

            if (MainStep >= MainStepType.Connected 
                && MainStep < MainStepType.Stopping 
                && !(Connection?.ServerConnected ?? false))
            {

                return;
            }

            if (MainStep == MainStepType.Upgrading)
            {
                if (string.IsNullOrEmpty(CurrentUpgradeFileName))
                {
                    string file_key = UpgradeQueue.Dequeue();
                    if (file_key == null)
                        MainStep = MainStepType.Upgraded;
                    else if (!string.IsNullOrEmpty(file_key))
                    {
                        CurrentUpgradeFileName = file_key;
                        CurrentUpgradeDatas = null;

                        Connection.Enqueue(new C.UpgradeClient()
                        {
                            FileKey = file_key,
                        });
                    }
                }
                return;
            }
        }

        public static void Upgrade(string file, int total_size, int index, byte[] datas)
        {
            if (file != CurrentUpgradeFileName) return;

            if (total_size <= 0)
            {
                Log($"更新 {CurrentUpgradeFileName} 时收到 0 大小的异常数据包，更新失败");
                CurrentUpgradeFileName = null;
                CurrentUpgradeDatas = null;
                Connection.TryDisconnect();

                return;
            }

            if (CurrentUpgradeDatas == null)
                CurrentUpgradeDatas = new byte[total_size];

            try
            {
                datas.CopyTo(CurrentUpgradeDatas, index);

                if ((index + datas.Length) >= total_size)
                {
                    string path = Path.Combine(RootPath, CurrentUpgradeFileName);
                    File.WriteAllBytes(path, CurrentUpgradeDatas);

                    Log($"更新成功 {path}，文件大小 {CurrentUpgradeDatas.Length}");

                    CurrentUpgradeFileName = null;
                    CurrentUpgradeDatas = null;
                }
            }
            catch (Exception ex)
            {
                Log($"更新异常 {CurrentUpgradeFileName}");
                Log(ex.Message);
                Log(ex.StackTrace);
                CurrentUpgradeFileName = null;
                CurrentUpgradeDatas = null;
                Connection.TryDisconnect();
            }
        }

        public static void Connect()
        {
            if (MainStep != MainStepType.Ready) return;

            MainStep = MainStepType.Connecting;
        }
        public static void Stop()
        {
            if (MainStep >= MainStepType.Stopping) return;

            MainStep = MainStepType.Stopping;
        }
        public static void Disconnect()
        {
            Connection = null;
            UpgradeQueue.Clear();
            CurrentUpgradeFileName = null;
            CurrentUpgradeDatas = null;
            ConnectingClient = null;
            ClientFileHash.Clear();

            MainStep = MainStepType.Ready;
        }
        private static void OnStopping()
        {
            Connection?.TryDisconnect();
            Disconnect();
        }
        private static void Connecting(IAsyncResult result)
        {
            try
            {
                TcpClient client = (TcpClient)result.AsyncState;
                client.EndConnect(result);

                if (!client.Connected) return;

                if (client != ConnectingClient)
                {
                    ConnectingClient = null;
                    client.Close();
                    return;
                }

                //ConnectionTime = Now.AddSeconds(5); //Add 5 more seconds to timeout for delayed HandShake
                ConnectingClient = null;

                Connection = new CConnection(client);
            }
            catch { }
        }

        //private static void A()
        //{
        //    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Zircon");

        //    if (!Directory.Exists(path))
        //        Directory.CreateDirectory(path);

        //    if (File.Exists(path + "\\CheckSum.bin"))
        //    {
        //        using (BinaryReader E = new BinaryReader(File.OpenRead(path + "\\CheckSum.bin")))
        //            C = E.ReadString();
        //    }
        //    else
        //    {
        //        using (BinaryWriter E = new BinaryWriter(File.Create(path + "\\CheckSum.bin")))
        //            E.Write(C = Functions.RandomString(Random, 20));
        //    }

        //}

        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            LogQueue.Enqueue(message);
        }
    }
}
