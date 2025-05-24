using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Projects;

public class ProjectFileCollection : List<LunaProjectFile>
{
    public int MaxHash { get; private set; } = 0;

    public LunaProjectFile? Current { get; set; } = null;

    public new void Add(LunaProjectFile proj)
    {
        proj.Hash = MaxHash;
        base.Add(proj);
        MaxHash++;
    }
}
