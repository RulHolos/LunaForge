using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeNode = LunaForge.EditorData.Nodes.TreeNode;

namespace LunaForge.EditorData.Commands;

public class InsertBeforeCommand : InsertCommand
{
    public InsertBeforeCommand(TreeNode source, TreeNode node)
        : base(source, node) { }

    public override void Execute()
    {
        TreeNode parent = Source.Parent;
        ToInsert.RaiseCreate(new() { Parent = parent });
        parent?.InsertChild(ToInsert, parent.Children.IndexOf(Source));
    }

    public override void Undo()
    {
        ToInsert.RaiseRemove(new() { Parent = ToInsert.Parent });
        Source.Parent?.RemoveChild(ToInsert);
    }
}