using WallyMapSpinzor2;
using ImGuiNET;
using Raylib_cs;
using System;

namespace WallyMapEditor;

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

    private static bool ObjectChangeType<T>(T obj, CommandHistory cmd, Func<T, Maybe<T>> menu, T[] objectList)
        where T : class
    {
        Maybe<T> maybeNew = menu(obj);
        if (!maybeNew.TryGetValue(out T? newObj))
            return false;

        T[] list = objectList;
        int indexInList = Array.FindIndex(list, o => o == obj);

        if (indexInList == -1)
        {
            Rl.TraceLog(TraceLogLevel.Error, $"Attempt to change type of orphaned {typeof(T).Name}");
            return false;
        }

        cmd.Add(new SelectPropChangeCommand<T>(val => list[indexInList] = val, obj, newObj));
        cmd.SetAllowMerge(false);
        return true;
    }

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