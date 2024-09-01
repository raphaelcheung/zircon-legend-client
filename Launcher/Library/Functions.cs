using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Library
{
    public static class Functions
    {
        public static TimeSpan Max(TimeSpan value1, TimeSpan value2)
        {
            return value1 > value2 ? value1 : value2;
        }
        public static TimeSpan Min(TimeSpan value1, TimeSpan value2)
        {
            return value1 < value2 ? value1 : value2;
        }

        public static string CalcMD5(byte[] bytes)
        {
            MD5 calc = MD5.Create();
            byte[] datas = calc.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < datas.Length; i++)
            {
                sb.Append(datas[i].ToString("x2"));
            }

            return sb.ToString();
        }
        public static string BytesToString(long byteCount)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = Convert.ToDouble(byteCount);
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

        public static string GetEnumDesc<T>(T e) where T : Enum
        {
            var type = e.GetType();
            var name = Enum.GetName(type, e);
            if (string.IsNullOrEmpty(name)) return null;

            var field = type.GetField(name);
            if (field == null) return null;

            DescriptionAttribute desc = field.GetCustomAttribute<DescriptionAttribute>();
            if (desc == null) return null;

            return desc.Description;
        }

        public static string CalcMD5(string src)
        {
            return CalcMD5(System.Text.Encoding.UTF8.GetBytes(src));
        }

        public static string ToString(TimeSpan time, bool details, bool small = false)
        {
            string textD = null;
            string textH = null;
            string textM = null;
            string textS = null;

            if (time.Days >= 1) textD = $"{time.Days} {(small ? "D" : "天")}";

            if (time.Hours >= 1) textH = $"{time.Hours} {(small ? "H" : "小时")}";

            if (time.Minutes >= 1) textM = $"{time.Minutes} {(small ? "M" : "分")}";

            if (time.Seconds >= 1) textS = $"{ time.Seconds} {(small ? "S" : "秒")}";
            else if (time.TotalSeconds > 1 && time.Seconds > 0) textS = "少于一秒";

            if (!details)
                return textD ?? textH ?? textM ?? textS;

            if (textD != null)
                return textD + " " + (textH ?? textM ?? textS);

            if (textH != null)
                return textH + " " + (textM ?? textS);

            if (textM != null)
                return textM + " " + textS;

            return textS ?? string.Empty;
        }

        public static string RandomString(Random Random, int length)
        {
            StringBuilder str = new StringBuilder();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";

            for (int i = 0; i < length; i++)
                str.Append(chars[Random.Next(chars.Length)]);

            return str.ToString();
        }
    }
}
