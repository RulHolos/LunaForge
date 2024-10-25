using IconFonts;
using ImGuiNET;
using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Project;
using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.InputWindows.Windows;

public class ObjectDefInput : InputWindow
{
    private string FilterByType = string.Empty;
    private string FilterByDifficulty = string.Empty;
    private string[] DifficultyValues = [];
    private CachedDefinition[] Definitions = [];
    private int selectedDef = 0;

    private string searchInput = string.Empty;

    public ObjectDefInput(string s, string filter, NodeAttribute owner)
        : base("Create Object", new Vector2(700, 400))
    {
        Result = s;
        FilterByType = filter;
        DifficultyValues = [.. MainWindow.Workspaces.Current.Difficulties];
        FilterByDifficulty = DifficultyValues[0] ?? string.Empty; // By default: "Easy". Defaults on the first defined difficulty.
        string relativeSourcePath = Path.GetRelativePath(owner.ParentNode.ParentDef.ParentProject.PathToProjectRoot, owner.ParentNode.ParentDef.FullFilePath);
        Definitions = MainWindow.Workspaces.Current.DefCache.GetAccessibleDefinitionsWithType(relativeSourcePath, FilterByType);
    }

    public override void RenderModal()
    {
        SetModalToCenter();
        if (BeginPopupModal())
        {
            ImGui.InputText("##CreateObjectInputText", ref Result, 2048);
            ImGui.Separator();

            // Search
            ImGui.Text(FontAwesome6.MagnifyingGlass);
            ImGui.SameLine();
            ImGui.InputText("##CreateObjectSearchText", ref searchInput, 2048);
            ImGui.Separator();

            int index = 0;
            Vector2 size = ImGui.GetContentRegionAvail() - new Vector2(0, 30);
            ImGui.BeginListBox("##CreateObjectScrollbarComponent", size);
            foreach (CachedDefinition definition in Definitions)
            {
                string[] housama = definition.ClassName.Split(':');
                string className = housama[0];
                string diff = (housama.Length > 1) ? $":{housama[1]}" : "";
                string parameters = string.Join(", ", definition.Parameters);
                if (string.IsNullOrEmpty(searchInput) || definition.ClassName.Contains(searchInput, StringComparison.CurrentCultureIgnoreCase))
                {
                    bool isSelected = selectedDef == index;
                    if (ImGui.Selectable($"Name: {className}\nDifficulty: {(string.IsNullOrEmpty(diff) ? "Any" : diff)}\nParameters: {parameters}", isSelected, ImGuiSelectableFlags.NoAutoClosePopups))
                    {
                        selectedDef = index;
                        Result = Definitions[selectedDef].ClassName;
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                index++;
            }
            ImGui.EndListBox();

            RenderModalButtons();
            ImGui.EndPopup();
        }
    }
}
