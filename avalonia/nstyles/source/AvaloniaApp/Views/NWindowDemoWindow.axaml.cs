using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NStyles;
using NStyles.Controls;

namespace AvaloniaApp.Views;

public partial class NWindowDemoWindow : NWindow
{
    public NWindowDemoWindow()
    {
        InitializeComponent();
    }

    private NTheme Theme
    {
        get
        {
            var _theme = NTheme.GetInstance();
            return _theme;
        }
    }

    private void Button_ToggleBaseTheme_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Theme.SwitchBaseTheme();
    }

    private void Button_ToggleColorTheme_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Theme.SwitchColorTheme();
    }
}