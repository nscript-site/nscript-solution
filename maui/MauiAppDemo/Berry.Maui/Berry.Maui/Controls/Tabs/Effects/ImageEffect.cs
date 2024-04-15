﻿using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Berry.Maui.Controls.Effects;

public static class ImageEffect
{
#pragma warning disable SA1401 // Fields should be private
    public static BindableProperty TintColorProperty = BindableProperty.CreateAttached(
        "TintColor",
        typeof(Color),
        typeof(ImageEffect),
#if NET6_0_OR_GREATER
        Colors.DodgerBlue,
#else
        Color.Default,
#endif
        propertyChanged: OnTintColorPropertyPropertyChanged
    );
#pragma warning restore SA1401 // Fields should be private

    public static Color GetTintColor(BindableObject element)
    {
        return (Color)element.GetValue(TintColorProperty);
    }

    public static void SetTintColor(BindableObject element, Color value)
    {
        element.SetValue(TintColorProperty, value);
    }

    private static void OnTintColorPropertyPropertyChanged(
        BindableObject bindable,
        object oldValue,
        object newValue
    )
    {
        if (!(bindable is Image))
        {
            throw new InvalidOperationException(
                "Tint effect is only applicable on CachedImage and Image"
            );
        }

        AttachEffect((View)bindable, (Color)newValue);
    }

    private static void AttachEffect(View element, Color color)
    {
        if (
            element.Effects.FirstOrDefault(x => x is TintableImageEffect)
            is TintableImageEffect effect
        )
        {
            element.Effects.Remove(effect);
        }

        element.Effects.Add(new TintableImageEffect(color));
    }
}

public class TintableImageEffect : RoutingEffect
{
    public static readonly string Name = $"Sharpnado.{nameof(TintableImageEffect)}";

    public TintableImageEffect(Color color)
#if !NET6_0_OR_GREATER
        : base(Name)
#endif
    {
        TintColor = color;
    }

    public Color TintColor { get; }
}
