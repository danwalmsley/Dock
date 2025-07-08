// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal abstract class DockManagerState : IDockManagerState
{
    private readonly IDockManager _dockManager;

    protected IDockManager DockManager => _dockManager;

    protected Control? DropControl { get; set; }

    protected AdornerHelper<DockTarget> LocalAdornerHelper { get; }

    protected AdornerHelper<GlobalDockTarget> GlobalAdornerHelper { get; }
 
    /// <summary>
    /// Initializes a new instance of the <see cref="DockManagerState"/> class.
    /// </summary>
    /// <param name="dockManager">The dock manager.</param>
    protected DockManagerState(IDockManager dockManager)
    {
        _dockManager = dockManager;
        LocalAdornerHelper = new AdornerHelper<DockTarget>(DockSettings.UseFloatingDockAdorner);
        GlobalAdornerHelper = new AdornerHelper<GlobalDockTarget>(DockSettings.UseFloatingDockAdorner);
    }

    protected void AddAdorners(bool isLocalValid, bool isGlobalValid)
    {
        // Local dock target
        if (isLocalValid && DropControl is { } control)
        {
            if (DockProperties.GetIsDockTarget(control))
            {
                LocalAdornerHelper.AddAdorner(control);
            }
            else
            {
                var dockControl = control.GetVisualAncestors().OfType<DockControl>().FirstOrDefault();

                if (dockControl is { })
                {
                    LocalAdornerHelper.AddAdorner(dockControl);
                }
            }
        }

        // Global dock target
        if (DockSettings.EnableGlobalDocking && isGlobalValid && DropControl is { } dropControl)
        {
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            if (dockControl is not null)
            {
                GlobalAdornerHelper.AddAdorner(dockControl);
            }
        }
    }

    protected void RemoveAdorners()
    {
        // Local dock target
        if (DropControl is { } control)
        {
            if (DockProperties.GetIsDockTarget(control))
            {
                LocalAdornerHelper.RemoveAdorner(control);
            }
            else
            {
                var dockControl = control.GetVisualAncestors().OfType<DockControl>().FirstOrDefault();

                if (dockControl is { })
                {
                    LocalAdornerHelper.RemoveAdorner(dockControl);
                }
            }
        }

        // Global dock target
        if (DockSettings.EnableGlobalDocking && DropControl is { } dropControl)
        {
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            if (dockControl is not null)
            {
                GlobalAdornerHelper.RemoveAdorner(dockControl);
            }
        }
    }

    protected virtual void Execute(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo, IDockable sourceDockable, IDockable targetDockable)
    {
        _dockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            return;
        }

        var relativePoint = DockHelpers.GetScreenPoint(relativeTo, point);
        _dockManager.ScreenPosition = DockHelpers.ToDockPoint(relativePoint);

        _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: true);
    }

    protected bool ValidateDockable(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo, IDockable sourceDockable)
    {
        if (DropControl?.DataContext is not IDockable targetDockable)
        {
            return false;
        }

        _dockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            return false;
        }

        var screenPoint = DockHelpers.GetScreenPoint(relativeTo, point);
        _dockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

        return _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
    }


    protected static void Float(Point point, DockControl inputActiveDockControl, IDockable dockable, IFactory factory)
    {
        var screen = inputActiveDockControl.PointToScreen(point);
        dockable.SetPointerScreenPosition(screen.X, screen.Y);
        factory.FloatDockable(dockable);
    }
}
