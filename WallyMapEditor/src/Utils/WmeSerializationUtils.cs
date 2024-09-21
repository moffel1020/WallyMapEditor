using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using nietras.SeparatedValues;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public static partial class WmeUtils
{
    public static readonly XmlWriterSettings StandardSaveSettings = new()
    {
        OmitXmlDeclaration = true, // no xml header
        IndentChars = "    ",
        Indent = true, // indent with four spaces
        NewLineChars = "\n", // use UNIX line endings
        Encoding = new UTF8Encoding(false) // use UTF8 (no BOM) encoding
    };

    public static readonly XmlWriterSettings MinifiedSaveSettings = new()
    {
        OmitXmlDeclaration = true, //no xml header
        NewLineChars = "", // do not newline
        Encoding = new UTF8Encoding(false) // use UTF8 (no BOM) encoding
    };

    public static T DeserializeFromPath<T>(string fromPath, bool bhstyle = false) where T : IDeserializable, new()
    {
        if (bhstyle)
        {
            return DeserializeFromPath<T>(fromPath, bhstyle: false);
            // return DeserializeFromString<T>(File.ReadAllText(fromPath), true);
        }
        else
        {
            using FileStream stream = new(fromPath, FileMode.Open, FileAccess.Read);
            return XElement.Load(stream).DeserializeTo<T>();
        }
    }

    public static void SerializeToPath<T>(T serializable, string toPath, bool minify = false, bool bhstyle = false)
        where T : ISerializable
    {
        XElement e = serializable.SerializeToXElement();
        if (bhstyle)
        {
            SerializeToPath(serializable, toPath, minify, bhstyle: false);
            /*
            string str = BhXmlPrinter.Print(e, !minify);
            using StreamWriter writer = new(toFile);
            writer.Write(str);
            */
        }
        else
        {
            using FileStream toFile = new(toPath, FileMode.Create, FileAccess.Write);
            using XmlWriter writer = XmlWriter.Create(toFile, minify ? MinifiedSaveSettings : StandardSaveSettings);
            e.Save(writer);
        }
    }

    public static string SerializeToString<T>(T serializable, bool minify = false, bool bhstyle = false)
        where T : ISerializable
    {
        XElement e = serializable.SerializeToXElement();
        if (bhstyle)
        {
            return SerializeToString(serializable, minify, bhstyle: false);
            // return BhXmlPrinter.Print(e, !minify);
        }
        else
        {
            using StringWriter sw = new();
            using XmlWriter writer = XmlWriter.Create(sw, minify ? MinifiedSaveSettings : StandardSaveSettings);
            e.Save(writer);
            writer.Flush();
            return sw.ToString();
        }
    }

    public static T DeserializeFromString<T>(string xmldata, bool bhstyle = false)
        where T : IDeserializable, new()
    {
        if (bhstyle)
        {
            return BhXmlParser.ParseElement(xmldata).DeserializeTo<T>();
        }
        else
        {
            return XElement.Parse(xmldata).DeserializeTo<T>();
        }
    }

    public static T? DeserializeSwzFromPath<T>(string swzPath, string filename, uint key, bool bhstyle = false)
        where T : IDeserializable, new()
    {
        string? content = GetFileInSwzFromPath(swzPath, filename, key);
        if (content is null) return default;
        return DeserializeFromString<T>(content, bhstyle);
    }

    public static string[]? ParsePowerTypesFromPath(string path) => ParsePowerTypesFromString(File.ReadAllText(path));

    public static string[]? ParsePowerTypesFromString(string str)
    {
        int lineEnd = str.IndexOf('\n');
        str = str[(lineEnd + 1)..];
        using SepReader reader = Sep.New(',').Reader().FromText(str);
        return [.. reader.Enumerate(row => row[0].ToString())];
    }
}