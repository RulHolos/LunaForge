using LunaForge.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Raylib_cs;
using System.Numerics;
using LunaForge.Plugins.System;
using LunaForge.Plugins;

namespace LunaForge.GUI.Windows;

internal class PluginManagerWindow : ImGuiWindow
{
    public Vector2 ModalSize = new(800, 600);
    LunaPluginInfo? selectedPlugin = null;

    public PluginManagerWindow()
        : base(false)
    {

    }

    public override void Render()
    {
        if (ShowWindow)
        {
            ImGui.OpenPopup("Plugin Manager");
        }

        SetModalToCenter();
        if (ImGui.BeginPopupModal("Plugin Manager", ref ShowWindow, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginGroup();
            {
                if (ImGui.BeginListBox("##PluginList", new Vector2(300, ImGui.GetContentRegionAvail().Y)))
                {
                    foreach (LunaPluginInfo plugin in MainWindow.PluginManager.Plugins)
                    {
                        uint color = plugin.State switch
                        {
                            LunaPluginState.Disabled => ImGui.GetColorU32(ImGuiCol.TextDisabled),
                            LunaPluginState.ErrorWhileLoading => 0xFF0000FFu,
                            _ => ImGui.GetColorU32(ImGuiCol.Text),
                        };
                        ImGui.PushStyleColor(ImGuiCol.Text, color);
                        if (ImGui.Selectable($"{plugin.Meta.Name} {(plugin.State == LunaPluginState.ErrorWhileLoading ? "(Error)" : "")}", plugin == selectedPlugin))
                            selectedPlugin = plugin;
                        ImGui.PopStyleColor();
                    }
                    ImGui.EndListBox();
                }

                ImGui.SameLine();

                if (selectedPlugin != null)
                {
                    ImGui.BeginGroup();

                    string authors = string.Join(" ; ", selectedPlugin.Meta.Authors);
                    ImGui.TextWrapped($"{selectedPlugin.Meta.Name} ({(selectedPlugin.State == LunaPluginState.Enabled ? "Enabled" : "Disabled")})");
                    ImGui.TextWrapped($"Author{(selectedPlugin.Meta.Authors.Length > 1 ? "s" : "")}: {authors}");
                    if (selectedPlugin.State == LunaPluginState.Enabled)
                    {
                        if (ImGui.Button($"Disable##{selectedPlugin.Meta.Name}"))
                            MainWindow.PluginManager.UnloadPlugin(selectedPlugin);
                    }
                    else
                    {
                        if (ImGui.Button($"Enable##{selectedPlugin.Meta.Name}"))
                            MainWindow.PluginManager.LoadPlugin(selectedPlugin);
                    }
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.TextWrapped(selectedPlugin.Meta.Description);

                    ImGui.EndGroup();
                }
            }
            ImGui.EndGroup();
        }
    }

    protected void SetModalToCenter()
    {
        Vector2 renderSize = new(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
        ImGui.SetNextWindowSize(ModalSize);
        ImGui.SetNextWindowPos(renderSize / 2 - (ModalSize / 2));
    }

    protected void RenderModalButtons()
    {
        // Set buttons at the bottom.
        float availableHeight = ImGui.GetWindowHeight() - ImGui.GetCursorPosY();
        float buttonHeight = ImGui.CalcTextSize("Close").Y + ImGui.GetStyle().FramePadding.Y * 2;
        float spacing = ImGui.GetStyle().ItemSpacing.Y + 4;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + availableHeight - buttonHeight - spacing);

        if (ImGui.Button("Close"))
        {
            ShowWindow = false;
            ImGui.CloseCurrentPopup();
        }
    }
}
