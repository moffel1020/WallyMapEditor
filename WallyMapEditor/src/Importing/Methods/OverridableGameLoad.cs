using System;
using System.IO;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class OverridableGameLoad : ILoadMethod
{
    public string BrawlPath { get; init; }

    public string? SwzLevelName { get; init; } 
    public string? DescOverride { get; init; }
    public string? TypesOverride { get; init; }
    public string? BonesOverride { get; init; }
    public string? PowersOverride { get; init; }

    public OverridableGameLoad(string brawlPath, string? swzLevelName, string? descPath=null, string? typesPath=null, string? bonesPath = null, string? powersPath = null)
    {
        if (descPath is null && swzLevelName is null) 
            throw new ArgumentException("Could not create OverridableGameLoad. swzLevelName or descPath has to be set"); 

        if (!WmeUtils.IsValidBrawlPath(brawlPath))
            throw new ArgumentException($"{brawlPath} is not a valid brawlhalla path");

        BrawlPath = brawlPath;
        SwzLevelName = swzLevelName;
        DescOverride = descPath;
        TypesOverride = typesPath;
        BonesOverride = bonesPath;
        PowersOverride = powersPath;
    }

    public LoadedData Load()
    {
        uint key = WmeUtils.FindDecryptionKeyFromPath(Path.Combine(BrawlPath, "BrawlhallaAir.swf")) ?? throw new Exception("Could not find swz decryption key");
        (LevelDesc? ld, LevelTypes lt, LevelSetTypes lst, BoneTypes bt, string[]? pn) = DecryptSwzFiles(key);
        if (DescOverride is not null) ld = WmeUtils.DeserializeFromPath<LevelDesc>(DescOverride, bhstyle: true);
        if (ld is null) throw new Exception("LevelDesc was not loaded from path or swz");

        Level l = new(ld, lt, lst);
        return new LoadedData(l, bt, pn);
    }

    private (LevelDesc?, LevelTypes, LevelSetTypes, BoneTypes, string[]?) DecryptSwzFiles(uint key)
    {
        string dynamicPath = Path.Combine(BrawlPath, "Dynamic.swz");
        string gamePath = Path.Combine(BrawlPath, "Game.swz");
        string initPath = Path.Combine(BrawlPath, "Init.swz");

        LevelDesc? ld = null;
        if (SwzLevelName is not null)
            ld = WmeUtils.DeserializeSwzFromPath<LevelDesc>(dynamicPath, "LevelDesc_" + SwzLevelName, key, bhstyle: true);

        LevelTypes lt = WmeUtils.DeserializeSwzFromPath<LevelTypes>(initPath, "LevelTypes.xml", key, bhstyle: true) ?? throw new FileLoadException("Could not load LevelTypes from swz");
        LevelSetTypes lst = WmeUtils.DeserializeSwzFromPath<LevelSetTypes>(gamePath, "LevelSetTypes.xml", key, bhstyle: true) ?? throw new FileLoadException("Could not load LevelSetTypes from swz");
        BoneTypes bt = WmeUtils.DeserializeSwzFromPath<BoneTypes>(initPath, "BoneTypes.xml", key, bhstyle: true) ?? throw new FileLoadException("Could not load BoneTypes from swz");

        string? powerTypesContent = WmeUtils.GetFileInSwzFromPath(gamePath, "powerTypes.csv", key);
        string[]? powerTypes = powerTypesContent is null ? null
            : WmeUtils.ParsePowerTypesFromString(powerTypesContent);

        return (ld, lt, lst, bt, powerTypes);
    }
}