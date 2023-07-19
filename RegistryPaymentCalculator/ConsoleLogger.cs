using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RegistryPaymentCalculator
{
    internal class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
            var stream = File.AppendText("log.txt");
            stream.WriteLine(string.Format("[Info][{0}]: ", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss")) + message);
            stream.Close();
        }

        public static void Error(string message)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = fg;
            var stream = File.AppendText("log.txt");
            stream.WriteLine(string.Format("[Error][{0}]: ", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss")) + message);
            stream.Close();
        }
    }
}
