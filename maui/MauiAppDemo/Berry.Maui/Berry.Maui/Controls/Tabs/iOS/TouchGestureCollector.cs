﻿using System;
using System.Collections.Generic;
using Berry.Maui.Controls.Effects.iOS.GestureRecognizers;
using UIKit;

namespace Berry.Maui.Controls.Effects.iOS.GestureCollectors;

internal static class TouchGestureCollector
{
    static Dictionary<UIView, GestureActionsContainer> Collection { get; } = [];

    public static void Add(UIView view, Action<TouchGestureRecognizer.TouchArgs> action)
    {
        if (Collection.ContainsKey(view))
        {
            Collection[view].Actions.Add(action);
        }
        else
        {
            var gest = new TouchGestureRecognizer
            {
                CancelsTouchesInView = false,
                Delegate = new TouchGestureRecognizerDelegate(view)
            };
            gest.OnTouch += ActionActivator;
            Collection.Add(
                view,
                new GestureActionsContainer { Recognizer = gest, Actions = [action] }
            );
            view.AddGestureRecognizer(gest);
        }
    }

    public static void Delete(UIView view, Action<TouchGestureRecognizer.TouchArgs> action)
    {
        if (!Collection.ContainsKey(view))
            return;

        var ci = Collection[view];
        ci.Actions.Remove(action);

        if (ci.Actions.Count != 0)
            return;
        view.RemoveGestureRecognizer(ci.Recognizer);
        Collection.Remove(view);
    }

    static void ActionActivator(object sender, TouchGestureRecognizer.TouchArgs e)
    {
        var gest = (TouchGestureRecognizer)sender;
        if (!Collection.ContainsKey(gest.View))
            return;

        var actions = Collection[gest.View].Actions.ToArray();
        foreach (var valueAction in actions)
        {
            valueAction?.Invoke(e);
        }
    }

    class GestureActionsContainer
    {
        public TouchGestureRecognizer? Recognizer { get; set; }
        public List<Action<TouchGestureRecognizer.TouchArgs>>? Actions { get; set; }
    }
}
