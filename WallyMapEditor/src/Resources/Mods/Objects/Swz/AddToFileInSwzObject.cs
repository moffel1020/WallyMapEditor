using System;
using System.IO;

namespace WallyMapEditor.Mod;

public class AddToFileInSwzObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required SwzFileEnum SwzFile { get; set; }
    public required string FileName { get; set; }
    public required string ElementContent { get; set; }

    internal static AddToFileInSwzObject Get(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        _ = (VersionEnum)stream.GetU8();
        SwzFileEnum swzFile = (SwzFileEnum)stream.GetU8();
        string fileName = stream.GetStr(buf);
        string elementContent = stream.GetLongStr(buf);
        return new()
        {
            SwzFile = swzFile,
            FileName = fileName,
            ElementContent = elementContent,
        };
    }

    internal void Put(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutU8((byte)SwzFile);
        stream.PutStr(buf, FileName);
        stream.PutLongStr(buf, ElementContent);
    }
}