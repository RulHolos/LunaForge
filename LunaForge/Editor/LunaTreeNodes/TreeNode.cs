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

public abstract class TreeNode : IDisposable
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

    public delegate void NodeCreatedEventHandler(TreeNode node);
    public delegate void NodeDeletedEventHandler(TreeNode node);
    public delegate void NodeAttributeChanged(NodeAttribute attr, NodeAttributeChangedEventArgs args);
    public event NodeCreatedEventHandler? OnNodeCreated;
    public event NodeDeletedEventHandler? OnNodeDeleted;
    public event NodeAttributeChanged OnNodeAttributeChanged;

    public TreeNode()
    {
        OnNodeAttributeChanged += RaiseAttributeChanged;
        MetaData = new(this);
        OnNodeCreated?.Invoke(this);
    }

    public TreeNode(LunaNodeTree parentTree)
    {
        ParentTree = parentTree;
    }

    public void Dispose()
    {
        OnNodeDeleted?.Invoke(this);
    }

    [JsonIgnore]
    public string DisplayString => ToString();

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

    public void ClearChildSelection()
    {
        IsSelected = false;
        foreach (TreeNode child in Children)
            child.ClearChildSelection();
    }

    #endregion
    #region Attributes

    public void RaiseAttributeChanged(NodeAttribute attr, NodeAttributeChangedEventArgs args)
    {
        OnNodeAttributeChanged?.Invoke(attr, args);
    }

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
}
