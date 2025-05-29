using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using LunaForge.Editor.LunaTreeNodes;
using System.Xml.Linq;

namespace LunaForge.Editor.Projects;

[Serializable]
public class WorkTree : List<TreeNode>
{
    private ulong Hash = 0;

    public delegate void NodeAdded(TreeNode node);
    public delegate void NodeRemoved(TreeNode node);

    public event NodeAdded OnNodeAdded;
    public event NodeRemoved OnNodeRemoved;

    public new void Add(TreeNode node)
    {
        node.Hash = Hash;
        base.Add(node);
        Hash++;
        OnNodeAdded?.Invoke(node);
    }

    public new void Remove(TreeNode node)
    {
        base.Remove(node);
        OnNodeRemoved?.Invoke(node);
    }
}

public class LunaNodeTree : LunaProjectFile
{
    public WorkTree Nodes { get; set; } = [];

    public LunaNodeTree()
        : base()
    {

    }

    public override void Draw()
    {
        ImGui.BeginTable("TreeNodeTableLayout", 2,
            ImGuiTableFlags.Resizable
            | ImGuiTableFlags.Reorderable);
        ImGui.TableSetupColumn("Node Tree", ImGuiTableColumnFlags.WidthFixed, 1240f);
        ImGui.TableSetupColumn("Node Attributes", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.BeginChild($"TreeView", ImGui.GetContentRegionAvail());
        RenderTreeView();
        ImGui.EndChild();

        ImGui.TableSetColumnIndex(1);

        ImGui.BeginChild($"NodeAttributes", ImGui.GetContentRegionAvail());
        RenderAttributes();
        ImGui.EndChild();

        ImGui.EndTable();
    }

    #region TreeView

    private void RenderTreeView()
    {
        RenderTreeViewRecursive(Nodes);
    }

    private void RenderTreeViewRecursive(List<TreeNode> parents)
    {
        foreach (TreeNode child in parents)
        {
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Selected;

            if (child.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet;

            if (ImGui.TreeNodeEx($"{child}##{child.Hash}", flags))
            {
                RenderTreeViewRecursive(child.Children);

                ImGui.TreePop();
            }
        }
    }

    #endregion TreeView
    #region Data Attributes

    private void RenderAttributes()
    {
        ImGui.BeginTable("NodeAttributes", 3,
            ImGuiTableFlags.Resizable
            | ImGuiTableFlags.RowBg
            | ImGuiTableFlags.Borders);
        ImGui.TableSetupColumn("Attributes", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Values", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableHeadersRow();
        
        for (int i = 0; i < 5; i++)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text($"Attribute {i}");

            ImGui.TableSetColumnIndex(1);

            ImGui.Text($"Value {i}");

            ImGui.TableSetColumnIndex(2);

            ImGui.Button($"[...]##{i}");
        }

        ImGui.EndTable();
    }

    #endregion Data Attributes

    #region Node Logic

    public void AddNode(TreeNode node)
    {
        // Rework this to use a command instead of directly modifying the collection.
        // Like, the History for each file loaded.
        Nodes.Add(node);
        IsUnsaved = true;
    }

    #endregion

    public override void Dispose()
    {
        
    }
}
