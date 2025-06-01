using Hexa.NET.ImGui;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.Projects;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Popups;

public class NewProjWindow : Modal
{
    private static ILogger Logger = CoreLogger.Create("New Project Win");

    private bool first = true;

    public override string Name { get; } = "New Project";

    protected override ImGuiWindowFlags Flags { get; } =
        ImGuiWindowFlags.NoSavedSettings
        //ImGuiWindowFlags.MenuBar
        | ImGuiWindowFlags.NoCollapse
        | ImGuiWindowFlags.NoDocking
        //| ImGuiWindowFlags.NoTitleBar
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoMove;

    private string projectName = "Untitled";
    private string author = EditorConfig.Default.Get<string>("ProjectAuthor").Value;
    private bool initializeWithDefaultLib = true;
    private TemplateDef SelectedTemplate;
    private HashSet<TemplateDef> Templates;

    private class TemplateDef()
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0.0";
        public string ZipPath { get; set; } = string.Empty;
    }

    public NewProjWindow()
    {
        string templateDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LunaForge", "Templates");
        Directory.CreateDirectory(templateDir);

        DirectoryInfo df = new(templateDir);
        List<FileInfo> fis = [.. df.GetFiles("*.json")];

        Templates = [.. from FileInfo fi in fis
            where File.Exists(Path.Combine(templateDir, Path.ChangeExtension(fi.Name, ".zip")))
            select GetTemplateInfo(templateDir, fi)];
        Templates.Add(new() { Name = "Empty", Description = "Empty Project" });
    }

    private TemplateDef GetTemplateInfo(string templateDir, FileInfo fi)
    {
        try
        {
            using StreamReader sr = fi.OpenText();
            TemplateDef def = JsonConvert.DeserializeObject<TemplateDef>(sr.ReadToEnd());
            def.ZipPath = Path.Combine(templateDir, Path.ChangeExtension(fi.Name, ".zip"));
            return def;
        }
        catch (Exception ex)
        {
            Logger.Error($"Cannot find template info for template '{fi.Name}'. Reason:\n{ex}");
            return default;
        }
    }

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
        Vector2 avail = ImGui.GetContentRegionAvail();
        const float footerHeight = 50;
        avail.Y -= footerHeight;
        ImGui.BeginChild("Content", avail);

        ImGui.BeginGroup();
        {
            if (ImGui.BeginListBox("##TemplateList"))
            {
                foreach (var def in Templates)
                {
                    if (ImGui.Selectable($"{def.Name}", SelectedTemplate == def))
                    {
                        SelectedTemplate = def;
                    }
                }
                ImGui.EndListBox();
            }
            ImGui.SameLine();

            if (SelectedTemplate != null)
            {
                ImGui.BeginGroup();
                ImGui.TextWrapped($"{SelectedTemplate.Name} (v{SelectedTemplate.Version})");
                ImGui.TextWrapped(SelectedTemplate.Description);
                ImGui.EndGroup();
            }
        }
        ImGui.EndGroup();

        ImGui.Separator();

        ImGui.InputText("Name", ref projectName, 128);
        ImGui.InputText("Author", ref author, 128);
        ImGui.Checkbox("Initialize with default library", ref initializeWithDefaultLib);

        ImGui.EndChild();

        ImGui.BeginTable("#Table", 2, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("");

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(1);

        if (ImGui.Button("Cancel"))
        {
            Close();
        }
        ImGui.SameLine();
        ImGui.BeginDisabled(SelectedTemplate == null);
        if (ImGui.Button("Create"))
        {
            EditorConfig.Default.Get<string>("ProjectAuthor").Value = author;
            CreateProject();
            Close();
        }
        ImGui.EndDisabled();

        ImGui.EndTable();
    }

    private void CreateProject()
    {
        if (SelectedTemplate.Name == "Empty")
        {
            ProjectManager.CreateEmpty(Path.Combine(EditorConfig.Default.Get<string>("ProjectsFolder").Value, projectName), initializeWithDefaultLib);
        }
        else
        {
            ProjectManager.CreateFromTemplate(Path.Combine(EditorConfig.Default.Get<string>("ProjectsFolder").Value, projectName), SelectedTemplate.ZipPath);
        }
    }

    public override void Reset()
    {
        
    }
}
