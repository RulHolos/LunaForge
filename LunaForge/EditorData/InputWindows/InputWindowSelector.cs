using LunaForge.EditorData.InputWindows.Windows;
using LunaForge.EditorData.Nodes;
using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.InputWindows;

public static class InputWindowSelector
{
    public static InputWindow? CurrentInputWindow;

    public static readonly string[] NullSelection = [];
    public static readonly Func<NodeAttribute, string, InputWindow> NullWindow = (e, s) => new SingleLineInput(s);

    private static Dictionary<string, string[]> ComboBox = [];
    private static Dictionary<string, Func<NodeAttribute, string, InputWindow>> WindowGenerator = [];

    public static void Register(InputWindowSelectorRegister register)
    {
        ComboBox = register.RegisterComboBoxText(ComboBox);
        WindowGenerator = register.RegisterInputWindow(WindowGenerator);

        AfterRegister();
    }

    public static void AfterRegister()
    {
        List<string> vs = new(WindowGenerator.Keys)
        {
            ""
        };
        vs.Sort();
        ComboBox.Add("editWindow", [.. vs]);
        WindowGenerator.Add("editWindow", (src, tar) => new Selector(tar, SelectComboBox("editWindow"), "Input Edit Window"));
    }

    public static string[] SelectComboBox(string name)
    {
        if (name == "difficulty")
            return GetDifficulties();
        return ComboBox.GetValueOrDefault(name, NullSelection);
    }

    public static string[] GetDifficulties()
    {
        List<string> diffs = ["Any"];
        diffs.AddRange(MainWindow.Workspaces.Current.Difficulties);
        return [.. diffs];
    }

    public static InputWindow SelectInputWindow(NodeAttribute source, string name, string toEdit)
    {
        InputWindow iw = WindowGenerator.GetValueOrDefault(name, NullWindow)(source, toEdit);
        iw.AppendTitle(source.AttrName);
        return iw;
    }
}
