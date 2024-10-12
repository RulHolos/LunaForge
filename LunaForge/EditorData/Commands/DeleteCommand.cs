using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeNode = LunaForge.EditorData.Nodes.TreeNode;

namespace LunaForge.EditorData.Commands;

public class DeleteCommand : Command
{
    private readonly int index;
    private TreeNode toOperate;

    public DeleteCommand(TreeNode treeNode)
    {
        toOperate = treeNode;
        index = toOperate.Parent.Children.IndexOf(toOperate);
    }

    public override void Execute()
    {
        toOperate.RaiseRemove(new() { Parent = toOperate.Parent ?? toOperate });
        toOperate.ParentDef.DeselectAllNodes();
        toOperate.Parent.RemoveChild(toOperate);
    }

    public override void Undo()
    {
        toOperate.RaiseCreate(new() { Parent = toOperate.Parent ?? toOperate });
        toOperate.ParentDef.DeselectAllNodes();
        toOperate.Parent.InsertChild(toOperate, index);
        toOperate.IsSelected = true;
    }

    public override string ToString() => $"Delete node {toOperate.DisplayString}";
}
