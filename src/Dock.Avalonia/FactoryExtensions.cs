using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Model.Core;

namespace Dock.Avalonia;

/// <summary>
/// Provides helper methods for activating windows.
/// </summary>
public static class FactoryExtensions
{
    /// <summary>
    /// Activates the window that hosts the specified dockable.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="dockable">The dockable whose window should be activated.</param>
    public static void ActivateWindow(this IFactory factory, IDockable dockable)
    {
        if (factory.VisibleRootControls.TryGetValue(dockable, out var root) && root is Control control)
        {
            if (control.GetVisualRoot() is Window window)
            {
                window.Activate();
                return;
            }
        }

        if (factory.PinnedRootControls.TryGetValue(dockable, out var pinnedRoot) && pinnedRoot is Control pinnedControl)
        {
            if (pinnedControl.GetVisualRoot() is Window pinnedWindow)
            {
                pinnedWindow.Activate();
                return;
            }
        }

        if (factory.TabRootControls.TryGetValue(dockable, out var tabRoot) && tabRoot is Control tabControl)
        {
            if (tabControl.GetVisualRoot() is Window tabWindow)
            {
                tabWindow.Activate();
            }
        }
    }
}
