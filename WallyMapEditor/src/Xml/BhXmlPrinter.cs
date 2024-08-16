using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WallyMapEditor;

public partial class BhXmlPrinter
{
    public static string Print(XNode xml, bool pretty = false)
    {
        BhXmlPrinter printer = new(pretty);
        printer.WriteNode(xml, "");
        return printer._output.ToString();
    }

    private readonly StringBuilder _output = new();
    private readonly bool _pretty;

    private BhXmlPrinter(bool pretty)
    {
        _pretty = pretty;
    }

    private static string HtmlEscape(string s, bool quotes = false)
    {
        StringBuilder buf = new();
        foreach (char c in s)
            buf.Append(c switch
            {
                '&' => "&amp;",
                '<' => "&lt;",
                '>' => "&gt;",
                '"' when quotes => "&quot;",
                '\'' when quotes => "&#039;",
                _ => c,
            });
        return buf.ToString();
    }

    private void WriteNode(XNode value, string tabs)
    {
        if (value is XCData cdata)
        {
            Write(tabs + "<![CDATA[");
            Write(cdata.Value);
            Write("]]>");
            Newline();
        }
        else if (value is XComment comment)
        {
            string commentContent = comment.Value;
            commentContent = CommentRegex.Replace(commentContent, "");
            commentContent = "<!--" + commentContent + "-->";
            Write(tabs);
            Write(commentContent.Trim());
            Newline();
        }
        else if (value is XDocument document)
        {
            foreach (XNode node in document.Nodes())
                WriteNode(node, tabs);
        }
        else if (value is XElement element)
        {
            Write(tabs + "<");
            Write(element.Name.LocalName);
            foreach (XAttribute attribute in element.Attributes())
            {
                Write(" " + attribute.Name.LocalName + "=\"");
                Write(HtmlEscape(attribute.Value, true));
                Write("\"");
            }
            if (HasChildren(element))
            {
                Write(">");
                Newline();
                foreach (XNode child in element.Nodes())
                {
                    WriteNode(child, _pretty ? tabs + "\t" : tabs);
                }
                Write(tabs + "</");
                Write(element.Name.LocalName);
                Write(">");
                Newline();
            }
            else
            {
                Write("/>");
                Newline();
            }
        }
        else if (value is XText pcdata)
        {
            if (pcdata.Value.Length != 0)
            {
                Write(tabs + HtmlEscape(pcdata.Value));
                Newline();
            }
        }
        else if (value is XProcessingInstruction proc)
        {
            Write("<?" + proc.Data + "?>");
            Newline();
        }
        else if (value is XDocumentType type)
        {
            Write("<!DOCTYPE " + type.Name + ">");
            Newline();
        }
    }

    private void Write(string input)
    {
        _output.Append(input);
    }

    private void Newline()
    {
        if (_pretty)
        {
            _output.Append('\n');
        }
    }

    private static bool HasChildren(XNode node)
    {
        if (node is not XContainer container)
            return false;
        foreach (XNode child in container.Nodes())
        {
            if (child is XElement)
                return true;
            else if (child is XCData cdata)
            {
                if (cdata.Value.TrimStart().Length != 0)
                    return true;
            }
            else if (child is XComment comment)
            {
                if (comment.Value.TrimStart().Length != 0)
                    return true;
            }
            else if (child is XText)
                return true;
        }
        return false;
    }

    private static readonly Regex CommentRegex = CommentRegexGenerator();
    [GeneratedRegex("[\n\r\t]+")]
    private static partial Regex CommentRegexGenerator();
}