namespace MauiAppDemo;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new MainPage2();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);

#if WINDOWS
        window.Width = 512;
        window.Height = 900;
#endif

        return window;
    }
}
