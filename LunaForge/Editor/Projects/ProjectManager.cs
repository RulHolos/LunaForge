using Hexa.NET.ImGui.Widgets;
using LunaForge.Editor.Backend.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Projects;

public static class ProjectManager
{
    private static readonly ILogger Logger = CoreLogger.Create("Project Manager");
    public static bool Loaded;
    private static FileSystemWatcher? watcher;

    private static readonly SemaphoreSlim semaphore = new(1);
    
    static ProjectManager()
    {

    }

    public static string? CurrentProjectFolder { get; private set; }
    public static string? CurrentProjectFilePath { get; private set; }
    public static string? CurrentProjectAssetsFolder { get; private set; }
    public static LunaProject? CurrentProject { get; private set; }

    public static event ProjectUnloadedHandler? ProjectUnloaded;

    public static event ProjectLoadingHandler? ProjectLoading;

    public static event ProjectLoadedHandler? ProjectLoaded;

    public delegate void ProjectUnloadedHandler(string? projectFile);

    public delegate void ProjectLoadingHandler(string projectFile);

    public delegate void ProjectLoadFailedHandler(string projectFile, Exception exception);

    public delegate void ProjectLoadedHandler(LunaProject project);

    public static Task Load(string path)
    {
        return Task.Run(() =>
        {
            semaphore.Wait();

            try
            {
                ProjectLoading?.Invoke(path);

                CurrentProjectFilePath = path;

                Loaded = true;

                CurrentProjectFolder = Path.GetDirectoryName(CurrentProjectFilePath) ?? throw new Exception($"GetDirectoryName returned null for '{CurrentProjectFilePath}'");

                CurrentProjectAssetsFolder = Path.Combine(CurrentProjectFolder, "Assets");
                string projectName = Path.GetFileName(CurrentProjectFolder);
                Directory.CreateDirectory(CurrentProjectAssetsFolder);

                ProjectHistory.AddEntry(projectName, CurrentProjectFilePath);
            }
            catch (Exception ex)
            {
                UnloadInternal();
                Logger.Error($"Failed to load project '{path}'.");
                MessageBox.Show($"Failed to load project '{path}'.", ex.Message);
                return;
            }

            Logger.Information($"Loaded Project '{path}'.");
            ProjectLoaded?.Invoke(CurrentProject);

            semaphore.Release();
        });
    }

    public static void Unload()
    {
        semaphore.Wait();

        UnloadInternal();

        semaphore.Release();
    }

    private static void UnloadInternal()
    {
        if (Loaded)
        {
            ProjectUnloaded?.Invoke(CurrentProjectFilePath);
            CurrentProjectFilePath = null;
            CurrentProjectFolder = null;
            CurrentProjectAssetsFolder = null;
            CurrentProject = null;
            Loaded = false;
        }
    }

    private static void WatcherChanged(object sender, FileSystemEventArgs e)
    {

    }

    public static Task Create(string path)
    {
        return Task.Run(async () =>
        {
            string projectFilePath;
            try
            {
                projectFilePath = GenerateProject(path);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create project. Reason:\n{ex}");
                MessageBox.Show($"Failed to create project '{path}'.", ex.Message);
                return;
            }

            await Load(projectFilePath);
        });
    }

    private static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    private static string GenerateProject(string path)
    {
        if (Directory.Exists(path) && !IsDirectoryEmpty(path))
        {
            throw new InvalidOperationException($"Directory '{path}' doesn't exist or is not empty.");
        }

        string projectName = Path.GetFileName(path);
        string projectFilePath = Path.Combine(path, Path.ChangeExtension(projectName, ".lfp"));
        LunaProject project = LunaProject.CreateNew(path);

        var currentProjectAssetsFolder = Path.Combine(path, "Assets");
        Directory.CreateDirectory(currentProjectAssetsFolder);

        return projectFilePath;
    }
}
