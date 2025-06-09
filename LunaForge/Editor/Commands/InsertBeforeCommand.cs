using LunaForge.Editor.LunaTreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Commands;

public class InsertBeforeCommand : InsertCommand
{
    public InsertBeforeCommand(TreeNode source, TreeNode node)
        : base(source, node)
    { }

    public override void Execute()
    {
        TreeNode parent = Source.ParentNode;
        ToInsert.RaiseCreate(parent);
        parent?.InsertChild(ToInsert, parent.Children.IndexOf(Source));
    }

    public override void Undo()
    {
        ToInsert.RaiseRemove(ToInsert.ParentNode);
        Source.ParentNode?.RemoveChild(ToInsert);
    }

    public override string ToString() => $"Insert node {Source.NodeName} before";
}
