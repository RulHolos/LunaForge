using LunaForge.EditorData.Nodes.Attributes;
using LunaForge.EditorData.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.NodeData;

[NodeIcon("Folder")]
[CannotBeDeleted, CannotBeBanned]
public class RootNode : TreeNode
{
    public RootNode() : base() { }
    public RootNode(LunaDefinition document) : base(document) { }

    public override string ToString() => "Root";

    public override IEnumerable<string> ToLua(int spacing)
    {
        string sp = Indent(spacing);
        yield return $"-- Definition generated from {ParentDef.FileName}\n";
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
