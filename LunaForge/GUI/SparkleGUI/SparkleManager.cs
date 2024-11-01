using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.SparkleGUI;

internal partial class SparkleManager : SparkleUpdater
{
    public SparkleManager()
        : base("temp", new Ed25519Checker(SecurityMode.Unsafe))
    {
        UpdateCheckStarted += SparkleManager_UpdateCheckStarted;
        UpdateDetected += SparkleManager_UpdateDetected;

        CloseApplicationAsync += SparkleManager_CloseApplicationAsync;

        StartLoop(true, true); // TODO: Replace this with Settings auto update.
    }

    public void Render()
    {
        
    }

    #region Events

    private void SparkleManager_UpdateCheckStarted(object sender)
    {
        NotificationManager.AddToast("Running Update Check...", ToastType.Info);
    }

    private void SparkleManager_UpdateDetected(object sender, UpdateDetectedEventArgs e)
    {
        void InstallUpdate(Toast sourceToast)
        {

        }

        NotificationManager.AddToast("Update found.\nClick to update.", ToastType.Info, clickCallback: InstallUpdate);
    }

    private Task SparkleManager_CloseApplicationAsync()
    {
        return Task.CompletedTask;   
    }

    #endregion
}
