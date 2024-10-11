using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Nodes.NodeData;
using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LunaForge.EditorData.Toolbox;

public class NodePickerItem
{
    public bool IsSeparator { get; set; }
    public string Tag { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Tooltip { get; set; } = "";

    public NodePicker.AddNode AddNodeMethod { get; set; }

    public NodePickerItem(bool isSeparator)
    {
        IsSeparator = isSeparator;
    }

    public NodePickerItem(string tag, string image, string tooltip, NodePicker.AddNode addNodeMethod)
        : this(false)
    {
        Tag = tag;
        Icon = $"{(string.IsNullOrEmpty(image) ? "Unknown" : image)}";
        Tooltip = tooltip;
        AddNodeMethod = addNodeMethod;
    }

    #region Parsing Xml

    /// <summary>
    /// Creates a <see cref="NodePickerItem"/> from a <see cref="XmlNode"/>.
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public static NodePickerItem FromNode(NodePlugin plugin, XmlNode node)
    {
        string name = node.Attributes["name"]?.Value ?? "no-name";
        string path = Path.Combine(plugin.PathToPlugin, plugin.Namespace, node.Attributes["path"]?.Value ?? "");
        string icon = Path.Combine(plugin.PathToPlugin, plugin.Namespace, node.Attributes["icon"]?.Value ?? "");

        string iconName = MainWindow.LoadEditorImageFromFile(icon);

        Console.WriteLine($"Node \"{name}\" loaded successfully.");
        return new($"{plugin.Namespace}-{name}", iconName, name, () =>
        {
            // TODO : Check the innerxml to see if the node has children. If so: add them to the node.
            TreeNode node = new LuaNode(path);
            
            MainWindow.Insert(node);
        });
    }

    #endregion
}
