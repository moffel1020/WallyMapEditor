using System.Collections.Generic;

namespace WallyMapSpinzor2.Raylib;

public class SwfSpriteFrame
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