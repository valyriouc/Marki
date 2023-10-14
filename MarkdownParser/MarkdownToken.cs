using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarkdownParser
{
    internal enum MarkdownTokenType
    {
        Sign = 0,
        Word = 1,
    }

    internal enum TokenLength
    {
        MultiLine = 0,
        OneLine = 1,
        Inline = 2
    }

    internal class MarkdownToken
    {
        
    }
}
