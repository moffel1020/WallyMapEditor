using SwfLib.Data;
using SwfLib.Tags.DisplayListTags;

namespace WallyMapSpinzor2.Raylib;

public class SwfSpriteFrameLayer
{
    public int FrameOffset { get; set; }
    public SwfMatrix Matrix { get; set; }
    public ColorTransform? ColorTransform { get; set; }
    public ushort CharacterId { get; set; }

    public void ModifyBy(PlaceObjectBaseTag placeObject)
    {
        if (placeObject is PlaceObject2Tag placeObject2)
        {
            if (placeObject2.HasCharacter)
                CharacterId = placeObject.CharacterID;
            if (placeObject2.HasMatrix)
                Matrix = placeObject.Matrix;
            if (placeObject2.HasColorTransform)
                ColorTransform = new(placeObject2.ColorTransform);
        }
        else if (placeObject is PlaceObject3Tag placeObject3)
        {
            if (placeObject3.HasCharacter)
                CharacterId = placeObject.CharacterID;
            if (placeObject3.HasMatrix)
                Matrix = placeObject.Matrix;
            if (placeObject3.HasColorTransform)
                ColorTransform = placeObject3.ColorTransform is not null ? new(placeObject3.ColorTransform.Value) : null;
        }
    }

    public SwfSpriteFrameLayer Clone() => new()
    {
        FrameOffset = FrameOffset + 1,
        Matrix = Matrix,
        ColorTransform = ColorTransform,
        CharacterId = CharacterId
    };
}