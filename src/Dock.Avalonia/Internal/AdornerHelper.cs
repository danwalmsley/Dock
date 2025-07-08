﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class AdornerHelper<T> where T : Control, new()
{
    private readonly bool _useFloatingDockAdorner;
    public Control? Adorner;
    private DockAdornerWindow? _window;

    public AdornerHelper(bool useFloatingDockAdorner)
    {
        _useFloatingDockAdorner = useFloatingDockAdorner;
    }

    public void AddAdorner(Visual visual)
    {
        if (_useFloatingDockAdorner)
        {
            AddFloatingAdorner(visual);
        }
        else
        {
            AddRegularAdorner(visual);
        }
    }

    private void AddFloatingAdorner(Visual visual)
    {
        if (_window is not null)
        {
            _window.Close();
            _window = null;
        }

        Adorner = new T();

        if (visual.GetVisualRoot() is not Window root)
        {
            return;
        }

        var position = visual.PointToScreen(new Point());
        var width = visual.Bounds.Width;
        var height = visual.Bounds.Height;

        _window = new DockAdornerWindow
        {
            Width = width,
            Height = height,
            Content = Adorner,
            Position = new PixelPoint(position.X, position.Y),
            SizeToContent = SizeToContent.Manual,
            IsHitTestVisible = true
        };

        if (Adorner is { } control)
        {
            control.Width = width;
            control.Height = height;
        }
            
        _window.Show(root);
    }

    private void AddRegularAdorner(Visual visual)
    {
        var layer = AdornerLayer.GetAdornerLayer(visual);
        if (layer is null)
        {
            return;
        }

        if (Adorner is not null)
        {
            layer.Children.Remove(Adorner);
            Adorner = null;
        }

        Adorner = new T
        {
            [AdornerLayer.AdornedElementProperty] = visual
        };
        ((ISetLogicalParent) Adorner).SetParent(visual);

        layer.Children.Add(Adorner);
    }

    public void RemoveAdorner(Visual visual)
    {
        if (_useFloatingDockAdorner)
        {
            RemoveFloatingAdorner();
        }
        else
        {
            RemoveRegularAdorner(visual);
        }
    }

    private void RemoveFloatingAdorner()
    {
        if (_window is not null)
        {
            _window.Close();
            _window = null;
        }

        Adorner = null;
    }

    private void RemoveRegularAdorner(Visual visual)
    {
        var layer = AdornerLayer.GetAdornerLayer(visual);
        if (layer is null || Adorner is null)
        {
            return;
        }

        layer.Children.Remove(Adorner);
        ((ISetLogicalParent) Adorner).SetParent(null);
        Adorner = null;
    }
}
