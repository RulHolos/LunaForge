using LunaForge.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using LunaForge.EditorData.Project;
using TreeNode = LunaForge.EditorData.Nodes.TreeNode;
using LunaForge.EditorData.Nodes;
using System.Numerics;

namespace LunaForge.GUI.Windows;

public class NodeAttributeWindow : ImGuiWindow
{
    public TreeNode? CurrentNode => (MainWindow.Workspaces.Current?.CurrentProjectFile as LunaDefinition)?.SelectedNode;

    public NodeAttributeWindow()
        : base(true)
    {

    }

    public override void Render()
    {
        if (BeginNoClose("Node Attributes"))
        {
            if (CurrentNode == null)
            {
                End();
                return;
            }

            ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;
            if (ImGui.BeginTable($"NodeAttributeTable_{CurrentNode.NodeName}", 3, flags))
            {
                ImGui.TableSetupColumn("Properties", ImGuiTableColumnFlags.NoResize, 1.3f);
                ImGui.TableSetupColumn("Parameters", ImGuiTableColumnFlags.NoResize, 1.7f);
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.NoResize, 0.4f);
                ImGui.TableHeadersRow();

                for (int i = 0; i < CurrentNode.Attributes.Count; i++)
                {
                    NodeAttribute attr = CurrentNode.Attributes[i];
                    if (attr == null || !attr.IsUsed)
                        continue; // Skip displaying the attribute if it's not used by the node.

                    ImGui.TableNextRow();

                    // Attr Name
                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextWrapped(attr.AttrName);

                    // Attr Value
                    ImGui.TableSetColumnIndex(1);
                    ImGui.SetNextItemWidth(-1);
                    ImGuiInputTextFlags Tflags = ImGuiInputTextFlags.EnterReturnsTrue;
                    if (attr.AttrValue != string.Empty && attr.TempAttrValue == string.Empty)
                        attr.TempAttrValue = attr.AttrValue;
                    if (ImGui.InputText($"##{attr.AttrName}_input", ref attr.TempAttrValue, 2048, Tflags))
                        CommitEdit(attr);
                    if (ImGui.IsItemDeactivated())
                        CommitEdit(attr);

                    // More...
                    Vector2 vec = ImGui.CalcTextSize("...");
                    ImGui.SetNextItemWidth(vec.X + 5);
                    ImGui.TableSetColumnIndex(2);
                    if (ImGui.Button($"...##{attr.AttrName}", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
                    {
                        CurrentNode.ParentDef.ShowEditWindow(CurrentNode, CurrentNode.Attributes.IndexOf(attr));
                    }
                }

                ImGui.EndTable();
            }
            End();
        }
    }

    public void CommitEdit(NodeAttribute attr)
    {
        attr.RaiseEdit(attr.TempAttrValue);
    }
}
