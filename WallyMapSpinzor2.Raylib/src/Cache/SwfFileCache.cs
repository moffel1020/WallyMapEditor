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

    public void LoadSwfAsync(string path)
    {
        lock (_loadingSwf)
        {
            if (_loadingSwf.Contains(path) || Cache.ContainsKey(path)) return;
            _loadingSwf.Add(path);
        }

        Task.Run(() =>
        {
            SwfFileData swf;
            using (FileStream stream = new(path, FileMode.Open, FileAccess.Read))
                swf = SwfFileData.CreateFrom(stream);
            Cache[path] = swf;
            lock (_loadingSwf)
            {
                _loadingSwf.Remove(path);
            }
        });
    }

    public void Clear()
    {
        Cache.Clear();
    }
}