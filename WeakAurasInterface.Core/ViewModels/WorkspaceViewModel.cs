using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WeakAurasInterface.Core.ViewModels;

/// <summary>
///     Expands the ViewModelBase functionality with IDataErrorInfo and a default close command.
/// </summary>
public abstract class WorkspaceViewModel : ViewModelBase, IDataErrorInfo
{
    protected WorkspaceViewModel(string displayName) : base(displayName)
    {
        CloseCommand = new RelayCommand(OnRequestClose);
    }

    public IRelayCommand CloseCommand { get; set; }

    public abstract string Error { get; }
    public abstract string this[string columnName] { get; }


    public event EventHandler? RequestClose;

    private void OnRequestClose()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}