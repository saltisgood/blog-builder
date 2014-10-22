using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogBuilder
{
    class Code
    {
        public static int WARNING_CODE = 0x100;
        public static int ERROR_CODE = 0x100;

        public enum Language
        {
            NONE, CPP
        }

        public interface IParser
        {
            String InterpretLine(String line);
        }

        public static Language FindLanguage(String str)
        {
            if (String.Compare(str, "CPP", true) == 0)
            {
                return Language.CPP;
            }

            return Language.NONE;
        }

        public static IParser BeginParse(Language lang)
        {
            switch (lang)
            {
                case Language.CPP:
                default:
                    return CPP.RetrieveParser();
            }
        }

        private static int CountTabs(String str)
        {
            return str.Count(c => (c == '\t')); // Yergh
        }

        private class CPP
        {
            public static int WARNING_CODE = 0x10;
            public static int ERROR_CODE = 0x10;

            private enum Type
            {
                NONE, NAMESPACE, TEMPLATE, TYPENAME, IDENTIFIER, CLASS, VISIBILITY, FUNCTION, QUALIFIER, USING
            }

            private static void AppendCodeWord(StringBuilder stream, String word, bool leadingSpace = false, bool trailingSpace = true)
            {
                if (leadingSpace)
                {
                    stream.Append(' ');
                }

                stream.AppendFormat("\\<span class=\"code-b\"\\>{0}\\</span\\>", word);

                if (trailingSpace)
                {
                    stream.Append(' ');
                }
            }

            private static void AppendVisibility(StringBuilder stream, String word)
            {
                stream.AppendFormat("\\<span class=\"code-b\"\\>{0}\\</span\\>: ", word);
            }

            private static void AppendPlain(StringBuilder stream, String word, bool leadingSpace = false, bool trailingSpace = true)
            {
                if (leadingSpace)
                {
                    stream.Append(' ');
                }

                stream.Append(word);

                if (trailingSpace)
                {
                    stream.Append(' ');
                }
            }

            private static void AppendIdentifier(StringBuilder stream, String word, bool leadingSpace = false, bool trailingSpace = true)
            {
                if (leadingSpace)
                {
                    stream.Append(' ');
                }

                stream.AppendFormat("\\<span class=\"code-lb\"\\>{0}\\</span\\>", word);

                if (trailingSpace)
                {
                    stream.Append(' ');
                }
            }

            private static void AppendPreprocessor(StringBuilder stream, String word)
            {
                stream.AppendFormat("\\<span class=\"code-r\"\\>{0}\\</span\\>", word);
            }

            private static void AppendComments(StringBuilder stream, params String[] words)
            {
                stream.Append("\\<span class=\"code-g\"\\>");
                foreach (var word in words)
                {
                    stream.AppendFormat("{0} ", word);
                }
                stream.Append("\\</span\\>");
            }

            private static void AppendUnknown(StringBuilder stream, String word)
            {
                stream.AppendFormat("{0} ", word);
            }

            /// <summary>
            /// Check whether a word is a type qualifier, e.g. const
            /// </summary>
            /// <param name="word"></param>
            /// <returns></returns>
            public static bool IsTypeQualifier(String word)
            {
                switch (word)
                {
                    case "const":
                        return true;

                    default:
                        return false;
                }
            }

            public static bool IsPrimitive(String word)
            {
                switch (word)
                {
                    case "bool":
                    case "int":
                    case "float":
                        return true;
                    default:
                        return false;
                }
            }

            private class CPPState : IParser
            {
                private Type mLastToken = Type.NONE;
                private int mTemplateDepth = 0;
                private bool mInFunctionBrackets = false;
                private bool mInPragma = false;
                private bool mInLineComments = false;
                private bool mInBlockComments = false;

                private void Reset()
                {
                    mLastToken = Type.NONE;
                    mTemplateDepth = 0;
                    mInFunctionBrackets = false;
                    mInPragma = false;
                    mInLineComments = false;
                    // Purposely don't reset in block comments!
                }

                public String InterpretLine(String line)
                {
                    if (String.IsNullOrEmpty(line))
                    {
                        return "";
                    }

                    Reset();

                    StringBuilder outStr = new StringBuilder();

                    var toks = line.Split(' ');

                    int startIndex = CountTabs(toks[0]);

                    if (startIndex > 0)
                    {
                        outStr.Append('\t', startIndex);
                        toks[0] = toks[0].TrimStart();

                        startIndex = String.IsNullOrEmpty(toks[0]) ? 1 : 0;
                    }

                    if (startIndex >= toks.Length)
                    {
                        return outStr.ToString();
                    }

                    if (toks[startIndex].CompareTo("namespace") == 0)
                    {
                        AppendCodeWord(outStr, "namespace", trailingSpace: false);

                        if ((startIndex + 1) < toks.Length)
                        {
                            AppendPlain(outStr, toks[startIndex + 1], true, false);
                            ++startIndex;
                        }

                        ++startIndex;
                    }
                    else if (toks[startIndex].CompareTo("#include") == 0)
                    {
                        AppendCodeWord(outStr, "#include");

                        if ((startIndex + 1) < toks.Length)
                        {
                            AppendPreprocessor(outStr, toks[startIndex + 1]);
                            ++startIndex;
                        }
                        
                        ++startIndex;
                    }
                    else if (toks[startIndex].CompareTo("#pragma") == 0)
                    {
                        AppendCodeWord(outStr, "#pragma");

                        if ((startIndex + 1) < toks.Length)
                        {
                            ++startIndex;
                            AppendCodeWord(outStr, toks[startIndex]);
                        }

                        ++startIndex;
                        mInPragma = true;
                    }

                    for (; startIndex < toks.Length; ++startIndex)
                    {
                        if (mInPragma)
                        {
                            AppendPlain(outStr, toks[startIndex]);
                            continue;
                        }

                        if (mInLineComments || toks[startIndex].StartsWith("//"))
                        {
                            mInLineComments = true;

                            AppendComments(outStr, toks[startIndex]);
                            continue;
                        }

                        if (toks[startIndex].StartsWith("::"))
                        {
                            --outStr.Length;
                            AppendPlain(outStr, toks[startIndex], trailingSpace: false);
                            mLastToken = Type.IDENTIFIER;
                            
                            if (toks[startIndex].EndsWith("("))
                            {
                                mLastToken = Type.FUNCTION;
                                mInFunctionBrackets = true;
                            }
                            continue;
                        }
                        
                        if (mLastToken == Type.TYPENAME || mLastToken == Type.CLASS || mLastToken == Type.QUALIFIER ||
                            (mLastToken == Type.FUNCTION && mInFunctionBrackets))
                        {
                            if (IsPrimitive(toks[startIndex]))
                            {
                                AppendCodeWord(outStr, toks[startIndex], trailingSpace: false);
                            }
                            else if (IsTypeQualifier(toks[startIndex]))
                            {
                                AppendCodeWord(outStr, toks[startIndex]);
                                mLastToken = Type.QUALIFIER;
                                continue;
                            }
                            else
                            {
                                AppendIdentifier(outStr, toks[startIndex], trailingSpace: false);
                            }

                            mLastToken = Type.IDENTIFIER;
                        }
                        else if (toks[startIndex][0] == '<')
                        {
                            if (mLastToken == Type.TEMPLATE)
                            {
                                outStr.Append('<');
                            }
                            else if ((mLastToken == Type.IDENTIFIER || mLastToken == Type.NONE))
                            {
                                if (outStr[outStr.Length - 1] == ' ')
                                {
                                    --outStr.Length;
                                }

                                if (mTemplateDepth == 0)
                                {
                                    mTemplateDepth = 1;
                                }

                                mLastToken = Type.TYPENAME;
                                outStr.Append('<');
                            }
                            else
                            {
                                outStr.Append("< ");
                            }   
                        }
                        else if (toks[startIndex][0] == '>')
                        {
                            if (mTemplateDepth > 0)
                            {
                                --mTemplateDepth;

                                if (outStr[outStr.Length - 1] == ' ')
                                {
                                    --outStr.Length;
                                }
                            }

                            outStr.Append("> ");
                        }
                        else if (toks[startIndex][0] == ',')
                        {
                            outStr.Append(", ");

                            if (mTemplateDepth != 0)
                            {
                                mLastToken = Type.TEMPLATE;
                            }
                            else if (mInFunctionBrackets)
                            {
                                mLastToken = Type.FUNCTION;
                            }
                        }
                        else if (mLastToken == Type.IDENTIFIER && mInFunctionBrackets)
                        {
                            if (toks[startIndex].IndexOf(')') == -1)
                            {
                                AppendPlain(outStr, toks[startIndex], true, false);
                            }
                            else
                            {
                                AppendPlain(outStr, toks[startIndex], trailingSpace: false);
                            }
                            
                            mLastToken = Type.NONE;
                        }
                        else if (toks[startIndex].CompareTo("namespace") == 0)
                        {
                            AppendCodeWord(outStr, "namespace", trailingSpace: false);

                            if ((startIndex + 1) < toks.Length)
                            {
                                AppendPlain(outStr, toks[startIndex + 1], true, false);
                                ++startIndex;
                            }
                        }
                        else if (toks[startIndex].CompareTo("template") == 0)
                        {
                            AppendCodeWord(outStr, "template");
                            mLastToken = Type.TEMPLATE;
                            ++mTemplateDepth;
                        }
                        else if (toks[startIndex].CompareTo("typename") == 0)
                        {
                            AppendCodeWord(outStr, "typename");
                            mLastToken = Type.TYPENAME;
                        }
                        else if (toks[startIndex].CompareTo("class") == 0)
                        {
                            AppendCodeWord(outStr, "class");
                            mLastToken = Type.CLASS;
                        }
                        else if (toks[startIndex].StartsWith("public"))
                        {
                            AppendCodeWord(outStr, "public:");
                            mLastToken = Type.VISIBILITY;
                        }
                        else if (toks[startIndex].CompareTo("using") == 0)
                        {
                            AppendCodeWord(outStr, "using");
                            mLastToken = Type.USING;
                        }
                        else if (toks[startIndex].EndsWith("("))
                        {
                            if (mLastToken == Type.IDENTIFIER && (outStr[outStr.Length - 1] == ' '))
                            {
                                --outStr.Length;
                            }

                            AppendPlain(outStr, toks[startIndex], trailingSpace: false);
                            mLastToken = Type.FUNCTION;
                            mInFunctionBrackets = true;
                        }
                        else if (IsTypeQualifier(toks[startIndex]))
                        {
                            AppendCodeWord(outStr, toks[startIndex]);
                            mLastToken = Type.QUALIFIER;
                        }
                        else if (IsPrimitive(toks[startIndex]))
                        {
                            AppendCodeWord(outStr, toks[startIndex]);
                            mLastToken = Type.IDENTIFIER;
                        }
                        else if (mTemplateDepth != 0)
                        {
                            AppendIdentifier(outStr, toks[startIndex]);
                            mLastToken = Type.IDENTIFIER;
                        }
                        else if (toks[startIndex].IndexOfAny(new char[] { '(', ')', ';', '{', '}' }) != -1)
                        {
                            if (outStr.Length > 3 && outStr[outStr.Length - 1] == ' ' && 
                                ((((outStr[outStr.Length - 2] == '>' && outStr[outStr.Length - 3] != '\\'))) ||
                                (mLastToken == Type.IDENTIFIER && mInFunctionBrackets)))
                            {
                                --outStr.Length;
                            }

                            AppendUnknown(outStr, toks[startIndex]);
                        }
                        else
                        {
                            // Should be for when template types are in random places
                            AppendIdentifier(outStr, toks[startIndex]);
                        }
                    }

                    return outStr.ToString();
                }
            }

            public static IParser RetrieveParser()
            {
                return new CPPState();
            }
        }
    }

}
