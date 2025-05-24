using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Projects;

public class LunaTreeView : LunaProjectFile
{
    public override void Draw()
    {
        ImGui.BeginTable("#Table", 2, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 1240f);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        RenderTreeView();

        ImGui.TableSetColumnIndex(1);

        // TODO: Nodes Attribute window

        ImGui.EndTable();
    }

    private void RenderTreeView()
    {
        
    }

    public override void Dispose()
    {
        
    }
}
