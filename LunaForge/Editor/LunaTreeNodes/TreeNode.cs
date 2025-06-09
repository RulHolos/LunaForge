using LunaForge.Editor.Projects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes;

/*
 * Do that in a lua script, not TreeNode
 */

public abstract class TreeNode : IDisposable, ICloneable
{
    /// <summary>
    /// Things like IsFolder, IsLeaf, CannotBeDeleted, ...
    /// </summary>
    public TreeNodeMetaData MetaData { get; set; }

    [JsonProperty, DefaultValue(false)]
    public bool IsSelected { get; set; }

    [JsonProperty, DefaultValue(false)]
    public bool IsExpanded { get; set; }

    [JsonProperty, DefaultValue(false)]
    public bool IsBanned { get; set; }

    [JsonIgnore]
    public ulong Hash { get; set; }

    [JsonIgnore]
    public TreeNode ParentNode;

    [JsonIgnore]
    public LunaNodeTree ParentTree;

    [JsonIgnore]
    private ObservableCollection<TreeNode> children = [];
    [JsonIgnore]
    public ObservableCollection<TreeNode> Children
    {
        get => children;
        set => children = value;
    }
    [JsonIgnore]
    public bool HasChildren => Children.Count > 0;

    [JsonIgnore]
    private ObservableCollection<NodeAttribute> attributes = [];
    [JsonIgnore]
    public ObservableCollection<NodeAttribute> Attributes
    {
        get => attributes;
        set {
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

    public TreeNode()
    {
        OnNodeAttributeChanged += RaiseAttributeChanged;
        MetaData = new(this);
        OnCreate?.Invoke(this);
    }

    public TreeNode(LunaNodeTree parentTree)
    {
        ParentTree = parentTree;
    }

    public void Dispose()
    {
        OnRemove?.Invoke(this);
    }

    [JsonIgnore]
    public string DisplayString => ToString();
    [JsonIgnore]
    public virtual string NodeName { get; set; } = string.Empty;

    public abstract object Clone();

    #region Children logic

    // Validation and other things

    public bool ValidateChild(TreeNode toValidate)
    {
        return ValidateChild(this, toValidate);
    }

    public static bool ValidateChild(TreeNode parent, TreeNode toValidate)
    {
        return true;
    }

    public TreeNode GetRealParent()
    {
        TreeNode p = ParentNode;
        while (p != null && p.MetaData.IsFolder)
            p = p.ParentNode;
        return p;
    }

    public IEnumerable<TreeNode> GetRealChildren()
    {
        foreach (TreeNode n in Children)
        {
            if (n.ParentNode == this)
            {
                if (n.MetaData.IsFolder)
                    foreach (TreeNode t in n.GetRealChildren())
                        yield return t;
                else
                    yield return n;
            }
        }
    }

    public bool CanLogicallyDelete()
    {
        if (MetaData.IsFolder)
        {
            foreach (TreeNode t in GetRealChildren())
                if (t.MetaData.CannotBeDeleted)
                    return false;
            return true;
        }
        else
            return !MetaData.CannotBeDeleted;
    }

    private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs args)
    {

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
        ParentTree.SelectedNode = node.GetNearestEdited();
        Children.Remove(node);
    }

    public void ClearChildSelection()
    {
        IsSelected = false;
        foreach (TreeNode child in children)
            child.ClearChildSelection();
    }

    #endregion
    #region Attributes

    private void AttributesChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        NodeAttribute attr;
        if (args.NewItems != null)
        {
            foreach (NodeAttribute na in Attributes)
            {
                attr = na;
                if (attr != null)
                    attr.ParentNode = this;
            }
        }
    }

    public NodeAttribute? GetAttr(int n)
    {
        if (Attributes.Count > n)
            return attributes[n];
        else
            return null;
    }

    public NodeAttribute? GetAttr(string name)
    {
        var attrs = from NodeAttribute na in Attributes
                    where na != null && !string.IsNullOrEmpty(na.Name) && na.Name == name
                    select na;
        if (attrs != null && attrs.Any())
            return attrs.First();
        return null;
    }

    #endregion
    #region Near

    public TreeNode GetNearestEdited()
    {
        TreeNode node = ParentNode;
        if (node != null)
        {
            int id = node.children.IndexOf(this) - 1;
            if (id >= 0)
                node = node.children[id];
            return node;
        }
        else
        {
            return this;
        }
    }

    #endregion
    #region Events

    public delegate void NodeCreatedEventHandler(TreeNode node);
    public delegate void NodeDeletedEventHandler(TreeNode node);
    public delegate void NodeAttributeChanged(NodeAttribute attr, NodeAttributeChangedEventArgs args);
    public event NodeCreatedEventHandler? OnCreate;
    public event NodeCreatedEventHandler? OnVirtualCreate;
    public event NodeDeletedEventHandler? OnRemove;
    public event NodeDeletedEventHandler? OnVirtualRemove;
    public event NodeAttributeChanged OnNodeAttributeChanged;
    public event NodeAttributeChanged OnDependencyAttributeChanged;

    public void RaiseCreate(TreeNode node)
    {
        if (!IsBanned)
            OnVirtualCreate?.Invoke(node);
        OnCreate?.Invoke(node);
        foreach (TreeNode n in children)
            node.RaiseCreate(n);
    }

    public void RaiseVirtualCreate(TreeNode node)
    {
        if (!IsBanned)
        {
            OnVirtualCreate?.Invoke(node);
            foreach (TreeNode n in children)
                node.RaiseVirtualCreate(n);
        }
    }

    public void RaiseRemove(TreeNode node)
    {
        if (!IsBanned)
            OnVirtualRemove?.Invoke(node);
        OnRemove?.Invoke(node);
        foreach (TreeNode n in children)
            node.RaiseRemove(n);
    }

    public void RaiseVirtualRemove(TreeNode node)
    {
        if (!IsBanned)
        {
            OnVirtualRemove?.Invoke(node);
            foreach (TreeNode n in children)
                node.RaiseVirtualRemove(n);
        }
    }

    public void RaiseAttributeChanged(NodeAttribute attr, NodeAttributeChangedEventArgs args)
    {
        OnNodeAttributeChanged?.Invoke(attr, args);
        OnAttributeChangedImpl(attr, args);
    }

    public virtual void OnAttributeChangedImpl(NodeAttribute attr, NodeAttributeChangedEventArgs args) { }

    public void RaiseDependencyAttributeChanged(NodeAttribute attr, NodeAttributeChangedEventArgs args)
    {
        OnDependencyAttributeChanged?.Invoke(attr, args);
    }

    #endregion
}
