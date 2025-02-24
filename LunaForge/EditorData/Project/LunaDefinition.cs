﻿using LunaForge.EditorData.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeNode = LunaForge.EditorData.Nodes.TreeNode;
using ImGuiNET;
using LunaForge.EditorData.Commands;
using System.ComponentModel;
using LunaForge.GUI;
using Raylib_cs;
using System.Numerics;
using rlImGui_cs;
using LunaForge.GUI.Helpers;
using LunaForge.EditorData.InputWindows;
using LunaForge.GUI.ImGuiFileDialog;
using LunaForge.EditorData.Traces;
using TextCopy;
using LunaForge.EditorData.Nodes.NodeData;
using Org.BouncyCastle.Asn1.Cms;
using System.Runtime.InteropServices;

namespace LunaForge.EditorData.Project;

/*
 * defcache:
 * Adds a new value to "accessible from" each time the definition is loaded from another definition.
 * 
 * In the definition's select object selector, check for every definition files and check if "accessible from" contains the name of the current definition file.
 * If so, let the user choose between all definitions inside "definitions"
 * 
 * Each "load definition file" node adds ALL the definitions contained inside the file to "definitions" field.
 * (Clear and add everyone back, this is happening at file saving.)
 */

public class LunaDefinition : LunaProjectFile
{
    public WorkTree TreeNodes { get; set; } = [];
    public int TreeNodeMaxHash { get; set; } = 0;

    public TreeNode? SelectedNode = null;

    public bool JustOpened = true;
    public bool JustInserted = false;

    public HashSet<CachedDefinition> Definitions { get; set; } = [];
    public HashSet<TreeNode> TempDefinitions { get; set; } = [];

    public TreeNode CopyClipboard { get; set; } = null;

    public LunaDefinition(LunaForgeProject parentProj, string path)
        : base(parentProj, path) { }

    #region Rendering

    private TreeNode? DraggedNode = null;

    public override void Render()
    {
        RenderNodeToolbar();
        ImGui.Separator();
        ImGui.BeginChild($"{FullFilePath}_nodetree");
        RenderTreeView(TreeNodes[0], TreeNodes[0].IsBanned);
        ImGui.EndChild();
    }

    private void RenderNodeToolbar()
    {
        if (ImGui.RadioButton("Insert Before", MainWindow.InsertMode == InsertMode.Before))
        {
            MainWindow.InsertMode = InsertMode.Before;
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Insert as Child", MainWindow.InsertMode == InsertMode.Child))
        {
            MainWindow.InsertMode = InsertMode.Child;
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Insert After", MainWindow.InsertMode == InsertMode.After))
        {
            MainWindow.InsertMode = InsertMode.After;
        }
    }

    private void RenderTreeView(TreeNode node, bool parentBanned)
    {
        if (node.IsSelected && (SelectedNode != node || SelectedNode == null))
            SelectedNode = node;

        if ((JustOpened || JustInserted) && node.IsSelected)
        {
            ImGui.SetScrollHereY();
            JustOpened = false;
            JustInserted = false;
        }

        ImGui.PushID($"{node.Hash}");

        // Propagate color to the child if the parent is banned.
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32((node.IsBanned || parentBanned) ? ImGuiCol.TextDisabled : ImGuiCol.Text));

        ImGuiTreeNodeFlags flags =
            ImGuiTreeNodeFlags.OpenOnArrow
            | ImGuiTreeNodeFlags.OpenOnDoubleClick
            | ImGuiTreeNodeFlags.SpanTextWidth
            | ImGuiTreeNodeFlags.FramePadding;
        if (node == SelectedNode)
            flags |= ImGuiTreeNodeFlags.Selected;
        if (node.HasNoChildren)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet;
        if (node.IsExpanded)
            flags |= ImGuiTreeNodeFlags.DefaultOpen;

        
        rlImGui.ImageSize(MainWindow.FindTexture(node.MetaData.Icon), 18, 18);
        ImGui.SameLine(0, 1.5f);

#if DEBUG
        bool isOpen = ImGui.TreeNodeEx($"{node.DisplayString} | Hash={node.Hash} | NodeName={node.NodeName}", flags);
#else
        bool isOpen = ImGui.TreeNodeEx(node.DisplayString, flags);
#endif

        DoDragDropBehavior(node);

        ImGui.PopStyleColor();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            SelectNode(node);
        if (ImGui.BeginPopupContextItem($"{node.Hash}_context"))
        {
            SelectNode(node);
            node.RenderNodeContext();
            ImGui.EndPopup();
        }

        ImGui.PopID();

        if (isOpen)
        {
            node.IsExpanded = true;
            if (!node.HasNoChildren)
            {
                foreach (TreeNode child in node.Children.ToList())
                {
                    RenderTreeView(child, node.IsBanned || parentBanned);
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
        //ImGui.SetScrollHereY(); // No.
    }

    public void DeselectAllNodes()
    {
        SelectedNode = null;
        TreeNodes[0].ClearChildSelection();
    }

    public void RevealNode(TreeNode node)
    {
        if (node == null)
            return;
        TreeNode temp = node.Parent;
        TreeNodes[0].ClearChildSelection();
        Stack<TreeNode> stack = [];
        while (temp != null)
        {
            stack.Push(temp);
            temp = temp.Parent;
        }
        while (stack.Count > 0)
            stack.Pop().IsExpanded = true;
        SelectedNode = node;
        node.IsSelected = true;
        JustInserted = true;
    }

    public unsafe void DoDragDropBehavior(TreeNode node)
    {
        if (!node.MetaData.CannotBeDragged)
        {
            if (ImGui.BeginDragDropSource())
            {
                ImGui.SetDragDropPayload("DRAG_TREE_NODE", 0, 0);
                DraggedNode = node;
                ImGui.Text($"Dragging node {node.NodeName}");
                ImGui.EndDragDropSource();
            }
        }

        if (!node.MetaData.CannotBeDragTarget)
        {
            if (ImGui.BeginDragDropTarget())
            {
                if (ImGui.AcceptDragDropPayload("DRAG_TREE_NODE").NativePtr != null && DraggedNode != null)
                {
                    if (node.ValidateChild(DraggedNode))
                        AddAndExecuteCommand(new DragDropCommand(DraggedNode, node));
                    DraggedNode = null;
                }
                ImGui.EndDragDropTarget();
            }
        }
    }

    #endregion
    #region Serialization

    public static async Task<WorkTree> DeserializeTree(StreamReader sr, LunaDefinition def)
    {
        WorkTree tree = [];
        TreeNode root = null;
        TreeNode parent = null;
        TreeNode tempNode = null;
        int previousLevel = -1;
        int i;
        int levelGraduation;
        string nodeToDeserialize;
        char[] temp;
        try
        {
            while (!sr.EndOfStream)
            {
                temp = (await sr.ReadLineAsync()).ToCharArray();
                i = 0;
                while (temp[i] != ',') i++;
                nodeToDeserialize = new string(temp, i + 1, temp.Length - i - 1);
                if (previousLevel != -1)
                {
                    levelGraduation = Convert.ToInt32(new string(temp, 0, i)) - previousLevel;
                    if (levelGraduation <= 0)
                    {
                        for (int j = 0; j >= levelGraduation; j--)
                        {
                            parent = parent.Parent;
                        }
                    }
                    tempNode = TreeSerializer.DeserializeTreeNode(def, nodeToDeserialize);
                    tempNode.ParentDef = def;
                    tempNode.Hash = def.TreeNodeMaxHash;
                    def.TreeNodeMaxHash++;
                    tempNode.RaiseCreate(new() { Parent = parent });
                    parent.AddChild(tempNode);
                    parent = tempNode;
                    previousLevel += levelGraduation;
                }
                else
                {
                    root = TreeSerializer.DeserializeTreeNode(def, nodeToDeserialize);
                    root.ParentDef = def;
                    root.Hash = def.TreeNodeMaxHash;
                    root.RaiseCreate(new() { Parent = root });
                    def.TreeNodeMaxHash++;
                    parent = root;
                    previousLevel = 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        tree.Add(root);
        return tree;
    }

    #endregion
    #region IO

    public override async Task Save(bool saveAs = false)
    {
        async Task SelectPathAsync(bool success, string path)
        {
            if (success)
            {
                await MakeCache();
                ParentProject.DefCache.AddToCache(this);
                FullFilePath = path;
                FileName = Path.GetFileName(path);
                PushSavedCommand();
                try
                {
                    using StreamWriter sw = new(path);
                    await SerializeToFile(sw);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }

        void SelectPath(bool success, string path)
        {
            SelectPathAsync(success, path);
        }

        if (string.IsNullOrEmpty(FullFilePath) || saveAs)
        {
            string lastUsedPath = Configuration.Default.LastUsedPath;
            MainWindow.FileDialogManager.SaveFileDialog("Save Definition", "LunaForge Definition{.lfd}",
                saveAs ? string.Empty : FileName, "LunaForge Definition{.lfd}", SelectPath, string.IsNullOrEmpty(lastUsedPath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : lastUsedPath, true);
        }
        else
        {
            await SelectPathAsync(true, FullFilePath);
        }
    }

    public async Task SerializeToFile(StreamWriter sw)
    {
        await TreeNodes[0].SerializeToFile(sw, 0);
    }

    public static async Task<LunaDefinition> CreateFromFile(LunaForgeProject parentProject, string filePath)
    {
        try
        {
            LunaDefinition definition = new(parentProject, filePath);
            using (StreamReader sr = new(filePath, Encoding.UTF8))
            {
                definition.TreeNodes = await DeserializeTree(sr, definition);
            }

            definition.TreeNodes[0]?.CheckChildrenTraces();

            if (definition.TreeNodes[0] == null)
                definition.TreeNodes[0] = new RootNode(definition);

            return definition;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return default;
        }
    }

    public override void Close()
    {
        TreeNodes[0].RemoveTracesRecursive();
        EditorTraceContainer.RemoveChecksFromSource(this);
        SelectedNode = null;
        ParentProject.ProjectFiles.Remove(this);
        ParentProject.CurrentProjectFile = null;
    }

    #endregion
    #region TreeNodes

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
            node.ParentDef = this;
            switch (MainWindow.InsertMode)
            {
                case InsertMode.Before:
                    if (oldSelection.Parent == null || !oldSelection.Parent.ValidateChild(node))
                        return false;
                    cmd = new InsertBeforeCommand(oldSelection, node);
                    break;
                case InsertMode.Child:
                    if (!oldSelection.ValidateChild(node))
                        return false;
                    cmd = new InsertChildCommand(oldSelection, node);
                    break;
                case InsertMode.After:
                    if (oldSelection.Parent == null || !oldSelection.Parent.ValidateChild(node))
                        return false;
                    cmd = new InsertAfterCommand(oldSelection, node);
                    break;
            }
            if (oldSelection.Parent == null && MainWindow.InsertMode != InsertMode.Child)
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
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    public bool Insert(TreeNode node, bool doInvoke = true)
    {
        return Insert(SelectedNode, node, doInvoke);
    }

    public void CreateInvoke(TreeNode node)
    {
        NodeAttribute attr = node.GetCreateInvoke();
        if (attr != null)
        {
            InputWindow inputWindow = InputWindowSelector.SelectInputWindow(attr, attr.EditWindow, attr.AttrValue);
            inputWindow.Invoke((result) =>
            {
                AddAndExecuteCommand(new EditAttributeCommand(attr, attr.AttrValue, result));
            });
        }
    }

    public void ShowEditWindow(TreeNode node, int? id = null)
    {
        NodeAttribute attr = (id.HasValue) ? node.Attributes[id.Value] : node.GetRCInvoke();
        if (attr != null)
        {
            InputWindow inputWindow = InputWindowSelector.SelectInputWindow(attr, attr.EditWindow, attr.AttrValue);
            inputWindow.Invoke((result) =>
            {
                AddAndExecuteCommand(new EditAttributeCommand(attr, attr.AttrValue, result));
            });
        }
    }

    public void CutNode()
    {
        try
        {
            CopyClipboard = (TreeNode)SelectedNode.Clone();
            //ClipboardService.SetText(TreeSerializer.SerializeTreeNode((TreeNode)SelectedNode.Clone()));
            TreeNode prev = SelectedNode.GetNearestEdited();
            AddAndExecuteCommand(new DeleteCommand(SelectedNode));
            if (prev != null)
                RevealNode(prev);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public bool CutNode_CanExecute()
    {
        if (SelectedNode != null)
            return SelectedNode.CanLogicallyDelete();
        return false;
    }

    public void CopyNode()
    {
        try
        {
            CopyClipboard = (TreeNode)SelectedNode.Clone();
            //ClipboardService.SetText(TreeSerializer.SerializeTreeNode((TreeNode)SelectedNode.Clone()));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public bool CopyNode_CanExecute() => SelectedNode != null;

    public void PasteNode()
    {
        try
        {
            TreeNode node = CopyClipboard;
            node.ParentDef = this;
            TreeNode newNode = (TreeNode)node.Clone();
            Insert(newNode, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public bool PasteNode_CanExecute() => SelectedNode != null && CopyClipboard != null;

    public override void Delete()
    {
        TreeNode? prev = SelectedNode?.GetNearestEdited();
        if (SelectedNode == null) return;
        AddAndExecuteCommand(new DeleteCommand(SelectedNode));
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
    #region DefCache

    /// <summary>
    /// Called at saving time. Construct the node cache from all temporary nodes.
    /// </summary>
    public async Task MakeCache()
    {
        Definitions.Clear();
        ParallelOptions options = new() { CancellationToken = CancellationToken.None };
        await Parallel.ForEachAsync<TreeNode>(TempDefinitions, options, async (node, token) =>
        {
            if (!node.MetaData.IsDefinition || node.IsBanned)
                return;

            string? className = node.GetAttribute("Name");
            if (!(string.IsNullOrEmpty(node.GetAttribute("Difficulty")) || node.GetAttribute("Difficulty") == "Any"))
                className += $":{node.GetAttribute("Difficulty")}";
            string[]? parameters = node.GetInitParameters(); // ?? ([.. node.Attributes.Select(attr => attr.AttrName)]);
            if (className != null)
            {
                AddNodeToCache(className, parameters, node.MetaData.MetaModel);
            }
        });
    }

    /// <summary>
    /// Adds a new definition to the cache.
    /// </summary>
    /// <param name="className">The class name, example: "motae_player". Use in _editor_class["motae_player"].</param>
    /// <param name="parameterList">An array of parameters for the init function.</param>
    /// <param name="metaModel"></param>
    public void AddNodeToCache(string className, string[] parameterList, string metaModel)
    {
        CachedDefinition def = new()
        {
            ClassName = className,
            Parameters = parameterList ?? [],
            MetaModelName = metaModel
        };
        if (!Definitions.Any(x => x.ClassName == def.ClassName))
            Definitions.Add(def);
    }

    public void RemoveNodeFromCache(string className)
    {
        Definitions.RemoveWhere(x => x.ClassName == className);
    }

    #endregion
}
