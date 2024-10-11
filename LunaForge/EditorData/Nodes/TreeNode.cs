using ImGuiNET;
using Newtonsoft.Json;
using rlImGui_cs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LunaForge.EditorData.Project;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using LunaForge.EditorData.Nodes.NodeData;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using YamlDotNet.Core;
using System.Collections;
using LunaForge.EditorData.Commands;
using LunaForge.EditorData.Traces;
using LunaForge.GUI;
using MoonSharp.Interpreter;

namespace LunaForge.EditorData.Nodes;

[Serializable]
public abstract class TreeNode : ITraceThrowable
{
    #region Properties

    [JsonIgnore]
    public NodeMeta MetaData { get; set; }

    [JsonIgnore]
    public int Hash;

    [JsonIgnore]
    public TreeNode Parent;

    [JsonIgnore]
    public LunaDefinition ParentDef;

    [JsonProperty, DefaultValue(false)]
    public bool IsBanned { get; set; }

    [JsonProperty, DefaultValue(false)]
    public bool IsSelected { get; set; }

    [JsonProperty, DefaultValue(false)]
    public bool IsExpanded { get; set; }

    [JsonIgnore]
    public List<EditorTrace> Traces { get; private set; } = [];

    [JsonIgnore]
    private ObservableCollection<TreeNode> children = [];
    [JsonIgnore]
    public ObservableCollection<TreeNode> Children
    {
        get => children;
        set
        {
            if (value == null)
            {
                children = [];
                children.CollectionChanged += new NotifyCollectionChangedEventHandler(ChildrenChanged);
            }
            else
            {
                throw new InvalidOperationException("Cannot set a Children as a null value.");
            }
        }
    }

    [JsonIgnore]
    public ObservableCollection<NodeAttribute> attributes = [];
    public ObservableCollection<NodeAttribute> Attributes
    {
        get => attributes;
        set
        {
            if (value == null)
            {
                attributes = [];
                attributes.CollectionChanged += new NotifyCollectionChangedEventHandler(AttributesChanged);
            }
            else
            {
                throw new InvalidOperationException("Cannot add a null attribute to a TreeNode.");
            }
        }
    }

    [JsonIgnore]
    public bool HasNoChildren => Children.Count <= 0;

    [JsonIgnore]
    protected bool Activated = false;
    [JsonIgnore]
    private TreeNode LinkedPrevious = null;
    [JsonIgnore]
    private TreeNode LinkedNext = null;

    [JsonIgnore]
    public string DisplayString => ToString();

    #endregion

    protected TreeNode()
    {
        OnDependencyAttributeChanged += new OnDependencyAttributeChangedHandler(ReflectAttr);
        //OnVirtualCreate += new OnCreateNodeHandler(CreateMeta);
        OnCreate += new OnCreateNodeHandler(OnCreateNode);
        //OnVirtualRemove += new OnRemoveNodeHandler(RemoveMeta);
        OnRemove += new OnRemoveNodeHandler(OnRemoveNode);
        MetaData = new(this);
        Children = null;
        Attributes = null;
    }

    public TreeNode(LunaDefinition def)
        : this()
    {
        ParentDef = def;
        Hash = ParentDef.TreeNodeMaxHash;
        ParentDef.TreeNodeMaxHash++;
    }

    public abstract new string ToString();

    #region Attributes

    private void AttributesChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        NodeAttribute attr;
        if (args.NewItems != null)
        {
            foreach (NodeAttribute na in args.NewItems)
            {
                attr = na;
                if (attr != null)
                    attr.ParentNode = this;
            }
        }
    }

    public NodeAttribute GetAttr(int n)
    {
        if (Attributes.Count > n)
            return attributes[n];
        else
            return null;
    }

    public NodeAttribute GetAttr(string name)
    {
        var attrs = from NodeAttribute na in attributes
                    where na != null && !string.IsNullOrEmpty(na.AttrName) && na.AttrName == name
                    select na;
        if (attrs != null && attrs.Any())
            return attrs.First();
        return null;
    }

    private void InsertAttrAt(int id, NodeAttribute target)
    {
        if (id >= Attributes.Count)
        {
            for (int i = Attributes.Count; i < id; i++)
                attributes.Add(null);
            attributes.Insert(id, target);
        }
        else
        {
            target.AttrValue = Attributes[id]?.AttrValue ?? "";
            attributes[id] = target;
        }
    }

    public NodeAttribute CheckAttr(int id, [CallerMemberName] string name = "", string editWindow = "", bool isDependency = false)
    {
        NodeAttribute na = GetAttr(id);
        if (na == null || string.IsNullOrEmpty(na.AttrName) || na.AttrName != name)
        {
            na = GetAttr(name);
            if (na != null)
            {
                SwapAttr(id, na);
            }
            else
            {
                if (isDependency)
                    na = new NodeDependencyAttribute(name, (string)null, editWindow);
                else
                    na = new NodeAttribute(name, (string)null, editWindow);
                InsertAttrAt(id, na);
            }
        }
        na.EditWindow = editWindow;
        return na;
    }

    private void SwapAttr(int id, NodeAttribute na)
    {
        if (id >= Attributes.Count)
            for (int i = Attributes.Count; i <= id; i++)
                attributes.Add(null);
        int id2 = attributes.IndexOf(na);
        NodeAttribute na2 = Attributes[id];
        attributes[id2] = na2;
        attributes[id] = na;
    }

    public virtual void ReflectAttr(NodeDependencyAttribute o, DependencyAttributeChangedEventArgs e) { }

    public static bool CheckVarName(string s)
    {
        Regex regex = new("^[a-zA-Z_][\\w\\d_]*$");
        return regex.IsMatch(s);
    }

    public NodeAttribute GetCreateInvoke()
    {
        int? invokeID = MetaData.CreateInvokeId;
        return invokeID.HasValue ? Attributes[invokeID.Value] : null;
    }

    public NodeAttribute GetRCInvoke()
    {
        int? id = MetaData.RCInvokeId;
        return id.HasValue ? Attributes[id.Value] : null;
    }

    #endregion
    #region Rendering

    public void RenderNodeContext()
    {
        if (ImGui.MenuItem("Edit"))
            ParentDef.ShowEditWindow(this);
        ImGui.Separator();
        if (ImGui.MenuItem($"Undo - {((ParentDef.CommandStack.Count != 0) ? ParentDef.CommandStack.Peek() : "///")}", "Ctrl+Z", false, ParentDef.CommandStack.Count > 0))
            ParentDef.Undo();
        if (ImGui.MenuItem($"Redo - {((ParentDef.UndoCommandStack.Count != 0) ? ParentDef.UndoCommandStack.Peek() : "///")}", "Ctrl+Y", false, ParentDef.UndoCommandStack.Count > 0))
            ParentDef.Redo();
        ImGui.Separator();
        if (ImGui.MenuItem("Cut", "Ctrl+X", false, ParentDef.CutNode_CanExecute()))
            ParentDef.CutNode();
        if (ImGui.MenuItem("Copy", "Ctrl+C", false, ParentDef.CopyNode_CanExecute()))
            ParentDef.CopyNode();
        if (ImGui.MenuItem("Paste", "Ctrl+V", false, ParentDef.PasteNode_CanExecute()))
            ParentDef.PasteNode();
        ImGui.Separator();
        if (ImGui.MenuItem("Delete", "Del", false, ParentDef.Delete_CanExecute()))
            ParentDef.Delete();
        ImGui.Separator();
        if (ImGui.MenuItem("Ban", string.Empty, IsBanned, !MetaData.CannotBeBanned))
            ParentDef.AddAndExecuteCommand(new SwitchBanCommand(this, !IsBanned));
        ImGui.Separator();
        if (ImGui.MenuItem("View Code"))
        {
            if (EditorTraceContainer.ContainSeverity(TraceSeverity.Error))
                NotificationManager.AddToast("There are errors inside your code.\nPlease fix them before viewing code.", ToastType.Error);
            else
            {
                StringBuilder code = new();
                foreach (string codeLine in ToLua(0))
                    code.Append(codeLine);
                MainWindow.ViewCodeWin.ResetAndShow(code.ToString());
            }
        }
        ImGui.Separator();
        if (ImGui.MenuItem("Save as Preset", string.Empty, false, MainWindow.NodeToPreset_CanExecute()))
        {
            MainWindow.NodeToPreset(this);
        }
    }

    #endregion
    #region Children

    public bool ValidateChild(TreeNode nodeToValidate)
    {
        return ValidateChild(nodeToValidate, this);
    }

    public bool ValidateChild(TreeNode nodeToValidate, TreeNode sourceNode)
    {
        if (sourceNode.MetaData.IsLeafNode)
            return false;
        if (nodeToValidate is LuaNode)
            if (!File.Exists((nodeToValidate as LuaNode).PathToLua))
                return false; // ?
        if (MetaData.IsFolder)
            return GetRealParent()?.ValidateChild(nodeToValidate, sourceNode) ?? true;
        if (nodeToValidate.MetaData.IsFolder)
        {
            foreach (TreeNode t in nodeToValidate.GetRealChildren())
            {
                if (!ValidateChild(t, sourceNode))
                    return false;
            }
            return true;
        }
        if (!nodeToValidate.CheckRequiredParentsValidation(this))
            return false;

        var e = this != sourceNode
                ? this.GetRealChildren().Concat(sourceNode.GetRealChildren()).Distinct()
                : GetRealChildren();
        if (!MatchUniqueness(nodeToValidate, sourceNode.GetRealChildren()))
            return false;

        Stack<TreeNode> stack = [];
        stack.Push(nodeToValidate);
        TreeNode current;
        while (stack.Count != 0)
        {
            current = stack.Pop();
            if (!current.CheckRequiredAncestorValidation(current, nodeToValidate.Parent, this, null))
                return false;
            if (current.MetaData.IsDefinition)
                if (!(MatchClassNode(current, nodeToValidate.Parent) || MatchClassNode(this, null)))
                    return false;
            foreach (TreeNode t in current.Children)
                stack.Push(t);
        }
        return true;
    }

    private bool CheckRequiredParentsValidation(TreeNode toMatch)
    {
        if (toMatch == null)
            return false;
        if (MetaData.RequireParent == null)
            return true;
        foreach (Type type in MetaData.RequireParent)
        {
            if (toMatch.GetType().Equals(type))
                return true;
        }
        return false;
    }

    private static bool MatchClassNode(TreeNode beg, TreeNode end)
    {
        while (beg != end)
        {
            if (beg.MetaData.IgnoreValidation)
                return true;
            if (!beg.MetaData.IsFolder && beg.GetType() != typeof(RootNode))
                return false;
            beg = beg.Parent;
        }
        return true;
    }

    /// <summary>
    /// This method tests whether a group of nodes <see cref="TreeNode"/> can be unique one if marked as uniqueness.
    /// </summary>
    /// <param name="nodeToValidate"></param>
    /// <param name="sourceChildren"></param>
    private static bool MatchUniqueness(TreeNode nodeToValidate, IEnumerable<TreeNode> sourceChildren)
    {
        if (!nodeToValidate.MetaData.Unique)
            return true;
        List<Type> foundTypes = [];
        foreach (TreeNode node in sourceChildren)
            foundTypes.Add(node.GetType());
        return !foundTypes.Any(x => x == nodeToValidate.GetType());
    }

    private bool CheckRequiredAncestorValidation(TreeNode Beg1, TreeNode End1, TreeNode Beg2, TreeNode End2)
    {
        Type[][] ts = MetaData.RequireAncestor;
        if (ts == null)
            return true;
        List<Type[]> toSatisfiedGroups = ts.ToList();
        List<Type> Satisfied = [];
        List<Type[]> toRemove = [];
        while (Beg1 != End1)
        {
            if (Beg1.MetaData.IgnoreValidation)
                return true;
            foreach (Type[] t1 in ts)
            {
                foreach (Type t2 in t1)
                {
                    if (Beg1.GetType().Equals(t2))
                        Satisfied.Add(t2);
                }
            }
            foreach (Type[] t1 in toSatisfiedGroups)
            {
                foreach (Type t2 in t1)
                {
                    foreach (Type t3 in Satisfied)
                    {
                        if (t2 == t3 && !toRemove.Contains(t1))
                            toRemove.Add(t1);
                    }
                }
            }
            foreach (Type[] t1 in toRemove)
            {
                toSatisfiedGroups.Remove(t1);
            }
            if (toSatisfiedGroups.Count == 0)
                return true;
            Satisfied.Clear();
            toRemove.Clear();
            Beg1 = Beg1.Parent;
        }
        while (Beg2 != End2)
        {
            if (Beg2.MetaData.IgnoreValidation)
                return true;
            foreach (Type[] t1 in ts)
            {
                foreach (Type t2 in t1)
                {
                    if (Beg2.GetType().Equals(t2))
                        Satisfied.Add(t2);
                }
            }
            foreach (Type[] t1 in toSatisfiedGroups)
            {
                foreach (Type t2 in t1)
                {
                    foreach (Type t3 in Satisfied)
                    {
                        if (t2 == t3 && !toRemove.Contains(t1))
                            toRemove.Add(t1);
                    }
                }
            }
            foreach (Type[] t1 in toRemove)
            {
                toSatisfiedGroups.Remove(t1);
            }
            if (toSatisfiedGroups.Count == 0)
                return true;
            Satisfied.Clear();
            toRemove.Clear();
            Beg2 = Beg2.Parent;
        }
        return false;
    }

    public TreeNode GetRealParent()
    {
        TreeNode p = Parent;
        while (p != null && p.MetaData.IsFolder)
            p = p.Parent;
        return p;
    }

    public IEnumerable<TreeNode> GetRealChildren()
    {
        foreach (TreeNode n in Children)
        {
            if (n.Parent == this)
            {
                if (n.MetaData.IsFolder)
                {
                    foreach (TreeNode t in n.GetRealChildren())
                        yield return t;
                }
                else
                {
                    yield return n;
                }
            }
        }
    }

    public bool CanLogicallyDelete()
    {
        if (MetaData.IsFolder)
        {
            foreach (TreeNode t in GetRealChildren())
            {
                if (t.MetaData.CannotBeDeleted)
                    return false;
            }
            return true;
        }
        else
        {
            return !MetaData.CannotBeDeleted;
        }
    }

    private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        TreeNode node, previousNode = null;
        if (e.OldItems != null)
        {
            for (int index = 0; index < e.OldItems.Count; index++)
            {
                node = (TreeNode)e.OldItems[index];
                node.RaiseRemove(new OnRemoveEventArgs() { Parent = this });
                if (index + e.OldStartingIndex != 0)
                {
                    if (previousNode == null)
                        previousNode = Children[index + e.OldStartingIndex - 1];
                    previousNode.LinkedNext = node.LinkedNext;
                    if (node.LinkedNext != null)
                        node.LinkedNext.LinkedPrevious = previousNode;
                }
                else
                {
                    LinkedNext = node.LinkedNext;
                    if (LinkedNext != null)
                        node.LinkedNext.LinkedPrevious = this;
                    previousNode = this;
                }
            }
        }
        if (e.NewItems != null)
        {
            for (int index = 0; index < e.NewItems.Count; index++)
            {
                node = (TreeNode)e.NewItems[index];
                node.RaiseCreate(new OnCreateEventArgs() { Parent = this });
                node.Parent = this;
                if (index + e.NewStartingIndex != 0)
                {
                    if (previousNode == null)
                        previousNode = Children[index + e.NewStartingIndex - 1];
                    node.LinkedPrevious = previousNode;
                    node.LinkedNext = previousNode.LinkedNext;
                    if (previousNode.LinkedNext != null)
                        previousNode.LinkedNext.LinkedPrevious = node;
                    previousNode.LinkedNext = node;
                }
                else
                {
                    node.LinkedPrevious = this;
                    node.LinkedNext = LinkedNext;
                    if (LinkedNext != null)
                        LinkedNext.LinkedPrevious = node;
                    LinkedNext = node;
                }
                previousNode = node;
            }
        }
    }

    public void AddChild(TreeNode child)
    {
        Children.Add(child);
    }

    public void InsertChild(TreeNode node, int index)
    {
        Children.Insert(index, node);
    }

    public void RemoveChild(TreeNode node)
    {
        ParentDef.SelectedNode = node.GetNearestEdited();
        Children.Remove(node);
    }

    public void ClearChildSelection()
    {
        IsSelected = false;
        foreach (TreeNode child in Children)
            child.ClearChildSelection();
    }

    #endregion
    #region Serializer

    public async Task SerializeToFile(StreamWriter sw, int level)
    {
        await sw.WriteLineAsync($"{level},{TreeSerializer.SerializeTreeNode(this)}");
        foreach (TreeNode node in Children)
            await node.SerializeToFile(sw, level + 1);
    }

    #endregion
    #region ToLua

    public virtual IEnumerable<string> ToLua(int spacing)
    {
        return ToLua(spacing, Children);
    }

    protected IEnumerable<string> ToLua(int spacing, IEnumerable<TreeNode> children)
    {
        foreach (TreeNode t in children)
        {
            if (!t.IsBanned)
            {
                foreach (var a in t.ToLua(spacing))
                {
                    yield return a;
                }
            }
        }
    }

    #endregion
    #region Data Handle

    public void CopyData(TreeNode source)
    {
        var attributes = from NodeAttribute attr in source.Attributes select (NodeAttribute)attr.Clone();
        var childrens = from TreeNode childn in source.Children select (TreeNode)childn.Clone();
        Attributes = null;
        foreach (NodeAttribute na in attributes)
            this.Attributes.Add(na);
        Children = null;
        foreach (TreeNode tn in childrens)
            this.Children.Add(tn);
        Parent = source.Parent;
    }

    #endregion
    #region Macros

    /*public static string ExecuteMacro(DefineMacroSettings macro, string original)
    {
        Regex regex = new Regex("\\b" + macro.ToBeReplaced + "\\b"
            + @"(?<=^([^""]*((?<!(^|[^\\])(\\\\)*\\)""([^""]|((?<=(^|[^\\])(\\\\)*\\)""))*(?<!(^|[^\\])(\\\\)*\\)"")+)*[^""]*.)"
            + @"(?<=^([^']*((?<!(^|[^\\])(\\\\)*\\)'([^']|((?<=(^|[^\\])(\\\\)*\\)'))*(?<!(^|[^\\])(\\\\)*\\)')+)*[^']*.)");
        return regex.Replace(original, macro.NewString);
    }*/

    protected string Macrolize(NodeAttribute attr)
    {
        return Macrolize(attr.AttrValue);
    }

    protected string Macrolize(int i)
    {
        if (i < Attributes.Count)
            return Macrolize(Attributes[i]);
        return "";
    }

    protected string Macrolize(string attr)
    {
        /*foreach (DefineMacroSettings macro in ParentDocument.CompileProcess.MacroDefinitions)
        {
            attr = ExecuteMacro(macro, attr);
        }*/
        return attr;
    }

    protected string NonMacrolize(NodeAttribute attr)
    {
        return attr.AttrValue;
    }

    protected string NonMacrolize(int i)
    {
        if (i < Attributes.Count)
            return NonMacrolize(Attributes[i]);
        return "";
    }

    #endregion
    #region Events

    private event OnCreateNodeHandler OnCreate;
    private event OnCreateNodeHandler OnVirtualCreate;
    private event OnRemoveNodeHandler OnRemove;
    private event OnRemoveNodeHandler OnVirtualRemove;
    private event OnDependencyAttributeChangedHandler OnDependencyAttributeChanged;

    public void RaiseCreate(OnCreateEventArgs e)
    {
        if (e.Parent == null)
        {
            if (!IsBanned)
                OnVirtualCreate?.Invoke(e);
            OnCreate?.Invoke(e);
            OnCreateEventArgs args = new() { Parent = this };
            foreach (TreeNode node in Children)
                node.RaiseCreate(args);
        }
    }

    public void RaiseVirtuallyCreate(OnCreateEventArgs e)
    {
        if (!IsBanned)
        {
            OnVirtualCreate?.Invoke(e);
            foreach (TreeNode node in Children)
                node.RaiseVirtuallyCreate(e);
        }
    }

    public void RaiseRemove(OnRemoveEventArgs e)
    {
        if (e.Parent == null || e.Parent.Activated)
        {
            if (!IsBanned)
            {
                OnVirtualRemove?.Invoke(e);
            }
            OnRemove?.Invoke(e);
            OnCreateEventArgs args = new OnCreateEventArgs { Parent = this };
            foreach (TreeNode node in Children)
                node.RaiseRemove(e);
        }
    }

    public void RaiseVirtuallyRemove(OnRemoveEventArgs e)
    {
        if (Activated && IsBanned)
        {
            OnVirtualRemove?.Invoke(e);
            foreach (TreeNode node in Children)
                node.RaiseVirtuallyRemove(e);
        }
    }

    #region Event Implementation

    private void OnCreateNode(OnCreateEventArgs e)
    {
        
    }

    private void OnRemoveNode(OnRemoveEventArgs e)
    {
        var traces = from EditorTrace editorTrace in EditorTraceContainer.Traces
                    where editorTrace.Source == this
                    select editorTrace;
        List<EditorTrace> list = new(traces);
        foreach (EditorTrace trace in list)
            EditorTraceContainer.Traces.Remove(trace);
    }

    #endregion
    #endregion
    #region Near

    public IEnumerator<TreeNode> GetForwardNodes()
    {
        TreeNode node = this;
        while (node.LinkedNext != null)
        {
            yield return node;
            node = node.LinkedNext;
        }
    }

    public IEnumerator<TreeNode> GetBackwardNodes()
    {
        TreeNode node = this;
        while (node.LinkedPrevious != null)
        {
            yield return node;
            node = node.LinkedPrevious;
        }
    }

    public TreeNode GetNearestEdited()
    {
        TreeNode node = Parent;
        if (node != null)
        {
            int id = node.children.IndexOf(this) - 1;
            if (id >= 0)
            {
                node = node.children[id];
            }
            return node;
        }
        else
        {
            return this;
        }
    }

    #endregion
    #region Traces

    public void RemoveTracesRecursive()
    {
        RemoveTracesRecursive(this);
    }

    public void RemoveTracesRecursive(TreeNode node)
    {
        EditorTraceContainer.RemoveChecksFromSource(node);
        foreach (TreeNode child in node.Children)
            RemoveTracesRecursive(child);
    }

    public virtual List<EditorTrace> GetTraces()
    {
        return [];
    }

    public void CheckTrace()
    {
        List<EditorTrace> traces = [];
        if (!IsBanned)
            traces = GetTraces();
        Traces.Clear();

        foreach (EditorTrace trace in traces)
            Traces.Add(trace);
        EditorTraceContainer.UpdateTraces(this);
    }

    public void CheckChildrenTraces()
    {
        CheckChildrenTraces(this);
    }

    public void CheckChildrenTraces(TreeNode node)
    {
        node.CheckTrace();
        foreach (TreeNode child in node.Children)
        {
            node.CheckChildrenTraces(child);
        }
    }

    #endregion

    public string Indent(int length) => string.Empty.PadLeft(4 * length);

    public abstract object Clone();
}
