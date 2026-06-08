using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer
{
    internal static class Extensions
    {
        public static string DBugText(this string str)
        {
            return "[Mod Multiplayer Log] " + str;
        }
        public static byte[] ToByteArray(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        public static string ToUTF8String(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        public static byte[] ToByteArray(this int num)
        {
            return BitConverter.GetBytes(num);
        }
        public static int ToInt32(this byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToInt32(bytes, startIndex);
        }
        public static byte[] ToByteArray(this uint num)
        {
            return BitConverter.GetBytes(num);
        }
        public static uint ToUInt32(this byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToUInt32(bytes, startIndex);
        }
        public static byte[] ToByteArray(this float num)
        {
            return BitConverter.GetBytes(num);
        }
    }
}
