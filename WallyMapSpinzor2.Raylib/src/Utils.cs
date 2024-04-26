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

using BrawlhallaSwz;

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

    public static Raylib_cs.Color ToRlColor(Color c) => new(c.R, c.G, c.B, c.A);

    public static Raylib_cs.Image ImageSharpImageToRl(Image image)
    {
        using MemoryStream ms = new();
        image.SaveAsQoi(ms);
        return Rl.LoadImageFromMemory(".qoi", ms.ToArray());
    }

    public static Raylib_cs.Image LoadRlImage(string path)
    {
        //brawlhalla only supports JPG, so this is fine
        if (path.EndsWith(".jpg"))
        {
            using Image image = Image.Load(path);
            return ImageSharpImageToRl(image);
        }

        return Rl.LoadImage(path);
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
            element = XElement.Load(fromFile);
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
        XElement element = XElement.Parse(xmldata);
        return element.DeserializeTo<T>();
    }

    public static T? DeserializeSwzFromPath<T>(string swzPath, string filename, uint key)
        where T : IDeserializable, new()
    {
        using FileStream stream = new(swzPath, FileMode.Open, FileAccess.Read);
        using SwzReader reader = new(stream, key);
        while (reader.HasNext())
        {
            string data = reader.ReadFile();
            string name = SwzUtils.GetFileName(data);
            if (name == filename)
                return DeserializeFromString<T>(data);
        }

        return default;
    }
}
