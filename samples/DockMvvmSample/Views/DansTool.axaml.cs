using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockMvvmSample.Views;

public partial class DansToolView : UserControl
{
    public DansToolView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

