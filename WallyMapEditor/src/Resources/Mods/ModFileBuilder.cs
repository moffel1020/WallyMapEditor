using System;
using System.Collections.Generic;
using System.IO;
using WallyMapSpinzor2;

namespace WallyMapEditor.Mod;

public sealed class ModFileBuilder(ModHeaderObject header)
{
    private readonly ModHeaderObject _header = header;
    private readonly List<LevelDescObject> _levelDescs = [];
    private readonly List<LevelTypeObject> _levelTypes = [];
    private readonly List<LevelSetTypeObject> _levelSetTypes = [];
    private readonly List<LevelToPlaylistLinkObject> _levelToPlaylistLinks = [];
    private readonly List<AddToFileInSwzObject> _addToFilesInSwzs = [];
    private readonly List<NewFileInSwzObject> _newFilesInSwzs = [];
    private readonly List<OverwriteFileInSwzObject> _overwriteFilesInSwzs = [];
    private readonly List<ExtraFileObject> _extraFiles = [];

    public ModFile CreateMod() => new()
    {
        Header = _header,
        LevelDescs = [.. _levelDescs],
        LevelTypes = [.. _levelTypes],
        LevelSetTypes = [.. _levelSetTypes],
        LevelToPlaylistLinks = [.. _levelToPlaylistLinks],
        AddToFilesInSwzs = [.. _addToFilesInSwzs],
        NewFilesInSwzs = [.. _newFilesInSwzs],
        OverwriteFilesInSwzs = [.. _overwriteFilesInSwzs],
        ExtraFiles = [.. _extraFiles],
    };

    public void AddLevelDesc(LevelDesc levelDesc)
    {
        _levelDescs.Add(new() { FileContent = WmeUtils.SerializeToString(levelDesc, true, true) });
    }

    public void AddLevelType(LevelType levelType)
    {
        _levelTypes.Add(new() { ElementString = WmeUtils.SerializeToString(levelType, true, true) });
    }

    public void AddLevelSetType(LevelSetType levelSetType)
    {
        _levelSetTypes.Add(new() { ElementString = WmeUtils.SerializeToString(levelSetType, true, true) });
    }

    public void LinkLevelToPlaylists(string levelName, string[] playlists)
    {
        _levelToPlaylistLinks.Add(new() { LevelName = levelName, Playlists = playlists });
    }

    public void AddLevel(Level level)
    {
        AddLevelDesc(level.Desc);
        if (level.Type is not null) AddLevelType(level.Type);
        LinkLevelToPlaylists(level.Desc.LevelName, [.. level.Playlists]);
    }

    public void AddFilePath(string bhDir, string path)
    {
        if (!File.Exists(path))
            throw new ArgumentException("invalid path");
        if (!WmeUtils.IsInDirectory(bhDir, path))
            throw new ArgumentException("path has to be in the brawlhalla directory");

        string extension = Path.GetExtension(path);
        FileTypeEnum fileType = extension switch
        {
            ".png" => FileTypeEnum.PNG,
            ".jpg" => FileTypeEnum.JPG,
            ".anm" => FileTypeEnum.ANM,
            ".bin" => FileTypeEnum.BIN,
            ".bnk" => FileTypeEnum.BNK,
            _ => throw new ArgumentException($"given path has invalid extension: {extension}"),
        };

        byte[] content = File.ReadAllBytes(path);
        string relativePath = Path.ChangeExtension(Path.GetRelativePath(bhDir, path).Replace("\\", "/"), null);
        _extraFiles.Add(new()
        {
            FileType = fileType,
            FilePath = relativePath,
            FileContent = content
        });
    }
}