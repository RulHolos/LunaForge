using LunaForge.Plugins.Services;
using LunaForge.Plugins.System;
using LunaForge.Plugins.System.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Plugins;

internal enum LunaPluginState
{
    Enabled,
    Disabled,
    ErrorWhileLoading
}

internal class LunaPluginInfo
{
    public ILunaPlugin? Plugin { get; set; }
    public PluginLoadContext Context { get; set; }
    public LunaPluginState State { get; set; }

    public LunaPluginMeta Meta { get; set; }
}

[Serializable]
internal class LunaPluginMeta
{
    public string Name { get; set; }
    public string[] Authors { get; set; }
    public string Description { get; set; }
    /// <summary>
    /// Is the plugin loaded globally (true) or per-project (false)
    /// </summary>
    public bool IsEditorPlugin { get; set; }
}

internal sealed class PluginManager
{
    public List<LunaPluginInfo> Plugins { get; private set; }

    private readonly IServiceProvider serviceProvider;

    public PluginManager()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton<IToolboxService, ToolboxService>();
        serviceCollection.AddSingleton<IWindowService, WindowService>();

        serviceProvider = serviceCollection.BuildServiceProvider();

        Plugins = [];
    }

    public void GetAllPluginInfo()
    {
        try
        {
            string pluginDir = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);

            string[] plugins = Directory.GetFiles(pluginDir, "*.dll");

            foreach (string file in plugins)
            {
                Configuration.Default.EnabledPlugins.TryGetValue(Path.GetFileName(file), out bool enabled);
                string infoFile = Path.ChangeExtension(file, ".json");
                if (!File.Exists(infoFile))
                    continue; // Ensure the plugin isn't loaded if it's missing is meta file.

                using StreamReader sr = new(infoFile);
                LunaPluginMeta meta = JsonConvert.DeserializeObject<LunaPluginMeta>(sr.ReadToEnd());

                LunaPluginInfo pluginInfo = new()
                {
                    Plugin = null,
                    Context = new(file),
                    State = enabled ? LunaPluginState.Enabled : LunaPluginState.Disabled,
                    Meta = meta
                };
                Plugins.Add(pluginInfo);
                Configuration.Default.EnabledPlugins.TryAdd(Path.GetFileName(file), enabled);
            }
            Plugins.Sort((x, y) => string.Compare(x.Meta.Name, y.Meta.Name));
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Failed to locate or create the plugin directory. Reason:\n{ex}");
        }
    }

    public void LoadAllEnabledPlugins()
    {
        foreach (LunaPluginInfo pluginInfo in Plugins)
        {
            if (pluginInfo.State == LunaPluginState.Enabled)
                LoadPlugin(pluginInfo);
        }
    }

    public void LoadPlugin(LunaPluginInfo info)
    {
        try
        {
            Assembly assembly = info.Context.LoadFromAssemblyPath(info.Context.filePath);

            Type? pluginType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(ILunaPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (pluginType != null)
            {
                ILunaPlugin? newPlugin = (ILunaPlugin?)Activator.CreateInstance(pluginType);
                if (newPlugin != null)
                {
                    InitializePlugin(info, newPlugin);
                } 
            }

            info.State = LunaPluginState.Enabled;
            Configuration.Default.EnabledPlugins[Path.GetFileName(info.Context.filePath)] = true;
            Console.WriteLine($"Plugin \"{info.Meta.Name}\" ({Path.GetFileName(info.Context.filePath)}) loaded successfully.");
        }
        catch (Exception ex)
        {
            info.State = LunaPluginState.ErrorWhileLoading;
            Console.WriteLine($"Failed to load plugin {info.Meta.Name}. Reason:\n{ex}");
        }
    }

    private void InitializePlugin(LunaPluginInfo info, ILunaPlugin plugin)
    {
        Type pluginType = plugin.GetType();
        foreach (var property in pluginType.GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (property.GetCustomAttribute<PluginServiceAttribute>() != null)
            {
                Type serviceType = property.PropertyType;
                object? service = serviceProvider.GetService(serviceType);
                if (service != null)
                    property.SetValue(null, service);
            }
        }

        plugin.Initialize();
        info.Plugin = plugin;
    }

    public void UnloadPlugin(LunaPluginInfo info)
    {
        info.Plugin.Unload();
        info.Context.Unload();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        info.State = LunaPluginState.Disabled;
        info.Context = new(info.Context.filePath);
        Configuration.Default.EnabledPlugins[Path.GetFileName(info.Context.filePath)] = false;
        Console.WriteLine($"Plugin \"{info.Meta.Name}\" ({Path.GetFileName(info.Context.filePath)}) disabled.");
    }
}
