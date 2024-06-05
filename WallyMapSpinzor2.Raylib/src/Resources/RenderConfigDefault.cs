using System;
using System.IO;
using System.Xml.Linq;

namespace WallyMapSpinzor2.Raylib;

public class RenderConfigDefault : IDeserializable, ISerializable
{
    public const string APPDATA_DIR_NAME = "WallyMapSpinzor2.Raylib";
    public const string FILE_NAME = "RenderConfigDefault.xml";

    public RenderConfig ConfigDefault { get; set; } = RenderConfig.Default;

    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        APPDATA_DIR_NAME,
        FILE_NAME
    );


    public static RenderConfigDefault Load()
    {
        string? dir = Path.GetDirectoryName(FilePath);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
            if (File.Exists(FilePath))
            {
                return Utils.DeserializeFromPath<RenderConfigDefault>(FilePath);
            }
        }

        return new();
    }

    public void Save()
    {
        Utils.SerializeToPath(this, FilePath);
    }

    public void Deserialize(XElement e)
    {
        ConfigDefault.Deserialize(e);
    }

    public void Serialize(XElement e)
    {
        ConfigDefault.Serialize(e);
    }
}