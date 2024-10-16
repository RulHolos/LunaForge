using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.Attributes;

/// <summary>
/// Identify a <see cref="TreeNode"/> need parent of a given type.
/// Types are connected by OR operator.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RequireParentAttribute : Attribute
{
    public string[] ParentType { get; }

    public RequireParentAttribute(params string[] parent)
    {
        ParentType = parent;
    }
}
