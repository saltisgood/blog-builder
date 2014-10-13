using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogBuilder
{
    class Code
    {
        public enum Language
        {
            NONE, CPP
        }

        public static Language FindLanguage(String str)
        {
            if (String.Compare(str, "CPP", true) == 0)
            {
                return Language.CPP;
            }

            return Language.NONE;
        }

        public static String Parse(String str, Language lang)
        {
            switch (lang)
            {
                case Language.CPP:
                default:
                    return CPP.Parse(str);
            }
        }

        private class CPP
        {
            public static String Parse(String inStr)
            {
                StringBuilder outStr = new StringBuilder();

                {
                    int lastTab = -1;
                    int currTab = -1;
                    int count = 0;

                    while ((currTab = inStr.IndexOf('\t', lastTab + 1)) != -1)
                    {
                        lastTab = currTab;
                        ++count;
                    }

                    if (count != 0)
                    {
                        outStr.Append('\t', count);

                        inStr = inStr.Substring(lastTab + 1);
                    }
                }

                var toks = inStr.Split(' ', '\t');
                bool inComment = false;

                for (int i = 0; i < toks.Length; ++i)
                {
                    if (String.IsNullOrEmpty(toks[i]))
                    {
                        outStr.Append("&nbsp;");
                        continue;
                    }

                    if (inComment)
                    {
                        outStr.AppendFormat("{0} ", toks[i]);
                        continue;
                    }

                    if (toks[i].StartsWith("//"))
                    {
                        inComment = true;
                        outStr.AppendFormat("\\<span class=\"code-g\"\\>{0} ", toks[i]);
                        continue;
                    }

                    {
                        int index;
                        while ((index = toks[i].IndexOf('<')) != -1)
                        {
                            Interpret(outStr, toks[i].Substring(0, index), false);
                            outStr.Append('<');
                            toks[i] = toks[i].Substring(index + 1);
                        }

                        while ((index = toks[i].IndexOf('>')) != -1)
                        {
                            Interpret(outStr, toks[i].Substring(0, index), false);
                            outStr.Append('>');
                            toks[i] = toks[i].Substring(index + 1);
                        }

                        while ((index = toks[i].IndexOf('(')) != -1)
                        {
                            Interpret(outStr, toks[i].Substring(0, index), false);
                            outStr.Append('(');
                            toks[i] = toks[i].Substring(index + 1);
                        }

                        while ((index = toks[i].IndexOf(')')) != -1)
                        {
                            Interpret(outStr, toks[i].Substring(0, index), false);
                            outStr.Append(')');
                            toks[i] = toks[i].Substring(index + 1);
                        }

                        while ((index = toks[i].IndexOf(',')) != -1)
                        {
                            Interpret(outStr, toks[i].Substring(0, index), false);
                            outStr.Append(", ");
                            toks[i] = toks[i].Substring(index + 1);
                        }
                    }

                    String[] cpy = new String[toks.Length - i - 1];
                    Array.Copy(toks, i + 1, cpy, 0, cpy.Length);

                    i += Interpret(outStr, toks[i], strs: cpy);
                }

                if (inComment)
                {
                    outStr.Append("\\</span\\>");
                }

                return outStr.ToString();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="outStr"></param>
            /// <param name="str"></param>
            /// <param name="str2"></param>
            /// <returns>The number of extra strings consumed</returns>
            private static int Interpret(StringBuilder outStr, String str, bool inclSpace = true, params String[] strs)
            {
                if (String.IsNullOrEmpty(str))
                {
                    return 0;
                }

                var suffix = inclSpace ? " " : String.Empty;

                if (str.StartsWith("public"))
                {
                    if (str.Length == 6)
                    {
                        outStr.Append("\\<span class=\"code-b\"\\>public\\</span\\> ");
                        return 0;
                    }
                    else if (str.Length == 7 && str[6] == ':')
                    {
                        outStr.Append("\\<span class=\"code-b\"\\>public\\</span\\>: ");
                        return 0;
                    }
                }

                switch (str)
                {
                    case "#pragma":
                        if (strs.Length != 0)
                        {
                            outStr.AppendFormat("\\<span class=\"code-b\"\\>#pragma {0}\\</span\\> ", strs[0]);
                            return 1;
                        }
                        else
                        {
                            outStr.Append("\\<span class=\"code-b\"\\>#pragma\\</span\\> ");
                        }
                        break;
                    case "#include":
                        outStr.AppendFormat("\\<span class=\"code-b\"\\>#include\\</span\\> \\<span class=\"code-r\"\\>{0}\\</span\\> ", strs[0]);
                        return 1;
                    case "namespace":
                    case "typename":
                        if (strs.Length != 0)
                        {
                            outStr.AppendFormat("\\<span class=\"code-b\"\\>{0}\\</span\\> {1} ", str, strs[0]);
                            return 1;
                        }
                        else
                        {
                            outStr.AppendFormat("\\<span class=\"code-b\"\\>{0}\\</span\\> ", str);
                        }
                        break;
                    case "class":
                    case "template":
                    case "const":
                    case "static":
                    case "using":
                        outStr.AppendFormat("\\<span class=\"code-b\"\\>{0}\\</span\\> ", str);
                        break;
                    case "int":
                    case "bool":
                    case "float":
                    case "char":
                        outStr.AppendFormat("\\<span class=\"code-b\"\\>{0}\\</span\\>{1}", str, suffix);
                        break;
                    default:
                        outStr.AppendFormat("{0}{1}", str, suffix);
                        break;
                }

                return 0;
            }
        }
    }

}
