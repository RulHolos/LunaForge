using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using LunaForge.EditorData.Project;
using LunaForge.EditorData.Toolbox;
using LunaForge.GUI.Helpers;
using rlImGui_cs;

namespace LunaForge.GUI.Windows;

public class ToolboxWindow : ImGuiWindow
{
    public NodePicker NodePickerBox => MainWindow.Workspaces.Current?.Toolbox;

    public ToolboxWindow()
        : base(true)
    {

    }

    public override void Render()
    {
        if (BeginNoClose("Toolbox", flags: ImGuiWindowFlags.NoScrollbar))
        {
            if (ImGui.BeginTabBar("NodeToolbox"))
            {
                if (NodePickerBox == null)
                {
                    ImGui.EndTabBar();
                    End();
                    return;
                }

                int i = 0;
                foreach (NodePickerTab tab in NodePickerBox.GetAllTabs())
                {
                    ImGui.PushID($"{tab.Header}##{i}");
                    bool isOpened = ImGui.BeginTabItem($"{tab.Header}##{i}");
                    if (isOpened)
                    {
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip($"{tab.Header} ({tab.PluginName})");
                        }

                        ImGui.BeginDisabled(MainWindow.Workspaces.Current?.CurrentProjectFile is not LunaDefinition);

                        foreach (NodePickerItem item in tab.Items)
                        {
                            if (item.IsSeparator)
                                VerticalSeparator();
                            else
                            {
                                if (rlImGui.ImageButtonSize(item.Tag, MainWindow.FindTexture(item.Icon), new Vector2(24, 24)))
                                    item.AddNodeMethod();
                                if (ImGui.IsItemHovered())
                                    ImGui.SetTooltip(item.Tooltip);
                                ImGui.SameLine();
                            }
                        }

                        ImGui.EndDisabled();
                        ImGui.EndTabItem();
                    }
                    else if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip($"{tab.Header} ({tab.PluginName})");
                    }
                    ImGui.PopID();
                    i++;
                }
                ImGui.EndTabBar();
            }
            End();
        }
    }
}
