using LunaForge.Editor.LunaTreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Commands;

public class TreeDragDropCommand : Command
{
    private TreeNode DraggedNode { get; set; }
    private TreeNode NodeToInsert { get; set; }
    private TreeNode TargetNode { get; set; }
    private int OriginalIndex { get; set; }

    public TreeDragDropCommand(TreeNode drag, TreeNode target)
    {
        DraggedNode = drag;
        NodeToInsert = drag.Clone() as TreeNode;
        TargetNode = target;
        OriginalIndex = drag.ParentNode.Children.IndexOf(drag);
    }

    public override void Execute()
    {
        // Removing original
        DraggedNode.RaiseRemove(DraggedNode.ParentNode ?? DraggedNode);
        DraggedNode.ParentTree.DeselectAllNodes();
        DraggedNode.ParentNode.RemoveChild(DraggedNode);

        // Inserting into target
        NodeToInsert.RaiseCreate(TargetNode.ParentNode);
        TargetNode.AddChild(NodeToInsert);
        NodeToInsert.IsSelected = true;
    }

    public override void Undo()
    {
        // Removing cloned node
        NodeToInsert.RaiseRemove(NodeToInsert.ParentNode);
        TargetNode.RemoveChild(NodeToInsert);

        // Adding back original node
        DraggedNode.RaiseCreate(DraggedNode.ParentNode ?? DraggedNode);
        DraggedNode.ParentTree.DeselectAllNodes();
        DraggedNode.ParentNode.InsertChild(DraggedNode, OriginalIndex);
        DraggedNode.IsSelected = true;
    }

    public override string ToString()
    {
        return $"Drag-drop node {DraggedNode.NodeName} on {TargetNode.NodeName}";
    }
}
