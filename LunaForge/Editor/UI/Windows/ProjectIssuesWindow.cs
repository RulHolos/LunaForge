using Hexa.NET.ImGui;
using LunaForge.Editor.Projects;
using LunaForge.Editor.UI.ImGuiExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Windows;

[Flags]
public enum ShowingIssuesPriority
{
    Errors,
    Warnings,
    Messages,
}

public enum IssuesContext
{
    WholeProject,
    CurrentFile,
    OpenedFiles,
}

public class ProjectIssuesWindow : EditorWindow
{
    protected override string Name => "Issues List";

    public override bool IsShown { get; protected set; } = true;
    public override ImGuiWindowFlags Flags { get; set; } = ImGuiWindowFlags.NoCollapse;
    public override bool CanBeClosed { get; set; } = false;

    private ShowingIssuesPriority ShownIssues { get; set; } = ShowingIssuesPriority.Errors | ShowingIssuesPriority.Warnings | ShowingIssuesPriority.Messages;
    private IssuesContext CurrentContext = IssuesContext.WholeProject;

    protected override void InitWindow()
    {
        base.InitWindow();
    }

    public override void DrawContent()
    {
        ImGuiEx.EnumCombo("IssuesContext", ref CurrentContext, 150f);
        ImGui.SameLine();
        if (ImGuiEx.OnOffButton($"{0} Error(s)", ShownIssues.HasFlag(ShowingIssuesPriority.Errors)))
        {
            if (ShownIssues.HasFlag(ShowingIssuesPriority.Errors))
                ShownIssues &= ~ShowingIssuesPriority.Errors;
            else
                ShownIssues |= ShowingIssuesPriority.Errors;
        }
        ImGui.SameLine();
        ImGuiEx.OnOffButton($"{0} Warning(s)", ShownIssues.HasFlag(ShowingIssuesPriority.Warnings));
        ImGui.SameLine();
        ImGuiEx.OnOffButton($"{0} Message(s)", ShownIssues.HasFlag(ShowingIssuesPriority.Messages));

        // Draw by order: Error, Warning, Message

        ImGui.BeginChild("IssuesWindowScrolling");

        ImGui.BeginTable("#Table", 3, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("File", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        
        for (int i = 0; i < 5; i++)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text($"{FA.CircleExclamation}");

            ImGui.TableSetColumnIndex(1);

            ImGui.TextWrapped("This is a test issue.");

            ImGui.TableSetColumnIndex(2);

            ImGui.TextWrapped("File.cs:10");
        }

        ImGui.EndTable();

        ImGui.EndChild();
    }
}
