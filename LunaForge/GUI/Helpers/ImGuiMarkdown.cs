using ImGuiNET;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.Helpers;

public class ImGuiMarkdown
{
    public string MarkdownText { get; set; } = string.Empty;
    public MarkdownDocument Document { get; set; } = null;

    public ImGuiMarkdown(string text)
    {
        MarkdownText = text;
        Document = Markdown.Parse(MarkdownText);
    }

    #region Blocks

    private void RenderHeader(HeadingBlock heading)
    {
        float scale = 1f;
        switch (heading.Level)
        {
            case 1:
                scale = 2f;
                break;
            case 2:
                scale = 1.7f;
                break;
            case 3:
                scale = 1.5f;
                break;
            case 4:
                scale = 1f;
                break;
        }
        ImGui.SetWindowFontScale(scale);
        ImGui.TextUnformatted(heading.Inline.FirstChild.ToString());
        ImGui.SetWindowFontScale(1f);
    }

    private void RenderParagraph(ParagraphBlock paragraph)
    {
        foreach (var inline in paragraph.Inline)
        {
            RenderInline(inline);
        }
        ImGui.NewLine();
    }

    private void RenderInline(Inline inline)
    {
        switch (inline)
        {
            case LiteralInline literal:
                ImGui.TextUnformatted(literal.Content.ToString());
                break;
            case LinkInline link:
                ImGui.TextColored(new System.Numerics.Vector4(0, 0.5f, 1, 1), link.Url);
                break;
            case EmphasisInline emphasis:
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, emphasis.DelimiterCount == 2 ? 1f : 0.8f);
                foreach (var subInline in emphasis)
                {
                    RenderInline(subInline);
                }
                ImGui.PopStyleVar();
                break;
        }
    }

    private void RenderList(ListBlock list)
    {
        foreach (var item in list)
        {
            if (item is ListItemBlock listItem)
            {
                ImGui.BulletText(listItem.ToString());
            }
        }
    }

    private void RenderCodeBlock(CodeBlock codeBlock)
    {
        // Get the code content from the CodeBlock
        var codeContent = codeBlock.Lines.ToString();

        // Set a background color or a framed box to make it stand out
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.12f, 0.12f, 0.12f, 1f)); // Dark gray background
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));

        // Begin a child region for code appearance
        if (ImGui.BeginChild("##codeBlock", new Vector2(0, ImGui.GetTextLineHeight() * codeBlock.Lines.Count + 10)))
        {
            ImGui.TextWrapped(codeContent);
        }
        ImGui.EndChild();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
    }

    #endregion

    private void GetDescendantsRecursive(MarkdownObject obj)
    {
        foreach (MarkdownObject item in obj.Descendants())
        {
            ImGui.Text(item.GetType().Name);
            switch (item)
            {
                case HeadingBlock heading:
                    RenderHeader(heading);
                    break;
                case ParagraphBlock paragraph:
                    RenderParagraph(paragraph);
                    break;
                case ListBlock list:
                    RenderList(list);
                    break;
                case LiteralInline literal:
                    RenderInline(literal);
                    break;
                case CodeBlock codeBlock:
                    RenderCodeBlock(codeBlock);
                    break;
            }
            GetDescendantsRecursive(item);
        }
    }

    public void TextMarkdown()
    {
        ImGui.Text(Markdown.ToPlainText(MarkdownText));
        GetDescendantsRecursive(Document);
    }
}
