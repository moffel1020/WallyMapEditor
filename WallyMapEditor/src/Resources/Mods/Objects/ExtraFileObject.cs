using System;
using System.IO;

namespace WallyMapEditor.Mod;

public sealed class ExtraFileObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required string FilePath { get; set; } // relative to brawlhalla dir
    public required byte[] FileContent { get; set; }

    internal static ExtraFileObject Get(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        _ = (VersionEnum)stream.GetU8();
        string filePath = stream.GetStr(buf);
        byte[] fileContent = stream.GetBytes(buf);
        return new()
        {
            FilePath = filePath,
            FileContent = fileContent,
        };
    }

    internal void Put(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutStr(buf, FilePath);
        stream.PutBytes(buf, FileContent);
    }
}