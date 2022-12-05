using System;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WeakAurasInterface.Core.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private string _displayName;

    protected ViewModelBase(string displayName)
    {
        _displayName = displayName;
        ThrowOnInvalidPropertyName = true;
        PropertyChanging += (_, args) =>
        {
            if (args.PropertyName != null)
                VerifyPropertyName(args.PropertyName);
        };
    }

    public virtual string DisplayName
    {
        get => _displayName;
        protected set => SetProperty(ref _displayName, value);
    }

    protected virtual bool ThrowOnInvalidPropertyName { get; }

    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    public void VerifyPropertyName(string propertyName)
    {
        // Verify that the property name matches a real, 
        // public, instance property on this object. 
        if (TypeDescriptor.GetProperties(this)[propertyName] != null)
            return;
        string message = "Invalid property name: " + propertyName;
        if (ThrowOnInvalidPropertyName)
            throw new Exception(message);
        Debug.Fail(message);
    }
}