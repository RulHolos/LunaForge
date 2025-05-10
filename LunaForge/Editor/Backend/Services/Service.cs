using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.Services;

public abstract class Service
{
    public abstract string Name { get; }
    public bool Initialized { get; private set; } = false;

    public virtual void Initialize()
    {
        Initialized = true;
    }

    public abstract void Reset();
    public abstract void Dispose();
}
