using Hexa.NET.ImGui;
using Hexa.NET.Utilities.Text;
using LunaForge.Editor.Backend;
using LunaForge.Editor.Projects;
using LunaForge.Editor.UI.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PopupManager = LunaForge.Editor.UI.Managers.PopupManager;

namespace LunaForge.Editor.UI.Popups;

public class LauncherWindow : Modal
{
    private string searchString = string.Empty;
    private HistoryEntry historyEntry;
    private bool first = true;

    private bool createProjectDialog;

    public override string Name { get; } = "Launcher";

    protected override ImGuiWindowFlags Flags { get; } =
        ImGuiWindowFlags.MenuBar
        | ImGuiWindowFlags.NoSavedSettings
        | ImGuiWindowFlags.NoCollapse
        | ImGuiWindowFlags.NoDocking
        | ImGuiWindowFlags.NoTitleBar
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoMove;

    public override unsafe void Draw()
    {
        if (!shown || signalClose)
        {
            base.Draw();
            return;
        }

        if (ImGui.BeginPopupModal("DeleteNonExistingProject"))
        {
            ImGui.Text("The selected Project doesn't exist, do you want to remove it from the History?");

            if (ImGui.Button("Yes"))
            {
                ProjectHistory.RemoveEntryByPath(historyEntry.Path);
                ImGui.CloseCurrentPopup();
                Show();
            }
            if (ImGui.Button("No"))
            {
                ImGui.CloseCurrentPopup();
                Show();
            }

            ImGui.EndPopup();
        }

        Vector2 main_viewport_pos = ImGui.GetMainViewport().Pos;
        Vector2 main_viewport_size = ImGui.GetMainViewport().Size;

        ImGui.SetNextWindowPos(main_viewport_pos);
        ImGui.SetNextWindowSize(main_viewport_size);
        ImGui.SetNextWindowBgAlpha(0.9f);
        ImGui.Begin("Overlay", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs);
        ImGui.End();

        if (first)
        {
            ImGui.SetNextWindowSize(new(800, 500));
            Vector2 size = new(800, 500);
            Vector2 mainViewportPos = ImGui.GetMainViewport().Pos;

            ImGui.SetNextWindowPos(mainViewportPos + (main_viewport_size / 2 - size / 2));
            first = false;
        }
        base.Draw();
    }

    protected override unsafe void DrawContent()
    {
        ImGui.SeparatorText(Name);

        Vector2 pos = ImGui.GetCursorPos();
        Vector2 padding = ImGui.GetStyle().CellPadding;
        Vector2 spacing = ImGui.GetStyle().ItemSpacing;
        float lineHeight = ImGui.GetTextLineHeight();

        const float widthSide = 300;
        Vector2 avail = ImGui.GetContentRegionAvail();
        Vector2 entrySize = new(avail.X - widthSide, ImGui.GetTextLineHeight() * 2 + padding.Y * 2 + spacing.Y);
        Vector2 trueEntrySize = entrySize - new Vector2(ImGui.GetStyle().IndentSpacing, 0);

        byte* buffer = stackalloc byte[2048];
        StrBuilder builder = new(buffer, 2048);

        var entries = ProjectHistory.Entries;
        var pinned = ProjectHistory.Pinned;

        Vector2 entryChildSize = new(entrySize.X, avail.Y);
        ImGui.BeginChild("Entries", entryChildSize);

        ImGui.InputTextWithHint("##SearchBar", "Search ...", ref searchString, 1024);

        if (ImGui.TreeNodeEx("Pinned", ImGuiTreeNodeFlags.DefaultOpen))
        {
            for (int i = 0; i < pinned.Count; i++)
            {
                HistoryEntry entry = pinned[i];

                if (!string.IsNullOrWhiteSpace(searchString) && (!entry.Name.Contains(searchString) || !entry.Path.Contains(searchString)))
                {
                    continue;
                }

                DisplayEntry(entry, padding, spacing, lineHeight, trueEntrySize);
            }
            ImGui.TreePop();
        }

        int activeNode = -1;
        bool open = false;
        for (int i = 0; i < entries.Count; i++)
        {
            HistoryEntry entry = entries[i];
            if (!string.IsNullOrWhiteSpace(searchString) && (!entry.Name.Contains(searchString) || !entry.Path.Contains(searchString)))
            {
                continue;
            }

            if (entry.LastAccess.Date == DateTime.UtcNow.Date)
            {
                if (activeNode != 0)
                {
                    if (activeNode != -1 && open)
                    {
                        ImGui.TreePop();
                    }

                    open = ImGui.TreeNodeEx("Today", ImGuiTreeNodeFlags.DefaultOpen);
                    activeNode = 0;
                }
            }
            else if (entry.LastAccess.Date == DateTime.UtcNow.Date.AddDays(-1))
            {
                if (activeNode != 1)
                {
                    if (activeNode != -1 && open)
                    {
                        ImGui.TreePop();
                    }

                    open = ImGui.TreeNodeEx("Yesterday", ImGuiTreeNodeFlags.DefaultOpen);
                    activeNode = 1;
                }
            }
            else if (entry.LastAccess.Date > DateTime.UtcNow.Date.AddDays(-7))
            {
                if (activeNode != 2)
                {
                    if (activeNode != -1 && open)
                    {
                        ImGui.TreePop();
                    }

                    open = ImGui.TreeNodeEx("A week ago", ImGuiTreeNodeFlags.DefaultOpen);
                    activeNode = 2;
                }
            }
            else if (entry.LastAccess.Date > DateTime.UtcNow.Date.AddMonths(-1))
            {
                if (activeNode != 3)
                {
                    if (activeNode != -1 && open)
                    {
                        ImGui.TreePop();
                    }

                    open = ImGui.TreeNodeEx("A month ago", ImGuiTreeNodeFlags.DefaultOpen);
                    activeNode = 3;
                }
            }
            else if (activeNode != 4)
            {
                if (activeNode != -1 && open)
                {
                    ImGui.TreePop();
                }

                open = ImGui.TreeNode("Older");
                activeNode = 4;
            }

            if (open)
            {
                DisplayEntry(entry, padding, spacing, lineHeight, trueEntrySize);
            }
        }

        if (activeNode != -1 && open)
        {
            ImGui.TreePop();
        }
        ImGui.Dummy(new(1));
        ImGui.EndChild();

        Vector2 childSize = new(widthSide - padding.X, avail.Y);
        ImGui.SetCursorPos(pos + new Vector2(entrySize.X + padding.X, 0));
        ImGui.BeginChild("Child", childSize);

        if (ImGui.Button($"{FA.SquarePlus} New Project", new(childSize.X, 50)))
        {
            Close();
            PopupManager.Show<NewProjWindow>();
        }
        if (ImGui.Button($"{FA.MagnifyingGlass} Open Project", new(childSize.X, 50)))
        {
            MainWindow.FileDialogManager.OpenFileDialog("Open Project", "LunaForge Project{.lfp}",
                OpenProjectCallback, 1, EditorConfig.Default.Get<string>("ProjectsFolder").Value);
        }
        if (ImGui.Button($"{FA.Clone} Clone Project", new(childSize.X, 50)))
        {
            // TODO: Implement git clone.
        }
        if (ImGui.Button($"{FA.Xmark} Continue without Project", new(childSize.X, 50)))
        {
            Close();
        }

        ImGui.EndChild();
    }

    private void OpenProjectCallback(bool success, List<string> path)
    {
        if (!success)
            return;
        ProjectManager.Load(path[0]); // Only 1 possible choice so we can safely use index 0.
        Close();
    }

    private unsafe void DisplayEntry(HistoryEntry entry, Vector2 padding, Vector2 spacing, float lineHeight, Vector2 entrySize)
    {
        byte* buffer = stackalloc byte[512];
        StrBuilder builder = new(buffer, 512);
        Vector2 pos = ImGui.GetCursorPos();

        ImGui.SetCursorPos(new(entrySize.X, pos.Y + padding.Y));

        if (!entry.Pinned)
        {
            ImGuiManager.PushFont("Icons-Regular");
        }
        if (ImGui.SmallButton($"{FA.Bookmark} {entry.Path}"))
        {
            if (entry.Pinned)
            {
                ProjectHistory.Unpin(entry.Path);
            }
            else
            {
                ProjectHistory.Pin(entry.Path);
            }
        }

        ImGuiManager.PopFont();

        ImGui.SetCursorPos(pos);

        if (ImGui.Button(entry.Path, entrySize))
        {
            if (File.Exists(entry.Path))
            {
                ProjectManager.Load(entry.Path);
                Close();
            }
            else
            {
                historyEntry = entry;
                ImGui.OpenPopup("DeleteNonExistingProject");
            }
        }

        DisplayEntryContextMenu(entry);

        ImGui.SetCursorPos(pos + padding);

        Vector2 imageSize = new(entrySize.Y - padding.Y * 2);

        Vector2 nextPos = new(pos.X + padding.X + imageSize.X + spacing.X, pos.Y + padding.Y);

        ImGui.SetCursorPos(nextPos);

        ImGui.Text(entry.Name);

        builder.Reset();
        builder.Append(entry.LastAccess, "dd/MM/yyyy HH:mm");
        builder.End();

        float size = ImGui.CalcTextSize(builder).X;

        ImGui.SetCursorPos(new(entrySize.X - size, nextPos.Y));
        ImGui.Text(builder);

        nextPos.Y += spacing.Y + lineHeight;
        nextPos.X += 5;

        ImGui.SetCursorPos(nextPos);

        ImGui.TextDisabled(entry.Path);

        ImGui.SetCursorPosY(pos.Y + entrySize.Y + spacing.Y);
    }

    private static unsafe void DisplayEntryContextMenu(HistoryEntry entry)
    {
        byte* buffer = stackalloc byte[512];
        StrBuilder builder = new(buffer, 512);
        if (ImGui.BeginPopupContextItem(entry.Path))
        {
            if (ImGui.MenuItem($"{FA.Trash} Remove from List"))
            {
                ProjectHistory.RemoveEntryByPath(entry.Path);
            }
            if (!entry.Pinned)
            {
                ImGuiManager.PushFont("Icons-Regular");
            }
            if (!entry.Pinned && ImGui.MenuItem($"{FA.Bookmark} Pin"))
            {
                ProjectHistory.Pin(entry.Path);
            }
            if (entry.Pinned && ImGui.MenuItem($"{FA.Bookmark} Unpin"))
            {
                ProjectHistory.Unpin(entry.Path);
            }
            ImGuiManager.PopFont();
            if (ImGui.MenuItem($"{FA.Copy} Copy Path"))
            {
                //Clipboard.SetText(entry.Path);
            }

            ImGui.EndPopup();
        }
    }

    public override void Reset()
    {
        
    }
}

