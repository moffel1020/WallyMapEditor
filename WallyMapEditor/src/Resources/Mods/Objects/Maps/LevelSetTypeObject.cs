using System.IO;

namespace WallyMapEditor.Mod;

public sealed class LevelSetTypeObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required string ElementString { get; set; }

    internal static LevelSetTypeObject Get(Stream stream)
    {
        _ = (VersionEnum)stream.GetU8();
        string elementString = stream.GetStr();
        return new()
        {
            ElementString = elementString,
        };
    }

    internal void Put(Stream stream)
    {
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutStr(ElementString);
    }
}