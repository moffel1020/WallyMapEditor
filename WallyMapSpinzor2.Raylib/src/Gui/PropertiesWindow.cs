using ImGuiNET;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    public void Show(object o, CommandHistory cmd)
    {
        ImGui.Begin($"Properties - {o.GetType().Name}###properties", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        _propChanged |= ShowProperties(o, cmd);

        ImGui.End();
    }

    // TODO: for collision and itemspawns, add the ability to change their types
    // TODO: hardcollision should be edited as a shape rather than an individual collision, if they are not a shape they wont work properly ingame
    private static bool ShowProperties(object o, CommandHistory cmd) => o switch
    {
        Respawn r => ShowRespawnProps(r, cmd),

        Platform p => ShowPlatformProps(p, cmd),
        AnimatedBackground ab => ShowAnimatedBackgroundProps(ab, cmd),
        LevelAnim la => ShowLevelAnimProps(la, cmd),
        LevelAnimation la => ShowLevelAnimationProps(la, cmd),

        MovingPlatform mp => ShowMovingPlatformProps(mp, cmd),

        CameraBounds cb => ShowCameraBoundsProps(cb, cmd),
        SpawnBotBounds sb => ShowSpawnBotBoundsProps(sb, cmd),

        AbstractCollision ac => ShowCollisionProps(ac, cmd),
        AbstractItemSpawn i => ShowItemSpawnProps(i, cmd),
        AbstractAsset a => ShowAbstractAssetProps(a, cmd),

        DynamicCollision dc => ShowDynamicProps(dc, cmd),
        DynamicItemSpawn di => ShowDynamicProps(di, cmd),
        DynamicRespawn dr => ShowDynamicProps(dr, cmd),
        DynamicNavNode dn => ShowDynamicProps(dn, cmd),
        _ => ShowUnimplementedProps()
    };
}