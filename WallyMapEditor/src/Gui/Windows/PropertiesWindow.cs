using WallyMapSpinzor2;
using ImGuiNET;
using Raylib_cs;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    public void Show(object o, EditorLevel level, PropertiesWindowData data)
    {
        ImGui.Begin($"Properties - {o.GetType().Name}###properties", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            level.CommandHistory.SetAllowMerge(false);
        }

        _propChanged |= ShowProperties(o, level, data);

        ImGui.End();
    }

    private static bool RemoveButton<T>(T value, EditorLevel level)
        where T : class
    {
        if (!ImGui.Button($"Delete##{value.GetHashCode()}")) return false;
        return WmeUtils.RemoveObject(value, level.Level.Desc, level.CommandHistory);
    }

    private static bool ShowProperties(object o, EditorLevel level, PropertiesWindowData data) => o switch
    {
        Respawn r => ShowRespawnProps(r, level),
        Background b => ShowBackgroundProps(b, level, data),
        TeamScoreboard ts => ShowTeamScoreboardProps(ts, level),
        Platform p => ShowPlatformProps(p, level, data),
        AnimatedBackground ab => ShowAnimatedBackgroundProps(ab, level, data),
        Gfx gfx => ShowGfxProps(gfx, data),
        LevelAnim la => ShowLevelAnimProps(la, level),
        LevelAnimation la => ShowLevelAnimationProps(la, level),

        MovingPlatform mp => ShowMovingPlatformProps(mp, level, data),

        CameraBounds cb => ShowCameraBoundsProps(cb, level),
        SpawnBotBounds sb => ShowSpawnBotBoundsProps(sb, level),

        AbstractCollision ac => ShowCollisionProps(ac, level, data),
        AbstractItemSpawn i => ShowItemSpawnProps(i, level),
        AbstractAsset a => ShowAbstractAssetProps(a, level, data),
        AbstractVolume v => ShowAbstractVolumeProps(v, level, data),
        NavNode n => ShowNavNodeProps(n, level),

        LevelSound ls => ShowLevelSoundProps(ls, level),

        WaveData w => ShowWaveDataProps(w, level, data),
        CustomPath cp => ShowCustomPathProps(cp, level, data),
        Point p => ShowPointProps(p, level),
        Group g => ShowGroupProps(g, level),

        DynamicCollision dc => ShowDynamicProps(dc, level, data),
        DynamicItemSpawn di => ShowDynamicProps(di, level, data),
        DynamicRespawn dr => ShowDynamicProps(dr, level, data),
        DynamicNavNode dn => ShowDynamicProps(dn, level, data),
        _ => ShowUnimplementedProps()
    };
}