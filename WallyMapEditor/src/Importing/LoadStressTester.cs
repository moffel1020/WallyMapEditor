using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrawlhallaSwz;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public static class LoadStressTester
{
    public static string StressTest(string brawlPath, uint key)
    {
        string dynamicPath = Path.Combine(brawlPath, "Dynamic.swz");
        string initPath = Path.Combine(brawlPath, "Init.swz");

        LevelTypes lt = WmeUtils.DeserializeSwzFromPath<LevelTypes>(initPath, "LevelTypes.xml", key, bhstyle: true)!;
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

        return "all good!";
    }
}