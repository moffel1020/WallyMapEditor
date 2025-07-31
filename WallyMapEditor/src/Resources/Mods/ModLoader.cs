using WallyMapSpinzor2;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BrawlhallaSwz;
using System;

namespace WallyMapEditor.Mod;

using ModFileOverrides = Dictionary<string, byte[]>;

public sealed class ModLoader(string brawlDir)
{
    private readonly List<ModFile> _wallyFiles = [];

    private uint _swzKey = 0;

    private readonly Dictionary<string, string> _dynamicFiles = [];
    private readonly Dictionary<string, string> _engineFiles = [];
    private readonly Dictionary<string, string> _gameFiles = [];
    private readonly Dictionary<string, string> _initFiles = [];

    private uint _newFileCount = 0;

    public void AddModFiles(IEnumerable<ModFile> modfiles) => _wallyFiles.AddRange(modfiles);

    public ModFileOverrides Load()
    {
        LoadSwzFiles();
        AddModsToSwz(_wallyFiles);

        ModFileOverrides overrides = [];
        overrides.Add("Dynamic.swz", SaveSwzToByteArray(_dynamicFiles.Values));
        overrides.Add("Engine.swz", SaveSwzToByteArray(_engineFiles.Values));
        overrides.Add("Init.swz", SaveSwzToByteArray(_initFiles.Values));
        overrides.Add("Game.swz", SaveSwzToByteArray(_gameFiles.Values));

        AddExtraFiles(_wallyFiles, overrides);

        return overrides;
    }

    private void AddModsToSwz(IEnumerable<ModFile> mods)
    {
        AddLevelDescs(mods);
        AddLevelTypes(mods);

        const string LEVELSETTYPES_FILENAME = "LevelSetTypes.xml";
        LevelSetTypes playlists = WmeUtils.DeserializeFromString<LevelSetTypes>(_gameFiles[LEVELSETTYPES_FILENAME]);
        AddLevelSetTypes(mods, playlists);
        AddLevelToPlaylistLinks(mods, playlists);
        _gameFiles[LEVELSETTYPES_FILENAME] = WmeUtils.SerializeToString(playlists);

        AddNewAndOverwriteSwzObjects(mods);
        // TODO: AddToFileInSwzObject
    }

    private static void AddExtraFiles(IEnumerable<ModFile> mods, ModFileOverrides files)
    {
        foreach (ModFile m in mods)
        {
            foreach (ExtraFileObject efo in m.ExtraFiles)
                files.TryAdd(efo.FullPath, efo.FileContent);
        }
    }

    private void AddLevelDescs(IEnumerable<ModFile> mods)
    {
        foreach (ModFile m in mods)
        {
            foreach (LevelDescObject ld in m.LevelDescs)
            {
                string name = SwzUtils.GetFileName(ld.FileContent);
                _dynamicFiles[name] = ld.FileContent;
            }
        }
    }

    private void AddLevelTypes(IEnumerable<ModFile> mods)
    {
        const string LEVELTYPES_FILENAME = "LevelTypes.xml";
        LevelTypes lts = WmeUtils.DeserializeFromString<LevelTypes>(_initFiles[LEVELTYPES_FILENAME]);

        foreach (ModFile m in mods)
        {
            foreach (LevelTypeObject lto in m.LevelTypes)
            {
                LevelType ltNew = WmeUtils.DeserializeFromString<LevelType>(lto.ElementString);
                lts.AddOrUpdateLevelType(ltNew);
            }
        }

        _initFiles[LEVELTYPES_FILENAME] = WmeUtils.SerializeToString(lts);
    }

    private static void AddLevelSetTypes(IEnumerable<ModFile> mods, LevelSetTypes playlists)
    {
        foreach (ModFile m in mods)
        {
            foreach (LevelSetTypeObject lsto in m.LevelSetTypes)
            {
                LevelSetType lstNew = WmeUtils.DeserializeFromString<LevelSetType>(lsto.ElementString);
                playlists.Playlists = [.. playlists.Playlists, lstNew];
            }
        }
    }

    private static void AddLevelToPlaylistLinks(IEnumerable<ModFile> mods, LevelSetTypes playlists)
    {
        foreach (ModFile m in mods)
        {
            foreach (LevelToPlaylistLinkObject link in m.LevelToPlaylistLinks)
            {
                foreach (LevelSetType lst in playlists.Playlists)
                {
                    if (link.Playlists.Contains(lst.LevelSetName) && !lst.LevelSetName.Contains(link.LevelName))
                        lst.LevelTypes = [.. lst.LevelTypes, link.LevelName];
                }
            }
        }
    }

    private Dictionary<string, string> GetCachedSwzFiles(SwzFileEnum swzType) => swzType switch
    {
        SwzFileEnum.Engine => _engineFiles,
        SwzFileEnum.Init => _initFiles,
        SwzFileEnum.Game => _gameFiles,
        SwzFileEnum.Dynamic => _dynamicFiles,
        _ => throw new NotImplementedException(),
    };

    private void AddNewAndOverwriteSwzObjects(IEnumerable<ModFile> mods)
    {
        foreach (ModFile m in mods)
        {
            foreach (NewFileInSwzObject nfo in m.NewFilesInSwzs)
            {
                _newFileCount++;
                Dictionary<string, string> swzFiles = GetCachedSwzFiles(nfo.SwzFile);
                swzFiles.Add($"newfile{_newFileCount}", nfo.FileContent);
            }

            foreach (OverwriteFileInSwzObject ofo in m.OverwriteFilesInSwzs)
            {
                Dictionary<string, string> swzFiles = GetCachedSwzFiles(ofo.SwzFile);

                string name = SwzUtils.GetFileName(ofo.FileContent);
                if (!swzFiles.ContainsKey(name)) throw new Exception($"Tried to overwrite file in swz ({name}). But this file does not exist");

                swzFiles[SwzUtils.GetFileName(ofo.FileContent)] = ofo.FileContent;
            }
        }
    }

    private void LoadSwzFiles()
    {
        string airPath = Path.Combine(brawlDir, "BrawlhallaAir.swf");
        string dynamicPath = Path.Combine(brawlDir, "Dynamic.swz");
        string enginePath = Path.Combine(brawlDir, "Engine.swz");
        string gamePath = Path.Combine(brawlDir, "Game.swz");
        string initPath = Path.Combine(brawlDir, "Init.swz");

        _swzKey = WmeUtils.FindDecryptionKeyFromPath(airPath) ?? throw new Exception("Failed to find swz key");

        foreach (string file in WmeUtils.GetFilesInSwz(dynamicPath, _swzKey))
            _dynamicFiles.Add(SwzUtils.GetFileName(file), file);

        foreach (string file in WmeUtils.GetFilesInSwz(enginePath, _swzKey))
            _engineFiles.Add(SwzUtils.GetFileName(file), file);

        foreach (string file in WmeUtils.GetFilesInSwz(gamePath, _swzKey))
            _gameFiles.Add(SwzUtils.GetFileName(file), file);

        foreach (string file in WmeUtils.GetFilesInSwz(initPath, _swzKey))
            _initFiles.Add(SwzUtils.GetFileName(file), file);
    }

    private byte[] SaveSwzToByteArray(IEnumerable<string> files)
    {
        using MemoryStream ms = new();
        using SwzWriter writer = new(ms, _swzKey);
        foreach (string file in files)
            writer.WriteFile(file);

        writer.Flush();

        return ms.ToArray();
    }
}