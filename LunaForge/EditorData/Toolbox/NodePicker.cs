using LunaForge.EditorData.Nodes.Tabs;
using LunaForge.GUI;
using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Toolbox;

public struct NodePlugin
{
    public NodePlugin() { }

    public string DisplayName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string PathToPlugin { get; set; } = string.Empty;

    public List<NodePickerTab> NodePickerTabs { get; set; } = [];

    public void AddTab(NodePickerTab tab)
    {
        NodePickerTabs.Add(tab);
    }
}

public class NodePicker : IEnumerable<NodePlugin>
{
    public delegate void AddNode();

    public List<NodePlugin> Plugins { get; set; } = [];

    public NodePicker()
    {

    }

    public void Initialize()
    {
        //AddRegister(new TabGeneral(new("General")));
        //AddRegister(new TabStages(new("Stages")));
        //AddRegister(new TabProject(new("Project")));
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

    public static NodePicker FromXml(string pathToData)
    {
        NodePicker picker = new();
        
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
                plugin.PathToPlugin = pathToData;
                if (string.IsNullOrEmpty(plugin.DisplayName) || string.IsNullOrEmpty(plugin.Namespace))
                {
                    NotificationManager.AddToast($"Couldn't create node plugin:\nMissing displayname or namespace attributes on {Path.GetFileName(pathToData)}", ToastType.Error);
                    return null;
                }

                foreach (XmlNode tab in pluginDoc.ChildNodes)
                {
                    if (tab.Name != "tab")
                        continue;
                    NodePickerTab nodeTab = NodePickerTab.FromXml(plugin, tab);
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
