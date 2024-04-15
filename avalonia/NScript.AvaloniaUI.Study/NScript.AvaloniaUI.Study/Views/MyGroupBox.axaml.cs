using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NScript.AvaloniaUI.Study.Views;

public partial class MyGroupBox : ContentControl
{
    public static readonly StyledProperty<string> GroupNameProperty =
   AvaloniaProperty.Register<MyGroupBox, string>(nameof(GroupName));

    public string GroupName
    {
        get { return GetValue(GroupNameProperty); }
        set { SetValue(GroupNameProperty, value); }
    }

    public MyGroupBox()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}