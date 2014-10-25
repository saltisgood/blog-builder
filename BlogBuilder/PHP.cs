using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BlogBuilder
{
    class PHP
    {
        public const String QUOTE = "\\'";
        public const String TAB = "&nbsp;&nbsp;&nbsp;&nbsp;";
        public const String LEFT_ARROW = "&lt;";
        public const String RIGHT_ARROW = "&gt;";

        public enum FileType
        {
            IMAGE
        }

        public static String SanitiseFilename(String path, FileType type)
        {
            switch (type)
            {
                case FileType.IMAGE:
                    return "/blog/img/" + new Random().Next(0xFFFFFF).ToString("X") + "-" + Path.GetFileName(path).ToLowerInvariant().Replace(' ', '-');
                default:
                    throw new ArgumentException();
            }
        }

        public static String SanitiseString(String str, bool convertArrows)
        {
            var rt = new StringBuilder();

            for (int i = 0; i < str.Length; ++i)
            {
                var c = str[i];

                if (convertArrows)
                {
                    if (c == '\\')
                    {
                        if (str[i + 1] == '<')
                        {
                            rt.Append('<');
                            ++i;
                            continue;
                        }
                        else if (str[i + 1] == '>')
                        {
                            rt.Append('>');
                            ++i;
                            continue;
                        }
                    }
                    else if (c == '<')
                    {
                        rt.Append(LEFT_ARROW);
                        continue;
                    }
                    else if (c == '>')
                    {
                        rt.Append(RIGHT_ARROW);
                        continue;
                    }
                }

                switch (c)
                {
                    case '\'':
                        rt.Append(QUOTE);
                        continue;
                    case '\t':
                        rt.Append(TAB);
                        continue;
                    default:
                        rt.Append(c);
                        continue;
                }
            }

            return rt.ToString();
        }

        public static void SanitiseWriteLine(StreamWriter output, String formatStr, bool convertArrows, params String[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                args[i] = SanitiseString(args[i], convertArrows);
            }

            output.WriteLine(formatStr, args);
        }
    }
}
