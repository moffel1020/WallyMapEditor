using System;
using System.IO;

namespace WallyMapEditor.Mod;

public class LevelDescObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required string FileContent { get; set; }

    internal static LevelDescObject Get(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        _ = (VersionEnum)stream.GetU8();
        string fileContent = stream.GetLongStr(buf);
        return new()
        {
            FileContent = fileContent,
        };
    }

    internal void Put(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutLongStr(buf, FileContent);
    }
}