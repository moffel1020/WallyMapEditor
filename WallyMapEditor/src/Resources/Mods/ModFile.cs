using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace WallyMapEditor.Mod;

public sealed class ModFile
{
    public const string EXTENSION = "wally";

    public required ModHeaderObject Header { get; set; }
    // map objects
    public LevelDescObject[] LevelDescs { get; set; } = [];
    public LevelTypeObject[] LevelTypes { get; set; } = [];
    public LevelSetTypeObject[] LevelSetTypes { get; set; } = [];
    public LevelToPlaylistLinkObject[] LevelToPlaylistLinks { get; set; } = [];
    // swz objects
    public AddToFileInSwzObject[] AddToFilesInSwzs { get; set; } = [];
    public NewFileInSwzObject[] NewFilesInSwzs { get; set; } = [];
    public OverwriteFileInSwzObject[] OverwriteFilesInSwzs { get; set; } = [];
    // misc
    public ExtraFileObject[] ExtraFiles { get; set; } = [];

    public static ModFile Load(Stream stream, bool leaveOpen = false)
    {
        Span<byte> buf = stackalloc byte[4];
        ModVersionEnum version = (ModVersionEnum)stream.GetU32(buf);
        if (version > ModVersionEnum.LATEST)
            throw new ModSerializationException($"Invalid mod file version {version}");

        using ZLibStream zLibStream = new(stream, CompressionMode.Decompress, leaveOpen);

        ObjectTypeEnum firstObject = (ObjectTypeEnum)zLibStream.GetU8();
        if (firstObject != ObjectTypeEnum.Header)
            throw new ModSerializationException($"Invalid mod file. First object must be the header.");

        ModHeaderObject header = ModHeaderObject.Get(zLibStream);

        List<LevelDescObject> levelDescs = [];
        List<LevelTypeObject> levelTypes = [];
        List<LevelSetTypeObject> levelSetTypes = [];
        List<LevelToPlaylistLinkObject> levelToPlaylistLinks = [];
        List<AddToFileInSwzObject> addToFilesInSwzs = [];
        List<NewFileInSwzObject> newFilesInSwzs = [];
        List<OverwriteFileInSwzObject> overwriteFilesInSwzs = [];
        List<ExtraFileObject> extraFiles = [];

        bool gotToEnd = false;
        while (!gotToEnd)
        {
            ObjectTypeEnum objectType = (ObjectTypeEnum)zLibStream.GetU8();
            switch (objectType)
            {
                case ObjectTypeEnum.Header:
                    throw new ModSerializationException($"Invalid mod file. There should only be one header object.");
                case ObjectTypeEnum.LevelDesc:
                    levelDescs.Add(LevelDescObject.Get(zLibStream));
                    break;
                case ObjectTypeEnum.LevelType:
                    levelTypes.Add(LevelTypeObject.Get(zLibStream));
                    break;
                case ObjectTypeEnum.LevelSetType:
                    levelSetTypes.Add(LevelSetTypeObject.Get(zLibStream));
                    break;
                case ObjectTypeEnum.LevelToPlaylistLink:
                    levelToPlaylistLinks.Add(LevelToPlaylistLinkObject.Get(zLibStream));
                    break;
                case ObjectTypeEnum.AddToFileInSwz:
                    addToFilesInSwzs.Add(AddToFileInSwzObject.Get(zLibStream));
                    break;
                case ObjectTypeEnum.NewFileInSwz:
                    newFilesInSwzs.Add(NewFileInSwzObject.Get(zLibStream));
                    break;
                case ObjectTypeEnum.OverwriteFileInSwz:
                    overwriteFilesInSwzs.Add(OverwriteFileInSwzObject.Get(zLibStream));
                    break;
                case ObjectTypeEnum.ExtraFile:
                    extraFiles.Add(ExtraFileObject.Get(zLibStream));
                    break;
                case ObjectTypeEnum.END:
                    gotToEnd = true;
                    break;
                default:
                    throw new ModSerializationException($"Invalid mod file object type {objectType}");
            }
        }

        return new()
        {
            Header = header,
            LevelDescs = [.. levelDescs],
            LevelTypes = [.. levelTypes],
            LevelSetTypes = [.. levelSetTypes],
            LevelToPlaylistLinks = [.. levelToPlaylistLinks],
            AddToFilesInSwzs = [.. addToFilesInSwzs],
            NewFilesInSwzs = [.. newFilesInSwzs],
            OverwriteFilesInSwzs = [.. overwriteFilesInSwzs],
            ExtraFiles = [.. extraFiles],
        };
    }

    public void Save(Stream stream, bool leaveOpen = false)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.PutU32(buf, (uint)ModVersionEnum.LATEST);
        using ZLibStream zLibStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);

        zLibStream.PutU8((byte)ObjectTypeEnum.Header);
        Header.Put(zLibStream);

        foreach (LevelDescObject levelDesc in LevelDescs)
        {
            zLibStream.PutU8((byte)ObjectTypeEnum.LevelDesc);
            levelDesc.Put(zLibStream);
        }

        foreach (LevelTypeObject levelType in LevelTypes)
        {
            zLibStream.PutU8((byte)ObjectTypeEnum.LevelType);
            levelType.Put(zLibStream);
        }

        foreach (LevelSetTypeObject levelSetType in LevelSetTypes)
        {
            zLibStream.PutU8((byte)ObjectTypeEnum.LevelSetType);
            levelSetType.Put(zLibStream);
        }

        foreach (LevelToPlaylistLinkObject levelToPlaylistLink in LevelToPlaylistLinks)
        {
            zLibStream.PutU8((byte)ObjectTypeEnum.LevelToPlaylistLink);
            levelToPlaylistLink.Put(zLibStream);
        }

        foreach (AddToFileInSwzObject addToFileInSwz in AddToFilesInSwzs)
        {
            zLibStream.PutU8((byte)ObjectTypeEnum.AddToFileInSwz);
            addToFileInSwz.Put(zLibStream);
        }

        foreach (NewFileInSwzObject newFileInSwz in NewFilesInSwzs)
        {
            zLibStream.PutU8((byte)ObjectTypeEnum.NewFileInSwz);
            newFileInSwz.Put(zLibStream);
        }

        foreach (OverwriteFileInSwzObject overwriteFileInSwz in OverwriteFilesInSwzs)
        {
            zLibStream.PutU8((byte)ObjectTypeEnum.OverwriteFileInSwz);
            overwriteFileInSwz.Put(zLibStream);
        }

        foreach (ExtraFileObject extraFile in ExtraFiles)
        {
            zLibStream.PutU8((byte)ObjectTypeEnum.ExtraFile);
            extraFile.Put(zLibStream);
        }

        zLibStream.PutU8((byte)ObjectTypeEnum.END);
    }
}