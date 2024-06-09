using System.IO;

namespace WallyMapSpinzor2.Raylib;

public class SwfFileCache : ManagedCache<string, SwfFileData?>
{
    protected override SwfFileData? LoadInternal(string path)
    {
        path = Path.GetFullPath(path);
        SwfFileData swf;
        using (FileStream stream = new(path, FileMode.Open, FileAccess.Read))
            swf = SwfFileData.CreateFrom(stream);
        return swf;
    }
}