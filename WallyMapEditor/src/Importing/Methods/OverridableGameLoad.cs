using System;
using System.IO;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class OverridableGameLoad : ILoadMethod
{
    public string BrawlPath { get; init; }
    public uint DecryptionKey { get; init; }
    public string? SwzLevelName { get; init; }
    public string? DescOverride { get; init; }
    public string? TypesOverride { get; init; }
    public string? SetTypesOverride { get; init; }
    public string? BonesOverride { get; init; }
    public string? PowersOverride { get; init; }

    public OverridableGameLoad(string brawlPath, string? swzLevelName, uint key, string? descPath = null, string? typesPath = null, string? setTypesPath = null, string? bonesPath = null, string? powersPath = null)
    {
        if (descPath is null && swzLevelName is null)
            throw new ArgumentException("Could not create OverridableGameLoad. swzLevelName or descPath has to be set");

        if (!WmeUtils.IsValidBrawlPath(brawlPath))
            throw new ArgumentException($"{brawlPath} is not a valid brawlhalla path");

        BrawlPath = brawlPath;
        SwzLevelName = swzLevelName;
        DescOverride = descPath;
        DecryptionKey = key;
        TypesOverride = typesPath;
        SetTypesOverride = setTypesPath;
        BonesOverride = bonesPath;
        PowersOverride = powersPath;
    }

    public LoadedData Load()
    {
        string dynamicPath = Path.Combine(BrawlPath, "Dynamic.swz");
        string gamePath = Path.Combine(BrawlPath, "Game.swz");
        string initPath = Path.Combine(BrawlPath, "Init.swz");

        LevelDesc ld = LoadFile<LevelDesc>(DescOverride, dynamicPath, "LevelDesc_" + SwzLevelName, DecryptionKey) ?? throw new FileLoadException("Could not load LevelDesc from swz or path");
        LevelTypes lt = LoadFile<LevelTypes>(TypesOverride, initPath, "LevelTypes.xml", DecryptionKey) ?? throw new FileLoadException("Could not load LevelTypes from swz or path");
        LevelSetTypes lst = LoadFile<LevelSetTypes>(SetTypesOverride, gamePath, "LevelSetTypes.xml", DecryptionKey) ?? throw new FileLoadException("Could not load LevelSetTypes from swz or path");
        BoneTypes bt = LoadFile<BoneTypes>(BonesOverride, initPath, "BoneTypes.xml", DecryptionKey) ?? throw new FileLoadException("Could not load BoneTypes from swz or path");
        string? powerTypesContent = WmeUtils.GetFileInSwzFromPath(gamePath, "powerTypes.csv", DecryptionKey);
        string[]? pt = powerTypesContent is null ? null
            : WmeUtils.ParsePowerTypesFromString(powerTypesContent);

        return new LoadedData(new(ld, lt, lst), bt, pt);
    }

    private static T? LoadFile<T>(string? overridePath, string swzPath, string swzFile, uint key) where T : IDeserializable, new() =>
        overridePath is null
            ? WmeUtils.DeserializeSwzFromPath<T>(swzPath, swzFile, key, bhstyle: true)
            : WmeUtils.DeserializeFromPath<T>(overridePath);

}