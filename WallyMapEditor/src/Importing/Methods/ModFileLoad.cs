using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using WallyMapSpinzor2;
using WallyMapEditor.Mod;
using System.Xml.Linq;

namespace WallyMapEditor;

public sealed class ModFileLoad(string path) : ILoadMethod, IDeserializable<ModFileLoad>
{
    public string FilePath { get; init; } = path;
    public ModFile? ModFile => _cachedFile?.Item1;

    private (ModFile, DateTime)? _cachedFile;

    [MemberNotNullWhen(false, nameof(_cachedFile))]
    private bool CacheInvalid => _cachedFile is null || File.GetLastWriteTimeUtc(FilePath) != _cachedFile.Value.Item2;

    public LoadedData Load(PathPreferences pathPrefs)
    {
        string brawlPath = pathPrefs.BrawlhallaPath ?? throw new ArgumentException("Not brawlhalla path chosen");

        ModFile file = LoadModFile();

        LevelDescObject ldo = file.LevelDescs.Single(); // mod files with multiple levels are currently not supported in the editor
        LevelDesc ld = WmeUtils.DeserializeFromString<LevelDesc>(ldo.FileContent, true);

        LevelTypeObject lto = file.LevelTypes.Single();
        LevelType lt = WmeUtils.DeserializeFromString<LevelType>(lto.ElementString, true);

        HashSet<string> playlists = file.LevelToPlaylistLinks.Where(l => l.LevelName == ld.LevelName).SelectMany(l => l.Playlists).ToHashSet();

        foreach (ExtraFileObject efo in file.ExtraFiles)
        {
            string destPath = Path.Combine(brawlPath, efo.FullPath);
            // path not in brawl dir. stinky!
            if (!WmeUtils.IsInDirectory(brawlPath, destPath))
            {
                Rl.TraceLog(Raylib_cs.TraceLogLevel.Warning, $"Mod file had a file with dangerous path {destPath}. It was skipped");
                continue;
            }
            string? pathDir = Path.GetDirectoryName(destPath);
            if (pathDir is not null && !Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);
            using FileStream objectStream = new(destPath, FileMode.Create, FileAccess.Write);
            objectStream.Write(efo.FileContent);
        }

        return new(new(ld, lt, playlists), null, null);
    }

    private ModFile LoadModFile()
    {
        ModFile file;
        if (CacheInvalid)
        {
            using FileStream stream = new(FilePath, FileMode.Open, FileAccess.Read);
            file = ModFile.Load(stream);
            _cachedFile = (file, File.GetLastWriteTimeUtc(FilePath));
        }

        return _cachedFile.Value.Item1;
    }

    public void CacheModFile()
    {
        if (CacheInvalid) LoadModFile();
    }

    public static ModFileLoad Deserialize(XElement e)
    {
        string path = e.GetElement("FilePath");
        return new(path);
    }

    public void Serialize(XElement e)
    {
        e.SetElementValue("FilePath", FilePath);
    }
}