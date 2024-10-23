using ImGuiNET;
using LunaForge.EditorData.Traces;
using LunaForge.GUI.Helpers;
using rlImGui_cs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.Windows;

public class TracesWindow : ImGuiWindow
{
    public List<EditorTrace> Traces { get => EditorTraceContainer.Traces; }

    public TracesWindow()
        : base(true)
    {

    }

    public override void Render()
    {
        if (BeginNoClose("Traces"))
        {
            ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;
            if (ImGui.BeginTable("NodeAttributeTable", 3, flags))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.WidthFixed, 12f);
                ImGui.TableSetupColumn("Trace", ImGuiTableColumnFlags.None, 1.5f);
                ImGui.TableSetupColumn("Source", ImGuiTableColumnFlags.None, 1f);
                ImGui.TableHeadersRow();

                foreach (EditorTrace trace in Traces)
                {
                    ImGui.TableNextRow();

                    // Icon
                    ImGui.TableSetColumnIndex(0);
                    rlImGui.ImageSize(MainWindow.FindTexture(trace.Icon), 12, 12);

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(trace.Trace);

                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text(trace.SourceName ?? "Unknown source");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        ImGui.SetTooltip("Click this to go the source.");
                    }
                    if (ImGui.IsItemClicked())
                    {
                        trace.Invoke();
                    }
                }
                ImGui.EndTable();
            }
            End();
        }
    }
}
