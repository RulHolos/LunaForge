using IconFonts;
using ImGuiNET;
using LunaForge.GUI.Helpers;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
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

            if (ImGui.Button("Install Update"))
            {
                UserRespondedToUpdateCheck();
            }

            ImGui.BeginChild("##UpdateAvailableItems");
            foreach (AppCastItem item in UpdateDetectedArgs.AppCastItems)
            {
                ImGui.Spacing();
                ImGui.PushStyleColor(ImGuiCol.Text, item.Version == UpdateDetectedArgs.ApplicationConfig.InstalledVersion
                    ? ImGui.GetColorU32(new Vector4(0f, 1f, 0f, 1f))
                    : ImGui.GetColorU32(ImGuiCol.Text));
                ImGui.SeparatorText($"{item.Version} - {item.PublicationDate}");
                if (ImGui.IsItemHovered() && item.Version == UpdateDetectedArgs.ApplicationConfig.InstalledVersion)
                    ImGui.SetTooltip("Installed Version");
                ImGui.PopStyleColor();

                if (item.IsCriticalUpdate)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(new Vector4(1.0f, 0.0f, 0.0f, 1.0f)));
                    ImGui.Text("Critical Update!!");
                    ImGui.PopStyleColor();
                    ImGui.Spacing();
                }

                ImGui.Text("Changelog notes:");
                ImGui.Spacing();

                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 30);
                string finalString = "";
                foreach (string str in item.Description.Split('\n'))
                {
                    finalString += str.Trim() + '\n';
                }
                ImGui.TextWrapped(finalString);
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 30);

                ImGui.Spacing();
                ImGui.TextLinkOpenURL("Manual Download", item.DownloadLink);
            }
            ImGui.EndChild();

            ImGui.EndPopup();
        }
    }

    public void UserRespondedToUpdateCheck()
    {
        InitAndBeginDownload(UpdateDetectedArgs.LatestVersion);
    }
}
