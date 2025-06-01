using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Logging;
using Serilog.Core;

namespace LunaForge.Editor.Backend.Services;

public class DiscordRPCService : Service
{
    public override string Name => "Discord RPC";

    public DiscordRpcClient? Client { get; private set; }

    public override void Initialize()
    {
        Client = new("1301550302683725916")
        {
            Logger = new ConsoleLogger() { Level = LogLevel.Warning },
        };

        Client.Initialize();

        base.Initialize();

        Reset();
    }

    public override void Reset()
    {
        Client?.SetPresence(new RichPresence()
        {
            Details = "Idle",
            Timestamps = Timestamps.Now,
            Assets = new()
            {
                LargeImageKey = "lunaforgeicon",
                LargeImageText = "No Project Opened",
            }
        });
        if (Client == null)
            Logger.Warning("Discord Client is null. No changes made.");
    }

    public void SetState(RichPresence state)
    {
        Client?.SetPresence(state);
        if (Client == null)
            Logger.Warning("Discord Client is null. No changes made.");
    }

    public override void Dispose()
    {
        Client?.Dispose();
    }
}
