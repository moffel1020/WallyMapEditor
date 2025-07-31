using System.Linq;
using System.Xml.Linq;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class BoneTypes : ISerializable, IDeserializable<BoneTypes>
{
    public string[] Bones { get; set; } = [];

    private BoneTypes(XElement e)
    {
        Bones = [.. e.Elements("Bone").Select(ee => ee.Value)];
    }
    public static BoneTypes Deserialize(XElement e) => new(e);

    public void Serialize(XElement e)
    {
        foreach (string bone in Bones)
            e.Add(new XElement("Bone", bone));
    }
}