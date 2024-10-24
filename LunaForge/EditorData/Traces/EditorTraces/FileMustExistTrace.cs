using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using TreeNode = LunaForge.EditorData.Nodes.TreeNode;

namespace LunaForge.EditorData.Traces.EditorTraces;

public class FileMustExistTrace : EditorTrace
{
    public string ArgName { get; private set; }

    public FileMustExistTrace(TreeNode source, string argName)
        : base(TraceSeverity.Error, source, source?.ParentDef.FileName)
    {
        ArgName = argName;
    }

    public override string ToString()
    {
        return $"File \"{ArgName}\" must exist at the given location.";
    }

    public override object Clone()
    {
        return new FileMustExistTrace((TreeNode)Source, ArgName);
    }

    public override void Invoke()
    {
        (Source as TreeNode).ParentDef.RevealNode(Source as TreeNode);
    }
}
