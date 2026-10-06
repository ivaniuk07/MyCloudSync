using Microsoft.Extensions.DependencyInjection;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.App.Forms;

public partial class SettingsForm : Form
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAutostartService _autostartService;
    private readonly IServiceProvider _services;
    private AppSettings _settings = new();

    public SettingsForm(ISettingsRepository settingsRepository, IAutostartService autostartService, IServiceProvider services)
    {
        InitializeComponent();
        _settingsRepository = settingsRepository;
        _autostartService = autostartService;
        _services = services;
    }

    public string LocalPath => txtLocalPath.Text;

    public string DriveFolderName => txtDriveFolder.Text;

    public SyncMode SelectedMode
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

    public IReadOnlyList<string> ExcludePatterns =>
        txtExclude.Text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _settings = _settingsRepository.Load();
        numInterval.Value = Math.Clamp(_settings.CheckIntervalSec / 60, 1, 60);
        chkStartMinimized.Checked = _settings.StartMinimized;
        chkAutostart.Checked = _autostartService.IsEnabled();
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
        using var picker = _services.GetRequiredService<DriveFolderPickerForm>();
        if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedFolder != null)
        {
            txtDriveFolder.Text = picker.SelectedFolder.Name;
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            _settings.CheckIntervalSec = (int)numInterval.Value * 60;
            _settings.StartMinimized = chkStartMinimized.Checked;
            _settings.Autostart = chkAutostart.Checked;

            _settingsRepository.Save(_settings);
            _autostartService.SetEnabled(chkAutostart.Checked);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageHelper.ShowError(this, ex);
        }
    }
}
