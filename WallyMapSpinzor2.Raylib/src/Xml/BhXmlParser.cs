using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace WallyMapSpinzor2.Raylib;

// ported from: https://github.com/HaxeFoundation/haxe/blob/development/std/haxe/xml/Parser.hx
// which is the parser brawlhalla uses
public static class BhXmlParser
{
    private static readonly FrozenDictionary<string, char> ESCAPES = new Dictionary<string, char>
    {
        ["lt"] = '<',
        ["gt"] = '>',
        ["amp"] = '&',
        ["quot"] = '"',
        ["apos"] = '\'',
    }.ToFrozenDictionary();

    private static void AddChild(this XNode e, XNode c)
    {
        if (e is XElement ee)
            ee.Add(c);
        else if (e is XDocument ed)
            ed.Add(c);
        else
            throw new BhXmlException($"Bad node type, expected Element or Document but found {e.GetType().Name}");
    }

    private static bool Exists(this XNode e, string attribute)
    {
        if (e is not XElement ee)
            throw new BhXmlException($"Bad node type, expected Element but found {e.GetType().Name}");
        return ee.HasAttribute(attribute);
    }

    private static void Set(this XNode e, string key, string? value)
    {
        if (e is not XElement ee)
            throw new BhXmlException($"Bad node type, expected Element but found {e.GetType().Name}");
        ee.SetAttributeValue(key, value);
    }

    private enum S
    {
        IGNORE_SPACES,
        BEGIN,
        BEGIN_NODE,
        TAG_NAME,
        BODY,
        ATTRIB_NAME,
        EQUALS,
        ATTVAL_BEGIN,
        ATTRIB_VAL,
        CHILDS,
        CLOSE,
        WAIT_END,
        WAIT_END_RET,
        PCDATA,
        HEADER,
        COMMENT,
        DOCTYPE,
        CDATA,
        ESCAPE,
    }

    public static XDocument Load(Stream stream, bool strict = false)
    {
        string str;
        using (StreamReader sstream = new(stream))
            str = sstream.ReadToEnd();
        return Parse(str, strict);
    }

    public static XDocument Parse(string str, bool strict = false)
    {
        XDocument doc = new();
        DoParse(str, strict, 0, doc);
        return doc;
    }

    private static int DoParse(string str, bool strict, int p = 0, XNode? parent = null)
    {
        XNode? xml = null;
        S state = S.BEGIN;
        S next = S.BEGIN;
        string? aname = null;
        int start = 0;
        int nsubs = 0;
        int nbrackets = 0;
        StringBuilder? buf = new();
        // need extra state because next is in use
        S escapeNext = S.BEGIN;
        int attrValQuote = -1;

        while (p < str.Length)
        {
            char c = str[p];
            switch (state)
            {
                case S.IGNORE_SPACES:
                    switch (c)
                    {
                        case '\n' or '\r' or '\t' or ' ':
                            break;
                        default:
                            state = next;
                            continue;
                    }
                    break;
                case S.BEGIN:
                    switch (c)
                    {
                        case '<':
                            state = S.IGNORE_SPACES;
                            next = S.BEGIN_NODE;
                            break;
                        default:
                            start = p;
                            state = S.PCDATA;
                            continue;
                    }
                    break;
                case S.PCDATA:
                    if (c == '<')
                    {
                        buf.Append(str, start, p - start);
                        XNode child = new XText(buf.ToString());
                        buf.Clear();
                        parent!.AddChild(child);
                        nsubs++;
                        state = S.IGNORE_SPACES;
                        next = S.BEGIN_NODE;
                    }
                    else if (c == '&')
                    {
                        buf.Append(str, start, p - start);
                        state = S.ESCAPE;
                        escapeNext = S.PCDATA;
                        start = p + 1;
                    }
                    break;
                case S.CDATA:
                    if (c == ']' && str[p + 1] == ']' && str[p + 2] == '>')
                    {
                        XNode child = new XCData(str[start..p]);
                        parent!.AddChild(child);
                        nsubs++;
                        p += 2;
                        state = S.BEGIN;
                    }
                    break;
                case S.BEGIN_NODE:
                    switch (c)
                    {
                        case '!':
                            if (str[p + 1] == '[')
                            {
                                p += 2;
                                if (!str.Substring(p, 6).Equals("CDATA[", StringComparison.InvariantCultureIgnoreCase))
                                    throw new BhXmlException("Expected <![CDATA[", str, p);
                                p += 5;
                                state = S.CDATA;
                                start = p + 1;
                            }
                            else if (str[p + 1] == 'D' || str[p + 1] == 'd')
                            {
                                if (!str.Substring(p + 2, 6).Equals("OCTYPE", StringComparison.InvariantCultureIgnoreCase))
                                    throw new BhXmlException("Expected <!DOCTYPE", str, p);
                                p += 8;
                                state = S.DOCTYPE;
                                start = p + 1;
                            }
                            else if (str[p + 1] != '-' || str[p + 2] != '-')
                                throw new BhXmlException("Expected <!--", str, p);
                            else
                            {
                                p += 2;
                                state = S.COMMENT;
                                start = p + 1;
                            }
                            break;
                        case '?':
                            state = S.HEADER;
                            start = p;
                            break;
                        case '/':
                            if (parent == null)
                                throw new BhXmlException("Expected node name", str, p);
                            start = p + 1;
                            state = S.IGNORE_SPACES;
                            next = S.CLOSE;
                            break;
                        default:
                            state = S.TAG_NAME;
                            start = p;
                            continue;
                    }
                    break;
                case S.TAG_NAME:
                    if (!IsValidChar(c))
                    {
                        if (p == start)
                            throw new BhXmlException("Expected node name", str, p);
                        xml = new XElement(str[start..p]);
                        parent!.AddChild(xml);
                        nsubs++;
                        state = S.IGNORE_SPACES;
                        next = S.BODY;
                        continue;
                    }
                    break;
                case S.BODY:
                    switch (c)
                    {
                        case '/':
                            state = S.WAIT_END;
                            break;
                        case '>':
                            state = S.CHILDS;
                            break;
                        default:
                            state = S.ATTRIB_NAME;
                            start = p;
                            continue;
                    }
                    break;
                case S.ATTRIB_NAME:
                    if (!IsValidChar(c))
                    {
                        if (start == p)
                            throw new BhXmlException("Expected attribute name", str, p);
                        string tmp = str[start..p];
                        aname = tmp;
                        if (xml!.Exists(aname))
                            throw new BhXmlException("Duplicate attribute [" + aname + "]", str, p);
                        state = S.IGNORE_SPACES;
                        next = S.EQUALS;
                        continue;
                    }
                    break;
                case S.EQUALS:
                    switch (c)
                    {
                        case '=':
                            state = S.IGNORE_SPACES;
                            next = S.ATTVAL_BEGIN;
                            break;
                        default:
                            throw new BhXmlException("Expected =", str, p);
                    }
                    break;
                case S.ATTVAL_BEGIN:
                    switch (c)
                    {
                        case '"' or '\'':
                            buf.Clear();
                            state = S.ATTRIB_VAL;
                            start = p + 1;
                            attrValQuote = c;
                            break;
                        default:
                            throw new BhXmlException("Expected \"", str, p);
                    }
                    break;
                case S.ATTRIB_VAL:
                    switch (c)
                    {
                        case '&':
                            buf.Append(str, start, p - start);
                            state = S.ESCAPE;
                            escapeNext = S.ATTRIB_VAL;
                            start = p + 1;
                            break;
                        case '>' or '<' when strict:
                            // HTML allows these in attributes values
                            throw new BhXmlException("Invalid unescaped " + c + " in attribute value", str, p);
                        default:
                            if (c == attrValQuote)
                            {
                                buf.Append(str, start, p - start);
                                string val = buf.ToString();
                                buf.Clear();
                                xml!.Set(aname!, val);
                                state = S.IGNORE_SPACES;
                                next = S.BODY;
                            }
                            break;
                    }
                    break;
                case S.CHILDS:
                    p = DoParse(str, strict, p, xml);
                    start = p;
                    state = S.BEGIN;
                    break;
                case S.WAIT_END:
                    state = c switch
                    {
                        '>' => S.BEGIN,
                        _ => throw new BhXmlException("Expected >", str, p),
                    };
                    break;
                case S.WAIT_END_RET:
                    switch (c)
                    {
                        case '>':
                            if (nsubs == 0)
                                parent!.AddChild(new XText(""));
                            return p;
                        default:
                            throw new BhXmlException("Expected >", str, p);
                    }
                case S.CLOSE:
                    if (!IsValidChar(c))
                    {
                        if (start == p)
                            throw new BhXmlException("Expected node name", str, p);

                        string v = str[start..p];
                        if (parent is null || parent is not XElement eparent)
                            throw new BhXmlException($"Unexpected </{v}>, tag is not open", str, p);
                        if (v != eparent.Name)
                            throw new BhXmlException("Expected </" + eparent.Name + ">", str, p);
                        state = S.IGNORE_SPACES;
                        next = S.WAIT_END_RET;
                        continue;
                    }
                    break;
                case S.COMMENT:
                    if (c == '-' && str[p + 1] == '-' && str[p + 2] == '>')
                    {
                        parent!.AddChild(new XComment(str[start..p]));
                        nsubs++;
                        p += 2;
                        state = S.BEGIN;
                    }
                    break;
                case S.DOCTYPE:
                    if (c == '[')
                        nbrackets++;
                    else if (c == ']')
                        nbrackets--;
                    else if (c == '>' && nbrackets == 0)
                    {
                        parent!.AddChild(new XDocumentType(str[start..p], null, null, null));
                        nsubs++;
                        state = S.BEGIN;
                    }
                    break;
                case S.HEADER:
                    if (c == '?' && str[p + 1] == '>')
                    {
                        p++;
                        parent!.AddChild(new XProcessingInstruction("", str.Substring(start + 1, p - start - 2)));
                        nsubs++;
                        state = S.BEGIN;
                    }
                    break;
                case S.ESCAPE:
                    if (c == ';')
                    {
                        string s = str[start..p];
                        if (s[0] == '#')
                        {
                            buf.Append((char)int.Parse(s[1] == 'x' ? $"0{s[1..]}" : s[1..]));
                        }
                        else if (!ESCAPES.TryGetValue(s, out char value))
                        {
                            if (strict)
                                throw new BhXmlException("Undefined entity: " + s, str, p);
                            buf.Append($"&{s};");
                        }
                        else
                        {
                            buf.Append(value);
                        }
                        start = p + 1;
                        state = escapeNext;
                    }
                    else if (!IsValidChar(c) && c != '#')
                    {
                        if (strict)
                            throw new BhXmlException("Invalid character in entity: " + c, str, p);
                        buf.Append('&');
                        buf.Append(str, start, p - start);
                        p--;
                        start = p + 1;
                        state = escapeNext;
                    }
                    break;
            }
            ++p;
        }

        if (state == S.BEGIN)
        {
            start = p;
            state = S.PCDATA;
        }

        if (state == S.PCDATA)
        {
            if (parent is XElement eparent)
                throw new BhXmlException("Unclosed node <" + eparent.Name + ">", str, p);
            if (p != start || nsubs == 0)
            {
                buf.Append(str, start, p - start);
                parent!.AddChild(new XText(buf.ToString()));
            }
            return p;
        }

        if (!strict && state == S.ESCAPE && escapeNext == S.PCDATA)
        {
            buf.Append('&');
            buf.Append(str, start, p - start);
            parent!.AddChild(new XText(buf.ToString()));
            return p;
        }

        throw new BhXmlException("Unexpected end", str, p);
    }

    private static bool IsValidChar(char c)
    {
        return
            (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (c >= '0' && c <= '9') ||
            c == ':' ||
            c == '.' ||
            c == '_' ||
            c == '-';
    }
}