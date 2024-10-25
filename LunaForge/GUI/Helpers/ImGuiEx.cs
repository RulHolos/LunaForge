using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.Helpers;

public static class ImGuiEx
{
    public static bool ComboBox(string label, ref int currentItem, ref string currentInput, string[] items, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        bool pressedCombo = false;
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 20);

        if (ImGui.InputText($"{label}", ref currentInput, 5_000, flags))
        {
            return true;
        }
        ImGui.SameLine(0f, 0f);
        if (ImGui.ArrowButton($"{label}_ArrowCombo", ImGuiDir.Down))
        {
            ImGui.OpenPopup($"{label}_combo");
        }
        if (ImGui.BeginPopup($"{label}_combo"))
        {
            for (int i = 0; i < items.Length; i++)
            {
                bool isSelected = i == currentItem;
                if (ImGui.Selectable(items[i], isSelected))
                {
                    currentItem = i;
                    currentInput = items[i];
                    pressedCombo = true;
                }
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndPopup();
        }
        return pressedCombo;
    }

    [Obsolete("Not complete.", true)]
    public static void FileSelector(string label, ref string filePath, Action<bool, string[]> callback = null)
    {
        void SelectPath(bool success, string[] paths)
        {
            if (!success)
                return;
            //filePath = paths[0];
        }

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 20);

        ImGui.InputText($"{label}", ref filePath, 1024);
        ImGui.SameLine(0f, 0f);
        if (ImGui.Button($"{label}_btn", ImGui.CalcTextSize("...")))
        {

        }
    }

    [Flags]
    public enum ImGuiClickToCopyTextFlags
    {
        MouseToHand,
        TooltipToCopiedText,
    }

    public static void ClickToCopyText(
        string text,
        string? textToCopy = null,
        ImGuiClickToCopyTextFlags flags = ImGuiClickToCopyTextFlags.MouseToHand | ImGuiClickToCopyTextFlags.TooltipToCopiedText)
    {
        textToCopy ??= text;

        ImGui.Text(text);
        if (ImGui.IsItemHovered())
        {
            if (flags.HasFlag(ImGuiClickToCopyTextFlags.MouseToHand))
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (textToCopy != text && flags.HasFlag(ImGuiClickToCopyTextFlags.TooltipToCopiedText))
                ImGui.SetTooltip(textToCopy);
        }
        if (ImGui.IsItemClicked())
            ImGui.SetClipboardText(textToCopy);
    }

    public static void CenteredText(string text)
    {
        CenterCursorForText(text);
        ImGui.TextUnformatted(text);
    }

    public static void CenterCursorForText(string text) => CenterCursorFor(ImGui.CalcTextSize(text).X);

    public static void CenterCursorFor(float itemWidth) => ImGui.SetCursorPosX((int)((ImGui.GetWindowWidth() - itemWidth) / 2));

    public static bool DoubleClickButton(string label)
    {
        ImGui.Button(label);
        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            return true;
        return false;
    }
}
