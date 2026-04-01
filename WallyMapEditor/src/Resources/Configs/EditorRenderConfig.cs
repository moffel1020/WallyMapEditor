using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class EditorRenderConfig : IDeserializable<EditorRenderConfig>, ISerializable
{
    public required RenderConfig RenderConfig { get; set; }
    public required double RenderSpeed { get; set; }
    public required bool Paused { get; set; }

    public EditorRenderConfig() { }

    [SetsRequiredMembers]
    public EditorRenderConfig(EditorRenderConfig other)
    {
        RenderConfig = new(other.RenderConfig);
        RenderSpeed = other.RenderSpeed;
        Paused = other.Paused;
    }

    [SetsRequiredMembers]
    public EditorRenderConfig(XElement e)
    {
        RenderConfig = new(e);
        RenderSpeed = e.GetDoubleElement(nameof(RenderSpeed), 1);
        Paused = e.GetBoolElement(nameof(Paused), false);
    }
    public static EditorRenderConfig Deserialize(XElement e) => new(e);

    public void Serialize(XElement e)
    {
        RenderConfig.Serialize(e);
        e.AddChild(nameof(RenderSpeed), RenderSpeed);
        e.AddChild(nameof(Paused), Paused);
    }

    public static EditorRenderConfig Default => new()
    {
        RenderConfig = RenderConfig.Default,
        RenderSpeed = 1,
        Paused = false,
    };
}