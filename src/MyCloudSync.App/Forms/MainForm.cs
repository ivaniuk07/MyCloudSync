using MyCloudSync.App.Controls;
using MyCloudSync.App.Pages;
using MyCloudSync.App.Tray;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Sync;

namespace MyCloudSync.App.Forms;

public partial class MainForm : Form
{
    private const int MinimizeBoxStyle = 0x20000;

    private readonly SyncCoordinator _coordinator;
    private readonly SyncScheduler _scheduler;
    private readonly IFolderWatcher _watcher;
    private readonly IAuthService _authService;
    private readonly IAccountRepository _accountRepository;
    private readonly ISyncPairRepository _pairRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly SyncPage _syncPage;
    private readonly LogPage _logPage;
    private readonly SettingsPage _settingsPage;
    private readonly TrayIconController _tray;
    private Point _dragStart;

    public MainForm(
        SyncCoordinator coordinator,
        SyncScheduler scheduler,
        IFolderWatcher watcher,
        IAuthService authService,
        IAccountRepository accountRepository,
        ISyncPairRepository pairRepository,
        ISettingsRepository settingsRepository,
        SyncPage syncPage,
        LogPage logPage,
        SettingsPage settingsPage)
    {
        InitializeComponent();
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        _coordinator = coordinator;
        _scheduler = scheduler;
        _watcher = watcher;
        _authService = authService;
        _accountRepository = accountRepository;
        _pairRepository = pairRepository;
        _settingsRepository = settingsRepository;
        _syncPage = syncPage;
        _logPage = logPage;
        _settingsPage = settingsPage;
        _tray = new TrayIconController(this);

        AddPage(_syncPage);
        AddPage(_logPage);
        AddPage(_settingsPage);
        _syncPage.OpenLogRequested += () => ShowPage(_logPage);
        _settingsPage.Saved += OnSettingsSaved;

        _coordinator.StatusChanged += status => UiThread.Run(this, () => _tray.SetStatusText(SyncPage.GetStatusText(status)));
        _coordinator.ConflictDetected += path => UiThread.Run(this, () => ShowConflict(path));
        _scheduler.SyncRequested += () => _ = _coordinator.RunSyncAsync();
        _watcher.FileChanged += _scheduler.OnLocalChange;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.Style |= MinimizeBoxStyle;
            return parameters;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ShowAccount();
        ShowPage(_syncPage);
        StartSync();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        var startedByWindows = Environment.GetCommandLineArgs().Contains("--minimized");
        if (startedByWindows && _settingsRepository.Load().StartMinimized)
        {
            Hide();
        }
    }

    private void AddPage(UserControl page)
    {
        page.Dock = DockStyle.Fill;
        pnlContent.Controls.Add(page);
    }

    private void ShowPage(UserControl page)
    {
        page.BringToFront();
        SetNavState(btnNavSync, page == _syncPage);
        SetNavState(btnNavLog, page == _logPage);
        SetNavState(btnNavSettings, page == _settingsPage);

        if (page == _logPage)
        {
            _logPage.LoadEntries();
        }

        if (page == _settingsPage)
        {
            _settingsPage.LoadSettings();
        }
    }

    private static void SetNavState(Button button, bool active)
    {
        button.ForeColor = active ? Theme.Accent : Theme.Text;
        button.Font = new Font(button.Font, active ? FontStyle.Bold : FontStyle.Regular);
    }

    private void StartSync()
    {
        _watcher.Stop();
        _scheduler.Stop();
        _coordinator.Refresh();
        _syncPage.ShowPair();

        var pair = _pairRepository.GetActive();
        if (pair == null)
        {
            return;
        }

        try
        {
            _watcher.Start(pair.LocalPath);
        }
        catch (Exception ex)
        {
            MessageHelper.ShowError(this, ex);
            return;
        }

        var settings = _settingsRepository.Load();
        _scheduler.Start(TimeSpan.FromSeconds(settings.CheckIntervalSec));
        _ = _coordinator.RunSyncAsync();
    }

    private void ShowAccount()
    {
        var account = _accountRepository.GetCurrent();
        lblAccount.Text = account == null ? "Акаунт не підключено" : account.Email;
        lnkAccount.Text = account == null ? "Увійти через Google" : "Вийти з акаунта";
    }

    private void ShowConflict(string path)
    {
        if (_settingsRepository.Load().ShowNotifications)
        {
            _tray.ShowNotification("Конфлікт версій", $"Файл {path} змінено і тут, і на Google Drive. Збережено обидві версії.");
        }
    }

    private void OnSettingsSaved()
    {
        ShowPage(_syncPage);
        StartSync();
    }

    private async void lnkAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        lnkAccount.Enabled = false;
        try
        {
            var account = _accountRepository.GetCurrent();
            if (account == null)
            {
                await _authService.SignInAsync();
            }
            else
            {
                if (!MessageHelper.Confirm(this, $"Вийти з акаунта {account.Email}? Синхронізацію буде зупинено, файли залишаться на місці."))
                {
                    return;
                }

                _watcher.Stop();
                _scheduler.Stop();
                await _authService.SignOutAsync(account);
            }

            ShowAccount();
            StartSync();
        }
        catch (Exception ex)
        {
            MessageHelper.ShowError(this, ex);
        }
        finally
        {
            lnkAccount.Enabled = true;
        }
    }

    private void btnNavSync_Click(object sender, EventArgs e)
    {
        ShowPage(_syncPage);
    }

    private void btnNavLog_Click(object sender, EventArgs e)
    {
        ShowPage(_logPage);
    }

    private void btnNavSettings_Click(object sender, EventArgs e)
    {
        ShowPage(_settingsPage);
    }

    private void btnMinimize_Click(object sender, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        Hide();
    }

    private void header_MouseDown(object sender, MouseEventArgs e)
    {
        _dragStart = e.Location;
    }

    private void header_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Location = new Point(Location.X + e.X - _dragStart.X, Location.Y + e.Y - _dragStart.Y);
        }
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
        _watcher.Stop();
        _scheduler.Stop();
        _tray.Dispose();
        base.OnFormClosed(e);
    }
}
