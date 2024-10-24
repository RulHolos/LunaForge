using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using TreeNode = LunaForge.EditorData.Nodes.TreeNode;

namespace LunaForge.EditorData.Traces.EditorTraces;

public class ArgNotNullTrace : EditorTrace
{
    public string ArgName { get; private set; }

    public ArgNotNullTrace(TreeNode source, string argName)
        : base(TraceSeverity.Error, source, source?.ParentDef.FileName)
    {
        ArgName = argName;
    }

    public override string ToString()
    {
        return $"Attribute \"{ArgName}\" can not be blank.";
    }

    public override object Clone()
    {
        return new ArgNotNullTrace((TreeNode)Source, ArgName);
    }

    public override void Invoke()
    {
        (Source as TreeNode).ParentDef.RevealNode(Source as TreeNode);
    }
}
