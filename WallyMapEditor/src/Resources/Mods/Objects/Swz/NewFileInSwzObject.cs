using System.IO;

namespace WallyMapEditor.Mod;

public sealed class NewFileInSwzObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required SwzFileEnum SwzFile { get; set; }
    public required string FileContent { get; set; }

    internal static NewFileInSwzObject Get(Stream stream)
    {
        _ = (VersionEnum)stream.GetU8();
        SwzFileEnum swzFile = (SwzFileEnum)stream.GetU8();
        string fileContent = stream.GetLongStr();
        return new()
        {
            SwzFile = swzFile,
            FileContent = fileContent,
        };
    }

    internal void Put(Stream stream)
    {
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutU8((byte)SwzFile);
        stream.PutLongStr(FileContent);
    }
}