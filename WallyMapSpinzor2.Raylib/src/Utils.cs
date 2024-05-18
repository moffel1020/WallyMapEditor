using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using System.Xml;
using System.Xml.Linq;
using System.Text;

using Rl = Raylib_cs.Raylib;

using SixLabors.ImageSharp;

using SwfLib.Tags;
using SwfLib.Tags.DisplayListTags;
using SwfLib.Tags.ActionsTags;
using SwfLib;

using BrawlhallaSwz;

using AbcDisassembler;
using SwfLib.Data;

namespace WallyMapSpinzor2.Raylib;

public static class Utils
{
    //m11, m12, m13, m14
    //m21, m22, m23, m24
    //m31, m32, m33, m34
    //m41, m42, m43, m44
    public static Matrix4x4 TransformToMatrix(Transform t) => new(
        (float)t.ScaleX, (float)t.SkewY, 0, 0,
        (float)t.SkewX, (float)t.ScaleY, 0, 0,
        0, 0, 1, 0,
        (float)t.TranslateX, (float)t.TranslateY, 0, 1
    );

    public static Transform MatrixToTransform(Matrix4x4 m) => new(m.M11, m.M21, m.M12, m.M22, m.M41, m.M42);
    public static Transform SwfMatrixToTransform(SwfMatrix m) => new(m.ScaleX, m.RotateSkew1, m.RotateSkew0, m.ScaleY, m.TranslateX / 20.0, m.TranslateY / 20.0);

    public static Raylib_cs.Color ToRlColor(Color c) => new(c.R, c.G, c.B, c.A);

    public static Raylib_cs.Image ImageSharpImageToRl(Image image)
    {
        using MemoryStream ms = new();
        image.SaveAsQoi(ms);
        Raylib_cs.Image img = Rl.LoadImageFromMemory(".qoi", ms.ToArray());
        Rl.ImageAlphaPremultiply(ref img);
        return img;
    }

    public static Raylib_cs.Image LoadRlImage(string path)
    {
        //brawlhalla only supports JPG, so this is fine
        if (path.EndsWith(".jpg"))
        {
            using Image image = Image.Load(path);
            return ImageSharpImageToRl(image);
        }

        Raylib_cs.Image img = Rl.LoadImage(path);
        Rl.ImageAlphaPremultiply(ref img);
        return img;
    }

    public static bool IsPolygonClockwise(IReadOnlyList<(double, double)> poly)
    {
        double area = 0;
        for (int i = 0; i < poly.Count; ++i)
        {
            int j = (i + 1) % poly.Count;
            (double x1, double y1) = poly[i];
            (double x2, double y2) = poly[j];
            area += BrawlhallaMath.Cross(x1, y1, x2, y2);
        }
        return area > 0;
    }

    public static IEnumerable<ushort> GetShapeIds(this DefineSpriteTag tag) =>
        tag.Tags
            .OfType<PlaceObjectBaseTag>()
            .Select(place => place.CharacterID);

    public static T DeserializeFromPath<T>(string fromPath)
        where T : IDeserializable, new()
    {
        XElement element;
        using (FileStream fromFile = new(fromPath, FileMode.Open, FileAccess.Read))
        {
            // bmg moment
            if (fromPath.EndsWith("RefineryDoors.xml"))
            {
                string content;
                using (StreamReader reader = new(fromFile))
                    content = reader.ReadToEnd();
                content = content.Replace("--->", "-->");
                element = XElement.Parse(content);
            }
            else
            {
                element = XElement.Load(fromFile);
            }
        }
        return element.DeserializeTo<T>();
    }

    public static void SerializeToPath<T>(T serializable, string toPath)
        where T : ISerializable
    {
        XElement e = serializable.SerializeToXElement();
        using FileStream toFile = new(toPath, FileMode.Create, FileAccess.Write);
        using XmlWriter xmlw = XmlWriter.Create(toFile, new()
        {
            OmitXmlDeclaration = true, //no xml header
            IndentChars = "    ",
            Indent = true, //indent with four spaces
            NewLineChars = "\n", //use UNIX line endings
            Encoding = new UTF8Encoding(false) //use UTF8 (no BOM) encoding
        });
        e.Save(xmlw);
    }

    public static string? SerializeToString<T>(T serializable)
        where T : ISerializable
    {
        XElement e = serializable.SerializeToXElement();
        using StringWriter sw = new();
        using XmlWriter xmlw = XmlWriter.Create(sw, new()
        {
            OmitXmlDeclaration = true, //no xml header
            IndentChars = "    ",
            Indent = true, //indent with four spaces
            NewLineChars = "\n", //use UNIX line endings
            Encoding = new UTF8Encoding(false) //use UTF8 (no BOM) encoding
        });
        e.Save(xmlw);
        xmlw.Flush();
        return sw.ToString();
    }

    public static T DeserializeFromString<T>(string xmldata)
        where T : IDeserializable, new()
    {
        // bmg moment
        xmldata = xmldata.Replace("--->", "-->");
        XElement element = XElement.Parse(xmldata);
        return element.DeserializeTo<T>();
    }

    public static string? GetFileInSwzFromPath(string swzPath, string filename, uint key)
    {
        using FileStream stream = new(swzPath, FileMode.Open, FileAccess.Read);
        using SwzReader reader = new(stream, key);
        while (reader.HasNext())
        {
            string data = reader.ReadFile();
            string name = SwzUtils.GetFileName(data);
            if (name == filename)
                return data;
        }
        return null;
    }

    public static T? DeserializeSwzFromPath<T>(string swzPath, string filename, uint key)
        where T : IDeserializable, new()
    {
        string? content = GetFileInSwzFromPath(swzPath, filename, key);
        if (content is null) return default;
        return DeserializeFromString<T>(content);
    }

    private static List<int> FindGetlexPositions(CPoolInfo cpool, string lexName, List<Instruction> code) => code
        .Select((o, i) => new { Item = o, Index = i })
        .Where(o => o.Item.Name == "getlex" && o.Item.Args[0].Value is INamedMultiname name && cpool.Strings[(int)name.Name] == lexName)
        .Select(o => o.Index)
        .ToList();

    private static int FindCallpropvoidPos(CPoolInfo cpool, string methodName, List<Instruction> code) => code
        .FindIndex(i => i.Name == "callpropvoid" && i.Args[0].Value is INamedMultiname named && cpool.Strings[(int)named.Name] == methodName);

    private static uint? FindLastPushuintArg(List<Instruction> ins) => (uint?)ins
        .LastOrDefault(ins => ins.Name == "pushuint")?.Args[0].Value;

    public static uint? FindDecryptionKey(AbcFile abc)
    {
        foreach (MethodBodyInfo mb in abc.MethodBodies)
        {
            List<int> getlexPos = FindGetlexPositions(abc.ConstantPool, "ANE_RawData", mb.Code);

            for (int i = 0; i < getlexPos.Count; i++)
            {
                int callpropvoidPos = getlexPos[i] == getlexPos[^1]
                    ? FindCallpropvoidPos(abc.ConstantPool, "Init", mb.Code[getlexPos[i]..])
                    : FindCallpropvoidPos(abc.ConstantPool, "Init", mb.Code[getlexPos[i]..getlexPos[i + 1]]);

                if (callpropvoidPos != -1)
                    return FindLastPushuintArg(mb.Code[0..callpropvoidPos]);
            }
        }

        return null;
    }

    public static DoABCDefineTag? GetDoABCDefineTag(string swfPath)
    {
        SwfFile? swf;
        using (FileStream stream = new(swfPath, FileMode.Open, FileAccess.Read))
            swf = SwfFile.ReadFrom(stream);

        if (swf is not null)
        {
            foreach (SwfTagBase tag in swf.Tags)
            {
                if (tag is DoABCDefineTag abcTag)
                    return abcTag;
            }
        }

        return null;
    }
}
