using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace LunaForge.Editor.Projects;

public class LunaTreeView : LunaProjectFile
{
    public override void Draw()
    {
        ImGui.BeginTable("TreeNodeTableLayout", 2,
            ImGuiTableFlags.Resizable
            | ImGuiTableFlags.Reorderable);
        ImGui.TableSetupColumn("Node Tree", ImGuiTableColumnFlags.WidthFixed, 1240f);
        ImGui.TableSetupColumn("Node Attributes", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.BeginChild($"TreeView", ImGui.GetContentRegionAvail());
        RenderTreeView();
        ImGui.EndChild();

        ImGui.TableSetColumnIndex(1);

        ImGui.BeginChild($"NodeAttributes", ImGui.GetContentRegionAvail());
        // TODO: Nodes Attribute window
        ImGui.EndChild();

        ImGui.EndTable();
    }

    private void RenderTreeView()
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Selected;
        for (int i = 0; i < 5; i++)
        {
            if (ImGui.TreeNodeEx($"{i}", flags))
            {
                for (int j = 0; j < 5; j++)
                {
                    if (ImGui.TreeNodeEx($"{i}->{j}##{j}-{i}", flags))
                    {
                        ImGui.Text("Yep.");

                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }
        }
    }

    public override void Dispose()
    {
        
    }
}
