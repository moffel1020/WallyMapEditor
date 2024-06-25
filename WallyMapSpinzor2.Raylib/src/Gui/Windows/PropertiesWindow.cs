using ImGuiNET;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    public void Show(object o, CommandHistory cmd, PropertiesWindowData data)
    {
        ImGui.Begin($"Properties - {o.GetType().Name}###properties", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        _propChanged |= ShowProperties(o, cmd, data);

        ImGui.End();
    }

    // TODO: for collision and itemspawns, add the ability to change their types
    // TODO: hardcollision should be edited as a shape rather than an individual collision, if they are not a shape they wont work properly ingame
    private static bool ShowProperties(object o, CommandHistory cmd, PropertiesWindowData data) => o switch
    {
        Respawn r => ShowRespawnProps(r, cmd, data),
        Background b => ShowBackgroundProps(b, cmd, data),
        TeamScoreboard ts => ShowTeamScoreboardProps(ts, cmd),
        Platform p => ShowPlatformProps(p, cmd, data),
        AnimatedBackground ab => ShowAnimatedBackgroundProps(ab, cmd, data),
        Gfx gfx => ShowGfxProps(gfx, cmd, data),
        LevelAnim la => ShowLevelAnimProps(la, cmd),
        LevelAnimation la => ShowLevelAnimationProps(la, cmd),

        MovingPlatform mp => ShowMovingPlatformProps(mp, cmd, data),

        CameraBounds cb => ShowCameraBoundsProps(cb, cmd),
        SpawnBotBounds sb => ShowSpawnBotBoundsProps(sb, cmd),

        AbstractCollision ac => ShowCollisionProps(ac, cmd, data),
        AbstractItemSpawn i => ShowItemSpawnProps(i, cmd, data),
        AbstractAsset a => ShowAbstractAssetProps(a, cmd, data),
        AbstractVolume v => ShowAbstractVolumeProps(v, cmd),
        NavNode n => ShowNavNodeProps(n, cmd, data),

        LevelSound ls => ShowLevelSoundProps(ls, cmd),

        WaveData w => ShowWaveDataProps(w, cmd, data),
        CustomPath cp => ShowCustomPathProps(cp, cmd, data),
        Point p => ShowPointProps(p, cmd),
        Group g => ShowGroupProps(g, cmd),

        DynamicCollision dc => ShowDynamicProps(dc, cmd, data),
        DynamicItemSpawn di => ShowDynamicProps(di, cmd, data),
        DynamicRespawn dr => ShowDynamicProps(dr, cmd, data),
        DynamicNavNode dn => ShowDynamicProps(dn, cmd, data),
        _ => ShowUnimplementedProps()
    };
}