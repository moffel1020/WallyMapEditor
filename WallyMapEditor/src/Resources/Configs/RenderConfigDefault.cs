using System;
using System.IO;
using System.Xml.Linq;
using Raylib_cs;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class RenderConfigDefault : IDeserializable, ISerializable
{
    public const string APPDATA_DIR_NAME = "WallyMapEditor";
    public const string FILE_NAME = "RenderConfigDefault.xml";

    public RenderConfig ConfigDefault { get; set; } = RenderConfig.Default;

    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        APPDATA_DIR_NAME,
        FILE_NAME
    );


    public static RenderConfigDefault Load()
    {
        if (!File.Exists(FilePath)) return new();
        try
        {
            return WmeUtils.DeserializeFromPath<RenderConfigDefault>(FilePath);
        }
        catch (Exception e)
        {
            Rl.TraceLog(TraceLogLevel.Error, $"Load default render config failed with error: {e.Message}");
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
            Rl.TraceLog(TraceLogLevel.Error, $"Save default render config failed with error: {e.Message}");
            Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
        }
    }

    public void Deserialize(XElement e)
    {
        ConfigDefault.Deserialize(e);
    }

    public void Serialize(XElement e)
    {
        ConfigDefault.Serialize(e);
    }

    public void ApplyCmdlineOverrides(CommandLineArgs args)
    {
        if (args.TryGetArg("--defaultConfig", out string? defaultConfig))
        {
            if (!File.Exists(defaultConfig))
                Rl.TraceLog(TraceLogLevel.Warning, "Default config path does not exist");
            else
                try
                {
                    ConfigDefault = WmeUtils.DeserializeFromPath<RenderConfigDefault>(defaultConfig).ConfigDefault;
                }
                catch
                {
                    Rl.TraceLog(TraceLogLevel.Warning, "Default config path leads to an invalid file");
                }
        }
    }
}