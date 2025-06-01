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
    public bool Initialized { get; private set; } = false;

    public virtual void Initialize()
    {
        Logger = CoreLogger.Create(Name);
        Initialized = true;
    }

    public abstract void Reset();
    public abstract void Dispose();
}