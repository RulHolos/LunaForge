using LunaForge.EditorData.Nodes.Attributes;
using LunaForge.EditorData.Project;
using LunaForge.EditorData.Traces;
using LunaForge.EditorData.Traces.EditorTraces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.NodeData.Stages;

[DefinitionNode, NodeIcon("DefineStage")]
[CannotBeDeleted, CannotBeBanned]
[LeafNode]
public class MainMenuDefinition : TreeNode
{
    [JsonConstructor]
    private MainMenuDefinition() : base() { }

    public MainMenuDefinition(LunaDefinition def) : base(def) { }

    public override string ToString()
    {
        return $"Define Main Menu";
    }

    public override IEnumerable<string> ToLua(int spacing)
    {
        string sp = Indent(spacing);
        yield return $"{sp}stage_init = stage.New(\"menu\", true, true)\n";
        foreach (var a in base.ToLua(spacing))
            yield return a;
    }

    public override object Clone()
    {
        MainMenuDefinition node = new(ParentDef);
        node.CopyData(this);
        return node;
    }
}
