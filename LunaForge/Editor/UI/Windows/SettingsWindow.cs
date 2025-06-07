using Hexa.NET.ImGui;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.Projects;
using LunaForge.Editor.UI.ImGuiExtension;
using LunaForge.Editor.UI.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace LunaForge.Editor.UI.Windows;

public class SettingsWindow : EditorWindow
{
    private ConfigSystemCategory? displayedKey = ConfigSystemCategory.General;

    private Hotkey? recordingHotkey;
    private string? filter = string.Empty;

    private bool unsavedChanged;

    protected override string Name => $"{FA.Gear} Settings";

    private EditorConfig EditorConf => EditorConfig.Default;
    private ConfigSystem? CurrentProjConf => ProjectManager.CurrentProject?.ProjectConfig; // Fuck this line specifically

    private Dictionary<string, string> Descriptions { get; set; } = [];

    public SettingsWindow()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "assets", "SettingsDescription.json");
        if (Path.Exists(path))
            Descriptions = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
    }

    public override unsafe void DrawContent()
    {
        ImGui.BeginTable("Config", 2, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableNextColumn();

        DisplayKey(ConfigSystemCategory.General);
        DisplayKey(ConfigSystemCategory.Services);
        DisplayKey(ConfigSystemCategory.DefaultProject);
        if (CurrentProjConf != null)
            DisplayKey(ConfigSystemCategory.CurrentProject);

        ImGui.TableNextColumn();
        ImGui.InputText("Search", ref filter, 256);

        if (displayedKey != null)
        {
            ConfigSystemCategory _displayedKey = (ConfigSystemCategory)displayedKey;
            string displayedKeyName = Regex.Replace(Enum.GetName(_displayedKey), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");

            ImGui.BeginDisabled(!unsavedChanged);
            if (ImGui.Button("Save"))
            {
                EditorConf.CommitAll();
                EditorConf.Save();
                CurrentProjConf?.CommitAll();
                CurrentProjConf?.Save();

                unsavedChanged = false;
                Flags &= ~ImGuiWindowFlags.UnsavedDocument;

                ServiceManager.ResetServices();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                EditorConf.RevertAll();
                EditorConf.Save();
                CurrentProjConf?.RevertAll();
                CurrentProjConf?.Save();

                unsavedChanged = false;
                Flags &= ~ImGuiWindowFlags.UnsavedDocument;
            }
            ImGui.EndDisabled();

            ImGui.SeparatorText(displayedKeyName);

            ConfigSystem source = displayedKey == ConfigSystemCategory.CurrentProject ? CurrentProjConf : EditorConf;

            ImGui.BeginChild("OptionScroller");

            foreach (var (k, v) in source.AllEntries.Where(x => x.Value.Category == _displayedKey))
            {
                if (v is not IConfigSystemEntry value)
                    continue;
                if (!string.IsNullOrEmpty(filter) && !value.Key.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var val = value.TempValueObj;
                bool changed = false;
                if (ImGui.SmallButton($"{FA.CircleArrowLeft}##{value.Key}"))
                {
                    value.Revert();
                }
                ImGui.SetItemTooltip("Revert to default value");

                ImGui.SameLine();

                Descriptions.TryGetValue(value.Key, out string? description);
                ImGuiEx.HelpMarker(description ?? "No description found for this config key.");

                ImGui.SameLine();

                string keyName = Regex.Replace(value.Key, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");

                switch (value.TempValueObj)
                {
                    case string:
                        {
                            string str = value.TempValueObj as string ?? string.Empty;
                            changed = ImGui.InputText(keyName, ref str, 1024);
                            if (changed)
                                val = str;
                        }
                        break;
                    case bool:
                        {
                            bool vv = value.TempValueObj as bool? ?? false;
                            changed = ImGui.Checkbox(keyName, ref vv);
                            if (changed)
                                val = vv;
                        }
                        break;
                    case int:
                        {
                            int i = value.TempValueObj as int? ?? 0;
                            changed = ImGui.InputInt(keyName, ref i);
                            if (changed)
                                val = i;
                        }
                        break;
                    case float:
                        {
                            float f = value.TempValueObj as float? ?? 0f;
                            changed = ImGui.InputFloat(keyName, ref f);
                            if (changed)
                                val = f;
                        }
                        break;
                    case double:
                        {
                            double d = value.TempValueObj as double? ?? 0d;
                            changed = ImGui.InputDouble(keyName, ref d);
                            if (changed)
                                val = d;
                        }
                        break;
                    case Vector4:
                        {
                            Vector4 v4 = value.TempValueObj as Vector4? ?? Vector4.Zero;
                            changed = ImGui.ColorPicker4(keyName, ref v4);
                            if (changed)
                                val = v4;
                        }
                        break;
                    default:
                        ImGui.Text(keyName);
                        break;
                }

                if (changed)
                {
                    if (value.TempValueObj != val)
                        value.TempValueObj = val;
                    unsavedChanged = true;
                    Flags |= ImGuiWindowFlags.UnsavedDocument;
                }
            }

            ImGui.EndChild();
        }

        ImGui.EndTable();
    }

    private void DisplayKey(ConfigSystemCategory category)
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
        if (displayedKey is ConfigSystemCategory other && other == category)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        flags |= ImGuiTreeNodeFlags.Leaf;

        bool isOpen = ImGui.TreeNodeEx($"{Regex.Replace(Enum.GetName(category), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}", flags);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            displayedKey = category;
        }
        if (isOpen)
        {
            ImGui.TreePop();
        }
    }
}