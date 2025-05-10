using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Projects;

public class LunaProject
{
    private LunaProject()
    {

    }

    public static LunaProject CreateEmpty() => new();

    public static LunaProject CreateNew(string folder)
    {
        LunaProject project = new();
        return project;
    }
}
