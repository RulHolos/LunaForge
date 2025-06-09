using LunaForge.Editor.LunaTreeNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Commands;

public class SwitchBanCommand : Command
{
    private TreeNode target;
    private bool targetValue;
    private bool originalValue;

    public SwitchBanCommand(TreeNode node, bool value)
    {
        target = node;
        targetValue = value;
        originalValue = target.IsBanned;
    }

    public override void Execute()
    {
        target.IsBanned = targetValue;
    }

    public override void Undo()
    {
        target.IsBanned = originalValue;
    }

    public override string ToString() => $"Toggle ban to {targetValue}";
}

