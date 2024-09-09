using System.Collections.Generic;
using System.IO;
using System.Linq;

using WallyMapSpinzor2;
using WallyMapEditor.Mod;

namespace WallyMapEditor;

public class ModFileLoad(string path, string brawlPath) : ILoadMethod
{
    public string FilePath { get; init; } = path;
    public string BrawlPath { get; init; } = brawlPath;

    public LoadedData Load()
    {
        using FileStream stream = new(FilePath, FileMode.Open, FileAccess.Read);
        ModFile file = ModFile.Load(stream, false);

        LevelDescObject ldo = file.LevelDescs.Single(); // mod files with multiple levels are currently not supported in the editor
        LevelDesc ld = WmeUtils.DeserializeFromString<LevelDesc>(ldo.FileContent, true);

        LevelTypeObject lto = file.LevelTypes.Single();
        LevelType lt = WmeUtils.DeserializeFromString<LevelType>(lto.ElementString, true);

        HashSet<string> playlists = file.LevelToPlaylistLinks.Where(l => l.LevelName == ld.LevelName).SelectMany(l => l.Playlists).ToHashSet();

        Directory.CreateDirectory(Path.Combine(BrawlPath, "mapArt", ld.AssetDir));
        foreach (ExtraFileObject efo in file.ExtraFiles)
        {
            string destPath = Path.Combine(BrawlPath, efo.FullPath);
            using FileStream objectStream = new(destPath, FileMode.Create, FileAccess.Write);
            objectStream.Write(efo.FileContent);
        }

        return new(new(ld, lt, playlists), null, null);
    }
}