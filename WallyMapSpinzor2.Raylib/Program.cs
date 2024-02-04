using System.Xml.Linq;
using WallyMapSpinzor2;

static T DeserializeFromPath<T>(string fromPath) 
    where T : IDeserializable, new()
{
    using FileStream fromFile = new(fromPath, FileMode.Open, FileAccess.Read);
    using StreamReader fsr = new(fromFile);
    XElement element;
    if (fromPath.EndsWith("OneUpOneDownFFA3.xml"))
    {
        element = XElement.Parse(MapUtils.FixBmg(fsr.ReadToEnd()));
    }
    else
    {
        element = XElement.Load(fsr);
    }
    return element.DeserializeTo<T>();
}

string brawlPath = args[0];
string dumpPath = args[1];
string fileName = args[2];

LevelDesc ld = DeserializeFromPath<LevelDesc>(Path.Join(dumpPath, "Dynamic", fileName).ToString());
LevelTypes lt = DeserializeFromPath<LevelTypes>(Path.Join(dumpPath, "Init", "LevelTypes.xml").ToString());
LevelSetTypes lst = DeserializeFromPath<LevelSetTypes>(Path.Join(dumpPath, "Game", "LevelSetTypes.xml").ToString());
IDrawable drawable = new Level(ld, lt, lst);

WallyMapSpinzor2.Raylib.Editor editor = new(brawlPath, drawable);
editor.Run();