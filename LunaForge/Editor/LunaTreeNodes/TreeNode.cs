using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes;

/*
 * Do that in a lua script, not TreeNode
 */

public abstract class TreeNode
{
    [JsonIgnore]
    public ulong Hash { get; set; }

    [JsonIgnore]
    private List<TreeNode> children = [];
    [JsonIgnore]
    public List<TreeNode> Children
    {
        get => children;
        set => children = value;
    }

    [JsonIgnore]
    private List<NodeAttribute> attributes = [];
    [JsonIgnore]
    public List<NodeAttribute> Attributes
    {
        get => attributes;
        set => attributes = value;
    }

    public TreeNode()
    {
        NodeAttribute.OnNodeAttributeChanged += NodeAttribChanged;
    }

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

    #endregion

    public void NodeAttribChanged(NodeAttribute attr, NodeAttributeChangedEventArgs args)
    {

    }
}
