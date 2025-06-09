using LunaForge.Editor.LunaTreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Commands;

public class InsertChildCommand : InsertCommand
{
    public InsertChildCommand(TreeNode source, TreeNode node)
        : base(source, node)
    { }

    public override void Execute()
    {
        TreeNode parent = Source.ParentNode;
        ToInsert.RaiseCreate(parent);
        Source.AddChild(ToInsert);
    }

    public override void Undo()
    {
        ToInsert.RaiseRemove(ToInsert.ParentNode);
        Source.RemoveChild(ToInsert);
    }

    public override string ToString() => $"Insert node {Source.NodeName} as child";
}
