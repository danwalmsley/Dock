// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase : IFactory
{
    private bool IsDockPinned(IList<IDockable>? pinnedDockables, IDock dock)
    {
        if (pinnedDockables is not null && pinnedDockables.Count != 0)
        {
            foreach (var pinnedDockable in pinnedDockables)
            {
                if (pinnedDockable.Owner == dock)
                {
                    return true;
                }
            }
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public virtual void CollapseDock(IDock dock)
    {
        if (!dock.IsCollapsable || dock.VisibleDockables is null || dock.VisibleDockables.Count != 0)
        {
            return;
        }

        var rootDock = FindRoot(dock, _ => true);
        if (rootDock is { })
        {
            if (dock is IToolDock toolDock)
            {
                if (toolDock.Alignment == Alignment.Left 
                    && IsDockPinned(rootDock.LeftPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Right 
                    && IsDockPinned(rootDock.RightPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Top 
                    && IsDockPinned(rootDock.TopPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Bottom 
                    && IsDockPinned(rootDock.BottomPinnedDockables, dock))
                {
                    return;
                }
            }
        }

        if (dock.Owner is IDock ownerDock && ownerDock.VisibleDockables is { })
        {
            var toRemove = new List<IDockable>();
            var dockIndex = ownerDock.VisibleDockables.IndexOf(dock);

            if (dockIndex >= 0)
            {
                var indexSplitterPrevious = dockIndex - 1;
                if (dockIndex > 0 && indexSplitterPrevious >= 0)
                {
                    var previousVisible = ownerDock.VisibleDockables[indexSplitterPrevious];
                    if (previousVisible is IProportionalDockSplitter splitterPrevious)
                    {
                        toRemove.Add(splitterPrevious);
                    }
                }

                var indexSplitterNext = dockIndex + 1;
                if (dockIndex < ownerDock.VisibleDockables.Count - 1 && indexSplitterNext >= 0)
                {
                    var nextVisible = ownerDock.VisibleDockables[indexSplitterNext];
                    if (nextVisible is IProportionalDockSplitter splitterNext)
                    {
                        toRemove.Add(splitterNext);
                    }
                }

                foreach (var removeVisible in toRemove)
                {
                    RemoveDockable(removeVisible, true);
                }
            }
        }

        if (dock is IRootDock rootDockDock && rootDockDock.Window is { })
        {
            RemoveWindow(rootDockDock.Window);
        }
        else
        {
            RemoveDockable(dock, true);
        }
    }

    /// <inheritdoc/>
    public virtual IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation)
    {
        // If the dock is already a ProportionalDock, we can add directly to it
        if (dock is IProportionalDock proportionalDock)
        {
            return AddToExistingProportionalDock(proportionalDock, dockable, operation);
        }

        // Otherwise, create a new proportional dock as before
        return CreateNewProportionalDock(dock, dockable, operation);
    }

    private IDock AddToExistingProportionalDock(IProportionalDock dock, IDockable dockable, DockOperation operation)
    {
        var orientation = GetOrientationFromOperation(operation);
        
        // If orientations don't match, we still need to create a nested dock
        if (dock.Orientation != orientation)
        {
            return CreateNewProportionalDock(dock, dockable, operation);
        }

        // Find the insertion point based on the operation
        var insertionIndex = GetInsertionIndex(dock, operation);
        
        // Create a splitter
        var splitter = CreateProportionalDockSplitter();
        
        // Calculate proportions - distribute evenly among all children
        var totalChildren = dock.VisibleDockables?.Count ?? 0;
        var newProportion = 1.0 / (totalChildren + 1); // +1 for the new dockable
        
        // Adjust existing children proportions
        if (dock.VisibleDockables != null)
        {
            foreach (var child in dock.VisibleDockables)
            {
                var currentProportion = child.Proportion;
                var adjustedProportion = currentProportion * (1.0 - newProportion);
                child.Proportion = adjustedProportion;
            }
        }
        
        // Set proportion for the new dockable
        dockable.Proportion = newProportion;
        
        // Insert the splitter and dockable at the correct positions
        if (operation == DockOperation.Left || operation == DockOperation.Top)
        {
            dock.VisibleDockables?.Insert(insertionIndex, dockable);
            if (dock.VisibleDockables?.Count > 1)
            {
                dock.VisibleDockables?.Insert(insertionIndex + 1, splitter);
            }
        }
        else // Right or Bottom
        {
            if (dock.VisibleDockables?.Count > 0)
            {
                dock.VisibleDockables?.Insert(insertionIndex, splitter);
                dock.VisibleDockables?.Insert(insertionIndex + 1, dockable);
            }
            else
            {
                dock.VisibleDockables?.Add(dockable);
            }
        }

        return dock;
    }

    private IDock CreateNewProportionalDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        var orientation = GetOrientationFromOperation(operation);
        
        // Check if the dock's owner is already a ProportionalDock
        if (dock.Owner is IProportionalDock existingProportionalDock)
        {
            // Find the index of the current dock
            var dockIndex = existingProportionalDock.VisibleDockables?.IndexOf(dock) ?? -1;
            if (dockIndex >= 0)
            {
                // Create a splitter
                var splitter1 = CreateProportionalDockSplitter();
                
                // Insert splitter and dockable after the existing dock
                existingProportionalDock.VisibleDockables?.Insert(dockIndex + 1, splitter1);
                existingProportionalDock.VisibleDockables?.Insert(dockIndex + 2, dockable);
                
                InitDockable(splitter1, existingProportionalDock);
                InitDockable(dockable, existingProportionalDock);
                
                // Adjust proportions
                var equalProportion1 = 0.5;
                dock.Proportion = equalProportion1;
                dockable.Proportion = equalProportion1;
            }
            
            return existingProportionalDock;
        }
        
        // Create new ProportionalDock only if owner isn't already one
        var proportionalDock = CreateProportionalDock();
        proportionalDock.VisibleDockables = CreateList<IDockable>();
        proportionalDock.Orientation = orientation;
        proportionalDock.Proportion = dock.Proportion;
        
        // Create a splitter
        var splitter = CreateProportionalDockSplitter();
        
        proportionalDock.Owner = dock.Owner;
        // Add in order: dock, splitter, dockable
        proportionalDock.VisibleDockables?.Add(dock);
        proportionalDock.VisibleDockables?.Add(splitter);
        proportionalDock.VisibleDockables?.Add(dockable);
        
        InitDockable(dock, proportionalDock);
        InitDockable(splitter, proportionalDock);
        InitDockable(dockable, proportionalDock);
        
        // Set equal proportions for both dockables
        var equalProportion = 0.5;
        dock.Proportion = equalProportion;
        dockable.Proportion = equalProportion;
        
        return proportionalDock;
    }

    private int GetInsertionIndex(IProportionalDock dock, DockOperation operation)
    {
        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Top:
                return 0;
            case DockOperation.Right:
            case DockOperation.Bottom:
            default:
                return dock.VisibleDockables?.Count ?? 0;
        }
    }

    private Orientation GetOrientationFromOperation(DockOperation operation)
    {
        return operation == DockOperation.Left || operation == DockOperation.Right 
            ? Orientation.Horizontal 
            : Orientation.Vertical;
    }

    /// <inheritdoc/>
    public virtual void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Right:
            case DockOperation.Top:
            case DockOperation.Bottom:
            {
                if (dock.Owner is IDock ownerDock && ownerDock.VisibleDockables is { })
                {
                    var index = ownerDock.VisibleDockables.IndexOf(dock);
                    if (index >= 0)
                    {
                        var layout = CreateSplitLayout(dock, dockable, operation);
                        
                        if (layout != ownerDock)
                        {
                            RemoveVisibleDockableAt(ownerDock, index);
                            OnDockableRemoved(dockable);
                            OnDockableUndocked(dockable, operation);
                            InsertVisibleDockable(ownerDock, index, layout);
                            OnDockableAdded(dockable);
                            InitDockable(layout, ownerDock);
                            ownerDock.ActiveDockable = layout;
                        }
                        else
                        {
                            //ownerDock.ActiveDockable = dockable;
                        }
                        
                        OnDockableDocked(dockable, operation);
                    }
                }
                break;
            }
            default:
                throw new NotSupportedException($"Not supported split operation: {operation}.");
        }
    }

    /// <inheritdoc/>
    public virtual IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        IDockable? target;

        switch (dockable)
        {
            case ITool:
            {
                target = CreateToolDock();
                target.Title = nameof(IToolDock);
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dock.VisibleDockables is not null)
                    {
                        AddVisibleDockable(dock, dockable);
                        OnDockableAdded(dockable);
                        dock.ActiveDockable = dockable;
                    }
                }
                break;
            }
            case IDocument:
            {
                target = CreateDocumentDock();
                target.Title = nameof(IDocumentDock);
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dockable.Owner is IDocumentDock sourceDocumentDock)
                    {
                        if (target is IDocumentDock targetDocumentDock)
                        {
                            targetDocumentDock.Id = sourceDocumentDock.Id;
                            targetDocumentDock.CanCreateDocument = sourceDocumentDock.CanCreateDocument;
                            targetDocumentDock.EnableWindowDrag = sourceDocumentDock.EnableWindowDrag;

                            if (sourceDocumentDock is IDocumentDockContent sourceDocumentDockContent
                                && targetDocumentDock is IDocumentDockContent targetDocumentDockContent)
                            {
                                
                                targetDocumentDockContent.DocumentTemplate = sourceDocumentDockContent.DocumentTemplate;
                            }
                        }
                    }
                    if (dock.VisibleDockables is not null)
                    {
                        AddVisibleDockable(dock, dockable);
                        OnDockableAdded(dockable);
                        dock.ActiveDockable = dockable;
                    }
                }
                break;
            }
            case IToolDock:
            {
                target = dockable;
                break;
            }
            case IDocumentDock:
            {
                target = dockable;
                break;
            }
            case IProportionalDock proportionalDock:
            {
                target = proportionalDock;
                break;
            }
            case IDockDock dockDock:
            {
                target = dockDock;
                break;
            }
            case IRootDock rootDock:
            {
                target = rootDock.ActiveDockable;
                break;
            }
            default:
            {
                return null;
            }
        }

        var root = CreateRootDock();
        root.Title = nameof(IRootDock);
        root.VisibleDockables = CreateList<IDockable>();
        if (root.VisibleDockables is not null && target is not null)
        {
            AddVisibleDockable(root, target);
            OnDockableAdded(target);
            root.ActiveDockable = target;
            root.DefaultDockable = target;
        }
        root.Owner = null;

        var window = CreateDockWindow();
        window.Id = nameof(IDockWindow);
        window.Title = "";
        window.Width = double.NaN;
        window.Height = double.NaN;
        window.Layout = root;

        root.Window = window;

        return window;
    }

    /// <inheritdoc/>
    public virtual void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height)
    {
        var rootDock = FindRoot(dock, _ => true);
        if (rootDock is null)
        {
            return;
        }

        RemoveDockable(dockable, true);
        OnDockableUndocked(dockable, DockOperation.Window);

        var window = CreateWindowFrom(dockable);
        if (window is not null)
        {
            AddWindow(rootDock, window);
            window.X = x;
            window.Y = y;
            window.Width = width;
            window.Height = height;
            window.Present(false);

            OnDockableDocked(dockable, DockOperation.Window);

            if (window.Layout is { })
            {
                SetFocusedDockable(window.Layout, dockable);
            }
        }
    }
}
