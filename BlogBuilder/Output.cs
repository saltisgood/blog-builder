using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogBuilder
{
    class Output
    {
        public static bool PermissionMessage(String str = null)
        {
            if (str != null)
            {
                Warning(str);
            }

            Console.WriteLine("Enter y to proceed, n to cancel");

            String response;
            while ((response = Console.ReadLine()) != null)
            {
                if (String.Compare(response, "y", true) == 0)
                {
                    return true;
                }
                else if (String.Compare(response, "n", true) == 0)
                {
                    return false;
                }
            }

            return false;
        }

        public static void Message(String str)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write("MESSAGE: ");

            Console.ResetColor();
            Console.WriteLine(str);
        }

        public static void Warning(String str, int code = 0)
        {
            Console.BackgroundColor = ConsoleColor.Black;
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
            Console.BackgroundColor = ConsoleColor.Black;
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
