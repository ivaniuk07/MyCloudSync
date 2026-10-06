namespace MyCloudSync.App.Forms;

public static class MessageHelper
{
    public static void ShowError(IWin32Window owner, Exception exception)
    {
        MessageBox.Show(owner, exception.Message, "MyCloudSync", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
