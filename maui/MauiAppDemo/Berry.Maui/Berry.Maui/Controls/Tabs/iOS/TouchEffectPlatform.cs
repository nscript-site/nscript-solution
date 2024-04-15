﻿using System.ComponentModel;
using Berry.Maui.Controls.Effects.iOS.GestureCollectors;
using Berry.Maui.Controls.Effects.iOS.GestureRecognizers;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Berry.Maui.Controls.Effects.iOS;

public class TouchEffectPlatform : PlatformEffect
{
    public bool IsDisposed => Container == null || Container.Handle == NativeHandle.Zero;
    public UIView View => Control ?? Container;

    UIView _layer;
    float _alpha;

    protected override void OnAttached()
    {
        View.UserInteractionEnabled = true;
        _layer = new UIView
        {
            UserInteractionEnabled = false,
            Opaque = false,
            Alpha = 0,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        UpdateEffectColor();
        TouchGestureCollector.Add(View, OnTouch);

        View.AddSubview(_layer);
        View.BringSubviewToFront(_layer);
        _layer.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
        _layer.LeftAnchor.ConstraintEqualTo(View.LeftAnchor).Active = true;
        _layer.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
        _layer.RightAnchor.ConstraintEqualTo(View.RightAnchor).Active = true;
    }

    protected override void OnDetached()
    {
        TouchGestureCollector.Delete(View, OnTouch);
        _layer?.RemoveFromSuperview();
        _layer?.Dispose();
    }

    void OnTouch(TouchGestureRecognizer.TouchArgs e)
    {
        switch (e.State)
        {
            case TouchGestureRecognizer.TouchState.Started:
                BringLayer();
                break;

            case TouchGestureRecognizer.TouchState.Ended:
                EndAnimation();
                break;

            case TouchGestureRecognizer.TouchState.Cancelled:
                if (!IsDisposed && _layer != null)
                {
                    _layer.Layer.RemoveAllAnimations();
                    _layer.Alpha = 0;
                }

                break;
        }
    }

    protected override void OnElementPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnElementPropertyChanged(e);

        if (e.PropertyName == TouchEffect.ColorProperty.PropertyName)
        {
            UpdateEffectColor();
        }
    }

    void UpdateEffectColor()
    {
        var color = TouchEffect.GetColor(Element);
        if (color == Colors.Transparent)
        {
            return;
        }

        _alpha = color.Alpha < 1.0 ? 1 : (float)0.3;
        _layer.BackgroundColor = color.ToUIColor();
    }

    void BringLayer()
    {
        _layer.Layer.RemoveAllAnimations();
        _layer.Alpha = _alpha;
        View.BringSubviewToFront(_layer);
    }

    void EndAnimation()
    {
        if (!IsDisposed && _layer != null)
        {
            _layer.Layer.RemoveAllAnimations();
            UIView.Animate(
                0.225,
                () =>
                {
                    _layer.Alpha = 0;
                }
            );
        }
    }
}
