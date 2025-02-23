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
using System.ComponentModel;

namespace LunaForge.EditorData.Project;

public enum ShaderFormat
{
    [Description(".hlsl")]
    HLSL,
    [Description(".glsl")]
    GLSL
}

public static class EnumHelper
{
    /// <summary>
    /// Gets an attribute on an enum field value
    /// </summary>
    /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
    /// <param name="enumVal">The enum value</param>
    /// <returns>The attribute of type T that exists on the enum value</returns>
    /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
    public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return (attributes.Length > 0) ? (T)attributes[0] : null;
    }
}

public class LunaShader : LunaProjectFile
{
    public NodeGraph Graph { get; set; } = new();
    public List<GraphNode> GraphNodes { get; set; } = [];

    // Doesn't support ExPlus's format. Only sub and Evo.
    public ShaderFormat FileFormat { get; set; } = ShaderFormat.HLSL;

    public LunaShader(LunaForgeProject parentProj, string path)
        : base(parentProj, path)
    {
        
    }

    #region Rendering

    public override void Render()
    {
        ImGui.Text("Shader Editor is a WIP.");
        return;
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
