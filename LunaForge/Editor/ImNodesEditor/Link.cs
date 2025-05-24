using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using Hexa.NET.Raylib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LunaForge.Editor.ImNodesEditor;

public class Link
{
    private readonly LunaNode outputNode;
    private readonly Pin output;
    private readonly LunaNode inputNode;
    private readonly Pin input;

    private NodeEditor? editor;
    private int id;
    private int outputId;
    private int inputId;

    public Link(int id, LunaNode outputNode, Pin output, LunaNode inputNode, Pin input)
    {
        this.id = id;
        this.outputNode = outputNode;
        this.output = output;
        outputId = output.Id;
        this.inputNode = inputNode;
        this.input = input;
        inputId = input.Id;
    }

    public int Id => id;

    public LunaNode OutputNode => outputNode;

    public Pin Output => output;

    public LunaNode InputNode => inputNode;

    public Pin Input => input;

    public bool IsHovered { get; set; }

    public void Draw()
    {
        ImNodes.Link(id, outputId, inputId);
    }

    public virtual void Destroy()
    {
        if (editor == null)
            return;

        editor.RemoveLink(this);
        editor.RemoveLink(this);
        OutputNode.RemoveLink(this);
        Output.RemoveLink(this);
        InputNode.RemoveLink(this);
        Input.RemoveLink(this);
        editor = null;
    }

    public virtual void Initialize(NodeEditor editor)
    {
        this.editor = editor;

        outputId = output.Id;
        inputId = input.Id;

        OutputNode.AddLink(this);
        Output.AddLink(this);
        InputNode.AddLink(this);
        Input.AddLink(this);
    }
}
