using LunaForge.Editor.LunaTreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Commands;

public abstract class InsertCommand(TreeNode source, TreeNode toInsert) : Command
{
    protected TreeNode Source = source;
    protected TreeNode ToInsert = toInsert;
}
