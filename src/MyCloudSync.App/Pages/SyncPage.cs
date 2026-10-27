using MyCloudSync.App.Controls;
using MyCloudSync.App.Forms;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;
using MyCloudSync.Core.Sync;

namespace MyCloudSync.App.Pages;

public partial class SyncPage : UserControl
{
    private readonly SyncCoordinator _coordinator;
    private readonly ISyncPairRepository _pairRepository;

    public SyncPage(SyncCoordinator coordinator, ISyncPairRepository pairRepository)
    {
        InitializeComponent();
        _coordinator = coordinator;
        _pairRepository = pairRepository;

        _coordinator.StatusChanged += status => UiThread.Run(this, () => ShowStatus(status));
        _coordinator.ProgressChanged += (done, total) => UiThread.Run(this, () => ShowProgress(done, total));
    }

    public event Action? OpenLogRequested;

    public static string GetStatusText(SyncStatus status)
    {
        return status switch
        {
            SyncStatus.NotConnected => "Не підключено",
            SyncStatus.Idle => "Синхронізовано",
            SyncStatus.Syncing => "Виконується синхронізація",
            SyncStatus.Paused => "Пауза",
            SyncStatus.Offline => "Немає мережі",
            SyncStatus.Error => "Помилка (див. журнал)",
            _ => ""
        };
    }

    public void ShowPair()
    {
        var pair = _pairRepository.GetActive();
        lblLocalPath.Text = pair?.LocalPath ?? "Не вибрано";
        lblDrivePath.Text = pair?.DriveFolderName ?? "Не вибрано";
        ShowStatus(_coordinator.Status);
    }

    private void ShowStatus(SyncStatus status)
    {
        var (back, border, fore) = GetStatusColors(status);
        pnlStatus.BackColor = back;
        pnlStatus.BorderColor = border;
        lblDot.ForeColor = fore;
        lblStatus.ForeColor = fore;
        lblStatus.Text = GetStatusText(status);
        lblUpdated.Text = $"Оновлено о {DateTime.Now:HH:mm}";
        btnPause.Text = status == SyncStatus.Paused ? "Відновити" : "Пауза";
        progressSync.Visible = status == SyncStatus.Syncing;

        if (status != SyncStatus.Syncing)
        {
            ShowLastSync();
        }
    }

    private void ShowLastSync()
    {
        var lastSync = _pairRepository.GetActive()?.LastSyncAt;
        if (lastSync == null)
        {
            lblLastSync.Text = "Останній запуск: —";
            return;
        }

        var local = lastSync.Value.ToLocalTime();
        var text = local.Date == DateTime.Today ? $"сьогодні, {local:HH:mm}" : local.ToString("dd.MM.yyyy HH:mm");
        lblLastSync.Text = $"Останній запуск: {text}";
    }

    private void ShowProgress(int done, int total)
    {
        progressSync.Maximum = Math.Max(total, 1);
        progressSync.Value = Math.Min(done, progressSync.Maximum);
    }

    private static (Color Back, Color Border, Color Fore) GetStatusColors(SyncStatus status)
    {
        return status switch
        {
            SyncStatus.Idle => (Theme.GreenLight, Theme.GreenBorder, Theme.Green),
            SyncStatus.Syncing => (Theme.AccentLight, Theme.AccentBorder, Theme.Accent),
            SyncStatus.Offline => (Theme.OrangeLight, Theme.OrangeBorder, Theme.Orange),
            SyncStatus.Error => (Theme.RedLight, Theme.RedBorder, Theme.Red),
            _ => (Theme.GreyLight, Theme.Border, Theme.Muted)
        };
    }

    private async void btnSyncNow_Click(object sender, EventArgs e)
    {
        if (_pairRepository.GetActive() == null)
        {
            MessageHelper.ShowWarning(this, "Спочатку увійдіть у Google і оберіть папки в налаштуваннях.");
            return;
        }

        await _coordinator.RunSyncAsync();
    }

    private void btnPause_Click(object sender, EventArgs e)
    {
        if (_coordinator.Status == SyncStatus.Paused)
        {
            _coordinator.Resume();
            _ = _coordinator.RunSyncAsync();
        }
        else
        {
            _coordinator.Pause();
        }
    }

    private void lnkLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLogRequested?.Invoke();
    }
}
