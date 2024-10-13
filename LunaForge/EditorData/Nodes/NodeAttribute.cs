using LunaForge.EditorData.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace LunaForge.EditorData.Nodes;

[Serializable]
public class NodeAttribute : ICloneable
{
    [JsonProperty]
    public string AttrName;

    [JsonProperty]
    public string AttrValue { get; set; }

    [JsonProperty, DefaultValue(false)]
    public bool IsDependency { get; set; } = false;

    [JsonIgnore]
    public string TempAttrValue = string.Empty;
    [JsonProperty]
    public string EditWindow;

    [JsonIgnore]
    private TreeNode parentNode;
    [JsonIgnore]
    public TreeNode ParentNode
    {
        get => parentNode;
        set => parentNode = value;
    }

    public NodeAttribute() { }

    public NodeAttribute(string name, TreeNode parent)
    {
        AttrName = name;
        ParentNode = parent;
        AttrValue = "";
    }

    public NodeAttribute(string name, string value = "", string editWin = "")
        : this()
    {
        AttrName = name;
        AttrValue = value;
        EditWindow = editWin;
    }

    public NodeAttribute(string name, string value = "", string editWin = "", bool isDependency = true)
        : this(name, value, editWin)
    {
        IsDependency = isDependency;
    }

    public NodeAttribute(string name, TreeNode parent, string editWin)
        : this(name, parent)
    {
        EditWindow = editWin;
    }

    public NodeAttribute(string name, string value, TreeNode parent)
        : this(name, parent)
    {
        AttrValue = value;
    }

    public NodeAttribute(string name, TreeNode parent, string editWin, string value)
        : this(name, value, parent)
    {
        EditWindow = editWin;
    }

    public virtual void RaiseEdit(string value)
    {
        if (AttrValue == value)
            return; // No need for edit if the values match.
        if (IsDependency)
            ParentNode.RaiseDependencyPropertyChanged(this, new DependencyAttributeChangedEventArgs() { OriginalValue = value });
        ParentNode.ParentDef.AddAndExecuteCommand(new EditAttributeCommand(this, AttrValue, value));
        ParentNode.CheckTrace();
    }

    public virtual object Clone()
    {
        return new NodeAttribute(AttrName, ParentNode)
        {
            AttrValue = this.AttrValue,
            EditWindow = this.EditWindow
        };
    }
}