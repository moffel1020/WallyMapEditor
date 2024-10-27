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
        _ = (VersionEnum)stream.GetU8();
        string modName = stream.GetStr();
        string gameVersionInfo = stream.GetStr();
        string modVersionInfo = stream.GetStr();
        string modDescription = stream.GetStr();
        string creatorInfo = stream.GetStr();
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
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutStr(ModName);
        stream.PutStr(GameVersionInfo);
        stream.PutStr(ModVersionInfo);
        stream.PutStr(ModDescription);
        stream.PutStr(CreatorInfo);
    }
}