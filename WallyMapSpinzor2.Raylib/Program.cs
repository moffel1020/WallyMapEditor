using System.IO;
using System.Xml.Linq;
using WallyMapSpinzor2;

static T DeserializeFromPath<T>(string fromPath)
    where T : IDeserializable, new()
{
    XElement element;
    using (FileStream fromFile = new(fromPath, FileMode.Open, FileAccess.Read))
    {
        element = XElement.Load(fromFile);
    }
    return element.DeserializeTo<T>();
}

string brawlPath = args[0];
string dumpPath = args[1];
string fileName = args[2];

LevelDesc ld = DeserializeFromPath<LevelDesc>(Path.Combine(dumpPath, "Dynamic", fileName));
LevelTypes lt = DeserializeFromPath<LevelTypes>(Path.Combine(dumpPath, "Init", "LevelTypes.xml"));
LevelSetTypes lst = DeserializeFromPath<LevelSetTypes>(Path.Combine(dumpPath, "Game", "LevelSetTypes.xml"));
IDrawable drawable = new Level(ld, lt, lst);

WallyMapSpinzor2.Raylib.Editor editor = new(brawlPath, drawable);
editor.Run();