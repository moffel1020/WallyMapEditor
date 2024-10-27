using System.IO;

namespace WallyMapEditor.Mod;

public sealed class LevelDescObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required string FileContent { get; set; }

    internal static LevelDescObject Get(Stream stream)
    {
        _ = (VersionEnum)stream.GetU8();
        string fileContent = stream.GetLongStr();
        return new()
        {
            FileContent = fileContent,
        };
    }

    internal void Put(Stream stream)
    {
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutLongStr(FileContent);
    }
}