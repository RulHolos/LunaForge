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

    public override void Cut()
    {
        throw new NotImplementedException();
    }

    public override bool Cut_CanExecute()
    {
        throw new NotImplementedException();
    }

    public override void Copy()
    {
        throw new NotImplementedException();
    }

    public override bool Copy_CanExecute()
    {
        throw new NotImplementedException();
    }

    public override void Paste()
    {
        throw new NotImplementedException();
    }

    public override bool Paste_CanExecute()
    {
        throw new NotImplementedException();
    }

    public override void Delete()
    {
        throw new NotImplementedException();
    }

    public override bool Delete_CanExecute()
    {
        throw new NotImplementedException();
    }
}
