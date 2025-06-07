using LunaForge.Editor.UI.Popups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Managers;

public static class PopupManager
{
    private static readonly List<IPopup> popups = [];
    private static readonly object _lock = new();

    public static object SyncLock => _lock;

    public static IReadOnlyList<IPopup> Popups => popups;

    public static IPopup Show(IPopup popup)
    {
        lock (_lock)
        {
            popups.Add(popup);
            popup.Show();
        }
        return popup;
    }

    public static T Show<T>(T popup) where T : IPopup
    {
        lock (_lock)
        {
            popups.Add(popup);
            popup.Show();
        }
        return popup;
    }

    public static T Show<T>() where T : IPopup, new()
    {
        T popup = new();
        lock (_lock)
        {
            popups.Add(popup);
            popup.Show();
        }
        return popup;
    }

    public static void Close(IPopup popup)
    {
        lock (_lock)
        {
            popup.Close();
            popups.Remove(popup);
        }
    }

    public static void Draw()
    {
        lock (_lock)
        {
            if (popups.Count == 0)
            {
                return;
            }

            var popup = popups[^1];
            popup.Draw();
            if (!popup.Shown)
            {
                popups.RemoveAt(popups.Count - 1);
                if (popups.Count == 0)
                {
                    return;
                }

                popups[^1].Show();
            }
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            for (int i = 0; i < popups.Count; i++)
            {
                popups[i].Close();
            }
            popups.Clear();
        }
    }

    public static void Dispose()
    {
        Clear();
    }
}