using LunaForge.EditorData.Nodes.Attributes;
using LunaForge.EditorData.Project;
using LunaForge.EditorData.Traces;
using LunaForge.EditorData.Traces.EditorTraces;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.NodeData.Project;

[NodeIcon("LoadDef")]
[RequireAncestor("Repeat")]
[LeafNode]
public class LoadDefinition : TreeNode
{
    [JsonConstructor]
    private LoadDefinition() : base() { }
    public LoadDefinition(LunaDefinition def) : this(def, "") { }
    public LoadDefinition(LunaDefinition def, string filePath) : base(def)
    {
        PathToDefinition = filePath;
    }

    [JsonIgnore]
    public override string NodeName { get => "LoadDefinition"; }

    [JsonIgnore, NodeAttribute, DefaultValue("")]
    public string PathToDefinition
    {
        get => CheckAttr(0, "Path to Definition", "definitionFile").AttrValue;
        set => CheckAttr(0, "Path to Definition", "definitionFile").AttrValue = value;
    }

    public override string ToString()
    {
        return $"Load Definition from \"{NonMacrolize(0)}\"";
    }

    public override IEnumerable<string> ToLua(int spacing)
    {
        string sp = Indent(spacing);
        yield return sp + $"Include'{Path.ChangeExtension(
            Path.GetRelativePath(ParentDef.ParentProject.PathToProjectRoot, Macrolize(0) ?? string.Empty), ".lua")
            .Replace("\\", "/")}'\n";
    }

    public override object Clone()
    {
        LoadDefinition cloned = new(ParentDef);
        cloned.CopyData(this);
        return cloned;
    }

    public override List<EditorTrace> GetTraces()
    {
        List<EditorTrace> traces = [];
        if (string.IsNullOrEmpty(NonMacrolize(0)))
            traces.Add(new ArgNotNullTrace(this, GetAttr(0).AttrName));
        if (!string.IsNullOrEmpty(NonMacrolize(0)) && !File.Exists(Path.Combine(ParentDef.ParentProject.PathToProjectRoot, NonMacrolize(0))))
            traces.Add(new FileMustExistTrace(this, GetAttr(0).AttrName));
        return traces;
    }
}
