using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Projects;

public class LunaScriptEditor : LunaProjectFile
{
    public Hexa.NET.ImGui.Widgets.Extras.TextEditor.TextEditor TextEditor;

    public LunaScriptEditor()
        : base()
    {
        //TextEditor = new("", )
    }

    public override void Draw()
    {
        //TextEditor.Draw(Encoding.ASCII.GetBytes("d"), ImGui.GetContentRegionAvail());
    }

    public override void Dispose()
    {
        
    }
}
