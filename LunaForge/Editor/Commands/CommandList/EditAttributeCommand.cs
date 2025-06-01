using LunaForge.Editor.LunaTreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Commands.CommandList;

public sealed class EditAttributeCommand(NodeAttribute attr, string oldVal, string newVal) : Command
{
    private NodeAttribute Attr = attr;
    private string oldValue = oldVal;
    private string newValue = newVal;

    public override void Execute()
    {
        Attr.Value = newValue;
    }

    public override void Undo()
    {
        Attr.Value = oldValue;
    }

    public override string ToString() => $"Edit Attribute '{Attr.Name}' from '{oldValue}' to '{newValue}'";
}
