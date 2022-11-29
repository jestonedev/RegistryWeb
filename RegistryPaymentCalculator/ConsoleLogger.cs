using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentCalculator
{
    internal class ConsoleLogger
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = fg;
        }
    }
}
