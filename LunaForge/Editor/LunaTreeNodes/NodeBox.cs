using Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes;

public class NodeBox(string name, string namespice, string desc, List<string> authors = null)
{
    public string Name { get; set; } = name;
    public string Namespace { get; set; } = namespice;
    public string Description { get; set; } = desc;
    public List<string> Authors { get; set; } = authors ?? [];

    public List<NodeBoxTab> Tabs { get; private set; } = [];

    public void AddTab(string name, List<object> nodes)
    {
        Tabs.Add(new(name, nodes));
    }

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

public class NodeBoxTab(string name, List<object> nodes = null)
{
    public string Name { get; private set; } = name;
    public List<object> Nodes { get; private set; } = nodes ?? [];
}

public class NodeBoxTabItem(string? name, bool isSeparator = false)
{
    public bool IsSeparator { get; private set; } = isSeparator;
}