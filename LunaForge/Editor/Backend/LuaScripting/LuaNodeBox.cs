using Lua;
using LunaForge.Editor.LunaTreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.LuaScripting;

[LuaObject]
public partial class LuaNodeBox
{
    public NodeBox NodeBox { get; set; }

    [LuaMember("create")]
    public static LuaNodeBox Create(string name, string namespc, string description, string authors)
    {
        return new()
        {
            NodeBox = new NodeBox(name, namespc, description,
                [.. authors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)])
        };
    }

    [LuaMember]
    public void Tab(string name, LuaTable tabTable)
    {

    }
}
