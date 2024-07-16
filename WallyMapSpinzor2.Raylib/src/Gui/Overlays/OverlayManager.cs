namespace WallyMapSpinzor2.Raylib;

public class OverlayManager
{
    public IOverlay? ActiveOverlay { get; private set; } = null;
    private object? _currentObject = null;
    public bool IsUsing { get; private set; } = false;

    public void Update(SelectionContext selection, OverlayData data, CommandHistory cmd)
    {
        if (_currentObject != selection.Object)
        {
            ActiveOverlay = CreateOverlay(selection);
            _currentObject = selection.Object;
        }

        bool wasUsing = IsUsing;
        IsUsing = ActiveOverlay?.Update(data, cmd) ?? false;

        if (wasUsing && !IsUsing) cmd.SetAllowMerge(false);
    }

    public void Draw(OverlayData data) => ActiveOverlay?.Draw(data);

    private static IOverlay? CreateOverlay(SelectionContext selection) => selection.Object switch
    {
        CameraBounds cb => new CameraBoundsOverlay(cb),
        SpawnBotBounds sbb => new SpawnBotBoundsOverlay(sbb),
        AbstractCollision ac => new CollisionOverlay(ac),
        Respawn r => new RespawnOverlay(r),
        MovingPlatform mp => new MovingPlatformOverlay(mp),
        AbstractItemSpawn i => new ItemSpawnOverlay(i),
        AbstractVolume v => new VolumeOverlay(v),
        AbstractAsset a when a.AssetName is not null => new AssetOverlay(a),
        AbstractAsset a => new ParentAssetOverlay(a),
        NavNode n => new NavNodeOverlay(n),
        // ugly. maybe create separate class for each?
        DynamicCollision dc => new AbstracyDynamicOverlay<DynamicCollision, AbstractCollision>(dc),
        DynamicItemSpawn dis => new AbstracyDynamicOverlay<DynamicItemSpawn, AbstractItemSpawn>(dis),
        DynamicRespawn dr => new AbstracyDynamicOverlay<DynamicRespawn, Respawn>(dr),
        DynamicNavNode dn => new AbstracyDynamicOverlay<DynamicNavNode, NavNode>(dn),
        _ => null
    };
}