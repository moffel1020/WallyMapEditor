using System.Collections.Generic;

namespace WallyMapEditor;

public sealed class SwfSpriteFrame
{
    public SortedDictionary<ushort, SwfSpriteFrameLayer> Layers { get; set; } = [];

    public SwfSpriteFrame Clone()
    {
        SortedDictionary<ushort, SwfSpriteFrameLayer> layers = [];
        foreach ((ushort depth, SwfSpriteFrameLayer layer) in Layers)
            layers[depth] = layer.Clone();
        return new() { Layers = layers };
    }
}