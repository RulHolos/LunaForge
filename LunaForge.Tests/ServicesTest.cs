using LunaForge.Editor.Backend.Services;
using LunaForge.Editor.UI.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Tests;

public class ServicesTest
{
    [Fact]
    public void Service_RegisterService()
    {
        ServiceManager.RegisterService<DiscordRPCService>();

        Assert.NotEmpty(ServiceManager.Services);

        ServiceManager.UnregisterService<DiscordRPCService>();

        Assert.Empty(ServiceManager.Services);
    }

    [Fact]
    public void Service_InitService()
    {
        ServiceManager.RegisterService<DiscordRPCService>();
        ServiceManager.InitServices();

        DiscordRPCService? service = ServiceManager.GetService<DiscordRPCService>();

        Assert.NotNull(service);
        Assert.True(service.Initialized);

        ServiceManager.UnregisterService<DiscordRPCService>();

        Assert.Empty(ServiceManager.Services);
    }
}
