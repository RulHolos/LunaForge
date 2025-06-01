using LunaForge.Editor.Backend.Services;
using LunaForge.Editor.Backend.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Managers;

public static class ServiceManager
{
    private static readonly ILogger Logger;
    public static readonly List<Service> Services = [];

    static ServiceManager()
    {
        Logger = CoreLogger.Create("Services");
    }

    public static void InitServices()
    {
        foreach (Service service in Services)
        {
            if (!service.Initialized)
            {
                service.Initialize();
                Logger.Information($"Service '{service.Name}' initialized.");
            }
        }
    }

    public static void RegisterService<T>(bool forceInit = false) where T : Service, new()
    {
        if (!Services.Any(x => x is T))
        {
            Service service = new T();
            Services.Add(service);
            if (forceInit)
                service.Initialize();
        }
        else
            Logger.Warning($"Service of type '{typeof(T).Name}' is already registered. Skipping registration.");
    }

    public static void UnregisterService<T>() where T : Service
    {
        if (Services.Find(x => x is T) is Service service)
        {
            service?.Dispose();
            Services.Remove(service);
            Logger.Information($"Service '{service.Name}' unregistered.");
        }
        else
        {
            Logger.Warning($"Service of type '{typeof(T).Name}' not found. Cannot unregister.");
        }
    }

    public static void Dispose()
    {
        foreach (Service service in Services)
        {
            service.Dispose();
            Logger.Information($"Service '{service.Name}' disposed.");
        }
        Services.Clear();
    }

    public static T GetService<T>() where T : Service, new()
    {
        T? service = (T?)Services.FirstOrDefault(x => x is T);
        if (service != null)
            return service;

        service = new T();
        Services.Add(service);
        service.Initialize();

        return service;
    }
}
