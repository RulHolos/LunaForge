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
        : base("https://raw.githubusercontent.com/RulHolos/LunaForge/main/AppCast.xaml", new Ed25519Checker(SecurityMode.Unsafe))
    {
        // TODO: Remove the UI Factory, just handle the events manually, since I need to have to Render method eitherway.

        RelaunchAfterUpdate = true;
        CheckServerFileName = false;
        SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;

        UpdateDetected += SparkleManager_UpdateDetected;

        CloseApplicationAsync += SparkleManager_CloseApplicationAsync;

        StartLoop(
            LunaForge.Configuration.Default.CheckUpdatesAtStartup,
            LunaForge.Configuration.Default.CheckUpdatesAtStartup,
            TimeSpan.FromHours(LunaForge.Configuration.Default.CheckUpdateFrequency));
    }

    #region Main Rendering

    public void Render()
    {
        RenderUpdateAvailable();
    }

    #endregion
    #region Events

    private void SparkleManager_UpdateDetected(object sender, UpdateDetectedEventArgs e)
    {
        async void InstallCallback(Toast toast)
        {
            await ShowUpdateAvailable(e);
        }

        NotificationManager.AddToast("Update Found!\nClick to install.", "Install", duration: 10f, clickCallback: InstallCallback);
    }

    private Task SparkleManager_CloseApplicationAsync()
    {
        return Task.CompletedTask;   
    }

    #endregion
}
