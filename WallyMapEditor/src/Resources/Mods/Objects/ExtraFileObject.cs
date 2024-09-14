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

    public required FileTypeEnum FileType { get; set; }
    public required string FilePath { get; set; } // relative to brawlhalla dir. no extension.
    public required byte[] FileContent { get; set; }

    public string Extension => FileType switch
    {
        FileTypeEnum.PNG => ".png",
        FileTypeEnum.JPG => ".jpg",
        FileTypeEnum.ANM => ".anm",
        FileTypeEnum.BIN => ".bin",
        FileTypeEnum.BNK => ".bnk",
        _ => ""
    };

    public string FullPath => FilePath + Extension;

    internal static ExtraFileObject Get(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        _ = (VersionEnum)stream.GetU8();

        FileTypeEnum fileType = (FileTypeEnum)stream.GetU8();
        if (!Enum.IsDefined(fileType))
            throw new ModSerializationException($"Invalid file type {fileType} in mod file");

        string filePath = stream.GetStr(buf);
        byte[] fileContent = stream.GetBytes(buf);
        return new()
        {
            FileType = fileType,
            FilePath = filePath,
            FileContent = fileContent,
        };
    }

    internal void Put(Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutU8((byte)FileType);
        stream.PutStr(buf, FilePath);
        stream.PutBytes(buf, FileContent);
    }
}