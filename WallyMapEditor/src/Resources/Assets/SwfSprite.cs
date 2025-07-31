using System.Collections.Generic;
using SwfLib.Tags;
using SwfLib.Tags.DisplayListTags;

namespace WallyMapEditor;

public sealed class SwfSprite
{
    public SwfSpriteFrame[] Frames { get; set; } = [];

    public static SwfSprite CompileFrom(DefineSpriteTag spriteTag)
    {
        List<SwfSpriteFrame> frames = [new()];
        foreach (SwfTagBase tag in spriteTag.Tags)
        {
            if (tag is PlaceObjectBaseTag placeObject)
            {
                if (frames[^1].Layers.TryGetValue(placeObject.Depth, out SwfSpriteFrameLayer? layer))
                {
                    layer.ModifyBy(placeObject);
                }
                else
                {
                    frames[^1].Layers[placeObject.Depth] = new() { FrameOffset = 0, Matrix = placeObject.Matrix, CharacterId = placeObject.CharacterID };
                }
            }
            else if (tag is RemoveObject2Tag removeObject)
            {
                frames[^1].Layers.Remove(removeObject.Depth);
            }
            else if (tag is ShowFrameTag)
            {
                frames.Add(frames[^1].Clone());
            }
        }
        // we're adding a redundant frame at the end
        frames.RemoveAt(frames.Count - 1);

        return new() { Frames = [.. frames] };
    }
}