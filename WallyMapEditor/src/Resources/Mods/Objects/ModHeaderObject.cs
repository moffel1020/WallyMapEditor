using System;
using System.IO;

namespace WallyMapEditor.Mod;

public sealed class ModHeaderObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required string ModName { get; set; }
    public required string GameVersionInfo { get; set; }
    public required string ModVersionInfo { get; set; }
    public required string ModDescription { get; set; }
    public required string CreatorInfo { get; set; }

    internal static ModHeaderObject Get(Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        _ = (VersionEnum)stream.GetU8();
        string modName = stream.GetStr(buf);
        string gameVersionInfo = stream.GetStr(buf);
        string modVersionInfo = stream.GetStr(buf);
        string modDescription = stream.GetStr(buf);
        string creatorInfo = stream.GetStr(buf);
        return new()
        {
            ModName = modName,
            GameVersionInfo = gameVersionInfo,
            ModVersionInfo = modVersionInfo,
            ModDescription = modDescription,
            CreatorInfo = creatorInfo,
        };
    }

    internal void Put(Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutStr(buf, ModName);
        stream.PutStr(buf, GameVersionInfo);
        stream.PutStr(buf, ModVersionInfo);
        stream.PutStr(buf, ModDescription);
        stream.PutStr(buf, CreatorInfo);
    }
}