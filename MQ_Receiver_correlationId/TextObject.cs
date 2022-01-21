using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ_Receiver_correlationId
{
    public class TextObject
    {
        public uint Index { get; private set; }
        public string Text { get; private set; }

        public TextObject(uint index, string text)
        {
            Index = index;
            Text = text;
        }
    }
}
