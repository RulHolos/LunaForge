using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using LunaForge.EditorData.Nodes;
using LunaForge.GUI;
using LunaForge.GUI.Helpers;

namespace LunaForge.EditorData.InputWindows.Windows;

public class DifficultyNumSelectInput : InputWindow
{
    private int SelectedResult = 0;
    private string[] Items;

    public DifficultyNumSelectInput(string s, string title, NodeAttribute owner)
        : base(title)
    {
        Result = s;
        string[] diffs = InputWindowSelector.GetDifficulties();
        Items = Enumerable.Range(1, diffs.Length).Select(i => i.ToString()).ToArray();
        SelectedResult = Array.IndexOf(Items, Result);
    }

    public override void RenderModal()
    {
        SetModalToCenter();
        if (BeginPopupModal())
        {
            ImGuiEx.ComboBox($"##{Title}", ref SelectedResult, ref Result, Items);

            RenderModalButtons();
            CloseOnEnter();

            ImGui.EndPopup();
        }
    }
}
