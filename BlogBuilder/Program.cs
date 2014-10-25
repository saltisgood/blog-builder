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
        static void ChangeDeployPath(String newPath)
        {
            User.Default.DeployPath = newPath;
            User.Default.Save();
        }

        static bool CheckDeployPath()
        {
            var deployPath = User.Default.DeployPath;

            if (String.IsNullOrEmpty(deployPath))
            {
                Output.Message("No deploy path entered! Please enter root folder location!");

            start:
                var rootPath = Console.ReadLine();

                while (!Directory.Exists(rootPath))
                {
                    Output.Error("Given path does not exist! Try again");
                    rootPath = Console.ReadLine();
                }

                if (!Directory.Exists(Path.Combine(rootPath, "public_html" + Path.DirectorySeparatorChar)) && 
                    !Output.PermissionMessage("Are you sure? public_html folder not found in given path..."))
                {
                    goto start;
                }

                ChangeDeployPath(rootPath);
            }

            return true;
        }

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

            if (ops.SetDeployPath)
            {
                ChangeDeployPath(ops.DeployPath);
            }

            if (!CheckDeployPath())
            {
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
