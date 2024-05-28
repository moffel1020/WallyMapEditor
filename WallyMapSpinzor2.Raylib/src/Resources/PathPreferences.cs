using System;
using System.IO;
using System.Xml.Linq;

namespace WallyMapSpinzor2.Raylib;

public class PathPreferences : IDeserializable, ISerializable
{
    public const string APPDATA_DIR_NAME = "WallyMapSpinzor2.Raylib";
    public const string FILE_NAME = "PathPreferences.xml";

    public string? BrawlhallaPath { get; set; }
    public string? BrawlhallaAirPath { get; set; }

    public string? LevelDescPath { get; set; }
    public string? LevelTypePath { get; set; }
    public string? LevelSetTypesPath { get; set; }
    public string? BoneTypesPath { get; set; }

    public string? DecryptionKey { get; set; }

    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        APPDATA_DIR_NAME,
        FILE_NAME
    );


    public static PathPreferences Load()
    {
        string? dir = Path.GetDirectoryName(FilePath);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
            if (File.Exists(FilePath))
            {
                return Utils.DeserializeFromPath<PathPreferences>(FilePath);
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
        BrawlhallaPath = e.GetElementValue(nameof(BrawlhallaPath));
        BrawlhallaAirPath = e.GetElementValue(nameof(BrawlhallaAirPath));
        LevelDescPath = e.GetElementValue(nameof(LevelDescPath));
        LevelTypePath = e.GetElementValue(nameof(LevelTypePath));
        LevelSetTypesPath = e.GetElementValue(nameof(LevelSetTypesPath));
        BoneTypesPath = e.GetElementValue(nameof(BoneTypesPath));
        DecryptionKey = e.GetElementValue(nameof(DecryptionKey));
    }

    public void Serialize(XElement e)
    {
        e.AddIfNotNull(nameof(BrawlhallaPath), BrawlhallaPath);
        e.AddIfNotNull(nameof(BrawlhallaAirPath), BrawlhallaAirPath);
        e.AddIfNotNull(nameof(LevelDescPath), LevelDescPath);
        e.AddIfNotNull(nameof(LevelTypePath), LevelTypePath);
        e.AddIfNotNull(nameof(LevelSetTypesPath), LevelSetTypesPath);
        e.AddIfNotNull(nameof(BoneTypesPath), BoneTypesPath);
        e.AddIfNotNull(nameof(DecryptionKey), DecryptionKey);
    }
}