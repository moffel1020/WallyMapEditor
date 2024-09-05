using System;
using System.IO;

namespace WallyMapEditor.Mod;

public sealed class OverwriteFileInSwzObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required SwzFileEnum SwzFile { get; set; }
    public required string FileContent { get; set; }

    internal static OverwriteFileInSwzObject Get(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        _ = (VersionEnum)stream.GetU8();
        SwzFileEnum swzFile = (SwzFileEnum)stream.GetU8();
        string fileContent = stream.GetLongStr(buf);
        return new()
        {
            SwzFile = swzFile,
            FileContent = fileContent,
        };
    }

    internal void Put(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutU8((byte)SwzFile);
        stream.PutLongStr(buf, FileContent);
    }
}