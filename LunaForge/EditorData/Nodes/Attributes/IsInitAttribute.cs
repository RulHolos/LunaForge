using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.Attributes;

/// <summary>
/// Identify a <see cref="TreeNode"/> is an init node. Used for definitions only. This attribute will be inherited.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class IsInitAttribute : Attribute
{
}
