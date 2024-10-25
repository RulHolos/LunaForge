﻿using ImGuiNET;
using LunaForge.EditorData.Commands;
using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Traces;
using LunaForge.GUI.NodeGraphRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Project;

public class LunaShader : LunaProjectFile
{
    NodeGraph Graph = new();

    public LunaShader(LunaForgeProject parentProj, string path)
        : base(parentProj, path)
    {

    }

    #region Rendering

    public override void Render()
    {
        // TODO: wtf.
        ImGui.BeginChild("CanvasWindowsRenderer");
        if (Graph.BeginCanvas("test_canvas"))
        {
            Graph.EndCanvas();
        }
        ImGui.EndChild();
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
