using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library;

namespace Client.Envir
{
    [ConfigPath(@".\Laucher.ini")]
    public static class Config
    {
        [ConfigSection("Network")]
        public static bool DynamicServerIp { get; set; } = true;
        public static string IPAddress { get; set; } = "127.0.0.1";
        public static int Port { get; set; } = 7000;
        public static TimeSpan TimeOutDuration { get; set; } = TimeSpan.FromSeconds(15);


        [ConfigSection("Graphics")]
        public static bool FullScreen { get; set; } = true;
        public static Size GameSize { get; set; } = new Size(800, 600);

        [ConfigSection("Login")]
        public static bool Remember { get; set; } = false;
        public static string Account { get; set; } = string.Empty;
        public static string Password { get; set; } = string.Empty;
    }
}
