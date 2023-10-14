using System.Text;

namespace MarkdownParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using FileStream stream = File.OpenRead("markdown.md");
            MarkdownToHtmlConverter.ToHtml(stream, "markdown.html");
        }
    }
}