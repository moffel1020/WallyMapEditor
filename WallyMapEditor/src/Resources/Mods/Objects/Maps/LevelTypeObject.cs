using System;
using System.IO;

namespace WallyMapEditor.Mod;

public class LevelTypeObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required string ElementString { get; set; }

    internal static LevelTypeObject Get(Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        _ = (VersionEnum)stream.GetU8();
        string elementString = stream.GetStr(buf);
        return new()
        {
            ElementString = elementString,
        };
    }

    internal void Put(Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutStr(buf, ElementString);
    }
}