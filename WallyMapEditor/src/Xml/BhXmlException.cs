using System;

namespace WallyMapEditor;

[Serializable]
public class BhXmlException : Exception
{
    public BhXmlException() { }
    public BhXmlException(string message) : base(message) { }
    public BhXmlException(string message, Exception inner) : base(message, inner) { }
    public BhXmlException(string message, string xml, int position) : base(CreateMessage(message, xml, position)) { }
    public BhXmlException(string message, string xml, int position, Exception inner) : base(CreateMessage(message, xml, position), inner) { }

    private static string CreateMessage(string message, string xml, int position)
    {
        int lineNumber = 1;
        int positionAtLine = 0;
        for (int i = 0; i < position; ++i)
        {
            char c = xml[i];
            if (c == 10)
            {
                lineNumber++;
                positionAtLine = 0;
            }
            else if (c != 13)
            {
                positionAtLine++;
            }
        }

        return $"{nameof(BhXmlException)}: {message} at line {lineNumber} char {positionAtLine}";
    }
}