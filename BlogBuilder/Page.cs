using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BlogBuilder
{
    class Page
    {
        public struct Meta
        {
            public String Title;
            public String Subtitle;
            public String Description;
            public String Tags;
        }

        public interface IWrite
        {
            void Write(StreamWriter writer);
        }

        public class Paragraph : IWrite
        {
            protected String mText;

            public Paragraph(String text)
            {
                mText = text;
            }

            public void Write(StreamWriter writer)
            {
                PHP.SanitiseWriteLine(writer, "->addParagraph(new Paragraph('{0}'))", false, mText);
            }
        }

        public class SubSection : IWrite
        {
            protected String mHeader;
            protected LinkedList<String> mPars = new LinkedList<String>();

            public SubSection(String header)
            {
                mHeader = header;
            }

            public void AddParagraph(String par)
            {
                mPars.AddLast(par);
            }

            public void Write(StreamWriter writer)
            {
                PHP.SanitiseWriteLine(writer, "->addParagraph((new SubSection('{0}'))", false, mHeader);

                foreach (var item in mPars)
                {
                    PHP.SanitiseWriteLine(writer, "->addLine('{0}')", false, item);
                }

                writer.WriteLine(")");
            }
        }

        public class CodeBlock : IWrite
        {
            public String Header;
            public Code.Language Language = Code.Language.NONE;
            protected LinkedList<String> mLines = new LinkedList<string>();

            public CodeBlock(String header = null)
            {
                Header = header;
            }

            public void AddLine(String line)
            {
                mLines.AddLast(line);
            }

            public void Write(StreamWriter writer)
            {
                writer.Write("->addParagraph((new CodeBlock(");

                if (Header != null)
                {
                    PHP.SanitiseWriteLine(writer, "'{0}'))", false, Header);
                }
                else
                {
                    writer.WriteLine("))");
                }

                var parser = Code.BeginParse(Language);
                foreach (var line in mLines)
                {
                    PHP.SanitiseWriteLine(writer, "->addLine('{0}')", true, parser.InterpretLine(line));
                }

                writer.WriteLine(")");
            }
        }

        public class List : IWrite
        {
            public enum Type
            {
                Ordered, Unordered
            }

            public Type ListType
            {
                get;
                private set;
            }

            private LinkedList<String> mItems = new LinkedList<string>();

            public List(Type type)
            {
                ListType = type;
            }

            public void AddItem(String item)
            {
                mItems.AddLast(item);
            }

            public void Write(StreamWriter writer)
            {
                writer.WriteLine("->addParagraph((new ListHTML())");
                writer.WriteLine("->setOrdered({0})", (ListType == Type.Ordered));

                foreach (var item in mItems)
                {
                    PHP.SanitiseWriteLine(writer, "->addItem('{0}')", false, item);
                }

                writer.WriteLine(')');
            }
        }

        public class Image : IWrite 
        {
            public String Src { get; set; }

            public String Alt { get; set; }

            public bool HasAltText
            {
                get
                {
                    return Alt != null;
                }
            }

            public void Write(StreamWriter writer)
            {
                var img = System.Drawing.Image.FromFile(Src);

                var dest = PHP.SanitiseFilename(Src, PHP.FileType.IMAGE);

                if (!Directory.Exists(Path.Combine(User.Default.DeployPath, "public_html", "blog", "img")))
                {
                    Directory.CreateDirectory(Path.Combine(User.Default.DeployPath, "public_html", "blog", "img"));
                }

                File.Copy(Src, Path.Combine(User.Default.DeployPath, "public_html", dest), true);

                writer.WriteLine("->addImage((new Image('{0}', {1}, {2}))", dest, img.Width, img.Height);

                if (HasAltText)
                {
                    writer.WriteLine("->setAltText('{0}')->setCaption('{0}')", Alt);
                }

                writer.WriteLine(')');
            }
        }

        public class Section : IWrite
        {
            public String Header;
            public LinkedList<IWrite> Contents = new LinkedList<IWrite>();

            public virtual void Write(StreamWriter writer)
            {
                writer.Write("$this->content[] = (new Section(");
                if (Header != null)
                {
                    PHP.SanitiseWriteLine(writer, "'{0}'))", false, Header);
                }
                else
                {
                    writer.WriteLine("))");
                }

                foreach (var item in Contents)
                {
                    item.Write(writer);
                }

                writer.WriteLine(";");
            }
        }

        public class ImageSection : Section
        {
            public override void Write(StreamWriter writer)
            {
                writer.WriteLine("->addParagraph((new ImageSection(ImageSection::BLOCK))");

                foreach (var item in Contents)
                {
                    item.Write(writer);
                }

                writer.WriteLine(')');
            }
        }

        public static void WriteToStream(StreamWriter os, LinkedList<Section> sections, Meta metaData)
        {
            os.WriteLine("<?php");
            os.WriteLine("/**");
            os.WriteLine(" * Created by BlogBuilder");
            os.WriteLine(" * Which was created by Nick Stephen");
            os.WriteLine(" * TITLE: {0}", metaData.Title);
            os.WriteLine(" * SUBTITLE: {0}", metaData.Subtitle);
            os.WriteLine(" * DESC: {0}", metaData.Description);
            os.WriteLine(" * TAGS: {0}", metaData.Tags);
            os.WriteLine(" */");
            os.WriteLine();

            // Ignoring metadata for the moment

            foreach (var section in sections)
            {
                section.Write(os);
                os.WriteLine();
            }
        }
    }
}
