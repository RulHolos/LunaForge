using Hexa.NET.ImGui;
using LunaForge.Editor.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Windows.ProjectBrowser;

public class ProjectBrowserWindow : EditorWindow
{
    private DirectoryInfo? currentDir;
    private DirectoryInfo? parentDir;
    private readonly HashSet<Guid> groups = [];
    private readonly HashSet<Guid> openGroups = [];
    private readonly Stack<string> backHistory = [];
    private readonly Stack<string> forwardHistory = [];
    private string? CurrentFolder = null;

    private readonly Lock refreshLock = new();

    private bool showExtensions = false;
    private bool showHidden = false;

    private Vector2 chipSize = new(86, 92);
    private Vector2 imageSize = new(64, 64);

    protected override string Name => $"{FA.LinesLeaning} File Browser";
    public override ImGuiWindowFlags Flags { get; set; } = ImGuiWindowFlags.NoCollapse;
    public override bool CanBeClosed { get; set; } = false;

    public ProjectBrowserWindow()
    {
        IsShown = true;
        //MainWindow.DropFile += DropFile;
        
    }

    protected override void DisposeCore()
    {
        
    }

    private void FileSystemChanged(FileSystemEventArgs obj)
    {
        //Refresh(!File.Exists(obj.FullPath);
    }

    private void ProjectLoaded(LunaProject? obj)
    {
        RefreshDirs();
        //SetFolder(ProjectManager.CurrentProject);
    }

    public void RefreshDirs()
    {
        if (ProjectManager.CurrentProjectFolder == null)
            return;

        lock (this)
        {
            
        }
    }

    private static void TraverseDirs(string dir)
    {

    }

    public void Refresh(bool all)
    {
        if (all)
            RefreshDirs();

        lock (refreshLock)
        {
            /*
            files.Clear();
            dirs.Clear();
            */
            if (CurrentFolder == null)
                return;

            currentDir = new(CurrentFolder);
            parentDir = currentDir?.Parent;

            /*foreach (var fse in Directory.GetFileSystemEntries(CurrentFolder))
            {
                bool isDir = Directory.Exists(fse);
                bool isFile = File.Exists(fse);

                if ((!isDir && !isFile) || (fse.EndsWith(".meta") && !showHidden))
                    continue;

                if (Directory.Exists(fse))
                    dirs.Add(new(Path.GetFileName(fse), fse, null, null));
                else
                {
                    var metadata = 
                }
            }*/
        }
    }

    protected override void InitWindow()
    {
        base.InitWindow();
    }

    public override void DrawContent()
    {
        if (ProjectManager.CurrentProject?.ProjectFile == null)
            return;
    }
}
