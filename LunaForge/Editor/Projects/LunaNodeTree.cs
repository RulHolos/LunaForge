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
using Serilog;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.Commands;
using SQLitePCL;
using SQLite;
using Newtonsoft.Json;
using Hexa.NET.ImGui.Widgets;
using LunaForge.Editor.UI.Managers;

namespace LunaForge.Editor.Projects;

[Serializable]
public class WorkTree : List<TreeNode>
{
    public TreeNode? Root => this.Find(x => x.ParentNode == null); // Considered as the root node.
    private ulong Hash = 0;

    public delegate void NodeAdded(TreeNode node);
    public delegate void NodeRemoved(TreeNode node);

    public event NodeAdded? OnNodeAdded;
    public event NodeRemoved? OnNodeRemoved;

    public new void Add(TreeNode node)
    {
        if (!Contains(node))
        {
            node.Hash = Hash++;
            base.Add(node);
            OnNodeAdded?.Invoke(node);
        }
    }

    public new void Remove(TreeNode node)
    {
        if (Contains(node))
        {
            base.Remove(node);
            OnNodeRemoved?.Invoke(node);
        }
    }

    public void AddWithParent(TreeNode node, TreeNode? parent = null)
    {
        if (!Contains(node))
        {
            if (parent != null)
            {
                parent.AddChild(node);
                node.ParentNode = parent;
            }

            Add(node);
        }
    }
}

public enum InsertMode
{
    Ancestor,
    Before,
    After,
    Child,
}

public class LunaNodeTree : LunaProjectFile
{
    public static ILogger Logger = CoreLogger.Create("Node Tree");

    public InsertMode InsertMode { get; set; } = InsertMode.Child;

    public WorkTree Nodes { get; set; } = [];

    public TreeNode? SelectedNode { get; set; }
    public TreeNode? DraggedNode = null;

    private bool justOpened = true;
    private bool justInserted = false;

    private SQLiteConnection? database;
    public SQLiteConnection Database
    {
        get => database ??= new SQLiteConnection($"Data Source={FilePath}", SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
    }

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

        InsertButton($"{FA.ArrowLeft}", InsertMode.Ancestor); ImGui.SameLine();
        InsertButton($"{FA.ArrowUp}", InsertMode.Before); ImGui.SameLine();
        InsertButton($"{FA.ArrowDown}", InsertMode.After); ImGui.SameLine();
        InsertButton($"{FA.ArrowRight}", InsertMode.Child);

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

    private void InsertButton(string label, InsertMode mode)
    {
        if (InsertMode == mode)
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));
        else
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.Button));

        if (ImGui.Button(label))
            InsertMode = mode;

        ImGui.PopStyleColor();

        ImGui.SetItemTooltip(Enum.GetName(mode));
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

    public void RenderContextMenu(TreeNode node)
    {
        if (ImGui.MenuItem($"{FA.PenToSquare} Edit", string.Empty, false))
        { } // TODO Edit Window

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.ArrowRotateLeft} Undo", "Ctrl+Z", false, CanUndo))
            Undo();
        if (ImGui.MenuItem($"{FA.ArrowRotateRight} Redo", "Ctrl+Y", false, CanRedo))
            Redo();

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.Cut} Cut", "Ctrl+X", false, false))
            Cut();
        if (ImGui.MenuItem($"{FA.Copy} Copy", "Ctrl+C", false, false))
            Copy();
        if (ImGui.MenuItem($"{FA.Paste} Paste", "Ctrl+V", false, false))
            Paste();

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.Eraser} Delete", "Del", false, false))
            Delete();

        ImGui.Separator();

        if (ImGui.MenuItem($"{FA.FilterCircleXmark} Ban", string.Empty, node.IsBanned))
            AddAndExecuteCommand(new SwitchBanCommand(node, !node.IsBanned));

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
        if (SelectedNode == null)
            return;

        ImGui.BeginTable("NodeAttributes", 3,
            ImGuiTableFlags.Resizable
            | ImGuiTableFlags.RowBg
            | ImGuiTableFlags.Borders);
        ImGui.TableSetupColumn("Attributes", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Values", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableHeadersRow();
        
        for (int i = 0; i < SelectedNode.Attributes.Count; i++)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text($"{SelectedNode.Attributes[i].Name}");

            ImGui.TableSetColumnIndex(1);

            ImGui.Text($"{SelectedNode.Attributes[i].Value}");

            ImGui.TableSetColumnIndex(2);

            ImGui.Button($"[...]##Attr-{i}");
        }

        ImGui.EndTable();
    }

    #endregion Data Attributes
    #region Node Logic

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

    public unsafe void DoDragDropBehavior(TreeNode node)
    {
        //if (!node.MetaData.CannotBeDragged)
        //{
        if (ImGui.BeginDragDropSource())
        {
            ImGui.SetDragDropPayload("DRAG_TREE_NODE", null, 0);
            DraggedNode = node;
            ImGui.EndDragDropSource();
        }
        //}

        //if (!node.MetaData.CannotBeDragTarget)
        //{
        if (ImGui.BeginDragDropTarget())
        {
            if (!ImGui.AcceptDragDropPayload("DRAG_TREE_NODE").IsNull && DraggedNode != null)
            {
                if (node.ValidateChild(DraggedNode))
                    AddAndExecuteCommand(new TreeDragDropCommand(DraggedNode, node));
                DraggedNode = null;
            }
            ImGui.EndDragDropTarget();
        }
        //}
    }

    /// <summary>
    /// Command to lazyly add a node to the tree. Don't use for release, only debugging.
    /// </summary>
    /// <param name="node"></param>
    [Obsolete("Use Insert instead.")]
    public void AddNode(TreeNode node)
    {
        Nodes.Add(node);
    }

    public bool Insert(TreeNode parent, TreeNode node, bool doInvoke = true)
    {
        try
        {
            if (parent == null)
                return false;
            if (node.Children.Count > 0)
                node.IsExpanded = true;
            TreeNode oldSelection = parent;
            Command cmd = null;
            node.ParentTree = this;
            switch (InsertMode)
            {
                case InsertMode.Ancestor:
                    break;
                case InsertMode.Before:
                    if (oldSelection.ParentNode == null || !oldSelection.ParentNode.ValidateChild(node))
                        return false;
                    cmd = new InsertBeforeCommand(oldSelection, node);
                    break;
                case InsertMode.After:
                    if (oldSelection.ParentNode == null || !oldSelection.ParentNode.ValidateChild(node))
                        return false;
                    cmd = new InsertAfterCommand(oldSelection, node);
                    break;
                case InsertMode.Child:
                    if (!oldSelection.ValidateChild(node))
                        return false;
                    cmd = new InsertChildCommand(oldSelection, node);
                    break;
            }
            if (oldSelection.ParentNode == null && InsertMode != InsertMode.Child)
                return false;
            if (AddAndExecuteCommand(cmd))
            {
                RevealNode(node);
                if (doInvoke)
                {
                    CreateInvoke(node);
                }
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to insert node. Reason:\n{ex}");
            return false;
        }
    }

    public void CreateInvoke(TreeNode node)
    {
        /*
        NodeAttribute attr = node.GetCreateInvoke();
        if (attr != null)
        {
            
        }
        */
    }
    
    public bool Insert(TreeNode node, bool doInvoke = true) => Insert(SelectedNode, node, doInvoke);

    #endregion
    #region Command Logic

    TreeNode? CopyClipboard;

    public override void Cut()
    {
        try
        {
            CopyClipboard = (TreeNode)SelectedNode.Clone();
            TreeNode prev = SelectedNode.GetNearestEdited();
            AddAndExecuteCommand(new DeleteTreeNodeCommand(SelectedNode));
            if (prev != null)
                RevealNode(prev);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to cut node. Reason:\n{ex}");
        }
    }
    public override bool Cut_CanExecute()
    {
        if (SelectedNode != null)
            return SelectedNode.CanLogicallyDelete();
        return false;
    }

    public override void Copy()
    {
        try
        {
            CopyClipboard = (TreeNode)SelectedNode.Clone();
        }
        catch (Exception ex)
        {
            Logger.Error($"Cannot copy node. Reason:\n{ex}");
        }
    }
    public override bool Copy_CanExecute() => SelectedNode != null;

    public override void Paste()
    {
        try
        {
            TreeNode node = CopyClipboard;
            node.ParentTree = this;
            TreeNode newNode = (TreeNode)node.Clone();
            Insert(newNode, false);
        }
        catch (Exception ex)
        {
            Logger.Error($"Cannot paste node. Reason:\n{ex}");
        }
    }
    public override bool Paste_CanExecute() => SelectedNode != null && CopyClipboard != null;

    public override void Delete()
    {
        TreeNode? prev = SelectedNode?.GetNearestEdited();
        if (SelectedNode == null)
            return;
        AddAndExecuteCommand(new DeleteTreeNodeCommand(SelectedNode));
        if (prev != null)
            RevealNode(prev);
    }
    public override bool Delete_CanExecute()
    {
        if (SelectedNode == null)
            return false;
        return SelectedNode.CanLogicallyDelete();
    }

    #endregion
    #region Saving and Loading

    public static LunaNodeTree? Load(string filePath)
    {
        try
        {
            var file = new LunaNodeTree();

            using var db = new SQLiteConnection(filePath);
            db.CreateTable<TreeNodeRecord>();

            var rootSql = db.Table<TreeNodeRecord>().FirstOrDefault(x => x.ParentId == null);
            if (rootSql != null)
            {
                var rootNode = CreateNodeInstanceFromSql(file, rootSql);
                rootNode.Hash = rootSql.Id;
                rootNode.ParentTree = file;
                file.Nodes[0] = rootNode;
            }

            file.IsOpened = true;
            file.FilePath = filePath;

            return file;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load LunaNodeTree from file '{filePath}'. Reason:\n{ex}");
            MessageBox.Show("Failed to load file...", $"Failed to load from file '{filePath}': File corrupted or malformed.");
            return null;
        }
    }

    public static TreeNode CreateNodeInstanceFromSql(LunaNodeTree tree, TreeNodeRecord sql)
    {
        TreeNode node = (TreeNode)JsonConvert.DeserializeObject(sql.SerializedNode, Type.GetType(sql.NodeName));
        node.ParentNode = tree.Nodes.Find(x => x.Hash == sql.ParentId) ?? null;
        node.ParentTree = tree;

        return node;
    }

    public override void Save(bool saveAs = false)
    {
        void writeToFile(bool success, string path)
        {
            if (!success)
                return;
            using var db = new SQLiteConnection(path);
            db.CreateTable<TreeNodeRecord>();
            db.DeleteAll<TreeNodeRecord>();

            var flatNodes = FlattenTreeHelper.FlattenTree(Nodes.Root);
            foreach (var node in flatNodes)
                db.Insert(node);
        }

        if (!File.Exists(FilePath) || saveAs)
        {
            MainWindow.FileDialogManager.SaveFileDialog("Save Project File", "LunaForge Definition{.lfd}",
                FilePath, ".lfd", writeToFile, EditorConfig.Default.Get<string>("ProjectsFolder").Value);
        }
        else
        {
            writeToFile(true, FilePath);
        }
    }

    #endregion

    public override void Dispose()
    {
        //Nodes.Root.RemoveTraces();
        SelectedNode = null;
        database?.Close();
        database?.Dispose();
    }
}
