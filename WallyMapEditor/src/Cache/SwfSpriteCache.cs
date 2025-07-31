namespace WallyMapEditor;

public sealed class SwfSpriteCache : ManagedCache<SwfSpriteCache.SpriteInfo, SwfSprite?>
{
    public readonly record struct SpriteInfo(SwfFileData Swf, ushort SpriteId);

    protected override SwfSprite LoadInternal(SpriteInfo spriteInfo)
    {
        (SwfFileData swf, ushort spriteId) = spriteInfo;
        SwfSprite sprite = SwfSprite.CompileFrom(swf.SpriteTags[spriteId]);
        return sprite;
    }

    public void Load(SwfFileData swf, ushort spriteId) => Load(new(swf, spriteId));
    public void LoadInThread(SwfFileData swf, ushort spriteId) => LoadInThread(new(swf, spriteId));
}