using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LunaForge.EditorData.Toolbox;

public class NodePickerTab : IEnumerable<NodePickerItem>
{
    public string Header { get; set; }
    public List<NodePickerItem> Items { get; set; } = [];

    public NodePickerTab() { }
    public NodePickerTab(string header)
    {
        Header = header;
    }

    public void AddNode(NodePickerItem item)
    {
        Items.Add(item);
    }

    public IEnumerator<NodePickerItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #region Parsing

    public static NodePickerTab FromXml(NodePlugin plugin, XmlNode tabNode)
    {
        NodePickerTab tab = new()
        {
            Header = tabNode.Attributes["name"]?.Value ?? "??"
        };

        foreach (XmlNode node in tabNode.ChildNodes)
        {
            switch (node.Name)
            {
                case "node":
                    tab.AddNode(NodePickerItem.FromNode(plugin, node));
                    break;
                case "separator":
                    tab.AddNode(new(true));
                    break;
                default:
                    break;
            }
        }

        return tab;
    }

    #endregion
}
