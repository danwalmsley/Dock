// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Debug view showing properties of a <see cref="DockDockControl"/>.
/// </summary>
public partial class DockDockDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockDockDebugView"/> class.
    /// </summary>
    public DockDockDebugView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Loads the control's XAML.
    /// </summary>
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
