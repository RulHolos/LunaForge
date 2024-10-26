using LunaForge.EditorData.Nodes.Attributes;
using LunaForge.EditorData.Project;
using LunaForge.GUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.NodeData;

[NodeIcon("Folder")]
[CannotBeDeleted, CannotBeBanned]
[CannotBeDragged]
public class RootNode : TreeNode
{
    public RootNode() : base() { }
    public RootNode(LunaDefinition document) : base(document) { }

    public override string ToString() => "Root";

    [JsonIgnore]
    public override string NodeName { get => "Root"; }

    public override IEnumerable<string> ToLua(int spacing)
    {
        string sp = Indent(spacing);
        yield return $"-- Definition generated from \"{ParentDef.FileName}\" by {MainWindow.LunaForgeName} v{MainWindow.VersionNumber}\n";
        foreach (var a in base.ToLua(spacing))
            yield return a;
    }

    public override object Clone()
    {
        RootNode node = new(ParentDef);
        node.CopyData(this);
        return node;
    }
}
