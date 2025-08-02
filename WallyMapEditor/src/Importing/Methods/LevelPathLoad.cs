using System.Xml.Linq;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class LevelPathLoad(string path) : ILoadMethod, IDeserializable<LevelPathLoad>
{
    public string Path => path;

    public LoadedData Load(PathPreferences pathPrefs) => new(WmeUtils.DeserializeFromPath<Level>(path), null, null);


    public static LevelPathLoad Deserialize(XElement e)
    {
        string path = e.GetElement("Path");
        return new(path);
    }

    public void Serialize(XElement e)
    {
        e.SetElementValue("Path", Path);
    }
}