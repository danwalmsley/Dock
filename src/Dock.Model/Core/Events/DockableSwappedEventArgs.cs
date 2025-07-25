﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable swapped event args.
/// </summary>
public class DockableSwappedEventArgs : EventArgs
{
    /// <summary>
    /// Gets swapped dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableSwappedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The swapped dockable.</param>
    public DockableSwappedEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
