using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui;

namespace LunaForge.Editor.UI.ImGuiExtension;

public static class ImGuiEx
{
    public static void HelpMarker(string tooltip)
    {
        ImGui.TextDisabled($"(?)");
        ImGui.SetItemTooltip(tooltip);
    }
}
