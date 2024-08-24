using WallyMapSpinzor2;

namespace WallyMapEditor;

public class SelectionContext
{
    public object? Object { get; set; }

    public bool IsChildOf(object? o) => Object switch
    {
        AbstractAsset a => o == a.Parent || IsChildOf(a.Parent),
        AbstractCollision c => o == c.Parent,
        AbstractItemSpawn i => o == i.Parent,
        Respawn r => o == r.Parent,
        NavNode n => o == n.Parent,
        // keyframes can't be selected. AbstractKeyFrame k => o == k.Parent || IsChildOf(k.Parent),
        _ => false,
    };
}