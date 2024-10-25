using LunaForge.EditorData.InputWindows.Windows;
using LunaForge.EditorData.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.InputWindows;

public enum ImageClassType
{
    None,
    Animation,
    Particle,
    All
}

public class InputWindowSelectorRegister
{
    public Dictionary<string, string[]> RegisterComboBoxText(Dictionary<string, string[]> target)
    {
        target.Add("bool", ["true", "false"]);
        target.Add("bubble_style", ["1", "2", "3", "4"]);
        target.Add("sineinterpolation", ["SINE_ACCEL", "SINE_DECEL", "SINE_ACC_DEC"]);
        target.Add("interpolation", ["MOVE_NORMAL", "MOVE_ACCEL", "MOVE_DECEL", "MOVE_ACC_DEC"]);
        target.Add("target", ["self", "last", "last_task", "unit", "player", "_boss"]);
        target.Add("yield", ["_infinite"]);

        return target;
    }

    public Dictionary<string, Func<NodeAttribute, string, InputWindow>> RegisterInputWindow(Dictionary<string, Func<NodeAttribute, string, InputWindow>> target)
    {
        target.Add("bool", (src, tar) => new Selector(tar, InputWindowSelector.SelectComboBox("bool"), "Input Bool"));
        target.Add("sineinterpolation", (src, tar) => new Selector(tar, InputWindowSelector.SelectComboBox("sineinterpolation"), "Input Sine Interpolation Mode"));
        target.Add("code", (src, tar) => new CodeInput(tar));
        target.Add("target", (src, tar) => new Selector(tar, InputWindowSelector.SelectComboBox("target"), "Input Target Object"));
        target.Add("plainFile", (src, tar) => new PathInput(tar, "File{*.*}", src));
        target.Add("definitionFile", (src, tar) => new PathInput(tar, "LunaForge Definition{.lfd}", src));
        target.Add("objectDef", (src, tar) => new ObjectDefInput(tar, "Object", src));
        target.Add("difficulty", (src, tar) => new DifficultySelectInput(tar, "Difficulty", src));
        // TODO: difficulty

        return target;
    }
}
