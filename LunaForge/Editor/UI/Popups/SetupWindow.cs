using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets.Dialogs;
using LunaForge.Editor.UI.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Popups;

public class SetupWindow : Modal
{
    private int page = 0;
    private bool first = true;
    private const int pageCount = 3;

    public override string Name { get; } = "Getting Started";

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

    protected override void DrawContent()
    {
        ImGui.SeparatorText(Name);
        Vector2 avail = ImGui.GetContentRegionAvail();
        const float footerHeight = 50;
        avail.Y -= footerHeight;
        ImGui.BeginChild("Content", avail);

        switch (page)
        {
            case 0:
                Page1();
                break;

            case 1:
                Page2();
                break;

            case 2:
                Page3();
                break;
        }

        ImGui.EndChild();

        ImGui.BeginTable("#Table", 2, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("");

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(1);

        if (page > 0)
        {
            if (ImGui.Button("Back"))
            {
                page--;
            }
            ImGui.SameLine();
        }

        if (page == pageCount - 1)
        {
            if (ImGui.Button("Finish"))
            {
                FinishSetup();
            }
        }
        else
        {
            if (ImGui.Button("Next"))
            {
                page++;
            }
        }

        ImGui.EndTable();
    }

    private void FinishSetup()
    {
        EditorConfig config = EditorConfig.Default;
        Directory.CreateDirectory(projectsFolder);
        config.ProjectsFolder = projectsFolder;
        config.SetupDone = true;
        config.Save();
        Close();
    }

    private static void Page1()
    {
        ImGui.Text("Welcome to LunaForge!");

        ImGui.Dummy(new(0, 20));
        ImGui.Indent(48);

        ImGui.Text("This editor is still in early alpha. Expect some bugs or breaking changes.");

        ImGui.Unindent();
    }

    private string projectsFolder = DetermineDefaultProjectsPath();

    private static string DetermineDefaultProjectsPath()
    {
        string projectsPath;
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            projectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LunaForge", "Projects");
        }
        else
        {
            throw new PlatformNotSupportedException("LunaForge currently supports only Window, Linux and Mac.");
        }
        return projectsPath;
    }

    private void Page2()
    {
        ImGui.Text($"Step {page}: Setting a projects folder");

        ImGui.Dummy(new(0, 20));
        ImGui.Indent(48);

        ImGui.Text("Projects folder");
        ImGui.InputText("##TextInputProjectsFolder", ref projectsFolder, 1024);
        ImGui.SameLine();
        if (ImGui.Button("..."))
        {
            OpenFileDialog dialog = new();
            dialog.OnlyAllowFolders = true;
            dialog.Show((s, e) =>
            {
                if (e != DialogResult.Ok)
                    return;
                projectsFolder = ((OpenFileDialog)s!).SelectedFile!;
                Show();
            });
        }

        ImGui.Unindent();
    }

    private static void Page3()
    {
        ImGui.Text("Done!");

        ImGui.Dummy(new(0, 20));
        ImGui.Indent(48);

        ImGui.Text("Links:");
        ImGui.Indent();
        if (ImGui.MenuItem($"LunaForge on Github"))
        {
            Process.Start("explorer.exe", "https://github.com/RulHolos/LunaForge");
        }

        ImGui.Unindent();
        ImGui.Unindent();
    }

    public override void Reset()
    {

    }
}

