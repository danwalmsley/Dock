﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

/// <summary>
/// Dock helpers.
/// </summary>
internal static class DockHelpers
{
    public static Point GetScreenPoint(Visual visual, Point point)
    {
        // var scaling = (visual.GetVisualRoot() as TopLevel)?.RenderScaling ?? 1.0;
        var scaling = (visual.GetVisualRoot() as TopLevel)?.Screens?.ScreenFromVisual(visual)?.Scaling ?? 1.0;
        var screenPoint = visual.PointToScreen(point).ToPoint(scaling);
        return screenPoint;
    }

    private static bool IsHitTestVisible(Visual visual)
    {
        var element = visual as IInputElement;
        return element != null &&
               visual.IsVisible &&
               element.IsHitTestVisible &&
               element.IsEffectivelyEnabled &&
               visual.GetVisualRoot() != null;
    }

    public static Control? GetControl(Visual? input, Point point, StyledProperty<bool> property)
    {
        IEnumerable<Visual>? inputElements = null;
        try
        {
            inputElements = input?.GetVisualsAt(point, IsHitTestVisible);
        }
        catch (Exception ex)
        {
            Print(ex);
        }

        return inputElements?
            .OfType<Control>()
            .ToList()
            .Where(control => control.IsSet(property))
            .FirstOrDefault(control => control.GetValue(property));
    }

    private static void Print(Exception ex)
    {
        Debug.WriteLine(ex.Message);
        Debug.WriteLine(ex.StackTrace);
        if (ex.InnerException is { })
        {
            Print(ex.InnerException);
        }
    }

    public static DockPoint ToDockPoint(Point point)
    {
        return new(point.X, point.Y);
    }

    public static void ShowWindows(IDockable dockable)
    {
        if (dockable.Owner is IDock {Factory: { } factory} dock)
        {
            if (factory.FindRoot(dock, _ => true) is {ActiveDockable: IRootDock activeRootDockable})
            {
                if (activeRootDockable.ShowWindows.CanExecute(null))
                {
                    activeRootDockable.ShowWindows.Execute(null);
                }
            }
        }
    }

    public static IProportionalDock? FindProportionalDock(IDock dock)
    {
        if (dock.Factory is { } factory)
        {
            return factory.FindDockable(dock, d => d is IProportionalDock) as IProportionalDock;
        }

        return null;
    }
    
    private static int IndexOf(Window[] windowsArray, Window? windowToFind)
    {
        if (windowToFind == null)
        {
            return -1;
        }

        for (var i = 0; i < windowsArray.Length; i++)
        {
            if (ReferenceEquals(windowsArray[i], windowToFind))
            {
                return i;
            }
        }

        return -1;
    }

    public static IEnumerable<DockControl> GetZOrderedDockControls(IList<IDockControl> dockControls)
    {
        var windows = dockControls
            .OfType<DockControl>()
            .Select(dock => dock.GetVisualRoot() as Window)
            .OfType<Window>()
            .Distinct()
            .ToArray();

        Window.SortWindowsByZOrder(windows);

        return dockControls
            .OfType<DockControl>()
            .Select(dock => (dock, order: IndexOf(windows, dock.GetVisualRoot() as Window)))
            .OrderByDescending(x => x.order)
            .Select(pair => pair.dock);
    }
}
