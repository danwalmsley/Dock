using Avalonia;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.UnitTests;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
