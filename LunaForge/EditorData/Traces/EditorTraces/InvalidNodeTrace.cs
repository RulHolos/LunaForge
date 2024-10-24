using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using TreeNode = LunaForge.EditorData.Nodes.TreeNode;

namespace LunaForge.EditorData.Traces.EditorTraces;

public class InvalidNodeTrace : EditorTrace
{
    public InvalidNodeTrace(TreeNode source)
        : base(TraceSeverity.Error, source)
    {
    }

    public override string ToString()
    {
        return $"Node is Invalid.";
    }

    public override object Clone()
    {
        return new InvalidNodeTrace((TreeNode)Source);
    }

    public override void Invoke()
    {
        (Source as TreeNode).ParentDef.RevealNode(Source as TreeNode);
    }
}
