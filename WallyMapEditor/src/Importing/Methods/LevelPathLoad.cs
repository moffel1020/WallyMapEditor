using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class LevelPathLoad(string path) : ILoadMethod
{
    public string Path => path;
    public LoadedData Load() => new(WmeUtils.DeserializeFromPath<Level>(path), null, null);
}