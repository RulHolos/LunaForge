using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using LunaForge.Editor.Projects;

namespace LunaForge.Editor.UI.ImGuiExtension;

public static class ImGuiEx
{
    public static void HelpMarker(string tooltip)
    {
        ImGui.TextDisabled($"(?)");
        ImGui.SetItemTooltip(tooltip);
    }

    public static bool EnumCombo<T>(string id, ref T current, float maxWidth = 0) where T : Enum
    {
        string[] names = Enum.GetNames(typeof(T));
        int currentIndex = Array.IndexOf(names, current.ToString());
        bool changed = false;

        ImGui.SetNextItemWidth(maxWidth == 0 ? ImGui.GetContentRegionAvail().X : maxWidth);
        if (ImGui.BeginCombo($"##{id}", Regex.Replace(current.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")))
        {
            for (int i = 0; i < names.Length; i++)
            {
                bool isSelected = i == currentIndex;
                if (ImGui.Selectable(Regex.Replace(names[i], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0"), isSelected))
                {
                    current = (T)Enum.Parse(typeof(T), names[i]);
                    changed = true;
                }
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        return changed;
    }

    public static bool OnOffButton(string label, bool isOn)
    {
        bool returnVal = false;
        if (isOn)
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));
        else
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.Button));

        if (ImGui.Button(label))
            returnVal = true;

        ImGui.PopStyleColor();

        return returnVal;
    }
}
