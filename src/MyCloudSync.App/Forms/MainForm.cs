using Microsoft.Extensions.DependencyInjection;
using MyCloudSync.App.Tray;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;
using MyCloudSync.Core.Sync;

namespace MyCloudSync.App.Forms;

public partial class MainForm : Form
{
    private readonly SyncCoordinator _coordinator;
    private readonly IAuthService _authService;
    private readonly IServiceProvider _services;
    private readonly TrayIconController _tray;

    public MainForm(SyncCoordinator coordinator, IAuthService authService, IServiceProvider services)
    {
        InitializeComponent();
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        _coordinator = coordinator;
        _authService = authService;
        _services = services;
        _tray = new TrayIconController(this);

        _coordinator.StatusChanged += OnStatusChanged;
        ShowStatus(_coordinator.Status);
    }

    private void OnStatusChanged(SyncStatus status)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowStatus(status)));
            return;
        }

        ShowStatus(status);
    }

    private void ShowStatus(SyncStatus status)
    {
        var text = GetStatusText(status);
        lblStatus.Text = text;
        btnPause.Text = status == SyncStatus.Paused ? "Відновити" : "Пауза";
        _tray.SetStatusText(text);
    }

    private static string GetStatusText(SyncStatus status)
    {
        return status switch
        {
            SyncStatus.NotConnected => "Не підключено",
            SyncStatus.Idle => "Синхронізовано",
            SyncStatus.Syncing => "Виконується синхронізація",
            SyncStatus.Paused => "Пауза",
            SyncStatus.Offline => "Немає мережі",
            SyncStatus.Error => "Помилка",
            _ => ""
        };
    }

    private async void btnSignIn_Click(object sender, EventArgs e)
    {
        try
        {
            var account = await _authService.SignInAsync();
            lblAccount.Text = $"Акаунт: {account.Email}";
        }
        catch (Exception ex)
        {
            MessageHelper.ShowError(this, ex);
        }
    }

    private async void btnSyncNow_Click(object sender, EventArgs e)
    {
        try
        {
            await _coordinator.RunSyncAsync();
        }
        catch (Exception ex)
        {
            MessageHelper.ShowError(this, ex);
        }
    }

    private void btnPause_Click(object sender, EventArgs e)
    {
        if (_coordinator.Status == SyncStatus.Paused)
        {
            _coordinator.Resume();
        }
        else
        {
            _coordinator.Pause();
        }
    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
        using var form = _services.GetRequiredService<SettingsForm>();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            lblLocalPath.Text = form.LocalPath == "" ? "—" : form.LocalPath;
            lblDriveFolder.Text = form.DriveFolderName == "" ? "—" : form.DriveFolderName;
        }
    }

    private void btnLog_Click(object sender, EventArgs e)
    {
        using var form = _services.GetRequiredService<LogForm>();
        form.ShowDialog(this);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _tray.Dispose();
        base.OnFormClosed(e);
    }
}
