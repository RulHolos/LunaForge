using LunaForge.Plugins.Services;
using LunaForge.Plugins.System;
using LunaForge.Plugins.System.Attributes;

namespace DefaultToolbox;

public class Entry : ILunaPlugin
{
    [PluginService]
    public static IToolboxService ToolboxService { get; set; }

    public string Name => "Default Toolbox";
    public string[] Authors => [ "Rül Hölos" ];

    public void Initialize()
    {
        //ToolboxService.RegisterNodePickerRegister<TabGeneral>("General");
    }

    public void Update()
    {
        return;
    }

    public void Reload()
    {
        return;
    }

    public void Unload()
    {
        return;
    }
}