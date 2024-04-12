using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WallyMapSpinzor2.Raylib;

public class SwfFileCache
{
    public ConcurrentDictionary<string, SwfFileData?> Cache { get; } = new();
    public HashSet<string> _loadingSwf = [];

    public void LoadSwf(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        Cache[path] = SwfFileData.CreateFrom(stream);
    }

    public async Task LoadSwfAsync(string path)
    {
        if (_loadingSwf.Contains(path) || Cache.ContainsKey(path)) return;
        _loadingSwf.Add(path);

        await Task.Run(() =>
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            SwfFileData swf = SwfFileData.CreateFrom(stream);
            Cache[path] = swf;
            _loadingSwf.Remove(path);
        });
    }

    public void Clear()
    {
        Cache.Clear();
    }
}