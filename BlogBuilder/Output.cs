using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogBuilder
{
    class Output
    {
        public static void Warning(String str, int code = 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARNING");
            if (code != 0)
            {
                Console.Write(" {0:X}", code);
            }
            Console.Write(": ");

            Console.ResetColor();
            Console.WriteLine(str);
        }

        public static void Error(String str, int code = 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR");
            if (code != 0)
            {
                Console.Write(" {0:X}", code);
            }
            Console.Write(": ");

            Console.ResetColor();
            Console.WriteLine(str);
        }
    }
}
