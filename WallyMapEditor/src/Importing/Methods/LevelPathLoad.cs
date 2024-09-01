using WallyMapSpinzor2;

namespace WallyMapEditor;

public class LevelPathLoad(string path) : ILoadMethod
{
    public LoadedData Load() => new(WmeUtils.DeserializeFromPath<Level>(path), null, null);
}