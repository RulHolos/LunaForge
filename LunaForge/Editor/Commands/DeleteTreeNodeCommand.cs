using LunaForge.Editor.LunaTreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Commands;

public class DeleteTreeNodeCommand : Command
{
    private readonly int index;
    private TreeNode toOperate;

    public DeleteTreeNodeCommand(TreeNode node)
    {
        toOperate = node;
        index = toOperate.ParentNode.Children.IndexOf(node);
    }

    public override void Execute()
    {
        toOperate.RaiseRemove(toOperate.ParentNode ?? toOperate);
        toOperate.ParentTree.DeselectAllNodes();
        toOperate.ParentNode.RemoveChild(toOperate);
    }

    public override void Undo()
    {
        toOperate.RaiseCreate(toOperate.ParentNode ?? toOperate);
        toOperate.ParentTree.DeselectAllNodes();
        toOperate.ParentNode.InsertChild(toOperate, index);
        toOperate.IsSelected = true;
    }

    public override string ToString() => $"Delete node {toOperate.NodeName}";
}
