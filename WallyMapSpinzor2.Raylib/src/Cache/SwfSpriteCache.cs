using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SprId = System.ValueTuple<WallyMapSpinzor2.Raylib.SwfFileData, ushort>;

namespace WallyMapSpinzor2.Raylib;

public class SwfSpriteCache
{
    public ConcurrentDictionary<SprId, SwfSprite?> Cache { get; } = new();
    public HashSet<SprId> _loadingSprites = [];

    public void LoadSprite(SwfFileData data, ushort spriteId)
    {
        SwfSprite sprite = SwfSprite.CompileFrom(data.SpriteTags[spriteId]);
        Cache[(data, spriteId)] = sprite;
    }

    public void LoadSpriteAsync(SwfFileData data, ushort spriteId)
    {
        lock (_loadingSprites)
        {
            if (_loadingSprites.Contains((data, spriteId)) || Cache.ContainsKey((data, spriteId))) return;
            _loadingSprites.Add((data, spriteId));
        }

        Task.Run(() =>
        {
            SwfSprite sprite = SwfSprite.CompileFrom(data.SpriteTags[spriteId]);
            Cache[(data, spriteId)] = sprite;
            lock (_loadingSprites)
            {
                _loadingSprites.Remove((data, spriteId));
            }
        });
    }

    public void Clear()
    {
        Cache.Clear();
    }
}