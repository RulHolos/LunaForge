using LunaForge.Editor.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.Enums;

public interface INodeEditorWindow : IEditorWindow
{
    
}

public enum NodeEditorWindowType
{
    None,
    Boolean,
    Integer,
    Position,
}
