using Hexa.NET.ImGui.Widgets;
using LunaForge.Editor.Backend.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Compression;
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
                var result = LunaProject.Load(Path.Combine(CurrentProjectFolder, "Project.lfp"));
                CurrentProject = result.Item1;
                if (CurrentProject == null)
                    throw new InvalidOperationException($"There has been an error reading the Current Project file. Is it malformed? Reason:\n{result.Item2}");

                string projectName = Path.GetFileName(CurrentProjectFolder);
                ProjectHistory.AddEntry(projectName, CurrentProjectFilePath);
            }
            catch (Exception ex)
            {
                UnloadInternal();
                Logger.Error($"Failed to load project '{path}'. Reason:\n{ex}");
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
            CurrentProject = null;
            Loaded = false;
        }
    }

    private static void WatcherChanged(object sender, FileSystemEventArgs e)
    {

    }

    public static Task CreateEmpty(string path)
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

    public static Task CreateFromTemplate(string path, string templatePath)
    {
        return Task.Run(async () =>
        {
            string projectFilePath = "";

            await Load(projectFilePath);
        });
    }

    private static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    private static string GenerateProject(string path, string templatePath = "")
    {
        if (Directory.Exists(path) && !IsDirectoryEmpty(path))
        {
            throw new InvalidOperationException($"Directory '{path}' is not empty.");
        }
        else if (!string.IsNullOrEmpty(templatePath) && !File.Exists(templatePath))
        {
            throw new InvalidOperationException($"Project Template '{templatePath}' wasn't found.");
        }

        LunaProject project;
        // Empty
        if (string.IsNullOrEmpty(templatePath))
        {
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "Assets"));
            Directory.CreateDirectory(Path.Combine(path, "Definitions"));
            Directory.CreateDirectory(Path.Combine(path, "Scripts"));
            DirectoryInfo di = Directory.CreateDirectory(Path.Combine(path, ".lunaforge"));
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            project = LunaProject.CreateNew(path);
        }
        // From template
        else
        {
            ZipFile.ExtractToDirectory(Path.ChangeExtension(templatePath, ".zip"), path);

            project = LunaProject.CreateNew(path);
        }
        
        return project.ProjectFile;
    }
}
