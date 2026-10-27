namespace MyCloudSync.App.Forms;

public static class MessageHelper
{
    public static void ShowError(IWin32Window owner, Exception exception)
    {
        MessageBox.Show(owner, exception.Message, "MyCloudSync", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void ShowWarning(IWin32Window owner, string text)
    {
        MessageBox.Show(owner, text, "MyCloudSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static bool Confirm(IWin32Window owner, string text)
    {
        return MessageBox.Show(owner, text, "MyCloudSync", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
