using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BlogBuilder
{
    class Template
    {
        public static void OutputToFile(String outFile)
        {
            using (var fs = File.OpenWrite(outFile))
            {
                OutputToStream(fs);
            }
        }

        public static void OutputToConsole()
        {
            using (var stream = Console.OpenStandardOutput())
            {
                OutputToStream(stream);
            }
        }

        private static void OutputToStream(Stream outStream)
        {
            using (StreamWriter sw = new StreamWriter(outStream))
            {
                sw.WriteLine("TITLE:title");
                sw.WriteLine("SUBTITLE:subtitle|null");
                sw.WriteLine("DESC:description");
                sw.WriteLine("TAGS:tag1;tag2;");
                sw.WriteLine("CONTENT:\nblah di blah\n");

                sw.WriteLine("Language:");
                sw.WriteLine("{tag} - custom tag start");
                sw.WriteLine("{/tag} - custom tag end");
                sw.WriteLine("<tag> - inline HTML tag start");
                sw.WriteLine("</tag> - inline HTML tag end");
                sw.WriteLine();

                sw.WriteLine("Custom Tags:");
                sw.WriteLine("head - Section Header");
                sw.WriteLine("body - Section Body");
                sw.WriteLine("subhead - Subsection Header");
                sw.WriteLine("code - Code Block");
                sw.WriteLine();

                sw.WriteLine("Extra Notes:");
                sw.WriteLine("In normal situations, special characters can be escaped with \\ character. Inside code blocks, the situation is flipped; " +
                    "the special characters are taken verbatim and need to be un-escaped with \\.");
                sw.WriteLine("Code:");
                sw.WriteLine("Inside the code block, the head tag is supported but must come first. Body is optional.");
                sw.WriteLine("Every line inside the block is considered a different line in the code output.");
            }
        }
    }
}
