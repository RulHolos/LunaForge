using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.ImNodesEditor;

public enum PinType
{
    Boolean,
    Integer,
    Float,
    String,
    Function,
    RenderTarget,
}

public enum PinFlags
{
    None,
    ColorEdit,
    ColorPicker,
    Slider,
}
