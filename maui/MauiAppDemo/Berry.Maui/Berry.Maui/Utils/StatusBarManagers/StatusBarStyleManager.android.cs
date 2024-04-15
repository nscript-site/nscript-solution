﻿using System;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Color = Android.Graphics.Color;
using Window = Android.Views.Window;

namespace Berry.Maui;

public partial interface IStatusBarStyleManager
{
    void SetColoredNavigationBar(string hexColor);

    void SetStatusBarIsLight(Window currentWindow, bool isLight);
}

public class StatusBarStyleManager : IStatusBarStyleManager
{
    public void SetDefault()
    {
        if (Application.Current is null || Platform.CurrentActivity is null)
            return;

        // Transparent
        //https://gist.github.com/lopspower/03fb1cc0ac9f32ef38f4
        //manager.SetColoredStatusBar("#00FFFFFF");

        //manager.SetWhiteStatusBar();
        //var currentWindow = this.GetCurrentWindow();
        //currentWindow.SetStatusBarColor(Color.ParseColor("#00FFFFFF"));
        //SetStatusBarIsLight(currentWindow, true);

        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            //SetStatusBarIsLight(currentWindow, false);
            //SetColoredStatusBar("#00FFFFFF", false);
            SetColoredStatusBar("#000000", false);
        }
        else
        {
            //SetStatusBarIsLight(currentWindow, true);
            //SetColoredStatusBar("#00FFFFFF", true);
            SetColoredStatusBar("#FFFFFF", true);
        }

        //manager.SetColoredNavigationBar("#000000");
        //manager.SetColoredNavigationBar("#EEEEEE");

        var view = Platform.CurrentActivity.FindViewById(Android.Resource.Id.Content)!;
        view.SetFitsSystemWindows(false);
        //WindowCompat.SetDecorFitsSystemWindows(currentWindow, false);

        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            SetColoredNavigationBar("#000000");
            view.SetBackgroundColor(Color.ParseColor("#000000"));

            SetNavigationBarLight(false);
        }
        else
        {
            SetColoredNavigationBar("#EEEEEE");
            view.SetBackgroundColor(Color.ParseColor("#EEEEEE"));

            SetNavigationBarLight(true);
        }
    }

    public void SetColoredStatusBar(string hexColor, bool isLight)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var currentWindow = GetCurrentWindow();
            SetStatusBarIsLight(currentWindow, isLight);
            currentWindow.SetStatusBarColor(Color.ParseColor(hexColor));
            //currentWindow.SetNavigationBarColor(Color.ParseColor(hexColor));
        });
    }

    public void SetColoredNavigationBar(string hexColor)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var currentWindow = GetCurrentWindow();
            currentWindow.SetNavigationBarColor(Color.ParseColor(hexColor));

            //currentWindow!.DecorView!.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightNavigationBar;
        });
    }

    public void SetWhiteStatusBar()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var currentWindow = GetCurrentWindow();
            SetStatusBarIsLight(currentWindow, true);
            currentWindow.SetStatusBarColor(Color.White);
            //currentWindow.SetNavigationBarColor(Color.White);
        });
    }

    public void SetStatusBarIsLight(Window currentWindow, bool isLight)
    {
        var windowInsetsController = new WindowInsetsControllerCompat(
            currentWindow,
            currentWindow.DecorView
        )
        {
            AppearanceLightStatusBars = isLight
        };

        return;

        if ((int)Build.VERSION.SdkInt < 30)
        {
            currentWindow.DecorView.SystemUiVisibility = isLight
                ? (StatusBarVisibility)(SystemUiFlags.LightStatusBar)
                : 0;
        }
        else
        {
            var lightStatusBars = isLight ? WindowInsetsControllerAppearance.LightStatusBars : 0;
#pragma warning disable CA1416
            currentWindow.InsetsController?.SetSystemBarsAppearance(
                (int)lightStatusBars,
                (int)lightStatusBars
            );
#pragma warning restore CA1416
        }
    }

    public void SetNavigationBarLight(bool isLight)
    {
        var currentWindow = GetCurrentWindow();

        //var view = Platform.CurrentActivity.FindViewById(Android.Resource.Id.Content)!;
        //view.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightNavigationBar;

        var windowInsetsController = new WindowInsetsControllerCompat(
            currentWindow,
            currentWindow.DecorView
        );
        windowInsetsController.AppearanceLightNavigationBars = isLight;
    }

    private static Window GetCurrentWindow()
    {
        if (Platform.CurrentActivity is null)
            throw new ArgumentNullException(nameof(Platform.CurrentActivity));

        var window = Platform.CurrentActivity.Window!;
        window.ClearFlags(WindowManagerFlags.TranslucentStatus);
        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
        return window;
    }
}
