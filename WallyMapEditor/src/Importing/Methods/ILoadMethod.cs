using WallyMapSpinzor2;

namespace WallyMapEditor;

public record LoadedData(Level Level, BoneTypes? Bones, string[]? Powers);

public interface ILoadMethod : ISerializable
{
    public LoadedData Load(PathPreferences pathPrefs);
}