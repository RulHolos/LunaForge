using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes;

public interface ITreeNodeMeta
{

}

public class TreeNodeMetaData : ITreeNodeMeta
{
    /// <summary>
    /// Construct a <see cref="TreeNodeMetaData"/> Metadata based on a C#-defined <see cref="TreeNode"/> object.
    /// </summary>
    /// <param name="node">The node to construct the data from</param>
    public TreeNodeMetaData(TreeNode node)
    {
        Type type = node.GetType();

    }
}