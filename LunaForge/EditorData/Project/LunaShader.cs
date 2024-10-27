using ImGuiNET;
using LunaForge.EditorData.Commands;
using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Traces;
using LunaForge.GUI.NodeGraphRenderer;
using LunaForge.EditorData.GraphNodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Project;

public class LunaShader : LunaProjectFile
{
    public NodeGraph Graph { get; set; } = new();
    public List<GraphNode> GraphNodes { get; set; } = [];

    // TODO: support GLSL format too.
    public string FileFormat { get; set; } = ".hlsl";

    public LunaShader(LunaForgeProject parentProj, string path)
        : base(parentProj, path)
    {
        
    }

    #region Rendering

    public override void Render()
    {
        // TODO: wtf.
        using (Graph.DoCanvas($"ShaderCanvas_{FileName}"))
        {
            foreach (GraphNode node in GraphNodes)
            {

            }

#if DEBUG
            RenderDebug();
#endif
        }
    }

    private void RenderDebug()
    {
        ImGui.GetWindowDrawList().AddText(ImGui.GetMousePos() + new Vector2(20), ImGui.GetColorU32(ImGuiCol.Text), Graph.ScreenToGrid(ImGui.GetMousePos()).ToString());
    }

    #endregion
    #region Serialization



    #endregion
    #region Nodes

    public override void Delete()
    {
        return;
    }

    public override bool Delete_CanExecute()
    {
        return true;
    }

    #endregion
    #region IO

    public override async Task Save(bool saveAs = false)
    {

    }

    public static async Task<LunaShader> CreateFromFile(LunaForgeProject parentProject, string filePath)
    {
        try
        {
            LunaShader shader = new(parentProject, filePath);
            return shader;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return default;
        }
    }

    public override void Close()
    {
        EditorTraceContainer.RemoveChecksFromSource(this);
        ParentProject.ProjectFiles.Remove(this);
        ParentProject.CurrentProjectFile = null;
    }

    #endregion
}
