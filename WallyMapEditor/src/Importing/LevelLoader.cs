using System;
using System.Collections.Generic;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class LevelLoader(Editor editor)
{
    private readonly Editor _editor = editor;

    public string[]? PowerNames { get; set; }
    private BoneTypes? _boneTypes;
    public BoneTypes? BoneTypes
    {
        get => _boneTypes;
        set
        {
            _boneTypes = value;
            if (_editor.Canvas is not null && value is not null) _editor.Canvas.Loader.BoneTypes = value;
        }
    }

    public ILoadMethod? ReloadMethod { get; set; }

    public bool CanReImport => ReloadMethod is not null;

    public void ReImport()
    {
        if (ReloadMethod is not null) LoadMap(ReloadMethod);
    }

    public void LoadMap(ILoadMethod loadMethod)
    {
        (Level l, BoneTypes? bt, string[]? pn) = loadMethod.Load();
        if (BoneTypes is null && bt is null) throw new InvalidOperationException("Could not load map. BoneTypes has not been imported.");

        _editor.Level = l;

        if (bt is not null) (BoneTypes, PowerNames) = (bt, pn);
        ResetEditorState();

        ReloadMethod = loadMethod;
    }

    public void LoadDefaultMap(string levelName, string displayName, bool addDefaultPlaylists = true)
    {
        if (BoneTypes is null) throw new InvalidOperationException("Could not load default map. BoneTypes has not been imported.");

        LevelDesc ld = DefaultLevelDesc;
        LevelType lt = DefaultLevelType;
        ld.LevelName = ld.AssetDir = lt.LevelName = levelName;
        lt.DisplayName = displayName;
        HashSet<string> playlists = [.. (addDefaultPlaylists ? DefaultPlaylists : [])];

        _editor.Level = new(ld, lt, playlists);
        ResetEditorState();

        ReloadMethod = null; // loaded default map can't be reimported, it's not on disk
    }

    private void ResetEditorState()
    {
        _editor.Selection.Object = null;
        _editor.CommandHistory.Clear();
        _editor.Canvas?.ClearTextureCache();
        _editor.CommandHistory.Clear();
        _editor.ResetRenderState();
        _editor.ResetCam((int)_editor.ViewportWindow.Bounds.Width, (int)_editor.ViewportWindow.Bounds.Height);
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
            new NavNode() { X = 2000, Y = 3000, NavID = 1, Type = NavNodeTypeEnum.G, Path = [(2, NavNodeTypeEnum.G)] },
            new NavNode() { X = 3000, Y = 3000, NavID = 2, Type = NavNodeTypeEnum.G, Path = [(1, NavNodeTypeEnum.G)] },
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