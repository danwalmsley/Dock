// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Debug view for stack dock layouts.
/// </summary>
public partial class StackDockDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StackDockDebugView"/> class.
    /// </summary>
    public StackDockDebugView()
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
