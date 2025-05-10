using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.Utilities;

public static class CoreLogger
{
    public static ILogger Logger = Log.ForContext("Tag", "Core");

    public static void Initialize()
    {
        string pathToLog = Path.Combine(Directory.GetCurrentDirectory(), "editor.log");
        if (File.Exists(pathToLog))
            File.Delete(pathToLog);

        const string template = "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} [{Level:u3}] [{Tag}] {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#endif
            .WriteTo.Console(outputTemplate: template)
            .WriteTo.File(pathToLog,
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: template,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        Logger.Debug("Launching the Editor in DEBUG mode.");
    }

    public static ILogger Create(string name) => Log.ForContext("Tag", name);
}
