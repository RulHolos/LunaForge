using LunaForge.Editor.Backend.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend;

public struct LunaNodeEditorWindowType
{
    public NodeEditorWindowType Type { get; set; }
}

// Use this class for Editor Windows and Combo Boxes too.
public static class LunaNodeEditorRegister
{
    public static List<LunaNodeEditorWindowType> Windows;

    public static LunaNodeEditorWindowType GetEditorWindowFromType(NodeEditorWindowType type)
    {
        return Windows.FirstOrDefault(x => x.Type == type);
    }
}