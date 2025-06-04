using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using LunaForge.Editor.LunaTreeNodes;
using System.Xml.Linq;
using LunaForge.Editor.UI;
using Hexa.NET.Raylib;
using LunaForge.Editor.LunaTreeNodes.Nodes;

namespace LunaForge.Editor.Projects;

[Serializable]
public class WorkTree : List<TreeNode>
{
    public TreeNode? Root => this[0];
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

    public TreeNode? SelectedNode { get; set; }

    private bool justOpened = true;
    private bool justInserted = false;

    public LunaNodeTree()
        : base()
    {
        Nodes.Add(new RootNode());
    }

    public override void Draw()
    {
        // Node toolbox (tabs) and insert child control
        DrawHeader();

        // Tree and Attributes
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

    #region Data Control

    private void DrawHeader()
    {
        ImGui.BeginTable("#Table", 2, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.Button($"{FA.ArrowLeft}"); ImGui.SetItemTooltip("Ancestor"); ImGui.SameLine();
        ImGui.Button($"{FA.ArrowUp}"); ImGui.SetItemTooltip("Before"); ImGui.SameLine();
        ImGui.Button($"{FA.ArrowDown}"); ImGui.SetItemTooltip("After"); ImGui.SameLine();
        ImGui.Button($"{FA.ArrowRight}"); ImGui.SetItemTooltip("Child");

        ImGui.TableSetColumnIndex(1);

        if (ImGui.BeginTabBar("##NodeToolbox"))
        {
            if (ImGui.BeginTabItem("General"))
            {
                ImGui.Button("Add Node");
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.EndTable();
    }

    #endregion
    #region TreeView

    private void RenderTreeView()
    {
        if (Nodes.Count > 0) // There is no nodes in the WorkTree, so don't render anything. (Or else it crashes without any exceptions lmao)
            RenderTreeViewRecursive(Nodes.Root);
    }

    private void RenderTreeViewRecursive(TreeNode? node, bool parentBanned = false)
    {
        if (node.IsSelected && (SelectedNode != node || SelectedNode == null))
            SelectedNode = node;

        if ((justOpened || justInserted) && node.IsSelected)
        {
            ImGui.SetScrollHereY();
            justOpened = false;
            justInserted = false;
        }

        ImGui.PushID($"##Node-{node.Hash}");
        // Propagate banned state to children (only for display purposes)
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32((node.IsBanned || parentBanned) ? ImGuiCol.TextDisabled : ImGuiCol.Text));

        ImGuiTreeNodeFlags flags =
            ImGuiTreeNodeFlags.OpenOnArrow
            | ImGuiTreeNodeFlags.OpenOnDoubleClick
            | ImGuiTreeNodeFlags.SpanLabelWidth
            | ImGuiTreeNodeFlags.FramePadding;
        if (node == SelectedNode)
            flags |= ImGuiTreeNodeFlags.Selected;
        if (!node.HasChildren)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet;
        if (node.IsExpanded)
            flags |= ImGuiTreeNodeFlags.DefaultOpen;

        bool isOpen = ImGui.TreeNodeEx(node.DisplayString, flags);

        ImGui.PopStyleColor();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            SelectNode(node);
        if (ImGui.BeginPopupContextItem($"{node.Hash}_context"))
        {
            SelectNode(node);
            RenderContextMenu(node);
            ImGui.EndPopup();
        }

        ImGui.PopID();

        if (isOpen)
        {
            node.IsExpanded = true;
            if (node.HasChildren)
            {
                foreach (TreeNode child in node.Children)
                {
                    RenderTreeViewRecursive(child, node.IsBanned || parentBanned);
                }
            }
            ImGui.TreePop();
        }
        else
        {
            node.IsExpanded = false;
        }
    }

    private void SelectNode(TreeNode node)
    {
        if (SelectedNode != null)
            SelectedNode!.IsSelected = false;
        SelectedNode = node;
        node.IsSelected = true;
    }

    public void DeselectAllNodes()
    {
        SelectedNode = null;
        Nodes.Root.ClearChildSelection();
    }

    public void RevealNode(TreeNode? node)
    {
        if (node == null)
            return;
        TreeNode tmp = node.ParentNode;
        Nodes.Root.ClearChildSelection();
        Stack<TreeNode> stack = [];
        while (tmp != null)
        {
            stack.Push(tmp);
            tmp = tmp.ParentNode;
        }
        while (stack.Count > 0)
            stack.Pop().IsExpanded = true;
        SelectedNode = node;
        node.IsSelected = true;
        justInserted = true;
    }

    public void RenderContextMenu(TreeNode node)
    {
        if (ImGui.MenuItem($"{FA.PenToSquare} Edit", string.Empty, false, true))
        { } // TODO Edit Window

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.ArrowRotateLeft} Undo", "Ctrl+Z", false, false))
        { } // TODO Undo
        if (ImGui.MenuItem($"{FA.ArrowRotateRight} Redo", "Ctrl+Y", false, false))
        { } // TODO Undo

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.Cut} Cut", "Ctrl+X", false, false))
        { } // TODO Cut
        if (ImGui.MenuItem($"{FA.Copy} Copy", "Ctrl+C", false, false))
        { } // TODO Cut
        if (ImGui.MenuItem($"{FA.Paste} Paste", "Ctrl+V", false, false))
        { } // TODO Cut

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.Eraser} Delete", "Del", false, false))
        { } // TODO Delete

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.FilterCircleXmark} Ban", string.Empty, node.IsBanned, false))
        { } // TODO Ban

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.Code} View Code", string.Empty, false, false))
        { } // TODO View Code

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.FileCirclePlus} Save as Preset", string.Empty, false, false))
        { } // TODO Save as Preset
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
        //Nodes.Root.RemoveTraces();
        SelectedNode = null;
    }
}
