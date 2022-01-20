namespace MQ_Receiver_messageId
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
