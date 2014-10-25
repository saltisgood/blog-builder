using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogBuilder
{
    class ConsoleOptions
    {
        public static void OutputOptionsHelp()
        {
            Console.WriteLine("Usage Instructions:");
            Console.WriteLine("BlogBuilder.exe -if InputFilePath {-of OutputFilePath} {-deploy DeploymentRootPath}");
            Console.WriteLine("BlogBuilder.exe {h|gen} {-of OutputFilePath}");
            Console.WriteLine();

            Console.WriteLine("Command Line Switches:");
            Console.WriteLine("h|elp : Display this help message");
            Console.WriteLine("gen : Output a template file");
            Console.WriteLine("path : Set the deployment path");
            Console.WriteLine();

            Console.WriteLine("Command Line Options:");
            Console.WriteLine("-of OutputFilePath : Set the file path to write to");
            Console.WriteLine("-if InputFilePath : Set the file path to read from");
        }

        public bool GenerateTemplate
        {
            get;
            private set;
        }

        public String InputFile
        {
            get;
            private set;
        }

        public bool HasInFile
        {
            get
            {
                return !String.IsNullOrEmpty(InputFile);
            }
        }

        public String OutputFile
        {
            get;
            private set;
        }

        public bool HasOutFile
        {
            get
            {
                return !String.IsNullOrEmpty(OutputFile);
            }
        }

        public bool Help
        {
            get;
            private set;
        }

        public String DeployPath
        {
            get;
            private set;
        }

        public bool SetDeployPath
        {
            get
            {
                return !String.IsNullOrEmpty(DeployPath);
            }
        }

        public ConsoleOptions(string[] args)
        {
            GenerateTemplate = false;
            InputFile = null;
            OutputFile = null;
            Help = false;
            DeployPath = null;

            for (int i = 0; i < args.Length; ++i)
            {
                if (String.Compare(args[i], "gen", true) == 0)
                {
                    GenerateTemplate = true;
                }
                else if (String.Compare(args[i], "-of", true) == 0)
                {
                    if ((i + 1) < args.Length)
                    {
                        OutputFile = args[i + 1];
                        ++i;
                    }
                }
                else if (String.Compare(args[i], "h", true) == 0 || String.Compare(args[i], "help", true) == 0)
                {
                    Help = true;
                }
                else if (String.Compare(args[i], "-if", true) == 0)
                {
                    if ((i + 1) < args.Length)
                    {
                        InputFile = args[i + 1];
                        ++i;
                    }
                }
                else if (String.Compare(args[i], "-path", true) == 0)
                {
                    if ((i + 1) < args.Length)
                    {
                        DeployPath = args[i + 1];
                        ++i;
                    }
                }
            }
        }
    }
}
