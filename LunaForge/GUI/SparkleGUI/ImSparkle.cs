using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.SparkleGUI;

public class ImSparkle : IUIFactory
{
    public ImSparkle()
    {
        HideReleaseNotes = false;
        HideRemindMeLaterButton = false;
        HideSkipButton = false;
        ReleaseNotesHTMLTemplate = "";
        AdditionalReleaseNotesHeaderHTML = "";
    }

    public bool HideReleaseNotes { get; set; }
    public bool HideSkipButton { get; set; }
    public bool HideRemindMeLaterButton { get; set; }
    public string? ReleaseNotesHTMLTemplate { get; set; }
    public string? AdditionalReleaseNotesHeaderHTML { get; set; }

    public bool CanShowToastMessages(SparkleUpdater sparkle)
    {
        return true;
    }

    public IDownloadProgress CreateProgressWindow(SparkleUpdater sparkle, AppCastItem item)
    {
        throw new NotImplementedException();
    }

    public IUpdateAvailable CreateUpdateAvailableWindow(SparkleUpdater sparkle, List<AppCastItem> updates, bool isUpdateAlreadyDownloaded = false)
    {
        throw new NotImplementedException();
    }

    public void Init(SparkleUpdater sparkle)
    {
        
    }

    public void ShowCannotDownloadAppcast(SparkleUpdater sparkle, string appcastUrl)
    {
        throw new NotImplementedException();
    }

    public ICheckingForUpdates ShowCheckingForUpdates(SparkleUpdater sparkle)
    {
        NotificationManager.AddToast("Checking For Updates...");
        return null;
    }

    public void ShowDownloadErrorMessage(SparkleUpdater sparkle, string message, string appcastUrl)
    {
        throw new NotImplementedException();
    }

    public void ShowToast(SparkleUpdater sparkle, List<AppCastItem> updates, Action<List<AppCastItem>> clickHandler)
    {
        throw new NotImplementedException();
    }

    public void ShowUnknownInstallerFormatMessage(SparkleUpdater sparkle, string downloadFileName)
    {
        throw new NotImplementedException();
    }

    public void ShowVersionIsSkippedByUserRequest(SparkleUpdater sparkle)
    {
        throw new NotImplementedException();
    }

    public void ShowVersionIsUpToDate(SparkleUpdater sparkle)
    {
        throw new NotImplementedException();
    }

    public void Shutdown(SparkleUpdater sparkle)
    {
        throw new NotImplementedException();
    }
}
