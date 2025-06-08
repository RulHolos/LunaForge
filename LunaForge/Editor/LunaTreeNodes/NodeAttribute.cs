using LunaForge.Editor.Backend.Enums;
using LunaForge.Editor.Commands;
using LunaForge.Editor.Projects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes;

public struct NodeAttributeChangedEventArgs(string oldVal, string newVal)
{
    public string OldValue = oldVal;
    public string NewValue = newVal;
}

[Serializable]
public class NodeAttribute
{
    [JsonProperty]
    public string Name = "Attr";
    [JsonProperty]
    public NodeEditorWindowType EditorWindow = NodeEditorWindowType.None;
    [JsonIgnore]
    public string DefaultValue = string.Empty;
    [JsonProperty]
    public string Value = string.Empty;

    [JsonIgnore]
    public TreeNode ParentNode { get; set; }

    public NodeAttribute(string name, TreeNode parent)
    {
        Name = name;
        ParentNode = parent;
        Value = string.Empty;
    }

    public NodeAttribute(string name, NodeEditorWindowType editorWindow, string defaultValue)
    {
        Name = name;
        EditorWindow = editorWindow;
        DefaultValue = Value = defaultValue;
    }

    public NodeAttribute(string name, NodeEditorWindowType editorWindow = NodeEditorWindowType.None)
    {
        Name = name;
        EditorWindow = editorWindow;
        DefaultValue = Value = string.Empty;
    }

    /// <summary>
    /// Edits the attribute value with the given <paramref name="newValue"/> and raises the <see cref="OnNodeAttributeChanged"/> event.
    /// </summary>
    /// <param name="newValue">The value to be set.</param>
    public void EditAttr(string newValue, bool force = false)
    {
        string oldValue = Value;
        Value = newValue;

        if (ProjectFileCollection.CurrentF.AddAndExecuteCommand(new EditAttributeCommand(this, oldValue, newValue)) || force);
            ParentNode.RaiseAttributeChanged(this, new(oldValue, newValue));
    }

    public INodeEditorWindow? GetEditorWindowFromType()
    {
        return null;
    }
}
