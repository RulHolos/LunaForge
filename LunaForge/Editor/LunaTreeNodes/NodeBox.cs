using Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes;

[LuaObject]
public partial class NodeBox
{
    [LuaMember]
    public string Name { get; set; }
    [LuaMember]
    public string Namespace { get; set; }
    [LuaMember]
    public string Description { get; set; }
    public List<string> Authors { get; set; }

    public List<NodeBoxTab> Tabs { get; private set; } = [];

    public NodeBox() { }

    public NodeBox(string name, string namespice, string desc, List<string> authors = null)
    {
        Name = name;
        Namespace = namespice;
        Description = desc;
        Authors = authors ?? [];
    }

    [LuaMember("create")]
    public static NodeBox Create(string name, string namespc, string description, string authors)
    {
        return new(name, namespc, description,
            [.. authors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)]);
    }

    public void AddTab(string name, List<object> nodes)
    {
        Tabs.Add(new(name, nodes));
    }

    [LuaMember("add_tab")]
    public void AddTab(string name, LuaTable nodes)
    {
        NodeBoxTab tab = new(name);
        for (int i = 0; i < nodes.ArrayLength; i++)
        {
            if (nodes[i].Type == LuaValueType.Nil)
                tab.Nodes.Add(new NodeBoxTabItem(null, true));
            else if (nodes[i].Type == LuaValueType.Table)
                continue;
        }
        Tabs.Add(tab);
    }
}

[LuaObject]
public partial class NodeBoxTab(string name, List<object> nodes = null)
{
    public string Name { get; private set; } = name;
    public List<object> Nodes { get; private set; } = nodes ?? [];
}

[LuaObject]
public partial class NodeBoxTabItem(string? name, bool isSeparator = false)
{
    public bool IsSeparator { get; private set; } = isSeparator;
}