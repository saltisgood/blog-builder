using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BlogBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var ops = new ConsoleOptions(args);

            if (ops.GenerateTemplate)
            {
                if (ops.HasOutFile)
                {
                    Template.OutputToFile(ops.OutputFile);
                }
                else
                {
                    Template.OutputToConsole();
                }

                return;
            }
            else if (ops.Help || !ops.HasInFile)
            {
                ConsoleOptions.OutputOptionsHelp();
                return;
            }

            Parser parser = new Parser();
            
            using (var fs = File.OpenRead(ops.InputFile))
            {
                using (var sr = new StreamReader(fs))
                {
                    parser.Parse(sr);
                }
            }

            if (ops.HasOutFile)
            {
                parser.WriteToFile(ops.OutputFile);
            }
            else
            {
                parser.WriteToConsole();
            }
        }
    }
}
