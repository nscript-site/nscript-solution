﻿using System.ComponentModel;
using System.Reflection;

namespace NStyles.Controls;

public sealed class BoolViewModel : PropertyViewModelBase<bool?>
{
    public BoolViewModel(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
        : base(viewmodel, displayName, propertyInfo)
    {
    }
}