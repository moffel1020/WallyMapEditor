using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace WallyMapSpinzor2.Raylib;

public class SwfFileCache
{
    public ConcurrentDictionary<string, SwfFileData?> Cache { get; } = new();

    public void LoadSwf(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        Cache[path] = SwfFileData.CreateFrom(stream);
    }

    public async Task LoadSwfAsync(string path)
    {
        if (Cache.ContainsKey(path)) return;

        await Task.Run(() =>
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            SwfFileData swf = SwfFileData.CreateFrom(stream);
            Cache[path] = swf;
        });
    }

    public void Clear()
    {
        Cache.Clear();
    }
}