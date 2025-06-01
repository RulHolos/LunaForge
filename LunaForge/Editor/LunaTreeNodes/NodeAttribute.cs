using LunaForge.Editor.Backend.Enums;
using LunaForge.Editor.Commands;
using LunaForge.Editor.Commands.CommandList;
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

public class NodeAttribute
{
    public string Name = "Attr";
    public NodeEditorWindowType EditorWindow = NodeEditorWindowType.None;
    public string DefaultValue = string.Empty;

    public string Value = string.Empty;

    public delegate void NodeAttributeChanged(NodeAttribute attr, NodeAttributeChangedEventArgs args);
    public static event NodeAttributeChanged OnNodeAttributeChanged;

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

        if (CommandHistory.StaticAddAndExecuteCommand(new EditAttributeCommand(this, oldValue, newValue)) || force);
            OnNodeAttributeChanged?.Invoke(this, new(oldValue, newValue));
    }

    public INodeEditorWindow? GetEditorWindowFromType()
    {
        return null;
    }
}
