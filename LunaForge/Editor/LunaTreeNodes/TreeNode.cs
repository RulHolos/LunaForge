using LunaForge.Editor.Projects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    private List<TreeNode> children = [];
    [JsonIgnore]
    public List<TreeNode> Children
    {
        get => children;
        set => children = value;
    }
    [JsonIgnore]
    public bool HasChildren => Children.Count > 0;

    [JsonIgnore]
    private List<NodeAttribute> attributes = [];
    [JsonIgnore]
    public List<NodeAttribute> Attributes
    {
        get => attributes;
        set => attributes = value;
    }

    public delegate void NodeCreatedEventHandler(TreeNode node);
    public delegate void NodeDeletedEventHandler(TreeNode node);
    public event NodeCreatedEventHandler? OnNodeCreated;
    public event NodeDeletedEventHandler? OnNodeDeleted;

    public TreeNode()
    {
        NodeAttribute.OnNodeAttributeChanged += NodeAttribChanged;
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

    public void NodeAttribChanged(NodeAttribute attr, NodeAttributeChangedEventArgs args)
    {

    }
}
