﻿using System.ComponentModel;
using System.Reflection;

namespace NStyles.Controls;

public sealed class LongViewModel : PropertyViewModelBase<long?>
{
    public LongViewModel(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
        : base(viewmodel, displayName, propertyInfo)
    {
    }
}