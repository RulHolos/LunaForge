using Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes;

[LuaObject]
public partial class LuaTreeNode
{
    public TreeNode treeNode { get; set; }

    public LuaTreeNode(TreeNode node)
    {
        treeNode = node;
    }

    /// <summary>
    /// Gets the first logical parent of this Node (not folder, ...)
    /// </summary>
    /// <returns>The TreeNode parent</returns>
    [LuaMember]
    public LuaTreeNode GetRealParent()
    {
        return new(treeNode.GetRealParent());
    }
}
