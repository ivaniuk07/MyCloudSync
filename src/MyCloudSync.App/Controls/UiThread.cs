namespace MyCloudSync.App.Controls;

public static class UiThread
{
    public static void Run(Control control, Action action)
    {
        if (!control.IsHandleCreated || control.IsDisposed)
        {
            return;
        }

        if (control.InvokeRequired)
        {
            control.BeginInvoke(action);
        }
        else
        {
            action();
        }
    }
}
