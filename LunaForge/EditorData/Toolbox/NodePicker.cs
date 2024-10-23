using LunaForge.GUI;
using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaForge.EditorData.Nodes.NodeData;
using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Project;
using System.IO;
using System.Xml.Linq;
using LunaForge.EditorData.Nodes.NodeData.Project;

namespace LunaForge.EditorData.Toolbox;

public class NodePlugin
{
    public NodePlugin() { }

    public string DisplayName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public string PathToPlugin { get; set; } = string.Empty;
    public string PathToXml { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0.0";
    public string LowestCompatibleVersion { get; set; } = "1.0.0.0";

    public List<NodePickerTab> NodePickerTabs { get; set; } = [];

    public bool Enabled = true;

    public void AddTab(NodePickerTab tab)
    {
        NodePickerTabs.Add(tab);
    }

    public void ToggleState(LunaForgeProject proj)
    {
        Enabled = !Enabled;
        if (Enabled)
            proj.DisabledNodePlugins.Remove(Path.GetRelativePath(proj.PathToProjectRoot, PathToXml));
        else
            proj.SetDisabledPlugin(PathToXml);
    }
}

public class NodePicker : IEnumerable<NodePlugin>
{
    public delegate void AddNode();

    public List<NodePlugin> Plugins { get; set; } = [];

    public Dictionary<string, string> NameLookup { get; set; } = [];

    public NodePicker() { }

    public string LookupNameFromPath(string relativeLuaPath)
    {
        return NameLookup.FirstOrDefault(x => x.Value == relativeLuaPath).Key ?? "no-name";
    }

    /// <summary>
    /// Adds a <see cref="NodePlugin"/> tab collection to the collection.
    /// </summary>
    /// <param name="plugin">The plugin to add.</param>
    public void AddPlugin(NodePlugin plugin)
    {
        Plugins.Add(plugin);
    }

    public List<NodePickerTab> GetAllTabs()
    {
        List<NodePickerTab> tabs = [];
        if (Plugins == null)
            return tabs;
        foreach (NodePlugin plugin in Plugins)
            tabs.AddRange(plugin.NodePickerTabs);
        return tabs;
    }

    #region Parsing

    public static NodePicker FromXml(string pathToData, LunaForgeProject parentProj)
    {
        NodePicker picker = new();

        AddSystemPlugin(picker);
        
        foreach (string path in Directory.GetFiles(pathToData, "*.xml"))
        {
            try
            {
                NodePlugin plugin = new();
                XmlDocument doc = new();
                doc.Load(path);

                XmlNode pluginDoc = doc.DocumentElement.SelectSingleNode("/plugin");
                plugin.DisplayName = pluginDoc.Attributes["displayname"]?.Value ?? "Null";
                plugin.Namespace = pluginDoc.Attributes["namespace"]?.Value ?? "Null";
                plugin.Authors = pluginDoc.Attributes["authors"]?.Value ?? "";
                plugin.PathToXml = path;
                plugin.PathToPlugin = pathToData;

                if (parentProj.DisabledNodePlugins.Contains(Path.GetRelativePath(parentProj.PathToProjectRoot, path)))
                {
                    plugin.Enabled = false;
                    picker.AddPlugin(plugin);
                    continue;
                }

                if (string.IsNullOrEmpty(plugin.DisplayName) || string.IsNullOrEmpty(plugin.Namespace))
                {
                    NotificationManager.AddToast($"Couldn't create node plugin:\nMissing displayname or namespace attributes on {Path.GetFileName(pathToData)}", ToastType.Error);
                    return null;
                }

                foreach (XmlNode tab in pluginDoc.ChildNodes)
                {
                    if (tab.Name != "tab")
                        continue;
                    NodePickerTab nodeTab = NodePickerTab.FromXml(plugin, tab, picker);
                    plugin.AddTab(nodeTab);
                }
                picker.AddPlugin(plugin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't load node definition: {ex}");
            }
        }

        return picker;
    }

    /// <summary>
    /// Registers the System node plugin to the toolbox. Only registers C# defined nodes.<br/>
    /// This System plugin will be registered in every project and is not disablable.
    /// </summary>
    /// <param name="picker">The <see cref="NodePicker"/> toolbox in which to add the plugin.</param>
    public static void AddSystemPlugin(NodePicker picker)
    {
        NodePlugin plugin = new()
        {
            DisplayName = "System",
            Namespace = "lunaforge_system",
            PathToPlugin = "",
            Authors = "Rül Hölos",
        };

        NodePickerTab tab = new("System", "Built-in");
        tab.AddNode(new NodePickerItem("system_load_definition", "LoadDefinition", "Load Definition", () =>
        {
            LunaDefinition parentDef = MainWindow.Workspaces.Current.CurrentProjectFile as LunaDefinition;
            TreeNode node = new LoadDefinition(parentDef);
            MainWindow.Insert(node);
        }));

        plugin.AddTab(tab);
        picker.AddPlugin(plugin);
    }

    #endregion

    public IEnumerator<NodePlugin> GetEnumerator()
    {
        return Plugins.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
