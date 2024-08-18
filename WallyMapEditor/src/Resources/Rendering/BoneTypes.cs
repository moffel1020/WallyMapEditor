using System.Linq;
using System.Xml.Linq;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class BoneTypes : ISerializable, IDeserializable
{
    public string[] Bones { get; set; } = [];

    public void Deserialize(XElement e)
    {
        Bones = [.. e.Elements("Bone").Select(ee => ee.Value)];
    }

    public void Serialize(XElement e)
    {
        foreach (string bone in Bones)
            e.Add(new XElement("Bone", bone));
    }
}