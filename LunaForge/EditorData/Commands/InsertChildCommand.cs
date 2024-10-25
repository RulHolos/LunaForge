using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeNode = LunaForge.EditorData.Nodes.TreeNode;

namespace LunaForge.EditorData.Commands;

public class InsertChildCommand : InsertCommand
{
    public InsertChildCommand(TreeNode source, TreeNode node)
        : base(source, node) { }

    public override void Execute()
    {
        TreeNode parent = Source.Parent;
        ToInsert.RaiseCreate(new() { Parent = parent });
        Source.AddChild(ToInsert);
    }

    public override void Undo()
    {
        ToInsert.RaiseRemove(new() { Parent = ToInsert.Parent });
        Source.RemoveChild(ToInsert);
    }

    public override string ToString()
    {
        return $"Insert node {Source.NodeName} as child";
    }
}
