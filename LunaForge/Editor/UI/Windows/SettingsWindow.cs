using Hexa.NET.ImGui;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.UI.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Windows;

/*
public class SettingsWindow : EditorWindow
{
    private object? displayedKey;

    private Hotkey? recordingHotkey;
    private string? filter = string.Empty;

    private bool unsavedChanged;

    protected override string Name => $"{FA.Gear} Settings";

    public SettingsWindow()
    {

    }

    public override unsafe void DrawContent()
    {
        ImGui.BeginTable("Config", 2, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableNextColumn();

        DisplayKey("Display");

        DisplayKey("Text Editor");
        for (int i = 0; i < keys.Count; i++)
        {
            DisplayKeyNode(keys[i]);
        }

        DisplayKey("Hotkeys");

        ImGui.TableNextColumn();
        ImGui.InputText("Search", ref filter, 256);

        if (displayedKey is ConfigKey configKey)
        {
            ImGui.SameLine();

            ImGui.BeginDisabled(!unsavedChanged);
            if (ImGui.Button("Save"))
            {
                Config.SaveGlobal();
                unsavedChanged = false;
                Flags &= ~ImGuiWindowFlags.UnsavedDocument;
            }
            ImGui.EndDisabled();

            ImGui.Separator();

            ImGui.Text(configKey.Name);

            for (int j = 0; j < configKey.Values.Count; j++)
            {
                var value = configKey.Values[j];

                if (!string.IsNullOrEmpty(filter) && !value.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var val = value.Value;
                bool changed = false;
                if (value.IsReadOnly)
                {
                    ImGui.BeginDisabled(true);
                }

                if (!value.IsReadOnly)
                {
                    if (ImGui.SmallButton($"\uE777##{value.Name}"))
                    {
                        value.SetToDefault();
                    }

                    ImGui.SameLine();
                }

                switch (value.DataType)
                {
                    case DataType.String:
                        changed = ImGui.InputText(value.Name, ref val, 1024);
                        break;

                    case DataType.Bool:
                        {
                            var v = value.GetBool();
                            changed = ImGui.Checkbox(value.Name, ref v);
                            if (changed)
                                val = v.ToString();
                        }
                        break;
                }

                if (value.IsReadOnly)
                {
                    ImGui.EndDisabled();
                }

                if (changed)
                {
                    value.Value = val;
                    unsavedChanged = true;
                    Flags |= ImGuiWindowFlags.UnsavedDocument;
                }
            }
        }

        if (displayedKey is string key)
        {
            ImGui.Separator();

            switch (key)
            {
                case "Display":
                    //DisplayPage();
                    break;

                case "Text Editor":
                    //TextEditorPage();
                    break;

                case "Hotkeys":
                    lock (HotkeyManager.SyncObject)
                    {
                        for (int i = 0; i < HotkeyManager.Count; i++)
                        {
                            //EditHotkey(HotkeyManager.Hotkeys[i]);
                        }
                    }
                    break;
            }
        }

        ImGui.EndTable();
    }

    private void DisplayKeyNode(ConfigKey key)
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
        if (displayedKey == key)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        if (key.Keys.Count)
        {
            flags |= ImGuiTreeNodeFlags.Leaf;
        }

        bool isOpen = ImGui.TreeNodeEx(key.Name, flags);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            displayedKey = key;
        }
        if (isOpen)
        {
            for (int j = 0; j < key.Keys.Count; j++)
            {
                DisplayKeyNode(key.Keys[j]);
            }
            ImGui.TreePop();
        }
    }

    private void DisplayKey(string key)
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
        if (displayedKey is string other && other == key)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        flags |= ImGuiTreeNodeFlags.Leaf;

        bool isOpen = ImGui.TreeNodeEx(key, flags);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            displayedKey = key;
        }
        if (isOpen)
        {
            ImGui.TreePop();
        }
    }
}

*/