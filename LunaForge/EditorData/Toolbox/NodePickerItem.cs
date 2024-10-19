using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Nodes.NodeData;
using LunaForge.EditorData.Project;
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
    /// <param name="picker"></param>
    /// <returns></returns>
    public static NodePickerItem FromNode(NodePlugin plugin, XmlNode node, NodePicker picker)
    {
        string name = node.Attributes["name"]?.Value ?? "no-name";
        string path = Path.Combine(plugin.PathToPlugin, plugin.Namespace, node.Attributes["path"]?.Value ?? "");
        string icon = Path.Combine(plugin.PathToPlugin, plugin.Namespace, node.Attributes["icon"]?.Value ?? "");
        picker.NameLookup.TryAdd(name, path);

        List<Tuple<string, string>> childNodes = [];
        foreach (XmlNode childNode in node.ChildNodes)
        {
            if (childNode.Name != "node")
                continue;
            string childPath = Path.Combine(plugin.PathToPlugin, plugin.Namespace, childNode.Attributes["path"]?.Value ?? "");
            string childName = childNode.Attributes["name"]?.Value ?? "no-name";
            if (File.Exists(childPath))
            {
                childNodes.Add(new(childName, childPath));
                picker.NameLookup.TryAdd(childName, childPath);
            }
        }

        string iconName = MainWindow.LoadEditorImageFromFile(icon);

        Console.WriteLine($"Node \"{name}\" loaded successfully.");
        return new($"{plugin.Namespace}-{name}", iconName, name, () =>
        {
            LunaDefinition parentDef = MainWindow.Workspaces.Current.CurrentProjectFile as LunaDefinition;
            // TODO : Check the innerxml to see if the node has children. If so: add them to the node.
            TreeNode node = new LuaNode(parentDef, path) { NodeName = name };
            foreach (Tuple<string, string> child in childNodes)
            {
                node.AddChild(new LuaNode(parentDef, child.Item2) { NodeName = child.Item1 });
            }
            
            MainWindow.Insert(node);
        });
    }

    #endregion
}
