using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrawlhallaSwz;
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
        (LevelTypes lt, BoneTypes bt) = ReadFromInitSwz(initPath);
        (LevelSetTypes lst, string[]? pt) = ReadFromGameSwz(gamePath);

        return new LoadedData(new(ld, lt, lst), bt, pt);
    }

    private (LevelTypes, BoneTypes) ReadFromInitSwz(string initPath)
    {
        BoneTypes? bt = null;
        LevelTypes? lt = null;

        string[] initSwzFiles = [];
        if (TypesOverride is null) initSwzFiles = ["LevelTypes.xml"];
        else lt = WmeUtils.DeserializeFromPath<LevelTypes>(TypesOverride, bhstyle: true);

        if (BonesOverride is null) initSwzFiles = [..initSwzFiles, "BoneTypes.xml"];
        else bt = WmeUtils.DeserializeFromPath<BoneTypes>(BonesOverride, bhstyle: true);

        Dictionary<string, string> initFiles = ReadFilesFromSwz(initPath, initSwzFiles);
        lt ??= WmeUtils.DeserializeFromString<LevelTypes>(initFiles["LevelTypes.xml"]);
        bt ??= WmeUtils.DeserializeFromString<BoneTypes>(initFiles["BoneTypes.xml"]);

        return (lt, bt);
    }

    private (LevelSetTypes, string[]?) ReadFromGameSwz(string initPath)
    {
        LevelSetTypes? lst = null;
        string[]? pt = null;

        string[] gameSwzFiles = [];
        if (SetTypesOverride is null) gameSwzFiles = ["LevelSetTypes.xml"];
        else lst = WmeUtils.DeserializeFromPath<LevelSetTypes>(SetTypesOverride, bhstyle: true);

        if (PowersOverride is null) gameSwzFiles = [..gameSwzFiles, "powerTypes.csv"];
        else pt = WmeUtils.ParsePowerTypesFromPath(PowersOverride);

        Dictionary<string, string> gameFiles = ReadFilesFromSwz(initPath, gameSwzFiles);
        lst ??= WmeUtils.DeserializeFromString<LevelSetTypes>(gameFiles["LevelSetTypes.xml"]);
        pt ??= WmeUtils.ParsePowerTypesFromString(gameFiles["powerTypes.csv"]);

        return (lst, pt);
    }

    private Dictionary<string, string> ReadFilesFromSwz(string swzPath, string[] filesToSave)
    {
        Dictionary<string, string> files = [];
        foreach (string content in WmeUtils.GetFilesInSwz(swzPath, DecryptionKey))
        {
            string swzName = SwzUtils.GetFileName(content);
            if  (filesToSave.Contains(swzName))
                files.Add(swzName, content);

            if (files.Count == filesToSave.Length) break;
        }
        return files;
    }

    private static T? LoadFile<T>(string? overridePath, string swzPath, string swzFile, uint key) where T : IDeserializable, new() =>
        overridePath is null
            ? WmeUtils.DeserializeSwzFromPath<T>(swzPath, swzFile, key, bhstyle: true)
            : WmeUtils.DeserializeFromPath<T>(overridePath);

}