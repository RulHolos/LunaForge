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

[DefinitionNode, NodeIcon("DefineStageGroup")]
[CannotBeDeleted]
[RCInvoke(0)]
public class StageGroupDefinition : TreeNode
{
    [JsonConstructor]
    private StageGroupDefinition() : base() { }
    public StageGroupDefinition(LunaDefinition def) : this(def, "Spell Card", "2") { }
    
    public StageGroupDefinition(LunaDefinition def, string name, string life)
        : base(def)
    {
        Name = name;
        StartLife = life;
        StartPower = "400";
        StartFaith = "50000";
        StartBomb = "2";
        AllowPractice = "true";
        DifficultyValue = "1";
    }

    [JsonIgnore, NodeAttribute]
    public string Name
    {
        get => CheckAttr(0, editWindow: "stageGroup").AttrValue;
        set => CheckAttr(0, editWindow: "stageGroup").AttrValue = value;
    }

    [JsonIgnore, NodeAttribute]
    public string StartLife
    {
        get => CheckAttr(1, "Start life").AttrValue;
        set => CheckAttr(1, "Start life").AttrValue = value;
    }

    [JsonIgnore, NodeAttribute]
    public string StartPower
    {
        get => CheckAttr(2, "Start power").AttrValue;
        set => CheckAttr(2, "Start power").AttrValue = value;
    }

    [JsonIgnore, NodeAttribute]
    public string StartFaith
    {
        get => CheckAttr(3, "Start faith").AttrValue;
        set => CheckAttr(3, "Start faith").AttrValue = value;
    }

    [JsonIgnore, NodeAttribute]
    public string StartBomb
    {
        get => CheckAttr(4, "Start bombs").AttrValue;
        set => CheckAttr(4, "Start bombs").AttrValue = value;
    }

    [JsonIgnore, NodeAttribute]
    public string AllowPractice
    {
        get => CheckAttr(5, "Allow practice", "bool").AttrValue;
        set => CheckAttr(5, "Allow practice", "bool").AttrValue = value;
    }

    [JsonIgnore, NodeAttribute]
    public string DifficultyValue
    {
        get => CheckAttr(6, "Difficulty value", "difficulty").AttrValue;
        set => CheckAttr(6, "Difficulty value", "difficulty").AttrValue = value;
    }

    public override string ToString() => $"Define Stage Group \"{NonMacrolize(0)}\"";

    public override IEnumerable<string> ToLua(int spacing)
    {
        string sp = Indent(spacing);
        yield return $"{sp}stage.group.New('menu',{{}},\"{Lua.StringParser.ParseLua(Macrolize(0))}\"," +
            $"{{lifeleft={Macrolize(1)},power={Macrolize(2)},faith={Macrolize(3)},bomb={Macrolize(4)}}},{Macrolize(5)},{Macrolize(6)})\n";
        foreach (var a in base.ToLua(spacing))
            yield return a;
    }

    public override object Clone()
    {
        StageGroupDefinition node = new(ParentDef);
        node.CopyData(this);
        return node;
    }

    public override List<EditorTrace> GetTraces()
    {
        List<EditorTrace> traces = [];
        if (string.IsNullOrEmpty(NonMacrolize(0)))
            traces.Add(new ArgNotNullTrace(this, GetAttr(0).AttrName));
        return traces;
    }
}
