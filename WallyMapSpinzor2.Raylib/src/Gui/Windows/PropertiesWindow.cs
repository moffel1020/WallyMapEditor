using ImGuiNET;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    public void Show(object o, CommandHistory cmd, RaylibCanvas? canvas, string? assetDir)
    {
        ImGui.Begin($"Properties - {o.GetType().Name}###properties", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        _propChanged |= ShowProperties(o, cmd, canvas, assetDir);

        ImGui.End();
    }

    // TODO: for collision and itemspawns, add the ability to change their types
    // TODO: hardcollision should be edited as a shape rather than an individual collision, if they are not a shape they wont work properly ingame
    private static bool ShowProperties(object o, CommandHistory cmd, RaylibCanvas? canvas, string? assetDir) => o switch
    {
        Respawn r => ShowRespawnProps(r, cmd),

        Background b => ShowBackgroundProps(b, cmd, canvas, assetDir),
        TeamScoreboard ts => ShowTeamScoreboardProps(ts, cmd),
        Platform p => ShowPlatformProps(p, cmd, canvas, assetDir),
        AnimatedBackground ab => ShowAnimatedBackgroundProps(ab, cmd, canvas, assetDir),
        Gfx gfx => ShowGfxProps(gfx, cmd),
        LevelAnim la => ShowLevelAnimProps(la, cmd),
        LevelAnimation la => ShowLevelAnimationProps(la, cmd),

        MovingPlatform mp => ShowMovingPlatformProps(mp, cmd, canvas, assetDir),

        CameraBounds cb => ShowCameraBoundsProps(cb, cmd),
        SpawnBotBounds sb => ShowSpawnBotBoundsProps(sb, cmd),

        AbstractCollision ac => ShowCollisionProps(ac, cmd),
        AbstractItemSpawn i => ShowItemSpawnProps(i, cmd),
        AbstractAsset a => ShowAbstractAssetProps(a, cmd, canvas, assetDir),
        AbstractVolume v => ShowAbstractVolumeProps(v, cmd),
        NavNode n => ShowNavNodeProps(n, cmd),

        LevelSound ls => ShowLevelSoundProps(ls, cmd),

        WaveData w => ShowWaveDataProps(w, cmd, canvas, assetDir),
        CustomPath cp => ShowCustomPathProps(cp, cmd, canvas, assetDir),
        Point p => ShowPointProps(p, cmd),
        Group g => ShowGroupProps(g, cmd),

        DynamicCollision dc => ShowDynamicProps(dc, cmd, canvas, assetDir),
        DynamicItemSpawn di => ShowDynamicProps(di, cmd, canvas, assetDir),
        DynamicRespawn dr => ShowDynamicProps(dr, cmd, canvas, assetDir),
        DynamicNavNode dn => ShowDynamicProps(dn, cmd, canvas, assetDir),
        _ => ShowUnimplementedProps()
    };
}