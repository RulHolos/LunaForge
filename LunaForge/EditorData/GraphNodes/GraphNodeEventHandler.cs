using LunaForge.EditorData.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.GraphNodes;

public struct OnCreateEventArgs
{
    public GraphNode Parent;
}

public struct OnRemoveEventArgs
{
    public GraphNode Parent;
}

public struct AttributeChangedEventArgs
{
    public string NewValue;
    public string OriginalValue;
}

public delegate void OnCreateNodeHandler(OnCreateEventArgs e);
public delegate void OnRemoveNodeHandler(OnRemoveEventArgs e);
public delegate void OnAttributeChangedHandler(NodeAttribute o, AttributeChangedEventArgs e);