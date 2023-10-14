using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownParser
{
    internal interface IMarkdownToJsonModule
    { 
        public void Tokenize(TextReader reader);

        public void ToJson();
    }
}
