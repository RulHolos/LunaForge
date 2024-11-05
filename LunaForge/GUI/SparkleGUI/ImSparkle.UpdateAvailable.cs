using ImGuiNET;
using LunaForge.GUI.Helpers;
using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.SparkleGUI;

// Update Available
internal partial class SparkleManager
{
    public bool ShowUpdateAvailableWindow = false;
    public UpdateDetectedEventArgs UpdateDetectedArgs;
    public Dictionary<string, string> ReleaseNotes = [];

    public async Task ShowUpdateAvailable(UpdateDetectedEventArgs e)
    {
        UpdateDetectedArgs = e;
        ReleaseNotesGrabber grabber = new(string.Empty, string.Empty, this);
        try
        {
            foreach (AppCastItem item in e.AppCastItems)
            {
                string notes = await grabber.DownloadAllReleaseNotes([item], item, CancellationToken.None);
                ReleaseNotes.Add(item.Version, notes);
            }
        }
        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        ShowUpdateAvailableWindow = true;
    }

    public void RenderUpdateAvailable()
    {
        if (!ShowUpdateAvailableWindow)
            return;

        ImGui.OpenPopup("Update Available##UpdateAvailableWindow");

        Vector2 modalSize = new(800, 700);
        Vector2 renderSize = new(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
        ImGui.SetNextWindowSize(modalSize);
        ImGui.SetNextWindowPos(renderSize / 2 - (modalSize / 2));

        if (ImGui.BeginPopupModal("Update Available##UpdateAvailableWindow", ref ShowUpdateAvailableWindow, ImGuiWindowFlags.Modal))
        {
            ImGui.Text($"Update Available: {UpdateDetectedArgs.LatestVersion.Version}");
            ImGui.Text($"Current Version: {UpdateDetectedArgs.ApplicationConfig.InstalledVersion}");

            ImGui.BeginChild("##UpdateAvailableItems");
            foreach (AppCastItem item in UpdateDetectedArgs.AppCastItems)
            {
                ImGui.Spacing();
                ImGui.SeparatorText(item.Version);

                if (item.IsCriticalUpdate)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(new Vector4(1.0f, 0.0f, 0.0f, 1.0f)));
                    ImGui.Text("Critical Update!!");
                    ImGui.PopStyleColor();
                }

                ImGui.TextWrapped(item.Description);
                ImGui.Text("Changelog notes:");

                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 30);
                ImGui.TextWrapped(item.Description);
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 30);
            }
            ImGui.EndChild();

            ImGui.EndPopup();
        }
    }
}
