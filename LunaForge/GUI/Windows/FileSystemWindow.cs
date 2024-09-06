﻿using ImGuiNET;
using LunaForge.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LunaForge.GUI.Windows;

public class FileSystemWindow : ImGuiWindow
{
    public FileSystemWindow(MainWindow parent)
        : base(parent, true)
    {

    }

    public override void Render()
    {
        if (BeginNoClose("Project Files"))
        {
            // No project
            if (ParentWindow.Workspaces.Current == null)
            {
                End();
                return;
            }

            string dirPath = ParentWindow.Workspaces.Current.PathToProjectRoot;
            RenderFileTree(dirPath);

            End();
        }
    }

    public void RenderFileTree(string directoryPath)
    {
        string[] directories = Directory.GetDirectories(directoryPath);
        string[] files = Directory.GetFiles(directoryPath);

        foreach (var dir in directories)
        {
            string folderName = Path.GetFileName(dir);

            if (ImGui.TreeNode(folderName))
            {
                RenderFileTree(dir);
                ImGui.TreePop();
            }
        }

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.EndsWith(".lfp"))
                continue;
            ImGui.Selectable(fileName);
            if (ImGui.BeginPopupContextItem())
            {
                ImGui.Text(fileName);
                ImGui.Separator();

                if (ImGui.Selectable("Open file"))
                {
                    OpenFile(file);
                }

                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGui.GetIO().KeyShift ? ImGuiCol.Text : ImGuiCol.TextDisabled));
                if (ImGui.Selectable("Delete file") && ImGui.GetIO().KeyShift)
                {
                    Console.WriteLine($"Deleting file: {file}");
                }
                ImGui.PopStyleColor();
                if (!ImGui.GetIO().KeyShift && ImGui.IsItemHovered())
                    ImGui.SetTooltip("Hold SHIFT to delete.");
                ImGui.EndPopup();
            }
            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
            {
                OpenFile(file);
            }
        }
    }

    void OpenFile(string filePath)
    {
        Console.WriteLine($"Opening file: {filePath}");
    }
}
