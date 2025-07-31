using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class LevelLoader
{
    public delegate void OnNewMapLoadedEventHandler(LevelLoader? sender, EditorLevel newLevel);
    public delegate void OnMapReloadedEventHandler(LevelLoader? sender, EditorLevel level, Level newData, ILoadMethod loadMethod);

    public event OnNewMapLoadedEventHandler? OnNewMapLoaded;
    public event OnMapReloadedEventHandler? OnMapReloaded;

    public string[]? PowerNames { get; set; }

    public event EventHandler<BoneTypes>? BoneTypesChanged;
    private BoneTypes? _boneTypes;
    public BoneTypes? BoneTypes
    {
        get => _boneTypes;
        set
        {
            _boneTypes = value;
            if (value is not null)
                BoneTypesChanged?.Invoke(this, value);
        }
    }

    public static bool CanReImport([NotNullWhen(true)] EditorLevel? level) => level?.ReloadMethod is not null;

    public void ReImport(EditorLevel? level)
    {
        if (!CanReImport(level)) return;
        LoadMap(level.ReloadMethod!, level);
    }

    public void LoadMap(ILoadMethod loadMethod, EditorLevel? reload = null)
    {
        (Level l, BoneTypes? bt, string[]? pn) = loadMethod.Load();
        if (BoneTypes is null && bt is null) throw new InvalidOperationException("Could not load map. BoneTypes has not been imported.");

        if (bt is not null) (BoneTypes, PowerNames) = (bt, pn);

        if (reload is not null)
            OnMapReloaded?.Invoke(this, reload, l, loadMethod);
        else
        {
            EditorLevel editorLevel = new(l) { ReloadMethod = loadMethod };
            OnNewMapLoaded?.Invoke(this, editorLevel);
        }
    }

    public void LoadDefaultMap(string levelName, string assetDir, string displayName, bool addDefaultPlaylists = true)
    {
        if (BoneTypes is null) throw new InvalidOperationException("Could not load default map. BoneTypes has not been imported.");

        LevelDesc ld = DefaultLevelDesc;
        LevelType lt = DefaultLevelType;
        ld.LevelName = lt.LevelName = levelName;
        ld.AssetDir = assetDir;
        lt.DisplayName = displayName;
        HashSet<string> playlists = addDefaultPlaylists ? [.. DefaultPlaylists] : [];

        Level l = new(ld, lt, playlists);
        EditorLevel level = new(l);

        OnNewMapLoaded?.Invoke(this, level);
    }

    public static LevelDesc DefaultLevelDesc => new()
    {
        AssetDir = "UnknownLevel",
        LevelName = "UnknownLevel",
        SlowMult = 1,
        CameraBounds = new()
        {
            X = 0,
            Y = 0,
            W = 5000,
            H = 3000
        },
        SpawnBotBounds = new()
        {
            X = 1500,
            Y = 1000,
            W = 2000,
            H = 1000,
        },
        Backgrounds = [new() { AssetName = "BG_Brawlhaven.jpg", W = 2048, H = 1151 }],
        LevelSounds = [],
        Assets = [],
        LevelAnims = [],
        LevelAnimations = [],
        Volumes = [],
        Collisions = [],
        DynamicCollisions = [],
        Respawns = [],
        DynamicRespawns = [],
        ItemSpawns = [],
        DynamicItemSpawns = [],
        NavNodes = [
            new NavNode() { X = 2000, Y = 1500, NavID = 1, Type = NavNodeTypeEnum.G, Path = [(2, NavNodeTypeEnum.G)] },
            new NavNode() { X = 3000, Y = 1500, NavID = 2, Type = NavNodeTypeEnum.G, Path = [(1, NavNodeTypeEnum.G)] },
        ],
        DynamicNavNodes = [],
        WaveDatas = [],
        AnimatedBackgrounds = [],
    };

    public static LevelType DefaultLevelType => new()
    {
        LevelName = "UnknownLevel",
        DisplayName = "Unkown Level",
        AssetName = "a_Level_Unknown",
        FileName = "Level_Wacky.swf",
        DevOnly = false,
        TestLevel = false,
        LevelID = 0,
        CrateColorA = new(120, 120, 120),
        CrateColorB = new(120, 120, 120),
        LeftKill = 500,
        RightKill = 500,
        TopKill = 500,
        BottomKill = 500,
        BGMusic = "Level09Theme", // certified banger
        ThumbnailPNGFile = "wally.jpg"
    };

    public static readonly string[] DefaultPlaylists = [
        "StandardAll",
        "StandardFFA",
        "Standard1v1",
        "Standard2v2",
        "Standard3v3",
    ];
}