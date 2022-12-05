using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using WeakAurasInterface.Core.Services;

namespace WeakAurasInterface.WPF.Services;

public class Win32FileService : FileServiceBase
{
    public override string BrowseDirectory(string initialDirectory)
    {
        var dialog = new FolderBrowserDialog
        {
            InitialDirectory = initialDirectory,
            ShowNewFolderButton = false
        };
        DialogResult dialogResult = dialog.ShowDialog();
        return dialogResult == DialogResult.OK ? dialog.SelectedPath : string.Empty;
    }

    public override void OpenDirectory(string directory)
    {
        if (Directory.Exists(directory))
            Process.Start("explorer.exe", directory);
    }
}