using ImGuiNET;
using LunaForge.EditorData.Nodes;
using LunaForge.GUI;
using LunaForge.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.InputWindows.Windows;

public class PathInput : InputWindow
{
    public string CurrentFilePath = "";
    private string InitialDirectory = "";
    private string Filter = "";

    public PathInput(string s, string filter, NodeAttribute owner)
        : base("Open File")
    {
        Result = s;
        CurrentFilePath = Result;
        Filter = filter;
        InitialDirectory = Path.GetDirectoryName(owner?.ParentNode?.ParentDef?.FullFilePath ?? string.Empty);
    }

    public override void RenderModal()
    {
        SetModalToCenter();
        if (BeginPopupModal())
        {
            void SelectPath(bool success, List<string> paths)
            {
                if (!success)
                    return;
                Result = CurrentFilePath = paths[0];
                Close();
            }

            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 30);

            ImGui.InputText($"##AttributePathInput", ref CurrentFilePath, 1024);
            ImGui.SameLine(0f, 0f);
            if (ImGui.Button($"...##AttributePathInput_btn"))
            {
                Close(false);
                MainWindow.FileDialogManager.OpenFileDialog("Open File", Filter, SelectPath, 1, InitialDirectory, true);
            }

            RenderModalButtons();
            CloseOnEnter();

            ImGui.EndPopup();
        }
    }
}
