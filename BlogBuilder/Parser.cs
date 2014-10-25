using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BlogBuilder
{
    class Parser
    {
        private Page.Meta mMetadata;
        private LinkedList<Page.Section> mSections = new LinkedList<Page.Section>();

        public bool Parse(StreamReader sr)
        {
            mMetadata = new Page.Meta();
            mSections.Clear();

            if (!ReadMetadata(sr))
            {
                return false;
            }

            String line;
            Page.Section section = new Page.Section();
            Page.SubSection subsection = null;
            Page.CodeBlock code = null;
            Page.List list = null;
            Page.ImageSection imgsec = null;
            Page.Image img = null;
            bool inBody = false;
            bool inSubsection = false;
            bool inCode = false;
            bool inList = false;
            bool inImg = false;

            while ((line = sr.ReadLine()) != null)
            {
                if (!inCode)
                {
                    line = line.Trim();

                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                }

                if (inImg)
                {
                    if (line[0] == '{')
                    {
                        var endTag = line.IndexOf('}');
                        String tag = line.Substring(1, endTag - 1);

                        switch (tag)
                        {
                            case "src":
                                if (img != null)
                                {
                                    imgsec.Contents.AddLast(img);
                                }
                                img = new Page.Image();
                                img.Src = line.Substring(endTag + 1, line.IndexOf("{/src}") - endTag - 1);
                                continue;
                            case "alt":
                                img.Alt = line.Substring(endTag + 1, line.IndexOf("{/alt}") - endTag - 1);
                                continue;
                            case "/img":
                                if (img != null)
                                {
                                    imgsec.Contents.AddLast(img);
                                }
                                section.Contents.AddLast(imgsec);
                                inImg = false;
                                continue;
                        }
                    }

                    continue;
                }

                if (inList)
                {
                    if (line[0] == '{')
                    {
                        section.Contents.AddLast(list);
                        list = null;
                        inList = false;
                        continue;
                    }
                    else
                    {
                        list.AddItem(line);
                        continue;
                    }
                }

                if (inCode)
                {
                    if (!String.IsNullOrWhiteSpace(line) && line[0] == '\\' && line[1] == '{')
                    {
                        var endTag = line.IndexOf("\\}");
                        String tag = line.Substring(2, endTag - 2);

                        switch (tag)
                        {
                            case "head":
                                code.Header = line.Substring(endTag + 2, line.IndexOf("\\{/head\\}") - endTag - 2);
                                continue;
                            case "lang":
                                code.Language = Code.FindLanguage(line.Substring(endTag + 2, line.IndexOf("\\{/lang\\}") - endTag - 2));
                                continue;
                            case "/code":
                                inCode = false;
                                section.Contents.AddLast(code);
                                code = null;
                                continue;
                            default:
                                Output.Warning("Unknown tag inside code block: " + tag);
                                continue;
                        }
                    }

                    code.AddLine(line);
                    continue;
                }

                if (inSubsection)
                {
                    if (line[0] != '{')
                    {
                        subsection.AddParagraph(line);
                        continue;
                    }

                    section.Contents.AddLast(subsection);
                    subsection = null;
                    inSubsection = false;
                }

                if (inBody)
                {
                    if (line[0] == '{')
                    {
                        var endTag = line.IndexOf('}');
                        String tag = line.Substring(1, endTag - 1);

                        switch (tag)
                        {
                            case "/body":
                                mSections.AddLast(section);
                                section = new Page.Section();
                                inBody = false;
                                continue;
                            case "subhead":
                                inSubsection = true;
                                subsection = new Page.SubSection(line.Substring(endTag + 1, line.IndexOf("{/subhead}") - endTag - 1));
                                continue;
                            case "code":
                                inCode = true;
                                code = new Page.CodeBlock();
                                continue;
                            case "ul":
                                inList = true;
                                list = new Page.List(Page.List.Type.Unordered);
                                continue;
                            case "ol":
                                inList = true;
                                list = new Page.List(Page.List.Type.Ordered);
                                continue;
                            case "img":
                                inImg = true;
                                imgsec = new Page.ImageSection();
                                continue;
                            default:
                                Output.Warning("Unrecognised tag: " + tag);
                                continue;
                        }
                    }

                    section.Contents.AddLast(new Page.Paragraph(line));
                    continue;
                }
                
                if (line[0] == '{')
                {
                    var endTag = line.IndexOf('}');
                    String tag = line.Substring(1, endTag - 1);

                    switch (tag)
                    {
                        case "head":
                            section.Header = line.Substring(endTag + 1, line.IndexOf("{/head}") - endTag - 1);
                            continue;
                        case "body":
                            inBody = true;
                            continue;
                    }
                }
            }

            return true;
        }

        private bool ReadMetadata(StreamReader sr)
        {
            String line = sr.ReadLine();
            if (line.StartsWith("TITLE:"))
            {
                mMetadata.Title = line.Substring(6);
            }
            else
            {
                Output.Error("No title metadata found!");
                return false;
            }

            line = sr.ReadLine();
            if (line.StartsWith("SUBTITLE:"))
            {
                mMetadata.Subtitle = line.Substring(9);
            }
            else
            {
                Output.Error("No subtitle metadata found!");
                return false;
            }

            line = sr.ReadLine();
            if (line.StartsWith("DESC:"))
            {
                mMetadata.Description = line.Substring(5);
            }
            else
            {
                Output.Error("No description metadata found!");
                return false;
            }

            line = sr.ReadLine();
            if (line.StartsWith("TAGS:"))
            {
                mMetadata.Tags = line.Substring(5);
            }
            else
            {
                Output.Error("No tags metadata found!");
                return false;
            }

            line = sr.ReadLine();
            if (!line.StartsWith("CONTENT:"))
            {
                Output.Error("No content found!");
                return false;
            }

            return true;
        }

        public void WriteToFile(String outputFile)
        {
            using (var sw = File.CreateText(outputFile))
            {
                Page.WriteToStream(sw, mSections, mMetadata);
            }
        }

        public void WriteToConsole()
        {
            using (var cs = Console.OpenStandardOutput())
            {
                using (var sw = new StreamWriter(cs))
                {
                    sw.AutoFlush = true;
                    Page.WriteToStream(sw, mSections, mMetadata);
                }
            }
        }
    }
}
