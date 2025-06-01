using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using LibGit2Sharp;
using LunaForge.Editor.Backend.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Projects;

/*

public enum PasswordDialogMode
{
    Password,
    UsernamePassword,
    UsernameToken
}

public enum PasswordDialogResult
{
    Cancel,
    Ok
}

public class PasswordDialog
{
    private bool open;

    public PasswordDialog(PasswordDialogMode mode, Action<PasswordDialog, PasswordDialogResult> callback)
    {
        Mode = mode;
        Callback = callback;
    }

    public PasswordDialog(PasswordDialogMode mode)
    {
        Mode = mode;
        Callback = null;
    }

    public PasswordDialogMode Mode { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public PasswordDialogResult Result { get; private set; }
    public Action<PasswordDialog, PasswordDialogResult>? Callback { get; set; }

    public void Show()
    {
        open = true;
        while (open)
        {
            if (ImGui.Begin("Enter Password", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.Modal))
            {
                switch (Mode)
                {
                    case PasswordDialogMode.UsernamePassword:
                    case PasswordDialogMode.UsernameToken:
                        var username = Username;
                        ImGui.InputText("Username", ref username, 1024);
                        Username = username;
                        break;
                }
                var password = Password;
                ImGui.InputText(Mode == PasswordDialogMode.UsernameToken ? "Token" : "Password", ref password, 1024, ImGuiInputTextFlags.Password);
                Password = password;

                if (ImGui.Button("Cancel"))
                {
                    open = false;
                    Result = PasswordDialogResult.Cancel;
                }

                ImGui.SameLine();

                if (ImGui.Button("Ok"))
                {
                    open = false;
                    Result = PasswordDialogResult.Ok;
                }
            }

            ImGui.End();
        }

        Callback?.Invoke(this, Result);
    }

    public Task ShowAsync()
    {
        open = true;
        while (open)
        {
            if (ImGui.Begin("Enter Password", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.Modal))
            {
                switch (Mode)
                {
                    case PasswordDialogMode.UsernamePassword:
                    case PasswordDialogMode.UsernameToken:
                        var username = Username;
                        ImGui.InputText("Username", ref username, 1024);
                        Username = username;
                        break;
                }
                var password = Password;
                ImGui.InputText(Mode == PasswordDialogMode.UsernameToken ? "Token" : "Password", ref password, 1024, ImGuiInputTextFlags.Password);
                Password = password;

                if (ImGui.Button("Cancel"))
                {
                    open = false;
                    Result = PasswordDialogResult.Cancel;
                }

                ImGui.SameLine();

                if (ImGui.Button("Ok"))
                {
                    open = false;
                    Result = PasswordDialogResult.Ok;
                }
            }

            ImGui.End();
        }

        Callback?.Invoke(this, Result);

        return Task.CompletedTask;
    }
}

public static class ProjectVersionControl
{
    private static readonly ILogger Logger = CoreLogger.Create(nameof(ProjectVersionControl));
    private static readonly object _lock = new();
    private static readonly Identity identity = null!;

    private static FileSystemWatcher? watcher;
    private static readonly List<string> changedFiles = [];
    private static bool filesChanged = true;
    private static Task? changedFilesTask;

    public static Repository? Repository { get; private set; }
    public static string HeadName => Repository!.Head.FriendlyName;
    public static Branch Head = Repository!.Head;
    public static object SyncObject => _lock;

    static ProjectFileCollection()
    {
        string gitConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitconfig");
        if (File.Exists(gitConfigPath))
        {
            string[] lines = File.ReadAllLines(gitConfigPath);
            bool capture = false;
            string? name = null;
            string? email = null;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    capture = false;
                    continue;
                }

                if (line.Contains("[user]"))
                    capture = true;

                if (capture && line.Contains("name"))
                    name = line.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1];

                if (capture && line.Contains("email"))
                    email = line.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1];
            }

            identity = new(name, email);
        }
        else
        {
            MessageBox.Show("Warning", $"Warning! .gitconfig was not found, please make sure that {gitConfigPath} is present");
        }
    }

    public static GitCommitDifference CommitDifference { get; private set; } = new(0, 0);

    private static SecureUsernamePasswordCredentials CredentialsProvider(string url, string usernameFromUrl, SupportedCredentialTypes types)
    {
        if (!CredentialsManager.IsOpen)
        {

        }
    }

    public static void Init()
    {
        if (ProjectManager.CurrentProjectFolder == null || Repository != null)
        {
            return;
        }

        try
        {
            var path = Repository.Discover(ProjectManager.CurrentProjectFolder);

            path ??= Repository.Init(ProjectManager.CurrentProjectFolder);

            Repository = new(path);

            CommitChanges("Initial Commit", false);

            InitInternal(ProjectManager.CurrentProjectFolder);

            UpdateCommitDifference();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to init repo. Reason:\n{ex}");
            MessageBox.Show("Failed to init repo!", ex.Message);
        }
    }

    public static void TryInit()
    {
        if (ProjectManager.CurrentProjectFolder == null || Repository != null)
        {
            return;
        }

        var path = Repository.Discover(ProjectManager.CurrentProjectFolder);

        if (path == null)
        {
            return;
        }

        try
        {
            Repository = new(path);
            InitInternal(ProjectManager.CurrentProjectFolder);
            FetchAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to init repo. Reason:\n{ex}");
            MessageBox.Show("Failed to init repo!", ex.Message);
        }
    }

    private static void InitInternal(string path)
    {
        watcher = new();
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
            | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security;
        watcher.Changed += WatcherChanged;
        watcher.EnableRaisingEvents = true;
    }

    private static void WatcherChanged(object sender, FileSystemEventArgs e)
    {
        filedChanged = true;
    }

    public static Commit? CommitChanges(string message, bool amend)
    {
        if (Repository == null)
            return null;

        try
        {
            Commands.Stage(Repository, "*");

            var author = new Signature(identity, DataTimeOffset.Now);
            var committer = author;

            CommitOptions options = new();
            options.AmendPreviousCommit = amend;

            return Repository.Commit(message, author, committer, options);
        }
    }
}

public readonly struct GitCommitDifference(int behind, int ahead)
{
    public readonly int Behind = behind;
    public readonly int Ahead = ahead;
    public readonly string Text = $"\xE896 {behind} | \xE898 {ahead}";
}

*/