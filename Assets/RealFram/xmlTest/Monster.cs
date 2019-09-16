using System.Xml;
using System.Xml.Serialization;

public class Monster
{
    [XmlAttribute("name")]
    public string Name;
    [XmlAttribute("funny")]
    public string Funny;
    public int Health;

    public string Description;
}