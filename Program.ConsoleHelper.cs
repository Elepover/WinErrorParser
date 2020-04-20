using System;

namespace ConsoleApp_NetCore
{
    public partial class Program
    {
        public static void Out(string message)
        {
            Console.Write(message);
        }
        public static void Out(string format, params string[] arg)
        {
            Console.Write(format, arg);
        }
        public static void OutLine(string message)
        {
            Console.WriteLine(message);
        }
        public static void OutLine(string format, params string[] arg)
        {
            Console.WriteLine(format, arg);
        }
        public static string In()
        {
            return Console.ReadLine();
        }
        public static void Animate(string type, string code, int hResult)
        {
            Out($"\r{type, -10}{code.Substring(0, code.Length > 50 ? 50 : code.Length), -50}0x{hResult:x}");
        }
    }
}
