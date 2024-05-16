using SwfLib.Data;
using SwfLib.Tags.DisplayListTags;

namespace WallyMapSpinzor2.Raylib;

public class SwfSpriteFrameLayer
{
    public int FrameOffset { get; set; }
    public SwfMatrix Matrix { get; set; }
    public ushort CharacterId { get; set; }

    public void ModifyBy(PlaceObjectBaseTag placeObject)
    {
        if (placeObject.CharacterID != 0)
            CharacterId = placeObject.CharacterID;
        //MAYBE: need to check if HasMatrix
        Matrix = placeObject.Matrix;
    }

    public SwfSpriteFrameLayer Clone() => new() { FrameOffset = FrameOffset + 1, Matrix = Matrix, CharacterId = CharacterId };
}