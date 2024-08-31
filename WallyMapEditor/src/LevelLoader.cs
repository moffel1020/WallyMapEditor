using System.Collections.Generic;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class LevelLoader(Editor editor)
{
    private readonly Editor _editor = editor;

    public LevelTypes? LoadedLt { get; set; }
    public LevelSetTypes? LoadedLst { get; set; }
    public BoneTypes? BoneTypes { get; set; }
    public string[]? PowerNames { get; set; }
    public bool LoadedRequiredFiles => LoadedLt is not null && LoadedLst is not null && BoneTypes is not null;

    public void LoadMapFromLevel(Level l, BoneTypes boneTypes, string[]? powerNames)
    {
        l.Type ??= DefaultLevelType;
        _editor.Level = l;

        SetEditorData(boneTypes, powerNames);
        ClearEditorState();
    }

    public void LoadMapFromData(LevelDesc ld, LevelTypes lt, LevelSetTypes lst, BoneTypes bt, string[]? powerNames)
    {
        _editor.Level = new(ld, lt, lst);
        SetEditorData(bt, powerNames);
        ClearEditorState();
    }

    public void LoadDefaultMap(string levelName, string displayName, bool addDefaultPlaylists=true)
    {
        LevelDesc ld = DefaultLevelDesc;
        LevelType lt = DefaultLevelType;
        ld.LevelName = ld.AssetDir = lt.LevelName = levelName;
        lt.DisplayName = displayName;
        HashSet<string> playlists = [.. (addDefaultPlaylists ? DefaultPlaylists : [])];

        Level level = new(ld, lt, playlists);
        // FIXME: cba to load bonenames properly here. might become problematic if we ever allow the user to add animations
        LoadMapFromLevel(level, BoneTypes ?? new(), PowerNames);
    }

    private void SetEditorData(BoneTypes boneTypes, string[]? powerNames)
    {
        (BoneTypes, PowerNames) = (boneTypes, powerNames);
        if (_editor.Canvas is not null) _editor.Canvas.Loader.BoneTypes = boneTypes;
    }

    private void ClearEditorState()
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