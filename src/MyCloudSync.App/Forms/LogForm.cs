using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.App.Forms;

public partial class LogForm : Form
{
    private readonly ISyncLogRepository _logRepository;

    public LogForm(ISyncLogRepository logRepository)
    {
        InitializeComponent();
        _logRepository = logRepository;
        cmbFilter.SelectedIndex = 0;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        LoadEntries();
    }

    private void LoadEntries()
    {
        gridLog.Rows.Clear();
        var onlyErrors = cmbFilter.SelectedIndex == 1;

        foreach (var entry in _logRepository.GetRecent(500))
        {
            if (onlyErrors && entry.Result != LogResult.Error)
            {
                continue;
            }

            var rowIndex = gridLog.Rows.Add(
                entry.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                entry.Message ?? "",
                entry.Action,
                GetResultText(entry.Result));

            if (entry.Result == LogResult.Error)
            {
                gridLog.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Firebrick;
            }
        }
    }

    private static string GetResultText(LogResult result)
    {
        return result switch
        {
            LogResult.Success => "Успішно",
            LogResult.Warning => "Попередження",
            LogResult.Error => "Помилка",
            _ => ""
        };
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        LoadEntries();
    }

    private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (IsHandleCreated)
        {
            LoadEntries();
        }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        var answer = MessageBox.Show(this, "Очистити весь журнал?", "MyCloudSync", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer == DialogResult.Yes)
        {
            _logRepository.Clear();
            LoadEntries();
        }
    }
}
