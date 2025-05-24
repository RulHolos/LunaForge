using Hexa.NET.Raylib;
using LunaForge.Editor.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Windows.ProjectBrowser;

public struct BrowserItem : IEquatable<BrowserItem>
{
    public string Path;
    public string Name;
    public List<BrowserItem> GroupItems = [];
    public string NameNoExtension;
    public readonly Ref<Texture>? Thumbnail;

    public BrowserItem(string name, string path, object? metadata, Ref<Texture>? thumbnail)
    {
        Path = path;
        Name = name;
        NameNoExtension = System.IO.Path.GetFileNameWithoutExtension(name);
        Thumbnail = thumbnail;
    }


    public BrowserItem(string path)
    {
        Path = Name;
    }

    public override readonly bool Equals(object? obj) => obj is BrowserItem item && Equals(item);

    public readonly bool Equals(BrowserItem other) => Path == other.Path;

    public override readonly int GetHashCode() => HashCode.Combine(Path);

    public static bool operator ==(BrowserItem left, BrowserItem right) => left.Equals(right);

    public static bool operator !=(BrowserItem left, BrowserItem right) => !(left == right);

    //public static implicit operator BrowserFileInfo(BrowserItem item) => new(item.Path, item.Metadata);
}
