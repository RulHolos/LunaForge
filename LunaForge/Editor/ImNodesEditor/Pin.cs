using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.ImNodesEditor;

public class Pin
{
    private NodeEditor? editor;
    private LunaNode parent;
    private int id;

    public readonly string Name;
    public ImNodesPinShape Shape;
    public PinKind Kind;
    public PinType Type;
    public uint MaxLinks;

    private readonly List<Link> links = [];

    public Pin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue)
    {
        this.id = id;
        this.Name = name;
        this.Shape = shape;
        this.Kind = kind;
        this.Type = type;
        this.MaxLinks = maxLinks;
    }

    public event EventHandler<Link>? LinkCreated;
    public event EventHandler<Link>? LinkRemoved;

    public int Id => id;
    public LunaNode Parent => parent;
    public List<Link> Links => links;
    
    public void AddLink(Link link)
    {
        links.Add(link);
        LinkCreated?.Invoke(this, link);
    }

    public void RemoveLink(Link link)
    {
        links.Remove(link);
        LinkRemoved?.Invoke(this, link);
    }

    public virtual bool CanCreateLink(Pin other)
    {
        if (id == other.id) return false;
        if (Links.Count == MaxLinks) return false;
        if (!IsType(other)) return false;
        if (Kind == other.Kind) return false;

        return true;
    }

    public bool IsType(Pin other)
    {
        return other.Type == Type;
    }

    public void Draw()
    {
        if (Kind == PinKind.Input)
        {
            ImNodes.BeginInputAttribute(id, Shape);
            DrawContent();
            ImNodes.EndInputAttribute();
        }
        if (Kind == PinKind.Output)
        {
            ImNodes.BeginOutputAttribute(id, Shape);
            DrawContent();
            ImNodes.EndOutputAttribute();
        }
        if (Kind == PinKind.Static)
        {
            ImNodes.BeginStaticAttribute(id);
            DrawContent();
            ImNodes.EndStaticAttribute();
        }
    }

    protected virtual void DrawContent()
    {
        ImGui.Text(Name);
    }

    public virtual void Initialize(NodeEditor editor, LunaNode parent)
    {
        this.editor = editor;
        this.parent = parent;
        if (id == 0)
            id = editor.GetUniqueId();
    }

    public virtual void Destroy()
    {
        if (editor == null)
            return;

        var links = Links.ToArray();
        for (int i = 0; i < links.Length; i++)
        {
            links[i].Destroy();
        }

        editor = null;
    }
}
