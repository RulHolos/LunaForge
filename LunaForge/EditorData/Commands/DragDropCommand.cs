using LunaForge.EditorData.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Commands;

public class DragDropCommand : Command
{
    private TreeNode DraggedNode { get; set; }
    private TreeNode NodeToInsert { get; set; }
    private TreeNode TargetNode { get; set; }
    private int OriginalIndex { get; set; }

    public DragDropCommand(TreeNode drag, TreeNode target)
    {
        DraggedNode = drag;
        NodeToInsert = (TreeNode)drag.Clone(); // Cloning the new node.
        TargetNode = target;
        OriginalIndex = drag.Parent.Children.IndexOf(drag);
    }

    public override void Execute()
    {
        // Removing original
        DraggedNode.RaiseRemove(new() { Parent = DraggedNode.Parent ?? DraggedNode });
        DraggedNode.ParentDef.DeselectAllNodes();
        DraggedNode.Parent.RemoveChild(DraggedNode);

        // Inserting into target
        NodeToInsert.RaiseCreate(new() { Parent = TargetNode.Parent });
        TargetNode.AddChild(NodeToInsert);
        NodeToInsert.IsSelected = true;
    }

    public override void Undo()
    {
        // Adding back original node
        DraggedNode.RaiseCreate(new() { Parent = DraggedNode.Parent ?? DraggedNode });
        DraggedNode.ParentDef.DeselectAllNodes();
        DraggedNode.Parent.InsertChild(DraggedNode, OriginalIndex);
        DraggedNode.IsSelected = true;

        // Removing cloned node
        NodeToInsert.RaiseRemove(new() { Parent = NodeToInsert.Parent });
        TargetNode.RemoveChild(NodeToInsert);
    }

    public override string ToString()
    {
        return $"Drag-drop node {DraggedNode.NodeName} on {TargetNode.NodeName}";
    }
}
