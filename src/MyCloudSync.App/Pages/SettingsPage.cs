using Microsoft.Extensions.DependencyInjection;
using MyCloudSync.App.Forms;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.App.Pages;

public partial class SettingsPage : UserControl
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAutostartService _autostartService;
    private readonly IAccountRepository _accountRepository;
    private readonly ISyncPairRepository _pairRepository;
    private readonly IFileStateRepository _fileStateRepository;
    private readonly IServiceProvider _services;
    private AppSettings _settings = new();
    private SyncPair? _pair;
    private string _driveFolderId = "";

    public SettingsPage(
        ISettingsRepository settingsRepository,
        IAutostartService autostartService,
        IAccountRepository accountRepository,
        ISyncPairRepository pairRepository,
        IFileStateRepository fileStateRepository,
        IServiceProvider services)
    {
        InitializeComponent();
        _settingsRepository = settingsRepository;
        _autostartService = autostartService;
        _accountRepository = accountRepository;
        _pairRepository = pairRepository;
        _fileStateRepository = fileStateRepository;
        _services = services;
    }

    public event Action? Saved;

    private SyncMode SelectedMode
    {
        get
        {
            if (rbUploadOnly.Checked)
            {
                return SyncMode.UploadOnly;
            }

            if (rbDownloadOnly.Checked)
            {
                return SyncMode.DownloadOnly;
            }

            return SyncMode.TwoWay;
        }
    }

    private IReadOnlyList<string> ExcludePatterns =>
        txtExclude.Text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public void LoadSettings()
    {
        _settings = _settingsRepository.Load();
        numInterval.Value = Math.Clamp(_settings.CheckIntervalSec / 60, 1, 60);
        chkStartMinimized.Checked = _settings.StartMinimized;
        chkAutostart.Checked = _autostartService.IsEnabled();

        _pair = _pairRepository.GetActive();
        if (_pair == null)
        {
            return;
        }

        txtLocalPath.Text = _pair.LocalPath;
        txtDriveFolder.Text = _pair.DriveFolderName ?? "";
        _driveFolderId = _pair.DriveFolderId;
        rbUploadOnly.Checked = _pair.Mode == SyncMode.UploadOnly;
        rbDownloadOnly.Checked = _pair.Mode == SyncMode.DownloadOnly;
        rbTwoWay.Checked = _pair.Mode == SyncMode.TwoWay;
        txtExclude.Text = string.Join("; ", _pairRepository.GetExcludeRules(_pair.Id).Select(rule => rule.Pattern));
    }

    private void btnBrowseLocal_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtLocalPath.Text = dialog.SelectedPath;
        }
    }

    private void btnChooseDrive_Click(object sender, EventArgs e)
    {
        if (_accountRepository.GetCurrent() == null)
        {
            MessageHelper.ShowWarning(this, "Спочатку увійдіть у Google (ліва панель, унизу).");
            return;
        }

        using var picker = _services.GetRequiredService<DriveFolderPickerForm>();
        if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedFolder != null)
        {
            txtDriveFolder.Text = picker.SelectedFolder.Name;
            _driveFolderId = picker.SelectedFolder.Id;
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            if (!SavePair())
            {
                return;
            }

            _settings.CheckIntervalSec = (int)numInterval.Value * 60;
            _settings.StartMinimized = chkStartMinimized.Checked;
            _settings.Autostart = chkAutostart.Checked;

            _settingsRepository.Save(_settings);
            _autostartService.SetEnabled(chkAutostart.Checked);

            Saved?.Invoke();
        }
        catch (Exception ex)
        {
            MessageHelper.ShowError(this, ex);
        }
    }

    private bool SavePair()
    {
        var localPath = txtLocalPath.Text.Trim();
        if (localPath == "" && _driveFolderId == "")
        {
            return true;
        }

        var account = _accountRepository.GetCurrent();
        if (account == null)
        {
            MessageHelper.ShowWarning(this, "Спочатку увійдіть у Google (ліва панель, унизу).");
            return false;
        }

        if (localPath == "" || _driveFolderId == "")
        {
            MessageHelper.ShowWarning(this, "Оберіть і локальну папку, і папку на Google Drive.");
            return false;
        }

        if (!Directory.Exists(localPath))
        {
            MessageHelper.ShowWarning(this, "Локальну папку не знайдено.");
            return false;
        }

        var pair = _pair ?? new SyncPair();
        var foldersChanged = pair.LocalPath != localPath || pair.DriveFolderId != _driveFolderId;
        if (pair.Id != 0 && foldersChanged)
        {
            _fileStateRepository.DeleteByPair(pair.Id);
        }

        pair.AccountId = account.Id;
        pair.LocalPath = localPath;
        pair.DriveFolderId = _driveFolderId;
        pair.DriveFolderName = txtDriveFolder.Text;
        pair.Mode = SelectedMode;
        _pairRepository.Save(pair);
        _pairRepository.SaveExcludeRules(pair.Id, ExcludePatterns);
        _pair = pair;
        return true;
    }
}
