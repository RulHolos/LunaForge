using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes;

public struct OnCreateEventArgs
{
    public TreeNode Parent;
}

public struct OnRemoveEventArgs
{
    public TreeNode Parent;
}

public struct DependencyAttributeChangedEventArgs
{
    public string OriginalValue;
}

public delegate void OnCreateNodeHandler(OnCreateEventArgs e);
public delegate void OnRemoveNodeHandler(OnRemoveEventArgs e);
public delegate void OnDependencyAttributeChangedHandler(NodeAttribute o, DependencyAttributeChangedEventArgs e);