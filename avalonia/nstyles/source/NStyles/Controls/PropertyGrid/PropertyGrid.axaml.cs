using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace NStyles.Controls;

public partial class PropertyGrid : UserControl
{
    static PropertyGrid()
    {
        ItemProperty.Changed.AddClassHandler<PropertyGrid>((_, args) => OnItemChanged(args));
    }

    public PropertyGrid()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<INotifyPropertyChanged?> ItemProperty = AvaloniaProperty.Register<PropertyGrid, INotifyPropertyChanged?>(nameof(Item), defaultValue: null);

    public INotifyPropertyChanged? Item
    {
        get { return GetValue(ItemProperty); }
        set { SetValue(ItemProperty, value); }
    }

    public static readonly StyledProperty<InstanceViewModel?> InstanceProperty = AvaloniaProperty.Register<PropertyGrid, InstanceViewModel?>(nameof(Instance), defaultValue: null);

    public InstanceViewModel? Instance
    {
        get { return GetValue(InstanceProperty); }
        set { SetValue(InstanceProperty, value); }
    }
    
    private static void OnItemChanged( AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is PropertyGrid propertyGrid)
        {
            propertyGrid.OnItemChanged(e.NewValue);
        }
    }

    private void OnItemChanged(object? newValue)
    {
        SetItem(newValue as INotifyPropertyChanged);
    }

    protected virtual void SetItem(INotifyPropertyChanged? item)
    {
        if (item is null)
        {
            return;
        }

        Instance = new InstanceViewModel(item);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Instance?.Dispose();
    }
}
