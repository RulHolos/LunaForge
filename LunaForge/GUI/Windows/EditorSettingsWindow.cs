using IconFonts;
using ImGuiNET;
using LunaForge.GUI.Helpers;
using rlImGui_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TextCopy;

namespace LunaForge.GUI.Windows;

internal class EditorSettingsWindow : ImGuiWindow
{
    public Vector2 ModalSize = new(700, 500);
    private bool ModalClosed { get; set; } = true;
    private bool ShouldOpenSettings { get; set; } = false;

    public EditorSettingsWindow()
        : base(false)
    {

    }

    public override void Render()
    {
        if (ShowWindow)
        {
            ImGui.OpenPopup("Editor Settings");
        }

        SetModalToCenter(ModalSize);
        if (ImGui.BeginPopupModal("Editor Settings", ref ShowWindow, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDocking))
        {
            if (ModalClosed)
                GetSettings();

            if (ImGui.BeginTabBar("EditorSettingsTabBar"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    RenderDiscordRPC();
                    RenderAutoBackup();
                    RenderAutoUpdates();

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Theme"))
                {
                    RenderThemeContent();

                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }

            // Set buttons at the bottom.
            float availableHeight = ImGui.GetWindowHeight() - ImGui.GetCursorPosY();
            float buttonHeight = ImGui.CalcTextSize("Ok").Y + ImGui.GetStyle().FramePadding.Y * 2;
            float spacing = ImGui.GetStyle().ItemSpacing.Y + 4;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + availableHeight - buttonHeight - spacing);

            if (ImGui.Button("Ok"))
                ApplySettings();
            ImGui.SameLine();
            if (ImGui.Button("Apply"))
                ApplySettings(true);
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
                ModalClosed = true;
                ShowWindow = false;
            }

            ImGui.EndPopup();
        }
    }

    #region Settings get/set
    #region Temp Settings

    public int CurrentlyActiveTheme = 0;
    public string TempActiveTheme = "";

    public bool TempUseDiscordRPC = true;
    public bool TempAutoBackup = true;
    public int TempAutoBackupFreq = 1;
    public int TempBackupCountLimit = 5;

    public bool TempUseAutoUpdates = true;
    public bool TempCheckUpdatesAtStartup = true;
    public int TempCheckUpdateFrequency = 30;

    #endregion

    public void GetSettings()
    {
        TempActiveTheme = Configuration.Default.CurrentThemeProfile ?? "";
        CurrentlyActiveTheme = Configuration.Default.ThemeProfiles.IndexOf(Configuration.GetCurrentTheme());
        TempUseDiscordRPC = Configuration.Default.UseDiscordRPC;
        TempAutoBackup = Configuration.Default.AutoBackup;
        TempAutoBackupFreq = Configuration.Default.AutoBackupFreq;
        TempBackupCountLimit = Configuration.Default.BackupCountLimit;
        TempUseAutoUpdates = Configuration.Default.UseAutoUpdates;
        TempCheckUpdatesAtStartup = Configuration.Default.CheckUpdatesAtStartup;
        TempCheckUpdateFrequency = Configuration.Default.CheckUpdateFrequency;

        foreach (ThemeProfile profile in Configuration.Default.ThemeProfiles)
        {
            profile.TempFontPath = profile.FontPath ?? "";
            profile.TempFontSize = profile.FontSize;
            profile.TempColors = profile.GetTempColors();
            profile.TempName = profile.Name;
        }

        ModalClosed = false;
    }

    public void ApplySettings(bool quitPopup = false)
    {
        Configuration.Default.CurrentThemeProfile = TempActiveTheme;
        Configuration.Default.UseDiscordRPC = TempUseDiscordRPC;
        Configuration.Default.AutoBackup = TempAutoBackup;
        Configuration.Default.AutoBackupFreq = TempAutoBackupFreq;
        Configuration.Default.BackupCountLimit = TempBackupCountLimit;
        Configuration.Default.UseAutoUpdates = TempUseAutoUpdates;
        Configuration.Default.CheckUpdatesAtStartup = TempCheckUpdatesAtStartup;
        Configuration.Default.CheckUpdateFrequency = TempCheckUpdateFrequency;

        MainWindow.ResetRPCState(TempUseDiscordRPC);

        foreach (ThemeProfile profile in Configuration.Default.ThemeProfiles)
        {
            profile.FontPath = profile.TempFontPath ?? "";
            profile.FontSize = profile.TempFontSize;
            profile.Colors = profile.TempColors;
            profile.Name = profile.TempName;
        }

        Configuration.Save();

        MainWindow.LoadTTF(Configuration.GetCurrentTheme().FontPath, Configuration.GetCurrentTheme().FontSize);
        for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
        {
            ImGui.GetStyle().Colors[i] = Configuration.GetCurrentTheme().Colors[i];
        }

        if (quitPopup)
        {
            ImGui.CloseCurrentPopup();
            ShowWindow = false;
            ModalClosed = true;
        }
    }

    #endregion
    #region Render General Settings Part
    
    private void RenderDiscordRPC()
    {
        ImGui.Spacing();
        ImGui.SeparatorText("Discord Rich Presence");

        ImGui.Checkbox("Use Discord RPC", ref TempUseDiscordRPC);
    }

    private void RenderAutoBackup()
    {
        ImGui.Spacing();
        ImGui.SeparatorText("Automatic Backups");

        ImGui.Checkbox("Auto Backup", ref TempAutoBackup);

        ImGui.Text("Auto Backup Frequency (in minutes)");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        ImGui.InputInt("##AutoBackupFrequency", ref TempAutoBackupFreq, 1, 10);
        TempAutoBackupFreq = Math.Max(TempAutoBackupFreq, 1);

        ImGui.Text("Backup retain count");
        ImGuiEx.Tooltip("How many backups will be kept in the backups folder.\nThe oldest ones will be overwritten if the limit is reached.");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        ImGui.InputInt("##AutoBackupLimit", ref TempBackupCountLimit, 1, 2);
        TempBackupCountLimit = Math.Max(TempBackupCountLimit, 1);
    }

    private void RenderAutoUpdates()
    {
        ImGui.Spacing();
        ImGui.SeparatorText("Automatic Updates");

        ImGui.Checkbox("Use Auto Updates", ref TempUseAutoUpdates);
        ImGui.Checkbox("Check Updates at Launch", ref TempCheckUpdatesAtStartup);

        ImGui.Text("Update Check Frequency (in minutes)");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        ImGui.InputInt("##UpdateCheckFrequencyInt", ref TempCheckUpdateFrequency, 1, 10);
        TempCheckUpdateFrequency = Math.Max(TempCheckUpdateFrequency, 1);
    }

    #endregion
    #region Render Theme Settings Part

    private void RenderThemeContent()
    {
        int removeAt = -1;

        List<string> names = [];
        foreach (ThemeProfile profile in Configuration.Default.ThemeProfiles)
            names.Add(profile.Name);
        string[] actualNames = [.. names];
        if (ImGui.Combo("Current Active Theme", ref CurrentlyActiveTheme, actualNames, actualNames.Length))
        {
            TempActiveTheme = Configuration.Default.ThemeProfiles[CurrentlyActiveTheme].Name;
        }

        if (ImGui.BeginTabBar("EditorSettingsThemes"))
        {
            int i = 0;
            foreach (ThemeProfile profile in Configuration.Default.ThemeProfiles)
            {
                bool isOpened = ImGui.BeginTabItem($"{profile.Name}##EditorSettingsThemesTab_{i}");
                if (isOpened)
                {
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Press Shift and Right-click to delete this profile.\nWarning: This is not a reversible process.");
                    if (ImGui.IsItemHovered() && ImGui.GetIO().KeyShift && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        removeAt = i;
                    }

                    RenderThemeNameChooser(profile, i);
                    RenderFontChooser(profile, i);
                    RenderColorChooser(profile, i);

                    ImGui.EndTabItem();
                }
                else if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Press Shift and Right-click to delete this profile.\nWarning: This is not a reversible process.");
                else if (ImGui.IsItemHovered() && ImGui.GetIO().KeyShift && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    removeAt = i;
                }
                i++;
            }
            if (ImGui.TabItemButton("+##EditorSettingsThemesNewButton"))
            {
                Configuration.Default.ThemeProfiles.Add(new(Configuration.DefaultStyle));
            }
            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                try
                {
                    string? clipboard = ClipboardService.GetText();
                    if (clipboard != null)
                    {
                        ThemeProfile profile = ThemeProfile.FromBase64(clipboard);

                        profile.Name = NormalizeProfileName(profile.Name);
                        profile.TempFontPath = profile.FontPath ?? "";
                        profile.TempFontSize = profile.FontSize;
                        profile.TempColors = profile.GetTempColors();
                        profile.TempName = profile.Name;

                        Configuration.Default.ThemeProfiles.Add(profile);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Couldn't create profile from clipboard:\n{ex}");
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("[Left-click] Create new profile.\n[Right-click] Import from clipboard.");
            }

            ImGui.EndTabBar();
        }

        if (removeAt != -1)
        {
            Configuration.Default.ThemeProfiles.RemoveAt(removeAt);
        }
    }

    private void RenderThemeNameChooser(ThemeProfile profile, int index)
    {
        ImGui.Text("Theme name");
        ImGui.SameLine();
        if (ImGui.InputText($"##{profile.Name}_ThemeName_{index}", ref profile.TempName, 2048, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            profile.TempName = NormalizeProfileName(profile.TempName);
            profile.Name = profile.TempName;
        }
        ImGui.SameLine();
        if (ImGui.Button($"{FontAwesome6.FileExport}##{profile.Name}_ThemeName_{index}_export"))
        {
            try { ClipboardService.SetText(profile.ExportToBase64()); }
            catch (Exception ex) { Console.WriteLine($"There was an error trying to export profile {profile.Name}:\n{ex}"); }
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Export profile to clipboard");
        ImGui.Separator();
    }

    /// <summary>
    /// Checks if the wanted name isn't already used, if it is, then add a number at the end of the name.
    /// </summary>
    /// <param name="tempName">The name entered by the user.</param>
    /// <returns>A normalized name.</returns>
    private string NormalizeProfileName(string tempName)
    {
        if (Configuration.Default.ThemeProfiles.Any(p => p.Name == tempName))
        {
            // Contains because otherwise it would just return 1, since the normalized ones aren't the *same* string.
            return $"{tempName}_{Configuration.Default.ThemeProfiles.FindAll(p => p.Name.Contains(tempName)).Count}";
        }
        return tempName;
    }

    /// <summary>
    /// Handling the font size path selector + font size.
    /// </summary>
    private void RenderFontChooser(ThemeProfile profile, int j)
    {
        bool validPath = (File.Exists(profile.TempFontPath) && Path.GetExtension(profile.TempFontPath) == ".ttf")
            || string.IsNullOrEmpty(profile.TempFontPath); // If file exists OR the path is empty.
        ImGui.PushStyleColor(ImGuiCol.Text, validPath ? ImGui.GetColorU32(ImGuiCol.Text) : 0xFF0000FFu);
        ImGui.Text("Path to ttf font (optional)");
        if (!validPath && ImGui.IsItemHovered())
            ImGui.SetTooltip("This path is invalid.\nCheck that it redirects to a ttf file.");
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.SetNextItemWidth(450);
        ImGui.InputText("##TempFontPathText", ref profile.TempFontPath, 1024);
        ImGui.SameLine();
        if (ImGui.Button("...##TempFontPathButton"))
            PromptTTFPath(profile, j);

        ImGui.Text("Font size (optional)");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        ImGui.InputFloat("##TempFontSizeValue", ref profile.TempFontSize, 1, 10);
        profile.TempFontSize = Math.Max(profile.TempFontSize, 1);

        ImGui.Spacing();
        ImGui.Separator();
    }

    public void PromptTTFPath(ThemeProfile profile, int j)
    {
        void SelectPath(bool success, List<string> paths)
        {
            if (success)
                profile.TempFontPath = paths[0];
            ShowWindow = true;
        }

        ShowWindow = false;
        MainWindow.FileDialogManager.OpenFileDialog("Choose Font", "True Type Font{.ttf}",
            SelectPath, 1, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), true);
    }

    private void RenderColorChooser(ThemeProfile profile, int j)
    {
        float availableHeight = ImGui.GetWindowHeight() - ImGui.GetCursorPosY();
        float buttonHeight = ImGui.CalcTextSize("Ok").Y + ImGui.GetStyle().FramePadding.Y * 2;
        float spacing = ImGui.GetStyle().ItemSpacing.Y + 4;
        ImGui.BeginChild("##RenderColorChooserEditorSettings", new Vector2(ImGui.GetContentRegionAvail().X, availableHeight - buttonHeight - spacing - spacing));

        ImGui.Columns(3);
        for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
        {
            // TODO: Whatever this shit is.
            ImGui.ColorEdit4($"{Enum.GetName(typeof(ImGuiCol), i)}", ref profile.TempColors[i], ImGuiColorEditFlags.NoInputs);
            ImGui.NextColumn();
        }
        ImGui.Columns(1);

        ImGui.EndChild();
    }

    #endregion
}
