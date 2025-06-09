using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes.Nodes;

public class RootNode : TreeNode
{
    public override string NodeName => "Root";

    public override string ToString()
    {
        return "Root";
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}
