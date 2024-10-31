using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using LunaForge.GUI;

namespace LunaForge.EditorData.Project;

public class AutoBackup
{
    private Timer backupTimer = null;
    private LunaForgeProject project;
    private Dictionary<string, string> fileHashes = [];

    public void Setup(LunaForgeProject proj, int intervalMinutes)
    {
        project = proj;
        backupTimer = new(BackupProject, null, TimeSpan.Zero, TimeSpan.FromMinutes(intervalMinutes));
    }

    public void Stop()
    {
        backupTimer?.Dispose();
        backupTimer = null;
    }

    private async void BackupProject(object state)
    {
        NotificationManager.AddToast("Starting auto backup in 5 seconds...");

        await Task.Delay(TimeSpan.FromSeconds(5));

        try
        {
            DateTime startTime = DateTime.Now;
            NotificationManager.AddToast($"Starting backup at {startTime.ToLocalTime()}.");
            string timestamp = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
            string destination = Path.Combine(project.PathToData, "backups", timestamp);

            MaintainBackupLimit(Configuration.Default.BackupCountLimit);

            EnumerationOptions options = new()
            {
                RecurseSubdirectories = true,
                ReturnSpecialDirectories = false,
                AttributesToSkip = FileAttributes.Hidden
            };
            foreach (var file in Directory.EnumerateFiles(project.PathToProjectRoot, "*.*", options))
            {
                string destFile = Path.Combine(destination, Path.GetRelativePath(project.PathToProjectRoot, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                File.Copy(file, destFile, true);
            }
            NotificationManager.AddToast($"Backup complete in {(DateTime.Now - startTime).TotalSeconds} seconds.", ToastType.Success);
        }
        catch (Exception ex)
        {
            NotificationManager.AddToast("Backup failed.\nSee console for more informations.", ToastType.Warning);
            Console.WriteLine(ex.ToString());
        }
    }

    private void MaintainBackupLimit(int maxBackups)
    {
        // Get all backup directories sorted by creation time (oldest to newest)
        string backupPath = Path.Combine(project.PathToData, "backups");
        if (!Directory.Exists(backupPath))
            return;
        List<string> backupDirectories = [.. Directory.GetDirectories(backupPath).OrderBy(Directory.GetCreationTime)];

        while (backupDirectories.Count > maxBackups)
        {
            string oldestBackup = backupDirectories.First();
            Directory.Delete(oldestBackup, true);
            backupDirectories.RemoveAt(0);
        }
    }
}
