using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using nietras.SeparatedValues;
using WallyMapSpinzor2;
using BrawlhallaSwz.Xml;

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

    public static T DeserializeFromPath<T>(string fromPath, bool bhstyle = false) where T : IDeserializable<T>
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
        where T : IDeserializable<T>
    {
        if (bhstyle)
        {
            return BhXmlParser.ParseElement(xmldata)!.DeserializeTo<T>();
        }
        else
        {
            return XElement.Parse(xmldata).DeserializeTo<T>();
        }
    }

    public static T? DeserializeSwzFromPath<T>(string swzPath, string filename, uint key, bool bhstyle = false)
        where T : IDeserializable<T>
    {
        string? content = GetFileInSwzFromPath(swzPath, filename, key);
        if (content is null) return default;
        return DeserializeFromString<T>(content, bhstyle);
    }

    public static string[]? ParsePowerTypesFromPath(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        using StreamReader reader = new(stream);
        return ParsePowerTypes(reader);
    }

    public static string[]? ParsePowerTypesFromString(string str)
    {
        using StringReader reader = new(str);
        return ParsePowerTypes(reader);
    }

    private static string[]? ParsePowerTypes(TextReader reader)
    {
        reader.ReadLine(); // skip first line bs
        using SepReader sep = Sep.New(',').Reader().From(reader);
        return [.. sep.Enumerate(row => row["PowerName"].ToString())];
    }
}