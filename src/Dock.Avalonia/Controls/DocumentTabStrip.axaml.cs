﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStrip custom control.
/// </summary>
[PseudoClasses(":create", ":active")]
public class DocumentTabStrip : TabStrip
{
    private Point _dragStartPoint;
    private bool _pointerPressed;
    private bool _isDragging;
    private PointerPressedEventArgs? _lastPointerPressedArgs;
    
    /// <summary>
    /// Defines the <see cref="CanCreateItem"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanCreateItemProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(CanCreateItem));

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(IsActive));
    
    /// <summary>
    /// Define the <see cref="EnableWindowDrag"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableWindowDragProperty = 
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(EnableWindowDrag));
    
    /// <summary>
    /// Define the <see cref="CanRemoveLastDockable"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanRemoveLastDockableProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(CanRemoveLastDockable));

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<DocumentTabStrip, Orientation>(nameof(Orientation));

    /// <summary>
    /// Gets or sets if tab strop dock can create new items.
    /// </summary>
    public bool CanCreateItem
    {
        get => GetValue(CanCreateItemProperty);
        set => SetValue(CanCreateItemProperty, value);
    }

    /// <summary>
    /// Gets or sets if this is the currently active dockable.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
    
    /// <summary>
    /// Gets or sets if the window can be dragged by clicking on the tab strip.
    /// </summary>
    public bool EnableWindowDrag
    {
        get => GetValue(EnableWindowDragProperty);
        set => SetValue(EnableWindowDragProperty, value);
    }

    /// <summary>
    /// Gets of sets if the last dockable can be removed.
    /// </summary>
    public bool CanRemoveLastDockable
    {
        get => GetValue(CanRemoveLastDockableProperty);
        set => SetValue(CanRemoveLastDockableProperty, value);
    }

    /// <summary>
    /// Gets or sets orientation of the tab strip.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DocumentTabStrip);

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentTabStrip"/> class.
    /// </summary>
    public DocumentTabStrip()
    {
        UpdatePseudoClassesCreate(CanCreateItem);
        UpdatePseudoClassesActive(IsActive);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        RemoveHandler(PointerPressedEvent, OnPointerPressed);
        RemoveHandler(PointerReleasedEvent, OnPointerReleased);
        RemoveHandler(PointerMovedEvent, OnPointerMoved);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new DocumentTabStripItem();
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<DocumentTabStripItem>(item, out recycleKey);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanCreateItemProperty)
        {
            UpdatePseudoClassesCreate(change.GetNewValue<bool>());
        }

        if (change.Property == IsActiveProperty)
        {
            UpdatePseudoClassesActive(change.GetNewValue<bool>());
        }
    }

    private void UpdatePseudoClassesCreate(bool canCreate)
    {
        PseudoClasses.Set(":create", canCreate);
    }

    private void UpdatePseudoClassesActive(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!EnableWindowDrag)
        {
            return;
        }

        _lastPointerPressedArgs = e;

        // Only handle primary button clicks
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        bool shouldAllowDrag = false;
        var source = e.Source as Control;

        // Special case: if we have only one item and can't remove it, always allow dragging
        if (ItemCount == 1 && !CanRemoveLastDockable)
        {
            shouldAllowDrag = true;
        }
        // Otherwise, check if we're clicking on an empty area of the tab strip
        else if (source == this || (source != null &&
                                    !(source is DocumentTabStripItem) &&
                                    !(source is Button) &&
                                    !IsChildOfType<DocumentTabStripItem>(source) &&
                                    !IsChildOfType<Button>(source)))
        {
            shouldAllowDrag = true;
        }

        if (shouldAllowDrag)
        {
            _dragStartPoint = e.GetPosition(this);
            _pointerPressed = true;
            e.Handled = true;
        }

    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_pointerPressed && !_isDragging)
        {
            return;
        }

        _pointerPressed = false;
        _isDragging = false;
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_pointerPressed || _isDragging)
        {
            return;
        }

        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _dragStartPoint;

        // Check if we've moved enough to consider it a drag
        if (!(Math.Abs(delta.X) > DockSettings.MinimumHorizontalDragDistance)
            && !(Math.Abs(delta.Y) > DockSettings.MinimumVerticalDragDistance))
        {
            return;
        }

        _isDragging = true;
        _pointerPressed = false;

        // Find the window that contains this tab strip
        if (VisualRoot is not Window hostWindow)
        {
            return;
        }

        if (_lastPointerPressedArgs is null)
        {
            return;
        }

        // Call the MoveDrag method to start window dragging
        hostWindow.BeginMoveDrag(_lastPointerPressedArgs);
        e.Handled = true;
    }

    private bool IsChildOfType<T>(Control control) where T : Control
    {
        // Walk up the visual tree to find a parent of type T
        var parent = control;
        while (parent != null && parent != this)
        {
            if (parent is T)
            {
                return true;
            }

            parent = parent.Parent as Control;
        }

        return false;
    }
}
