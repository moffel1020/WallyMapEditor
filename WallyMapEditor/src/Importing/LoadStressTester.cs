using System;
using System.IO;
using BrawlhallaSwz;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public static class LoadStressTester
{
    public static string StressTest(string brawlPath, uint key)
    {
        string dynamicPath = Path.Combine(brawlPath, "Dynamic.swz");
        string initPath = Path.Combine(brawlPath, "Init.swz");

        try
        {
            WmeUtils.DeserializeSwzFromPath<LevelTypes>(initPath, "LevelTypes.xml", key, bhstyle: true);
        }
        catch (Exception e)
        {
            return $"Error while loading LevelTypes: {e.Message}";
        }

        try
        {
            foreach (string file in WmeUtils.GetFilesInSwz(dynamicPath, key))
            {
                string fileName = SwzUtils.GetFileName(file);
                if (!fileName.StartsWith("LevelDesc_")) continue;
                try
                {
                    WmeUtils.DeserializeFromString<LevelDesc>(file, bhstyle: true);
                }
                catch (Exception e)
                {
                    return $"Error while loading map {fileName}: {e.Message}";
                }
            }
        }
        catch (Exception e)
        {
            return $"Error while reading Dynamic.swz: {e.Message}";
        }

        return "all good!";
    }
}