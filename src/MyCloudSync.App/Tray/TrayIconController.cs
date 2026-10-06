namespace MyCloudSync.App.Tray;

public class TrayIconController : IDisposable
{
    private readonly Form _mainForm;
    private readonly NotifyIcon _notifyIcon;

    public TrayIconController(Form mainForm)
    {
        _mainForm = mainForm;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Відкрити", null, (sender, e) => ShowMainForm());
        menu.Items.Add("Вийти", null, (sender, e) => Exit());

        _notifyIcon = new NotifyIcon
        {
            Icon = mainForm.Icon,
            Text = "MyCloudSync",
            ContextMenuStrip = menu,
            Visible = true
        };
        _notifyIcon.DoubleClick += (sender, e) => ShowMainForm();
    }

    public void SetStatusText(string status)
    {
        _notifyIcon.Text = $"MyCloudSync — {status}";
    }

    public void ShowNotification(string title, string text)
    {
        _notifyIcon.ShowBalloonTip(3000, title, text, ToolTipIcon.Info);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }

    private void ShowMainForm()
    {
        _mainForm.Show();
        _mainForm.WindowState = FormWindowState.Normal;
        _mainForm.Activate();
    }

    private void Exit()
    {
        _notifyIcon.Visible = false;
        Application.Exit();
    }
}
