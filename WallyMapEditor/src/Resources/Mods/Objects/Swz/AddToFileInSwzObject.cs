using System.IO;

namespace WallyMapEditor.Mod;

public sealed class AddToFileInSwzObject
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
        _ = (VersionEnum)stream.GetU8();
        SwzFileEnum swzFile = (SwzFileEnum)stream.GetU8();
        string fileName = stream.GetStr();
        string elementContent = stream.GetLongStr();
        return new()
        {
            SwzFile = swzFile,
            FileName = fileName,
            ElementContent = elementContent,
        };
    }

    internal void Put(Stream stream)
    {
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutU8((byte)SwzFile);
        stream.PutStr(FileName);
        stream.PutLongStr(ElementContent);
    }
}