using LunaForge.Editor.Backend.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.Services;

public abstract class Service
{
    public ILogger Logger { get; private set; }

    public abstract string Name { get; }
    public bool Initialized { get; set; } = false;
    public abstract bool Resetable { get; }

    public virtual bool Initialize()
    {
        Logger = CoreLogger.Create(Name);
        Initialized = true;
        return Initialized;
    }

    public abstract void Reset();
    public abstract void Dispose();
}