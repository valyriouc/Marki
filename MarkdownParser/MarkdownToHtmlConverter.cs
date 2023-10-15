using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownParser
{
    internal enum MarkdownStartSign
    {
        Hash = 0,
        Blockquote = 1,
        Dash = 2,
        ExclamationMark = 3,
        OpenBracket = 4,
        Number = 5,
        Normal = 6
    }

    internal enum MarkdownInlineSign
    {
        Star = 0,
        Code = 1,
    }

    internal class MarkdownToHtmlConverter : IDisposable
    {

        #region StaticMembers 
       
        public static Dictionary<char, MarkdownInlineSign> MapToInlineSign { get; set; }
            = new Dictionary<char, MarkdownInlineSign>()
            {
                { '*', MarkdownInlineSign.Star },
                { '`', MarkdownInlineSign.Code }
            };

        public static MarkdownStartSign GetStartSign(char sign)
        {

            if (char.IsNumber(sign))
                return MarkdownStartSign.Number;    
            
            MarkdownStartSign markdownStartSign;

            switch (sign)
            {
                case '#':
                    markdownStartSign = MarkdownStartSign.Hash;
                    break;
                case '>':
                    markdownStartSign = MarkdownStartSign.Blockquote;
                    break;
                case '-':
                    markdownStartSign = MarkdownStartSign.Dash;
                    break;
                case '!':
                    markdownStartSign = MarkdownStartSign.ExclamationMark;
                    break;
                case '[':
                    markdownStartSign = MarkdownStartSign.OpenBracket;
                    break;
                default:
                    markdownStartSign = MarkdownStartSign.Normal;
                    break;
            }

            return markdownStartSign;
        }

        #endregion

        private TextReader Input { get; }

        private TextWriter Output { get; }

        #region Constructors

        private MarkdownToHtmlConverter(string markdown, StringBuilder html) 
            : this(new StringReader(markdown), new StringWriter(html))
        {
             
        }

        private MarkdownToHtmlConverter(Stream markdown, StringBuilder html) 
            : this(new StreamReader(markdown), new StringWriter(html))
        {

        }

        private MarkdownToHtmlConverter(string markdown, Stream html)  
            : this(new StringReader(markdown), new StreamWriter(html))
        {

        }

        private MarkdownToHtmlConverter(Stream markdown, Stream html) 
            : this(new StreamReader(markdown), new StreamWriter(html))
        {

        }

        private MarkdownToHtmlConverter(
            TextReader input, 
            TextWriter output)
        {
            Input = input;
            Output = output;
        }

        #endregion

        #region Parsing

        private void Parse()
        {
            while (true)
            {
                string? line = Input.ReadLine();
                if (line is null)
                {
                    Console.WriteLine("Finished the convertation to HTML!");
                    break;
                }

                MarkdownStartSign startSign;
                try
                {

                    startSign = GetStartSign(line[0]);
                }
                catch
                {
                    startSign = MarkdownStartSign.Normal;
                }

                switch (startSign)
                {
                    case MarkdownStartSign.Hash:
                        GenerateParagraph();
                        GenerateOrderedList();
                        GenerateUnorderedList();
                        GenerateBlockQuote();
                        (int level, string content) = ParseHeading(line);
                        GenerateHeading(level, content);
                        break;
                    case MarkdownStartSign.Number:
                        GenerateParagraph();
                        GenerateUnorderedList();
                        GenerateBlockQuote();
                        ParseOrderedList(line);
                        break;
                    case MarkdownStartSign.Dash:
                        GenerateParagraph();
                        GenerateOrderedList();
                        GenerateBlockQuote();
                        HandleDashFurther(line);
                        break;
                    case MarkdownStartSign.Blockquote:
                        GenerateParagraph();
                        GenerateOrderedList();
                        GenerateUnorderedList();
                        ParseBlockQuote(line);
                        break;
                    case MarkdownStartSign.ExclamationMark:
                        GenerateParagraph();
                        GenerateOrderedList();
                        GenerateUnorderedList();
                        GenerateBlockQuote();
                        (string alt, string image) = ParseImage(line);
                        GenerateImage(alt, image);
                        break;
                    case MarkdownStartSign.OpenBracket:
                        
                        break;
                    default:
                        // Reading paragraph 
                        GenerateOrderedList();
                        GenerateUnorderedList();
                        GenerateBlockQuote();
                        ParseParagraph(line);
                        break;
                }
            }

            Flush();
        }

        private void Flush()
        {
            GenerateParagraph();
            GenerateOrderedList();
            GenerateUnorderedList();
            GenerateBlockQuote();
        }

        private List<string> CurrentParagraph { get; } = new List<string>();

        private void ParseParagraph(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                // Generating paragraph
                GenerateParagraph();
                Console.WriteLine("Generated paragraph!");
                return;
            }

            string paragraphLine = line.Trim();
            CurrentParagraph.Add(paragraphLine);
        }

        private (int level, string content) ParseHeading(string line)
        {
            int level = line.Count(x => x == '#');
            if (0 >= level || level > 6)
            {
                throw new Exception(
                    "Expected a heading level between 1 and 6!");
            }

            // Generating the replacement 
            string replace = string.Empty;
            for (int i = 0; i < level; i++) replace += '#';

            // Check that the line starts with heading signs
            if (!line.StartsWith(replace))
                throw new Exception(
                "Expected heading level at line start!");

            string content = line.Replace(replace, string.Empty);

            return (level, content);
        }

        private List<string> CurrentOrdList { get; } = new List<string>();

        private void ParseOrderedList(string line)
        {
            string numberStr = string.Empty;
            for (int i = 0; i < line.Length; i++)
            {
                if (!char.IsDigit(line[i])) break;

                numberStr += line[i];
            }

            // getting the number 
            if (!int.TryParse(numberStr, out int number))
                throw new Exception(
                    "Invalid ordered list item start!");

            string content = line
                .Replace($"{number}.", string.Empty)
                .Trim();

            CurrentOrdList.Add(content);
        }

        private void HandleDashFurther(string line)
        {
            if (line[1] == '-' && line[2] == '-') 
            {
                Console.WriteLine("Horizontal rules are currently not supported by the parser!");
                bool shouldHorzRule = ParseHorizontalRule(line);
                GenerateHorizontalRule(shouldHorzRule);
                return;
            }
            else
            {
                // Parse an unordered list 
                ParseUnorderedList(line);
            }
        }

        private List<string> CurrentUnordList { get; } = new List<string>();

        private void ParseUnorderedList(string line)
        {
            if (!line.StartsWith("-"))
            {
                throw new Exception(
                    "Invalid unordered list");
            }

            string content = line
                .Replace("-", string.Empty)
                .Trim();

            CurrentUnordList.Add(content);  
        }

        private bool ParseHorizontalRule(string line)
        {
            int dashCount = line.Count(x => x == '-');
            string substring = string.Empty;
            for (int i = 0; i < dashCount; i++)
            {
                substring += '-';
            }

            string normalized = line.Trim();
            if (normalized != substring)
            {
                Console.WriteLine("Invalid horizontal rule!");
                return false;
            }

            return true;
        }

        private List<string> CurrentBlockQuote { get; } = new List<string>();

        private void ParseBlockQuote(string line)
        {
            int number = line.Count(x => x == '>');
            string start = string.Empty;
            for (int i = 0; i < number; i++)
            {
                start += '>';
            }

            if (!line.StartsWith(start))
            {
                throw new Exception(
                    "Invalid block quote start!");
            }

            string content = line.Replace(start, string.Empty).Trim();

            CurrentBlockQuote.Add(content);
        }

        private (string alt, string image) ParseImage(string line)
        {
            string alt = string.Empty;
            string image = string.Empty;
            bool change = true;
            Stack<char> bracketTrack = new Stack<char>();
            for (int i = 1; i < line.Length; i++)
            {
                if (line[i] == '[' ||
                    line[i] == '(')
                {
                    change = !change;
                    bracketTrack.Push(line[i]);
                    continue;
                }

                if (line[i] == ']' && bracketTrack.Peek() != '[' || 
                    line[i] == ')' && bracketTrack.Peek() != '(')
                {
                    throw new Exception(
                        "Invalid image!");
                }
                else if (line[i] == ']' && bracketTrack.Peek() == '[' ||
                    line[i] == ')' && bracketTrack.Peek() == '(')
                {
                    continue;
                }

                if (!change)
                {
                    alt += line[i];
                }
                else
                {
                    image += line[i];
                }
            } 

            return (alt, image);    
        }

        #endregion

        #region Generating

        private void GenerateParagraph()
        {
            if (CurrentParagraph.Count == 0)
            {
                return;
            }

            StringBuilder html = new StringBuilder();

            html.AppendLine("<p>");

            foreach (string line in CurrentParagraph)
            {
                html.AppendLine(line);
            }

            html.AppendLine("</p>");

            Output.Write(html);
            CurrentParagraph.Clear();
        }

        private void GenerateHeading(int level, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception(
                    "Content of heading is empty!");
            }

            string html = $"<h{level}>{content}</h{level}>";
            Output.Write(html);
        }

        private void GenerateOrderedList()
        {
            if (CurrentOrdList.Count == 0)
            {
                return;
            }

            StringBuilder html = new StringBuilder();

            html.AppendLine("<ol>");

            foreach (string line in CurrentOrdList)
            {
                html.AppendLine($"<li>{line}</li>");
            }

            html.AppendLine("</ol>");

            Output.Write(html);
            CurrentOrdList.Clear(); 
        }

        private void GenerateUnorderedList()
        {
            if (CurrentUnordList.Count == 0)
            {
                return;
            }

            StringBuilder html = new StringBuilder();

            html.AppendLine("<ul>");

            foreach (string line in CurrentUnordList)
            {
                html.AppendLine($"<li>{line}</li>");
            }

            html.AppendLine("</ul>");
            Output.Write(html);
            CurrentUnordList.Clear();
        }

        private void GenerateHorizontalRule(bool shouldWrite)
        {
            if (!shouldWrite)
            {
                return;
            }

            string html = "<div class=\"divider\"></div>";
            Output.WriteLine(html);
        }

        private void GenerateBlockQuote()
        {
            if (CurrentBlockQuote.Count == 0)
            {
                return;
            }

            StringBuilder html = new StringBuilder();

            html.AppendLine("<div>");

            foreach (string line in CurrentBlockQuote)
            {
                html.AppendLine(line);
            }

            html.AppendLine("</div>");
            
            Output.Write(html);
            CurrentBlockQuote.Clear();
        }

        private void GenerateImage(string alt, string image)
        {
            if (string.IsNullOrEmpty(image))
            {
                throw new Exception(
                    "Image is empty");
            }

            string html = $"<img alt=\"{alt}\" src=\"{image}\">";
            Output.WriteLine(html);
        }

        #endregion

        #region ToHtml

        public static string ToHtml(Stream stream)
        {
            try
            {
                StringBuilder html = new StringBuilder();

                using MarkdownToHtmlConverter converter
                    = new MarkdownToHtmlConverter(stream, html);

                converter.Parse();

                return html.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return string.Empty;
        }

        public static string ToHtml(string markdown)
        {
            try
            {
                StringBuilder html = new StringBuilder();

                using MarkdownToHtmlConverter converter
                    = new MarkdownToHtmlConverter(markdown, html);

                converter.Parse();

                return html.ToString();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return string.Empty;
        }

        public static void ToHtml(Stream stream, string filename)
        {
            try
            {

                if (File.Exists(filename))
                {
                    throw new Exception(
                        "File already exists!");
                }

                using FileStream htmlStream = File.OpenWrite(filename);

                using MarkdownToHtmlConverter converter =
                    new MarkdownToHtmlConverter(stream, htmlStream);

                converter.Parse();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
            }
        }

        public static void ToHtml(string markdown, string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    throw new Exception(
                        "File already exists!");
                }

                using FileStream htmlStream = File.OpenWrite(filename);

                using MarkdownToHtmlConverter converter =
                    new MarkdownToHtmlConverter(markdown, htmlStream);

                converter.Parse();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion

        private bool IsDisposed { get; set; } = false;

        public void Dispose()
        {
            if (IsDisposed) return;

            Input.Dispose();
            Output.Dispose();

            IsDisposed = true;
        }
    }
}
