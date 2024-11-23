using System;
using System.IO;
using System.Xml.Linq;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class PathPreferences : IDeserializable, ISerializable
{
    public const string APPDATA_DIR_NAME = "WallyMapEditor";
    public const string FILE_NAME = "PathPreferences.xml";

    public event EventHandler<string>? BrawlhallaPathChanged;

    private string? _brawlhallaPath;
    public string? BrawlhallaPath { get => _brawlhallaPath; set => SetBrawlhallaPath(value); }
    public string? BrawlhallaAirPath { get; set; }

    public string? LevelDescPath { get; set; }
    public string? LevelTypePath { get; set; }
    public string? LevelTypesPath { get; set; }
    public string? LevelSetTypesPath { get; set; }
    public string? LevelPath { get; set; }
    public string? BoneTypesPath { get; set; }
    public string? PowerTypesPath { get; set; }

    public string? DecryptionKey { get; set; }

    public string? ConfigFolderPath { get; set; }

    public string? ModFilePath { get; set; }
    public string? ModName { get; set; }
    public string? GameVersionInfo { get; set; }
    public string? ModVersionInfo { get; set; }
    public string? ModDescription { get; set; }
    public string? ModAuthor { get; set; }

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
                return WmeUtils.DeserializeFromPath<PathPreferences>(FilePath);
            }
        }

        return new();
    }

    public void Save()
    {
        WmeUtils.SerializeToPath(this, FilePath);
    }

    public void Deserialize(XElement e)
    {
        _brawlhallaPath = e.GetElementValue(nameof(BrawlhallaPath)); // don't trigger setter!
        BrawlhallaAirPath = e.GetElementValue(nameof(BrawlhallaAirPath));
        LevelDescPath = e.GetElementValue(nameof(LevelDescPath));
        LevelTypePath = e.GetElementValue(nameof(LevelTypePath));
        LevelTypesPath = e.GetElementValue(nameof(LevelTypesPath));
        LevelSetTypesPath = e.GetElementValue(nameof(LevelSetTypesPath));
        LevelPath = e.GetElementValue(nameof(LevelPath));
        BoneTypesPath = e.GetElementValue(nameof(BoneTypesPath));
        PowerTypesPath = e.GetElementValue(nameof(PowerTypesPath));
        DecryptionKey = e.GetElementValue(nameof(DecryptionKey));
        ConfigFolderPath = e.GetElementValue(nameof(ConfigFolderPath));
        ModFilePath = e.GetElementValue(nameof(ModFilePath));
        ModName = e.GetElementValue(nameof(ModName));
        GameVersionInfo = e.GetElementValue(nameof(GameVersionInfo));
        ModVersionInfo = e.GetElementValue(nameof(ModVersionInfo));
        ModDescription = e.GetElementValue(nameof(ModDescription));
        ModAuthor = e.GetElementValue(nameof(ModAuthor));
    }

    public void Serialize(XElement e)
    {
        e.AddIfNotNull(nameof(BrawlhallaPath), BrawlhallaPath);
        e.AddIfNotNull(nameof(BrawlhallaAirPath), BrawlhallaAirPath);
        e.AddIfNotNull(nameof(LevelDescPath), LevelDescPath);
        e.AddIfNotNull(nameof(LevelTypePath), LevelTypePath);
        e.AddIfNotNull(nameof(LevelTypesPath), LevelTypesPath);
        e.AddIfNotNull(nameof(LevelSetTypesPath), LevelSetTypesPath);
        e.AddIfNotNull(nameof(LevelPath), LevelPath);
        e.AddIfNotNull(nameof(BoneTypesPath), BoneTypesPath);
        e.AddIfNotNull(nameof(PowerTypesPath), PowerTypesPath);
        e.AddIfNotNull(nameof(DecryptionKey), DecryptionKey);
        e.AddIfNotNull(nameof(ConfigFolderPath), ConfigFolderPath);
        e.AddIfNotNull(nameof(ModFilePath), ModFilePath);
        e.AddIfNotNull(nameof(ModName), ModName);
        e.AddIfNotNull(nameof(GameVersionInfo), GameVersionInfo);
        e.AddIfNotNull(nameof(ModVersionInfo), ModVersionInfo);
        e.AddIfNotNull(nameof(ModDescription), ModDescription);
        e.AddIfNotNull(nameof(ModAuthor), ModAuthor);
    }

    public void SetBrawlhallaPath(string? path)
    {
        if (path is null)
        {
            _brawlhallaPath = null;
            return;
        }

        _brawlhallaPath = path;
        BrawlhallaAirPath ??= Path.Combine(_brawlhallaPath, "BrawlhallaAir.swf");
        BrawlhallaPathChanged?.Invoke(this, _brawlhallaPath);
    }

    public void ApplyCmdlineOverrides(CommandLineArgs args)
    {
        if (args.TryGetArg("--brawlPath", out string? brawlPath))
            BrawlhallaPath = brawlPath;
        if (args.TryGetArg("--brawlAir", out string? brawlAir))
            BrawlhallaAirPath = brawlAir;
        if (args.TryGetArg("--levelDesc", out string? levelDesc))
            LevelDescPath = levelDesc;
        if (args.TryGetArg("--levelType", out string? levelType))
            LevelTypePath = levelType;
        if (args.TryGetArg("--levelTypes", out string? levelTypes))
            LevelTypesPath = levelTypes;
        if (args.TryGetArg("--levelSetTypes", out string? levelSetTypes))
            LevelSetTypesPath = levelSetTypes;
        if (args.TryGetArg("--level", out string? level))
            LevelPath = level;
        if (args.TryGetArg("--boneTypes", out string? boneTypes))
            BoneTypesPath = boneTypes;
        if (args.TryGetArg("--powerTypes", out string? powerTypes))
            PowerTypesPath = powerTypes;
        if (args.TryGetArg("--swzKey", out string? swzKey))
            DecryptionKey = swzKey;
    }
}