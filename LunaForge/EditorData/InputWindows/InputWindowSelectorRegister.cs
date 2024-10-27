using LunaForge.EditorData.InputWindows.Windows;
using LunaForge.EditorData.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.InputWindows;

/// <summary>
/// Responsible for registering the input Windows and input ComboBoxes for the Node Attributes fields.
/// </summary>
public class InputWindowSelectorRegister
{
    /// <summary>
    /// Registers the Input ComboBox fields for the Node Attributes.
    /// </summary>
    /// <param name="target">A dictionary of targets.</param>
    /// <returns>The populated target dictionary.</returns>
    public Dictionary<string, string[]> RegisterComboBoxText(Dictionary<string, string[]> target)
    {
        target.Add("bool", ["true", "false"]);
        target.Add("bubble_style", ["1", "2", "3", "4"]);
        target.Add("sineinterpolation", ["SINE_ACCEL", "SINE_DECEL", "SINE_ACC_DEC"]);
        target.Add("interpolation", ["MOVE_NORMAL", "MOVE_ACCEL", "MOVE_DECEL", "MOVE_ACC_DEC"]);
        target.Add("target", ["self", "last", "last_task", "last_return", "unit", "player", "_boss"]);
        target.Add("yield", ["_infinite"]);

        return target;
    }

    /// <summary>
    /// Registers the Input Windows for the Node Attributes.
    /// </summary>
    /// <remarks>If a target registers a <see cref="InputWindowSelector.SelectComboBox(string)"/>,
    /// it should have the same name as the one registered in <see cref="RegisterComboBoxText(Dictionary{string, string[]})"/>.</remarks>
    /// <param name="target">A dictionary of targets.</param>
    /// <returns>The populated target dictionary.</returns>
    public Dictionary<string, Func<NodeAttribute, string, InputWindow>> RegisterInputWindow(Dictionary<string, Func<NodeAttribute, string, InputWindow>> target)
    {
        target.Add("bool", (src, tar) => new Selector(tar, InputWindowSelector.SelectComboBox("bool"), "Input Bool"));
        target.Add("sineinterpolation", (src, tar) => new Selector(tar, InputWindowSelector.SelectComboBox("sineinterpolation"), "Input Sine Interpolation Mode"));
        target.Add("interpolation", (src, tar) => new Selector(tar, InputWindowSelector.SelectComboBox("interpolation"), "Input Interpolation Mode"));
        target.Add("code", (src, tar) => new CodeInput(tar));
        target.Add("target", (src, tar) => new Selector(tar, InputWindowSelector.SelectComboBox("target"), "Input Target Object"));
        target.Add("plainFile", (src, tar) => new PathInput(tar, "File{*.*}", src));
        target.Add("definitionFile", (src, tar) => new PathInput(tar, "LunaForge Definition{.lfd}", src));
        target.Add("objectDef", (src, tar) => new ObjectDefInput(tar, "Object", src));
        target.Add("functionDef", (src, tar) => new ObjectDefInput(tar, "Function", src));
        target.Add("difficulty", (src, tar) => new DifficultySelectInput(tar, "Difficulty", src));
        // TODO: difficulty

        return target;
    }
}
