using SprId = System.ValueTuple<WallyMapSpinzor2.Raylib.SwfFileData, ushort>;

namespace WallyMapSpinzor2.Raylib;

public class SwfSpriteCache : ManagedCache<SprId, SwfSprite?>
{
    protected override SwfSprite LoadInternal(SprId sprId)
    {
        (SwfFileData data, ushort spriteId) = sprId;
        SwfSprite sprite = SwfSprite.CompileFrom(data.SpriteTags[spriteId]);
        return sprite;
    }

    public void Load(SwfFileData data, ushort spriteId) => Load((data, spriteId));
    public void LoadAsync(SwfFileData data, ushort spriteId) => LoadAsync((data, spriteId));
}