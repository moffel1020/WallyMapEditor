using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Raylib_cs;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class RecentlyOpened : IDeserializable<RecentlyOpened>, ISerializable
{
    public const string APPDATA_DIR_NAME = "WallyMapEditor";
    public const string FILE_NAME = "RecentlyOpened.xml";

    public List<ILoadMethod> LoadMethods { get; } = [];

    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        APPDATA_DIR_NAME,
        FILE_NAME
    );


    public static RecentlyOpened Load()
    {
        if (!File.Exists(FilePath)) return new();
        try
        {
            return WmeUtils.DeserializeFromPath<RecentlyOpened>(FilePath);
        }
        catch (Exception e)
        {
            Rl.TraceLog(TraceLogLevel.Error, $"Load recently opened failed with error: {e.Message}");
            Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
            return new();
        }
    }

    public void Save()
    {
        try
        {
            string? dir = Path.GetDirectoryName(FilePath);
            if (dir is not null) Directory.CreateDirectory(dir);
            WmeUtils.SerializeToPath(this, FilePath);
        }
        catch (Exception e)
        {
            Rl.TraceLog(TraceLogLevel.Error, $"Save recently opened failed with error: {e.Message}");
            Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
        }
    }

    public RecentlyOpened() { }
    private RecentlyOpened(XElement e)
    {
        LoadMethods = e.Elements().Select<XElement, ILoadMethod?>((child) =>
        {
            try
            {
                return child.Name.LocalName switch
                {
                    "LevelPathLoad" => child.DeserializeTo<LevelPathLoad>(),
                    "ModFileLoad" => child.DeserializeTo<ModFileLoad>(),
                    _ => null,
                };
            }
            catch (Exception e)
            {
                Rl.TraceLog(TraceLogLevel.Error, $"Error while loading recently opened: {e.Message}");
                Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                return null;
            }
        }).Where((lm) => lm is not null).ToList()!;
    }
    public static RecentlyOpened Deserialize(XElement e) => new(e);

    public void Serialize(XElement e)
    {
        e.AddManySerialized(LoadMethods);
    }
}